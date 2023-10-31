using ICSharpCode.SharpZipLib;
using ICSharpCode.SharpZipLib.Zip;
using SharpFileSystem;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
//using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libTech.FileSystem {
	public class SharpArchiveFileSystem : FileSystemProvider {
		ZipFile ZipFile;
		public string ArchiveName;

		public SharpArchiveFileSystem(string ArchiveName, Stream ZipFileStream) {
			this.ArchiveName = ArchiveName;
			ZipFile = new ZipFile(ZipFileStream);
		}

		public override bool Exists(FileSystemPath path) {
			foreach (ZipEntry Ent in ZipFile) {
				if (Ent.Name == GetZipPath(path))
					return true;
			}

			return false;
		}

		public override ICollection<FileSystemPath> GetEntities(FileSystemPath path) {
			HashSet<FileSystemPath> Paths = new HashSet<FileSystemPath>();

			foreach (ZipEntry Ent in ZipFile) {
				if (Ent.Name.StartsWith(GetZipPath(path))) {
					FileSystemPath FSPath = GetFSPath(Ent.Name);

					if (FSPath.ParentPath.Path != path.Path && FSPath.ParentPath.Path.StartsWith(path.Path) && !Paths.Contains(FSPath.ParentPath))
						Paths.Add(FSPath.ParentPath);

					Paths.Add(FSPath);
				}
			}

			return Paths;
		}

		public override Stream OpenFile(FileSystemPath path, FileAccess access) {
			ZipEntry Entry = ZipFile.GetEntry(GetZipPath(path));
			if (Entry == null)
				return null;

			MemoryStream MS = new MemoryStream();

			using (Stream InputStream = ZipFile.GetInputStream(Entry))
				InputStream.CopyTo(MS);

			MS.Seek(0, SeekOrigin.Begin);
			return MS;
		}

		public override string ToString() {
			return ArchiveName;
		}

		string GetZipPath(FileSystemPath path) {
			return path.Path.Substring(1);
		}

		FileSystemPath GetFSPath(string zipPath) {
			return FileSystemPath.Parse("/" + zipPath);
		}
	}
}
//*/
