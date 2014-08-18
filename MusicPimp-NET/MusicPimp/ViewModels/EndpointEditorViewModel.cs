using Mle.MusicPimp.Exceptions;
using Mle.MusicPimp.Util;
using Mle.ViewModels;
using Mle.Xaml.Commands;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Mle.MusicPimp.ViewModels {
    public abstract class EndpointEditorViewModel : ClosingViewModel {
        public MusicEndpoint EndpointItem { get; protected set; }
        public void CancelEndpoint(MusicEndpoint endpoint) { }
        public abstract Task AddOrUpdate(MusicEndpoint endpoint);
        public Provider Provider { get { return ProviderService.Instance; } }
        public EndpointTester Tester { get; protected set; }
        public EndpointsData Endpoints { get { return Provider.EndpointsData; } }
        public LibraryManager LibraryManager { get { return Provider.LibraryManager; } }

        private bool makeActiveLibrary;
        public bool MakeActiveLibrary {
            get { return makeActiveLibrary; }
            set { this.SetProperty(ref this.makeActiveLibrary, value); }
        }
        public ICommand Add { get; private set; }
        public ICommand Cancel { get; private set; }
        public ICommand DisplayHelp { get; private set; }

        public EndpointEditorViewModel() {
            Tester = new EndpointTester();
            Add = new AsyncUnitCommand(AddEndpoint);
            Cancel = new UnitCommand(Close);
            DisplayHelp = new UnitCommand(() => Send(MusicItemsBase.AppHelpHeader, MusicItemsBase.AppHelpMessage));
        }
        public virtual async Task SubmitEndpoint(MusicEndpoint endpoint) {
            await AddOrUpdate(endpoint);
            if(MakeActiveLibrary) {
                LibraryManager.SetActive(endpoint);
            }
        }
        /// <summary>
        /// Called when the user saves an endpoint (WP8) or performs any change whatsoever (Win8).
        /// </summary>
        /// <returns></returns>
        public Task SubmitChanges() {
            return SubmitEndpoint(EndpointItem);
        }
        private async Task AddEndpoint() {
            try {
                validate();
                await SubmitEndpoint(EndpointItem);
                Close();
            } catch(Exception e) {
                Send("Unable to add endpoint. " + e.Message);
            }
        }
        public void validate() {
            if(EndpointItem.Port <= 0 || EndpointItem.Port > 65535) {
                throw new PimpException("The specified port number must be a positive integer no greater than 65535. Please check your input.");
            }
            if(hasNullOrEmpties(EndpointItem.Name, EndpointItem.Server, EndpointItem.Username, EndpointItem.Password)) {
                throw new PimpException("Please check that all the fields are filled in properly.");
            }
        }
        private bool hasNullOrEmpties(params string[] values) {
            return exists(s => s == null || s == String.Empty, values);
        }
        private bool exists(Func<string, bool> predicate, params string[] values) {
            for(int i = 0; i < values.Length; ++i) {
                if(predicate(values[i])) {
                    return true;
                }
            }
            return false;
        }


    }
}
