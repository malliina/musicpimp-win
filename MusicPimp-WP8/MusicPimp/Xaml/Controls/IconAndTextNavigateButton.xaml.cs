using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Media;

namespace Mle.MusicPimp.Controls {
    // does this even work? poc plz.
    public partial class IconAndTextNavigateButton : HyperlinkButton {
        public IconAndTextNavigateButton() {
            InitializeComponent();
            (this.Content as FrameworkElement).DataContext = this;
            //(btn.Content as FrameworkElement).DataContext = this;
        }
        public string FirstRow {
            get { return (string)GetValue(FirstRowProperty); }
            set { SetValue(FirstRowProperty, value); }
        }
        public static readonly DependencyProperty FirstRowProperty =
            DependencyProperty.Register("FirstRow", typeof(string), typeof(IconAndTextNavigateButton), new PropertyMetadata("First row here"));
        public string SecondRow {
            get { return (string)GetValue(SecondRowProperty); }
            set { SetValue(SecondRowProperty, value); }
        }
        public static readonly DependencyProperty SecondRowProperty =
            DependencyProperty.Register("SecondRow", typeof(string), typeof(IconAndTextNavigateButton), new PropertyMetadata("Second row here"));
        public ImageSource LeftImage {
            get { return (ImageSource)GetValue(LeftImageProperty); }
            set { SetValue(LeftImageProperty, value); }
        }
        public static readonly DependencyProperty LeftImageProperty =
            DependencyProperty.Register("LeftImage", typeof(ImageSource), typeof(IconAndTextNavigateButton), new PropertyMetadata(null));
    }
}
