using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using globalwaves.Player.Backend;

namespace globalwaves.Player.Gui
{
    public partial class PlayerInterface : Form
    {
        StreamPlayer player = new StreamPlayer();

        delegate void TextVoidDelegate(string a);
        TextVoidDelegate nowplayingdel;
        TextVoidDelegate statusdel;

        bool rejectGuiChanges = false;

        List<float> audioDataL = new List<float>();

        public PlayerInterface()
        {
            InitializeComponent();
            player.Channel = "trance";
            player.Codec = StreamCodec.MP3;

            nowplayingdel = new TextVoidDelegate(SetNowPlaying);
            statusdel = new TextVoidDelegate(SetStatus);

            player.MetadataChanged += new EventHandler(player_MetadataChanged);
            player.StatusChanged += new EventHandler(player_StatusChanged);
            player.SampleReceived += new EventHandler<NAudio.Wave.SampleEventArgs>(player_SampleReceived);

            /*
            var output = new NAudio.Wave.WaveOut();
            output.DesiredLatency = 200;
            output.NumberOfBuffers = 3;
             */
            var output = new NAudio.Wave.DirectSoundOut(100);
            player.Output = output;
        }

        DateTime dt = DateTime.Now;
        void player_SampleReceived(object sender, NAudio.Wave.SampleEventArgs e)
        {
            audioDataL.Add(e.Left);
        }

        void player_StatusChanged(object sender, EventArgs e)
        {
            SetStatus(player.Status.ToString());
        }

        void player_MetadataChanged(object sender, EventArgs e)
        {
            SetNowPlaying(player.Metadata);
        }

        public void SetNowPlaying(string metadata)
        {
            if (metadata == null)
                return;
            if (this.Disposing || this.IsDisposed)
                return;
            if (rejectGuiChanges) return;
            if (this.InvokeRequired)
                this.Invoke(nowplayingdel, metadata);
            else
            {
                var g = System.Text.RegularExpressions.Regex.Match(metadata, @"^(?<artist>.+) \- (?<title>.+)$").Groups;
                this.lblArtist.Text = g["artist"].Value;
                this.lblTitle.Text = g["title"].Value;
            }
        }

        public void SetStatus(string status)
        {
            if (this.Disposing || this.IsDisposed)
                return;
            if (rejectGuiChanges) return;
            if (this.txtStatus.InvokeRequired)
                this.txtStatus.Invoke(statusdel, status);
            else
                this.txtStatus.Text = status;
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            if (!player.IsPlaying)
                player.Play();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            player.Stop(false);
        }

        private void PlayerInterface_FormClosed(object sender, FormClosedEventArgs e)
        {
            player.Stop(true);
            player.Output.Dispose();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (rejectGuiChanges) return;

            //System.Threading.Tasks.Task.Factory.StartNew(() => {
            try
            {
                //waveformTimer.Stop();
                lock (audioDataL)
                {
                    if (audioDataL.Count == 0 && player.Status == StreamStatus.Playing)
                        return;
                    else
                    {
                        audioDataL.Add(0);
                        audioDataL.Add(0);
                        audioDataL.Add(0);
                        audioDataL.Add(0);
                        audioDataL.Add(0);
                    }
                    float[] data = audioDataL.ToArray<float>();
                    audioDataL.Clear();
                    Image img = GetWaveform(ref data, Color.White, this.waveformL.Width, this.waveformL.Height);
                    this.waveformL.Image = BlurWaveform(img, waveformL.Image, this.BackColor);
                    var notify_bmp =
                        (player.Status == StreamStatus.Playing ? img : Properties.Resources.satellite_icon_iconfinder.ToBitmap())
                        .GetThumbnailImage(
                                16, 16,
                                new Image.GetThumbnailImageAbort(() => { return true; }),
                                IntPtr.Zero
                        ) as Bitmap;
                    var g = Graphics.FromImage(notify_bmp);
                    Image b = null;
                    switch (player.Status)
                    {
                        case StreamStatus.Buffering: b = Properties.Resources.buffering; break;
                        case StreamStatus.Connecting: b = Properties.Resources.arrow_dots; break;
                    }
                    if (b != null)
                        g.DrawImage(
                            b,
                            4f, 4f, 12f, 12f
                        );
                    notifyIcon1.Icon = Icon.FromHandle(notify_bmp.GetHicon());
                }
            }
            catch(Exception error)
            {
                Console.WriteLine("Error in waveform drawing: {0}", error.ToString());
            }
            finally
            {
                //waveformTimer.Start();
            }
            //});
        }

        public Image BlurWaveform(Image newwave, Image oldwave)
        {
            return BlurWaveform(newwave, oldwave, Color.Transparent);
        }
        public Image BlurWaveform(Image newwave, Image oldwave, Color backColor)
        {
            var output = new Bitmap(newwave.Width, newwave.Height);

            if (oldwave == null)
            {
                Console.WriteLine("[DrawWaveform] Cloning new bitmap as old bitmap (empty usually).");
                oldwave = output.Clone() as Image;
            }
            else
                oldwave = oldwave
                    .GetThumbnailImage(
                        oldwave.Width / 4, oldwave.Height / 4,
                        new Image.GetThumbnailImageAbort(() => { return true; }),
                        IntPtr.Zero
                    )
                    .GetThumbnailImage(
                        oldwave.Width, oldwave.Height,
                        new Image.GetThumbnailImageAbort(() => { return true; }),
                        IntPtr.Zero
                    );

            // Blur old waveform
            var attrib = new System.Drawing.Imaging.ImageAttributes();
            var matrix = new System.Drawing.Imaging.ColorMatrix();
            matrix.Matrix33 = 0.9f; // opactity
            attrib.SetColorMatrix(matrix);
            var g = Graphics.FromImage(output);
            if(backColor != null) g.Clear(backColor);
            g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;
            g.DrawImage(
                oldwave, // old bitmap
                new Rectangle(-2, 0, output.Width, output.Height), // Destination rectangle
                0, 0, oldwave.Width, oldwave.Height, // Source rectangle
                GraphicsUnit.Pixel, // Copy pixel-exact
                attrib // Matrix with opactiy
                );
            g.DrawImage(
                newwave,
                0, 0
                );
            g.Dispose();

            return output;
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            Rectangle rc = new Rectangle(0, 0, this.ClientSize.Width, this.ClientSize.Height);
            using (System.Drawing.Drawing2D.LinearGradientBrush brush = new System.Drawing.Drawing2D.LinearGradientBrush(rc, Color.DarkBlue, Color.RoyalBlue, 45F))
            {
                e.Graphics.FillRectangle(brush, rc);
            }
        }

        public Image GetWaveform(ref float[] data, Color color, int width, int height)
        {
            Bitmap new_bitmap = new Bitmap(width, height);

            using (Graphics g = Graphics.FromImage(new_bitmap))
            {
                g.Clear(Color.Transparent);

                g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;

                // Draw new waveform
                Pen pen = new Pen(color);
                pen.Color = color;
                pen.Width = 1;
                float pos = 0;
                float old_pos = 0;
                for (int i = 1; i < data.Length; i++)
                {
                    old_pos = pos;
                    pos = (float)new_bitmap.Width * (float)(i + 1) / (float)data.Length;
                    g.DrawLine(pen,
                        old_pos, // Old position
                        new_bitmap.Height - (int)((data[i - 1] + 1) * .5 * new_bitmap.Height),

                        pos, // New position
                        new_bitmap.Height - (int)((data[i] + 1) * .5 * new_bitmap.Height)
                        );
                }
            }
            return new_bitmap;
        }

        private void PlayerInterface_FormClosing(object sender, FormClosingEventArgs e)
        {
            rejectGuiChanges = true;
            waveformTimer.Stop();
        }

        private void volumeSlider1_VolumeChanged(object sender, EventArgs e)
        {
            player.Volume = volumeSlider1.Volume;
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            player.Stop(false);
        }

    }

}
