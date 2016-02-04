using System;
using System.IO;
using System.Net;

namespace PoE_Helper {
	class Downloader {
#if DEBUG
		private readonly string BaseUri = "http://mibbiodev.de/test";
		private readonly string Filename = "archlinux-2016.02.01-dual.iso";
#else
		private readonly string BaseUri = "https://github.com/mibbio/PoE-Helper/releases/download/v{0}.{1}.{2}";
		private readonly string Filename = "PoE-Helper-x86-{0}.{1}.{2}.{3}.msi";
#endif
		private readonly Uri sourceUri;

		public delegate void ProgressChangedHandler( DownloadProgressChangedEventArgs e );

		public Downloader( Version version ) {
#if !DEBUG
			this.BaseUri = string.Format(this.BaseUri, version.Major, version.Minor, version.Build);
			this.Filename = string.Format(this.Filename, version.Major, version.Minor, version.Build, version.Revision);
#endif
			this.sourceUri = new Uri(Path.Combine(this.BaseUri, this.Filename));
		}

		public bool Download() {
			string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			Console.WriteLine(Path.Combine(appData, "PoE-Helper", this.Filename));
			WebClient client = new WebClient();
			client.DownloadFileAsync(this.sourceUri, Path.Combine(appData, "PoE-Helper", this.Filename));
			client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(( sender, args ) => {
				Console.WriteLine(string.Format("[{0}] \t{1}/{2}", args.ProgressPercentage, args.BytesReceived, args.TotalBytesToReceive));
				//OnProgressChanged(args);
			});
			return true;
		}

		#region Events
		public event ProgressChangedHandler OnProgressChanged;
		#endregion
	}
}
