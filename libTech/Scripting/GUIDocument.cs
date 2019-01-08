using FishMarkupLanguage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libTech.Scripting {
	public class GUIDocument : FMLDocument {
		string FileName;

		public GUIDocument() {
			TagSet.AddTags("root", "window", "button", "label", "panel", "input", "layout", "row");
		}

		public GUIDocument(string FileName) : this() {
			Parse(FileName);
		}

		public void Parse(string FileName) {
			this.FileName = FileName;

			lock (this)
				FML.Parse(FileName, this);
		}

		public void UpdateIfChanged() {
			FileWatchHandle Handle = FileWatcher.Watch(FileName, true);

			if (Handle?.HasChanged ?? false) {
				Handle.Reset();

				Parse(Handle.FullFilePath);
			}
		}
	}
}
