
using Mle.ViewModels;
namespace Mle.MusicPimp.ViewModels {
    public class PimpDownloadManager : MessagingViewModel {
        //public async Task ValidateThenSubmitDownload(MusicItem item) {
        //    try {
        //        if (item.IsDir) {
        //            await DownloadFolder(item);
        //        } else {
        //            await SubmitDownload(item);
        //        }
        //    } catch (Exception e) {
        //        Send("An error occurred", "Unable to download " + item.Name + ". " + e.Message);
        //    }
        //}
        //public async Task DownloadFolder(MusicItem folder) {
        //    var tracks = await MusicProvider.SongsInFolder(folder);
        //    foreach (var t in tracks) {
        //        await SubmitDownload(t);
        //    }
        //}
    }
}
