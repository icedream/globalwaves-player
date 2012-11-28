using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Diagnostics;

// Audio engine
using NAudio;
using NAudio.Codecs;
using NAudio.Mixer;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace globalwaves.Player.Backend
{
    public class StreamPlayer
    {
        public StreamPlayer()
        {
            Output = null;
            Channel = "main";
            Codec = StreamCodec.OGG_Vorbis;
        }

        // Private properties
        private float volume = 1.0f;

        // ???
        private CancellationTokenSource _cancel_play;
        private CancellationToken _cancel_play_token;
        private CancellationTokenSource _cancel_buffer;
        private CancellationToken _cancel_buffer_token;
        private StreamStatus _status = StreamStatus.Stopped;

        // Tasks
        private Task _loadThread;
        private Task _bufferThread;
        private Task _playThread;

        // Audio chain
        private FadeInOutSampleProvider _fade;
        private VolumeSampleProvider _volume;
        private AcmMp3FrameDecompressor _decoder;
        private BufferedWaveProvider _wavebuffer;
        private WaveToSampleProvider _wavesampler;
        private FixedBufferedStream stream;
        private Mp3Frame frame;

        // Events
        public event EventHandler<SampleEventArgs> SampleReceived;
        public event EventHandler StatusChanged;
        public event EventHandler MetadataChanged;

        // Public writable properties
        public IWavePlayer Output { get; set; }
        public string Channel { get; set; }
        public StreamCodec Codec { get; set; }
        public float Volume { get { return volume; } set { volume = value; if (_volume != null) _volume.Volume = value; } }
        public RawSourceWaveStream ExternalWaveStream { get; set; }

        // Public read-only properties
        public string Metadata { get; private set; }
        public Dictionary<string, string> StationInformation { get; private set; }
        public StreamStatus Status
        {
            get { return _status; }
            private set
            {
                _status = value;
                if (StatusChanged != null)
                    StatusChanged.Invoke(this, new EventArgs());
            }
        }
        public bool IsPlaying
        { get { return Output.PlaybackState == PlaybackState.Playing || (_loadThread != null && _loadThread.Status == TaskStatus.Running); } }

        // Private dynamic strings
        private string _channelListenM3UUrl
        { get { return string.Format("http://listen.globalwaves.tk/{0}/{1}.m3u", Channel, _codecName); } }
        private string _codecName
        {
            get
            {
                switch (Codec)
                {
                    case StreamCodec.MP3:
                        return "mp3";
                    case StreamCodec.AACplus2:
                        return "aac";
                    default: // OGG vorbis usually. :P
                        return "ogg";
                }
            }
        }
        private string _userAgent
        { get { return string.Format("{0}/{1}", "gw-player", ApplicationInformation.Version.ToString()); } }

        /// <summary>
        /// Stops the streaming.
        /// </summary>
        /// <param name="blocking">Blocks until every thread of the stream player finished</param>
        public void Stop(bool blocking = true)
        {
            if (!IsPlaying) // It's already stopped!
                return;
            _cancel_play.Cancel();
            if(blocking)
                try
                {
                    _loadThread.Wait();
                }
                catch { { } }
        }

        /// <summary>
        /// Starts the streaming asynchronously.
        /// </summary>
        public void Play()
        {
            if (IsPlaying) // There's already a stream playing!
                throw new InvalidOperationException("Can't play something while another stream is playing/paused in the same instance.");

            if (Output == null)
                throw new InvalidOperationException("You need to define an output.");

            if (_loadThread != null)
            {
                Console.WriteLine("play task: {0}", _loadThread.Status);
                if (_loadThread.Status == TaskStatus.RanToCompletion)
                    _loadThread.Dispose();
                else
                    throw new InvalidOperationException("Playing task didn't exit cleanly, so can't start a new playing task.");
            }

            _cancel_play = new CancellationTokenSource();
            _cancel_play_token = _cancel_play.Token;
            _loadThread = Task.Factory.StartNew(() => _load(), _cancel_play_token);
        }

        private void _load()
        {
            List<string> stream_urls = new List<string>();

            Status = StreamStatus.Connecting;

            // Request playlist
            var gen_req = (HttpWebRequest)HttpWebRequest.Create(_channelListenM3UUrl);
            gen_req.UserAgent = _userAgent;
            var gen_resp = (HttpWebResponse)gen_req.GetResponse();
            var gen_resp_s = gen_resp.GetResponseStream();
            var gen_resp_r = new StreamReader(gen_resp_s);
            stream_urls.AddRange(from url in gen_resp_r.ReadToEnd().Split('\n') where !url.TrimStart().StartsWith("#") && !string.IsNullOrEmpty(url.Trim()) select url.Trim());

            // Go through each URL
            foreach (string stream_url in stream_urls)
            {
                try
                {
                    if (_cancel_play_token.IsCancellationRequested)
                        break;

                    // Request the stream
                    Console.WriteLine("Connecting to " + stream_url + "...");
                    var stream_req = (HttpWebRequest)HttpWebRequest.Create(stream_url);
                    stream_req.UserAgent = _userAgent;
                    stream_req.Headers.Add("icy-metadata", "1"); // Request icecast to also send metadata
                    var stream_resp = stream_req.GetResponse();

                    // Save stream info
                    var stream_info = new Dictionary<string, string>();
                    foreach (string key in stream_resp.Headers.AllKeys)
                        if (key.ToLower().StartsWith("ice-") || key.ToLower().StartsWith("icy-"))
                        {
                            stream_info.Add(key.ToLower(), stream_resp.Headers[key]);
                            Console.WriteLine("  " + key.ToLower() + ": " + stream_resp.Headers[key]);
                        }
                    StationInformation = stream_info;

                    // Wait for buffer to initialize properly
                    _buffer(stream_resp.GetResponseStream());

                    // Start playback
                    try
                    {
                        // Direct buffer for audio
                        _wavebuffer = new BufferedWaveProvider(_decoder.OutputFormat);
                        _wavebuffer.BufferDuration = TimeSpan.FromSeconds(10);

                        // Start buffering and playback
                        _cancel_buffer = new CancellationTokenSource();
                        _cancel_buffer_token = _cancel_buffer.Token;
                        _bufferThread = Task.Factory.StartNew(() => _bufferLoop(), _cancel_buffer_token);
                        _playThread = Task.Factory.StartNew(() => _play(), _cancel_play_token);
                        Console.WriteLine("Load thread now sleeping, waiting until play thread finishes.");
                        _playThread.Wait(_cancel_play_token);
                    }
                    catch (OperationCanceledException)
                    {
                        Console.WriteLine("Waiting for playback thread to exit...");
                        try
                        {
                            _playThread.Wait();
                        }
                        catch { { } }
                        _playThread.Dispose();
                    }
                    catch (Exception error)
                    {
                        Console.WriteLine("Load thread error: {0}", error.ToString());
                    }
                    if (_bufferThread.Status == TaskStatus.WaitingForChildrenToComplete || _bufferThread.Status == TaskStatus.Running)
                    {
                        Console.WriteLine("Waiting for buffering thread to exit...");
                        _cancel_buffer.Cancel();
                        try
                        {
                            _bufferThread.Wait();
                        }
                        catch { { } }
                        _bufferThread.Dispose();
                    }
                    break;
                }
                catch (Exception error)
                {
                    Console.WriteLine("Can't load stream: {0}", error.ToString());
                }
            }
            Console.WriteLine("Loading thread work done.");
        }

        private void _play()
        {
            /* Audio chain */

            // Sampling
            _wavesampler = new WaveToSampleProvider(new Wave16ToFloatProvider(_wavebuffer));

            // Fading component
            _fade = new FadeInOutSampleProvider(_wavesampler);
            _fade.BeginFadeIn(1500);

            // Notifying component
            var _notify = new NotifyingSampleProvider(_fade);
            _notify.Sample += new EventHandler<SampleEventArgs>(_notify_Sample);

            // Gain adjustment component
            _volume = new VolumeSampleProvider(_notify);
            _volume.Volume = this.Volume;

            // Output
            Output.Init(new SampleToWaveProvider16(_volume));

            /* Playback loop */
            do
            {
                if (_cancel_play.IsCancellationRequested)
                {
                    Console.WriteLine("[Playback thread] Cancellation requested.");
                    Console.WriteLine("[Playback thread] Stopping playback with fade out...");

                    // Fade out
                    _fade.BeginFadeOut(500);
                    Thread.Sleep(500);
                    Output.Stop();

                    //_cancel_play_token.ThrowIfCancellationRequested();
                    //Console.WriteLine("[Playback thread] WARNING: Cancellation token is not cleanly set!");
                    break;
                }

                if (Output.PlaybackState != PlaybackState.Playing && _wavebuffer.BufferedDuration.TotalMilliseconds > 2750)
                {
                    // Buffer is filled enough
                    Console.WriteLine("[Playback thread] Buffer is okay now, start playback!");
                    this.Status = StreamStatus.Playing;
                    Output.Play();
                }
                else if (Output.PlaybackState == PlaybackState.Playing && _wavebuffer.BufferedDuration.TotalMilliseconds < 2250)
                {
                    // Buffer is underrunning
                    Console.WriteLine("[Playback thread] Buffer is underrunning, pausing playback...");
                    this.Status = StreamStatus.Buffering;
                    Output.Pause();
                }

                if (_bufferThread.Exception != null)
                {
                    Console.WriteLine("[Playback thread] Buffering thread is faulted, aborting playback");
                    throw new Exception("Buffering thread faulted, aborting playback");
                }

                Thread.Sleep(100);
            }
            while (true);
        }

        void _notify_Sample(object sender, SampleEventArgs e)
        {
            if (SampleReceived != null)
                SampleReceived.Invoke(this, e);
        }

        private void _buffer(Stream mp3input)
        {
            Console.WriteLine("[Initial buffering] Meta interval: {0}", this.StationInformation.ContainsKey("icy-metaint") ? long.Parse(this.StationInformation["icy-metaint"]) : -1);
            
            // Auto-fix the stream and auto-parse metadata
            stream = new FixedBufferedStream(
                mp3input,
                this.StationInformation.ContainsKey("icy-metaint") ? long.Parse(this.StationInformation["icy-metaint"]) : -1
            );

            // Read first frame
            this.Status = StreamStatus.Buffering;
            frame = Mp3Frame.LoadFromStream(stream);

            Console.WriteLine("[Initial buffering] Initial frame: {0} bytes with {1} samples", frame.FrameLength, frame.SampleCount);
            
            // Make a decoder which can decode that (and sequentially following) frames
            // The decoder will decode to 16-bit samples
            _decoder = new AcmMp3FrameDecompressor(new Mp3WaveFormat(frame.SampleRate, frame.ChannelMode == ChannelMode.Mono ? 1 : 2, frame.FrameLength, frame.BitRate));
            Console.WriteLine("[Initial buffering] MP3 decoder will encode to {0}-encoded wave format.", _decoder.OutputFormat.Encoding);
        }

        private void _bufferLoop()
        {
            Console.WriteLine("[Buffering thread] Started.");

            // Buffering loop
            do
            {
                if (_cancel_buffer.IsCancellationRequested)
                {
                    Console.WriteLine("[Buffering thread] Cancellation requested.");
                    //_cancel_buffer_token.ThrowIfCancellationRequested();
                    //Console.WriteLine("[Buffering thread] WARNING: Cancellation token is not cleanly set!");
                    break;
                }

                // Checking metadata
                if (Metadata != stream.Metadata)
                {
                    Console.WriteLine("[Buffering thread] Metadata has been changed to \"{0}\"", stream.Metadata);
                    this.Metadata = stream.Metadata;
                    if (MetadataChanged != null)
                        MetadataChanged.Invoke(this, new EventArgs());
                }

                // Decompress a frame into a seperated buffer
                long bufferSize = 176000;
                byte[] buffer = new byte[bufferSize];
                int decompressedLength = _decoder.DecompressFrame(frame, buffer, 0);

                // If buffer overfills, clear the buffer
                if (_wavebuffer.BufferLength - _wavebuffer.BufferedBytes < decompressedLength)
                {
                    Console.WriteLine("[Buffering thread] Buffer is overrunning, clearing...");
                    _wavebuffer.ClearBuffer();
                }

                // Add the decompressed frame (samples) into the audio buffer for later playback
                _wavebuffer.AddSamples(buffer, 0, decompressedLength);

                // Read next frame
                frame = Mp3Frame.LoadFromStream(stream);

            } while (true);

            _decoder.Dispose();

        }
    }
}
