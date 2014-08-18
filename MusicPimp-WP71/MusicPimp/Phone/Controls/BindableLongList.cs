using Microsoft.Phone.Controls;
using System;
using System.Windows;

namespace Mle.MusicPimp.Phone.Controls {
    public class BindableLongList : LongListSelector {
        public static readonly DependencyProperty IsFlatListBindingProperty = DependencyProperty.Register(
          "IsFlatListBinding",
          typeof(Boolean),
          typeof(BindableLongList),
          new PropertyMetadata(false, new PropertyChangedCallback(OnIsFlatListChanged))
        );

        public Boolean IsFlatListBinding {
            get { return (Boolean)this.GetValue(IsFlatListBindingProperty); }
            set { this.SetValue(IsFlatListBindingProperty, value); }
        }

        private static void OnIsFlatListChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            ((BindableLongList)d).IsFlatList = (Boolean)e.NewValue;
        }
    }
}
