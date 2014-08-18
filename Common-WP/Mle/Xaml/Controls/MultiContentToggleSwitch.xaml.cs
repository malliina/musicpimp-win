using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Globalization;

namespace Mle.Xaml.Controls {
    public partial class MultiContentToggleSwitch : UserControl {
        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register("IsChecked", typeof(bool), typeof(MultiContentToggleSwitch), new PropertyMetadata(false));

        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(string), typeof(MultiContentToggleSwitch), new PropertyMetadata("header"));

        public static readonly DependencyProperty NameHeaderProperty =
            DependencyProperty.Register("Name", typeof(string), typeof(MultiContentToggleSwitch), new PropertyMetadata("name"));

        public static readonly DependencyProperty SubHeaderProperty =
            DependencyProperty.Register("SubHeader", typeof(string), typeof(MultiContentToggleSwitch), new PropertyMetadata("subheader"));

        public MultiContentToggleSwitch() {
            InitializeComponent();
            (this.Content as FrameworkElement).DataContext = this;
            CultureInfo.CurrentCulture.Weekdays();
        }
        public bool IsChecked {
            get { return (bool)GetValue(IsCheckedProperty); }
            set { SetValue(IsCheckedProperty, value); }
        }
        public string Header {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }
        public string NameHeader {
            get { return (string)GetValue(NameHeaderProperty); }
            set { SetValue(NameHeaderProperty, value); }
        }
        public string SubHeader {
            get { return (string)GetValue(SubHeaderProperty); }
            set { SetValue(SubHeaderProperty, value); }
        }
    }
}
