using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Mle.MusicPimp.ViewModels;

namespace Mle.MusicPimp.Controls {
    public partial class LockScreenPanel : UserControl {
        public LockScreenPanel() {
            InitializeComponent();
            //DataContext = LockScreen.Instance;
        }
    }
}
