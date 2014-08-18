using Mle.MusicPimp.Local;
using Mle.MusicPimp.Xaml;
using Mle.ViewModels;
using Mle.Xaml;
using System.Collections.ObjectModel;

namespace Mle.MusicPimp.ViewModels {
    public class HelpModel {
        public ObservableCollection<TitledImageItem> HelpItems { get; private set; }
        public TitledImageItem AddLocalFolderItem { get; private set; }
        public TitledImageItem AddServerItem { get; private set; }

        public HelpModel() {
            AddLocalFolderItem = new TitledImageItem(
                "ms-appx:///MusicPimpLibrary/Assets/folder-open-foldercolor-1024.png",
                "Add local folder",
                "Select an MP3 folder",
                onClicked: async () => await MultiFolderLibrary.Instance.AddFolder());
            AddServerItem = new TitledImageItem(
                "ms-appx:///MusicPimpLibrary/Assets/desktop-red-1024.png",
                "Add remote server",
                "Connect to a MusicPimp PC",
                onClicked: OpenAddEndPoint);
            HelpItems = new ObservableCollection<TitledImageItem>() { AddLocalFolderItem, AddServerItem };
        }
        private void OpenAddEndPoint() {
            PopupManager.Show(new AddEndpointPopup());
        }
    }
}
