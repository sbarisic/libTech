using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace libTech {
	public static class FileWatcher {
		static FileSystemWatcher Watcher;
		static Dictionary<string, FileWatchHandle> WatchedFiles = new Dictionary<string, FileWatchHandle>();

		internal static void Init(string RootDir) {
			Watcher = new FileSystemWatcher(RootDir);
			Watcher.NotifyFilter = NotifyFilters.Size;
			Watcher.IncludeSubdirectories = true;

			Watcher.Changed += OnChanged;
			Watcher.Created += OnCreated;

			Watcher.EnableRaisingEvents = true;
		}

		public static FileWatchHandle Watch(string FullPath, bool FetchOnly = false) {
			FullPath = Path.GetFullPath(FullPath).NormalizeFilePath();

			if (WatchedFiles.ContainsKey(FullPath))
				return WatchedFiles[FullPath];

			if (FetchOnly)
				return null;

			FileWatchHandle H = new FileWatchHandle();
			H.FullFilePath = FullPath;

			WatchedFiles.Add(FullPath, H);
			return H;
		}

		private static void OnChanged(object S, FileSystemEventArgs E) {
			FileWatchHandle H = Watch(E.FullPath);

			if (H != null)
				H.HasChanged = true;
		}

		private static void OnCreated(object S, FileSystemEventArgs E) {
		}
	}

	public class FileWatchHandle {
		public string FullFilePath { get; internal set; }
		public bool HasChanged { get; internal set; }

		internal FileWatchHandle() {
		}

		public void Reset() {
			HasChanged = false;
		}

		public static implicit operator bool(FileWatchHandle H) {
			return H.HasChanged;
		}
	}
}
