using Mle.MusicPimp.ViewModels;
using Mle.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Mle.MusicPimp.Network {
    abstract public class BaseDownloader : ViewModelBase, IDownloader {
        public ICommand Download { get; protected set; }
        public abstract Task SubmitDownload(MusicItem item);
        public abstract Task SubmitDownloads(IEnumerable<MusicItem> items);
        public abstract Task<Uri> DownloadAsync(MusicItem track);
    }
}
