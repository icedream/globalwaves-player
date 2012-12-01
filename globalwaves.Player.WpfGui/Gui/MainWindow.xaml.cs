using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Elysium.Theme.Controls;
using MetroWindow = Elysium.Theme.Controls.Window;

namespace globalwaves.Player.WpfGui.Gui
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        StreamPlayer player = new StreamPlayer();

        

        public MainWindow()
        {
            InitializeComponent();

            player.Channel = "trance";
            player.Codec = StreamCodec.MP3;

            player.MetadataChanged += new EventHandler(player_MetadataChanged);
            player.StatusChanged += new EventHandler(player_StatusChanged);

            var output = new NAudio.Wave.DirectSoundOut(100);
            player.Output = output;
        }

        delegate void statusChangedDelegate(object sender, EventArgs e);
        void player_StatusChanged(object sender, EventArgs e)
        {
            if (this.Dispatcher.CheckAccess())
            {
                switch (player.Status)
                {
                    case StreamStatus.Stopped:
                        toggleSwitch1.IsChecked = false;
                        break;
                    case StreamStatus.Buffering:
                    case StreamStatus.Connecting:
                    case StreamStatus.Playing:
                        toggleSwitch1.IsChecked = true;
                        break;
                }
            }
            else
                this.Dispatcher.Invoke(
                    new statusChangedDelegate(player_StatusChanged),
                    sender, e
                );
        }

        delegate void metadataChangedDelegate(object sender, EventArgs e);
        void player_MetadataChanged(object sender, EventArgs e)
        {
            if (player.Metadata == null)
                return;
            if (this.Dispatcher.CheckAccess())
            {
                var g = System.Text.RegularExpressions.Regex.Match(player.Metadata, @"^(?<artist>.+) \- (?<title>.+)$").Groups;
                lblArtist.Content = g["artist"].Value;
                lblTitle.Content = g["title"].Value;
            }
            else
                this.Dispatcher.Invoke(
                    new metadataChangedDelegate(player_MetadataChanged),
                    sender, e
                );
        }

        private void toggleSwitch1_Checked(object sender, RoutedEventArgs e)
        {
            player.Play();
        }

        private void toggleSwitch1_Unchecked(object sender, RoutedEventArgs e)
        {
            player.Stop(false);
        }
    }
}
