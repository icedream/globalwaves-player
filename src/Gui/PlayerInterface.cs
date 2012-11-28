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
        delegate void WaveFormDelegate(NAudio.Wave.WaveStream a);
        TextVoidDelegate nowplayingdel;
        TextVoidDelegate statusdel;
        WaveFormDelegate wavedel;

        List<float> audioDataL = new List<float>();

        public PlayerInterface()
        {
            InitializeComponent();
            player.Channel = "trance";
            player.Codec = StreamCodec.MP3;

            nowplayingdel = new TextVoidDelegate(SetNowPlaying);
            statusdel = new TextVoidDelegate(SetStatus);
            wavedel = new WaveFormDelegate(SetWave);

            player.MetadataChanged += new EventHandler(player_MetadataChanged);
            player.StatusChanged += new EventHandler(player_StatusChanged);
            player.SampleReceived += new EventHandler<NAudio.Wave.SampleEventArgs>(player_SampleReceived);

            var output = new NAudio.Wave.WaveOut();
            output.DesiredLatency = 200;
            output.NumberOfBuffers = 3;
            player.Output = output;
        }

        void player_SampleReceived(object sender, NAudio.Wave.SampleEventArgs e)
        {
            if (audioDataL.Count == 5000) audioDataL.RemoveAt(0);

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
            // TODO: Add now playing handler
        }

        public void SetStatus(string status)
        {
            if (this.txtStatus.InvokeRequired)
                this.txtStatus.Invoke(statusdel, status);
            else
                this.txtStatus.Text = status;
        }

        public void SetWave(NAudio.Wave.WaveStream stream)
        {
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
            try
            {
                lock (audioDataL)
                {
                    if (audioDataL.Count == 0)
                        return;
                    var data = audioDataL.ToArray<float>();
                    audioDataL.Clear();
                    DrawNormalizedAudio(ref data, this.waveformL, Color.DarkBlue);
                    data = null;
                }
            }
            catch { { } }
        }

        public static void DrawNormalizedAudio(ref float[] data, PictureBox pb, Color color)
        {
            Bitmap bmp;
            if (pb.Image == null)
            {
                bmp = new Bitmap(pb.Width, pb.Height);
            }
            else
            {
                bmp = (Bitmap)pb.Image;
            }

            int BORDER_WIDTH = 5;
            int width = bmp.Width - (2 * BORDER_WIDTH);
            int height = bmp.Height - (2 * BORDER_WIDTH);

            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.Clear(pb.BackColor);
                Pen pen = new Pen(color);
                int size = data.Length;
                for (int iPixel = 0; iPixel < width; iPixel++)
                {
                    // determine start and end points within WAV
                    int start = (int)((float)iPixel * ((float)size / (float)width));
                    int end = (int)((float)(iPixel + 1) * ((float)size / (float)width));
                    float min = float.MaxValue;
                    float max = float.MinValue;
                    for (int i = start; i < end; i++)
                    {
                        float val = data[i];
                        min = val < min ? val : min;
                        max = val > max ? val : max;
                    }
                    int yMax = BORDER_WIDTH + height - (int)((max + 1) * .5 * height);
                    int yMin = BORDER_WIDTH + height - (int)((min + 1) * .5 * height);
                    g.DrawLine(pen, iPixel + BORDER_WIDTH - 1, yMax,
                        iPixel + BORDER_WIDTH, yMin);
                }
            }

            pb.Image = bmp;

            pb.Refresh();
        }

    }

}
