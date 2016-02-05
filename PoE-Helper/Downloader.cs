using System;
using System.IO;
using System.Net;

namespace PoE_Helper {
	class Downloader {
		public class DownloadEventArgs {
			public DownloadEventArgs( Uri remote, string local ) {
				this.RemoteUri = remote;
				this.LocalFile = local;
			}

			public Uri RemoteUri { get; private set; }
			public string LocalFile { get; private set; }
		}
#if !DEBUG
		private string BaseUri = "http://mibbiodev.de/test";
		private string Filename = "archlinux-2016.02.01-dual.iso";
#else
		private string BaseUri = "https://github.com/mibbio/PoE-Helper/releases/download/v{0}.{1}.{2}";
		private string Filename = "PoE-Helper-x86-{0}.{1}.{2}.{3}.msi";
#endif
		private Uri sourceUri;

		public Downloader() { }

		public bool Download( Version version ) {
			this.BaseUri = string.Format(this.BaseUri, version.Major, version.Minor, version.Build);
			this.Filename = string.Format(this.Filename, version.Major, version.Minor, version.Build, version.Revision);
			this.sourceUri = new Uri(Path.Combine(this.BaseUri, this.Filename));
			string localFile = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			localFile = Path.Combine(localFile, "PoE-Helper", this.Filename);

			WebClient client = new WebClient();
			client.DownloadFileAsync(this.sourceUri, localFile);
			client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(( sender, data ) => {
				UpdateDownloadEvent(data.ProgressPercentage, new DownloadEventArgs(this.sourceUri, localFile));
			});
			return true;
		}

		#region Events
		public delegate void UpdateDownloadHandler( int progress, DownloadEventArgs e );
		public event UpdateDownloadHandler UpdateDownloadEvent = new UpdateDownloadHandler((p, f) => { });
		#endregion
	}
}
