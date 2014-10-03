using Mle.MusicPimp.Controls;
using Mle.MusicPimp.Exceptions;
using Mle.MusicPimp.ViewModels;
using Mle.Util;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace MusicPimp.Xaml {
    public partial class ConfigureEndpoint : AsyncPhoneApplicationPage {
        private EndpointEditorViewModel vm;
        public ConfigureEndpoint() {
            InitializeComponent();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e) {
            vm = BuildDataContext();
            DataContext = vm;
            base.OnNavigatedTo(e);
        }
        private EndpointEditorViewModel BuildDataContext() {
            string endpoint;
            if(NavigationContext.QueryString.TryGetValue("edit", out endpoint)) {
                return new EditEndpoint(Strings.decode(endpoint));
            }
            return new NewEndpoint();
        }
        private void CancelApplicationBar_Click(object sender, EventArgs e) {
            var model = (this.DataContext as EndpointEditorViewModel);
            model.CancelEndpoint(model.EndpointItem);
            if(NavigationService.CanGoBack)
                NavigationService.GoBack();
        }

        private async void SaveApplicationBar_Click(object sender, EventArgs e) {
            var model = (this.DataContext as EndpointEditorViewModel);
            try {
                model.validate();
                // christ
                await model.SubmitChanges();
                if(NavigationService.CanGoBack)
                    NavigationService.GoBack();
            } catch(PimpException pe) {
                MessageBox.Show(pe.Message);
            } catch(InvalidOperationException) {
                // may throw with message: "SelectedIndex must always be set to a valid value". TODO fix.
                MessageBox.Show("An error occurred. Please try again later.");
            }
        }
        private void NameTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            update(NameTextBox);
        }
        private void CloudTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) {
            update(CloudTextBox);
        }
        private void userTextBox_TextChanged_1(object sender, TextChangedEventArgs e) {
            update(userTextBox);
        }
        private void serverTextBox_TextChanged_1(object sender, TextChangedEventArgs e) {
            update(serverTextBox);
        }
        private void update(FrameworkElement control) {
            var be = control.GetBindingExpression(TextBox.TextProperty);
            be.UpdateSource();
        }
        private void PasswordBox_PasswordChanged_1(object sender, RoutedEventArgs e) {
            var be = passwordTextBox.GetBindingExpression(PasswordBox.PasswordProperty);
            be.UpdateSource();
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e) {
            //if(vm != null) {
            //    vm.Update();
            //}
        }

        private void RadioButton_Click(object sender, RoutedEventArgs e) {
            if(vm != null) {
                vm.Update();
            }
        }

        
    }
}