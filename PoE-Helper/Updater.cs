using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PoE_Helper {
	class Updater {
		private readonly string updateUrl = "http://www.mibbiodev.de/poe/version.txt";

		public delegate void VersionCheckDoneEventHandler();

		//private static Version _latestVersion = null;

		public async void CheckRemoteVersion( object sender, EventArgs e ) {
			await Task.Run(() => {
				int version = 0;
				HttpWebRequest request = (HttpWebRequest) WebRequest.Create(updateUrl);
				HttpWebResponse response = (HttpWebResponse) request.GetResponse();
				if (response.StatusCode != HttpStatusCode.OK) {
					return;
				}

				StringBuilder sb = new StringBuilder();
				byte[] buffer = new byte[8192];

				Stream resStream = response.GetResponseStream();
				string tmpString = null;
				int count = 0;
				do {
					count = resStream.Read(buffer, 0, buffer.Length);
					if (count != 0) {
						tmpString = Encoding.ASCII.GetString(buffer, 0, count);
						sb.Append(tmpString);
					}
				} while (count > 0);
				resStream.Close();
				int.TryParse(sb.ToString(), out version);
				LatestVersion = IntToVersion(version);
				VersionCheckDone();
			});
		}

		public event VersionCheckDoneEventHandler VersionCheckDone;

		public Version LatestVersion { get; private set; }

		public string VersionString {
			get {
				return string.Format("{0}.{1}.{2}",
					LatestVersion.Major,
					LatestVersion.Minor,
					LatestVersion.Build);
			}
			private set { }
		}

		private static Version IntToVersion( int value ) {
			var major = value / 100;
			var minor = (value - (major * 100)) / 10;
			var build = value - (major * 100) - (minor * 10);
			return new Version(major, minor, build, 0);
		}
	}
}
