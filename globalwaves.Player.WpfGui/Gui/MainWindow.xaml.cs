using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Net;
using globalwaves.Player.WpfGui.Compatibility;
using Elysium.Theme;
using Elysium.Theme.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Converters;

namespace globalwaves.Player.WpfGui.Gui
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Elysium.Theme.Controls.Window
    {
        StreamPlayer player = new StreamPlayer();
        SampleEngine soundEngine = SampleEngine.Instance;
        Dictionary<string, Dictionary<string, object>> channels;
        EventHandler statusChanged;
        EventHandler metadataChanged;

        public MainWindow()
        {
            InitializeComponent();

            player.Channel = "trance";
            player.Codec = StreamCodec.MP3;

            player.MetadataChanged += (metadataChanged = new EventHandler(player_MetadataChanged));
            player.StatusChanged += (statusChanged = new EventHandler(player_StatusChanged));
            player.SampleReceived += new EventHandler<NAudio.Wave.SampleEventArgs>(player_SampleReceived);

            var output = new NAudio.Wave.WaveOut();
            output.NumberOfBuffers = 3;
            output.Volume = 1.0f;
            player.Output = output;

            soundEngine.IsPlaying = false;
            spaVisualization.RegisterSoundPlayer(soundEngine);

            Task.Factory.StartNew(() => getChannels());

            //ThemeManager.Instance.Dark(Colors.LightSteelBlue);
            ThemeManager.Instance.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(Instance_PropertyChanged);
        }

        void Instance_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Console.WriteLine("[ThemeManager] Property changed: {0}", e.PropertyName);
        }

        void player_SampleReceived(object sender, NAudio.Wave.SampleEventArgs e)
        {
            soundEngine.AddSample(e.Left, e.Right);
        }

        delegate void getchannelsDelegate();
        void getChannels()
        {
            Console.WriteLine("[MainWindow] getChannels() called with access={0}", this.Dispatcher.CheckAccess());
            if (this.Dispatcher.CheckAccess())
            {
                channelsPanel.Items.Clear();

                WebClient wc = new WebClient();
                string c = wc.DownloadString("http://globalwaves.tk/api/channels/list");
                wc.Dispose();

                channels = (Dictionary<string, Dictionary<string, object>>)JsonConvert.DeserializeObject(c, new Dictionary<string, Dictionary<string, object>>().GetType());
                
                foreach (string hid in channels.Keys)
                {
                    var channel = channels[hid];

                    var item = new TabItem();
                    item.Header = channel["name"];
                    item.Background =
                        //new SolidColorBrush(Colors.Transparent);
                        channelsPanel.Background;
                    item.Name = "play_" + hid;

                    channelsPanel.Items.Add(item);
                }
            }
            else
                this.Dispatcher.Invoke(
                    new getchannelsDelegate(getChannels)
                );
        }
        
        delegate void statusChangedDelegate(object sender, EventArgs e);
        void player_StatusChanged(object sender, EventArgs e)
        {
            Console.WriteLine("[MainWindow] player_statusChanged called with access={0}", this.Dispatcher.CheckAccess());
            if (this.Dispatcher.CheckAccess())
            {
                Console.WriteLine("[MainWindow] player_statusChanged: Access ok (condition passed)");
                if (!this.IsVisible)
                    return;
                switch (player.Status)
                {
                    case StreamStatus.Stopped:
                        soundEngine.IsPlaying = false;
                        streamToggle.IsEnabled = true;
                        streamToggle.Content = "Play";
                        break;
                    case StreamStatus.Playing:
                        soundEngine.IsPlaying = true;
                        if (metadata.Title != "" && metadata.Artist != "")
                        {
                            metadata.ShowLoader = false;
                        }
                        streamToggle.IsEnabled = true;
                        streamToggle.Content = "Stop";
                        break;
                    case StreamStatus.Buffering:
                    case StreamStatus.Connecting:
                        metadata.ShowLoader = true;
                        metadata.Artist = metadata.Title = "";
                        streamToggle.Content = player.Status.ToString();
                        break;
                }
                Console.WriteLine("[MainWindow] player_statusChanged: Finished processing.");
            }
            else
            {
                Console.WriteLine("[MainWindow] Invoking...");
                this.Dispatcher.Invoke(
                    new statusChangedDelegate(player_StatusChanged),
                    sender, e
                );

                Console.WriteLine("[MainWindow] Invoke finished");
            }
        }

        delegate void metadataChangedDelegate(object sender, EventArgs e);
        void player_MetadataChanged(object sender, EventArgs e)
        {
            Console.WriteLine("[MainWindow] player_MetadataChanged() called with access={0}", this.Dispatcher.CheckAccess());
            if (!this.IsVisible)
                return;
            if (player.Metadata == null)
            {
                return;
            }
            if (this.Dispatcher.CheckAccess())
            {
                var g = System.Text.RegularExpressions.Regex.Match(player.Metadata, @"^(?<artist>.+) \- (?<title>.+)$").Groups;
                metadata.Artist = g["artist"].Value;
                metadata.Title = g["title"].Value;

                metadata.ShowLoader = false;
            }
            else
                this.Dispatcher.Invoke(
                    new metadataChangedDelegate(player_MetadataChanged),
                    sender, e
                );
        }

        private void Window_Closed(object sender, EventArgs e)
        {
        }

        private void channelsPanel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (channelsPanel.SelectedItem == null)
                return;
            if (!((TabItem)channelsPanel.SelectedItem).Name.StartsWith("play_"))
                return;
            streamToggle.IsEnabled = false;
            var channel_hardid = ((TabItem)channelsPanel.SelectedItem).Name.Substring(5);
            var channel_streamid = channels[channel_hardid]["channelid"].ToString();
            Task.Factory.StartNew(() =>
            {
                if (player.Status != StreamStatus.Stopped)
                    player.Stop(true);
                player.Channel = channel_streamid;
                Play();
            });
        }

        private void Play()
        {
            if (player.IsPlaying)
                player.Stop(false);
            else
                player.Play();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            streamToggle.IsEnabled = false;
            player.StatusChanged -= statusChanged;
            player.MetadataChanged -= metadataChanged;
            player.Stop(true);
            System.Threading.Thread.Sleep(500);
            player.Output.Dispose();
        }

        private void streamToggle_Click(object sender, RoutedEventArgs e)
        {
            //Task.Factory.StartNew(() =>
            //{
            streamToggle.IsEnabled = false;
            Play();
            //});
        }
    }
}
