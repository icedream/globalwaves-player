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
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace globalwaves.Player.WpfGui.Gui
{
    /// <summary>
    /// Interaktionslogik für MarqueeContentPanel.xaml
    /// </summary>
    public partial class MarqueeContentPanel : UserControl
    {
        DoubleAnimation _doubleAnimation;
        Storyboard _storyBoard;

        public MarqueeContentPanel()
        {
            InitializeComponent();

            _doubleAnimation = new DoubleAnimation();
            _doubleAnimation.RepeatBehavior = RepeatBehavior.Forever;
            _doubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(5));
            _doubleAnimation.EasingFunction = new QuadraticEase();
            ((QuadraticEase)_doubleAnimation.EasingFunction).EasingMode = EasingMode.EaseInOut;
            _storyBoard = new Storyboard();
            _storyBoard.Children.Add(_doubleAnimation);
            Storyboard.SetTarget(_doubleAnimation, this.title);
            Storyboard.SetTargetProperty(_doubleAnimation, new PropertyPath(Canvas.LeftProperty));

            Title = "";
            Artist = "";
        }

        public Duration Duration
        { get; set; }

        public bool ShowLoader
        { get { return this.fluidProgressBar1.Opacity > 0; } set { this.fluidProgressBar1.Opacity = value ? 1 : 0; this.artist.Opacity = this.title.Opacity = value ? 0 : 1; } }

        public string Title
        {
            get { return title.Content.ToString(); }
            set
            {
                _storyBoard.Stop();
                _storyBoard.Seek(TimeSpan.FromTicks(0));
                title.Content = value;
                if (title.ActualWidth < this.ActualWidth)
                {
                    return;
                }
                else
                {
                    //_doubleAnimation.From = this.ActualWidth;
                    //_doubleAnimation.To = -title.ActualWidth;
                    _doubleAnimation.From = 0;
                    _doubleAnimation.To = -title.ActualWidth + this.ActualWidth;
                    _storyBoard.Begin();
                }
            }
        }

        public string Artist
        { get { return artist.Content.ToString(); } set { artist.Content = value; } }

    }
}
