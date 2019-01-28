using SharpFileSystem;
using SharpFileSystem.FileSystems;
using SourceUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IODirectory = System.IO.Directory;

namespace libTech.FileSystem {
	public abstract class FileSystemProvider : IFileSystem, IResourceProvider {
		public abstract bool Exists(FileSystemPath path);
		public abstract ICollection<FileSystemPath> GetEntities(FileSystemPath path);
		public abstract Stream OpenFile(FileSystemPath path, FileAccess access);

		public virtual void CreateDirectory(FileSystemPath path) {
			throw new NotImplementedException();
		}

		public virtual Stream CreateFile(FileSystemPath path) {
			throw new NotImplementedException();
		}

		public virtual void Delete(FileSystemPath path) {
			throw new NotImplementedException();
		}

		public virtual void Dispose() {
		}

		public virtual bool Exists(string Path) {
			return Exists(RootParsePath(Path));
		}

		public IEnumerable<string> GetFiles(string directory = "") {
			if (directory.Contains("\\"))
				directory = directory.Replace("\\", "/");

			return GetFiles(directory, false);
		}

		public string[] GetFiles(string directory = "", bool recursive = false) {
			if (directory.Contains("\\"))
				directory = directory.Replace("\\", "/");

			List<string> Files = new List<string>();

			foreach (var E in GetEntities(RootParsePath((directory))))
				if (E.IsFile)
					Files.Add(E.Path);
				else if (E.IsDirectory && recursive)
					Files.AddRange(GetFiles(E.Path, true));

			return Files.ToArray();
		}

		public IEnumerable<string> GetDirectories(string directory = "") {
			return GetDirectories(directory, false);
		}

		public string[] GetDirectories(string directory = "", bool recursive = false) {
			List<string> Directories = new List<string>();

			foreach (var E in GetEntities(RootParsePath((directory))))
				if (E.IsDirectory) {
					Directories.Add(E.Path);

					if (recursive)
						Directories.AddRange(GetDirectories(E.Path, true));
				}

			return Directories.ToArray();
		}

		public virtual bool ContainsFile(string filePath) {
			return Exists(RootParsePath((filePath)));
		}

		public Stream OpenFile(string filePath) {
			return OpenFile(RootParsePath((filePath)), FileAccess.Read);
		}

		public string ReadAllText(string FilePath) {
			using (Stream S = OpenFile(FilePath)) {
				using (StreamReader SR = new StreamReader(S)) {
					return SR.ReadToEnd();
				}
			}
		}

		// ------------

		public static FileSystemPath RootParsePath(string Path) {
			if (Path.Contains("\\"))
				Path = Path.Replace("\\", "/");

			if (!Path.StartsWith("/"))
				Path = "/" + Path;

			return FileSystemPath.Parse(Path);
		}

		public static string UnrootPath(FileSystemPath Path) {
			return Path.Path.Substring(1);
		}
	}

	public class VPKProvider : FileSystemProvider {
		ResourceLoader Res;
		Dictionary<string, ValvePackage> MountedVPKs = new Dictionary<string, ValvePackage>();

		public VPKProvider() {
			Res = new ResourceLoader();
		}

		public string[] GetAllMountedVPKs() {
			return MountedVPKs.Keys.ToArray();
		}

		public VPKProvider Add(string FilePath) {
			FilePath = PathUtils.GetFullPath(FilePath);

			MountedVPKs.Add(FilePath, new ValvePackage(FilePath));
			Res.AddResourceProvider(MountedVPKs[FilePath]);

			return this;
		}

		public VPKProvider AddRoot(string RootDir) {
			string[] VPKs = IODirectory.GetFiles(RootDir, "*_dir.vpk", SearchOption.AllDirectories);

			foreach (var VPK in VPKs)
				Add(VPK);

			return this;
		}

		public void Remove(string FilePath) {
			FilePath = PathUtils.GetFullPath(FilePath);

			if (MountedVPKs.ContainsKey(FilePath)) {
				Res.RemoveResourceProvider(MountedVPKs[FilePath]);
				MountedVPKs.Remove(FilePath);
			}
		}

		public override bool Exists(FileSystemPath path) {
			return Res.ContainsFile(UnrootPath(path));
		}

		public override Stream OpenFile(FileSystemPath path, FileAccess access) {
			return Res.OpenFile(UnrootPath(path));
		}

		public override ICollection<FileSystemPath> GetEntities(FileSystemPath path) {
			if (path.IsFile)
				throw new InvalidOperationException();

			HashSet<FileSystemPath> Ents = new HashSet<FileSystemPath>();

			foreach (var Dir in Res.GetDirectories(UnrootPath(path)))
				Ents.Add(FileSystemPath.Parse(path.Path + Dir + "/"));

			foreach (var Fil in Res.GetFiles(UnrootPath(path)))
				Ents.Add(FileSystemPath.Parse(path.Path + Fil));

			return Ents;
		}
	}

	public class VirtualFileSystem : FileSystemProvider {
		List<IFileSystem> FileSystems = new List<IFileSystem>();

		public VirtualFileSystem(string PhysicalRoot) {
			AddProvider(new PhysicalFileSystem(PhysicalRoot));
		}

		public void Mount(string VirtualPath, IFileSystem FS) {
			AddProvider(new FileSystemMounter(new KeyValuePair<FileSystemPath, IFileSystem>(FileSystemPath.Parse(VirtualPath), FS)));
		}

		public void Mount(string VirtualPath, string PhysicalPath) {
			Console.WriteLine("Mounting {0} -> {1}", PhysicalPath, VirtualPath);
			Mount(VirtualPath, new PhysicalFileSystem(PhysicalPath));
		}

		public void MountArchive(string VirtualPath, string ArchiveName, Stream Archive) {
			Mount(VirtualPath, new SharpArchiveFileSystem(ArchiveName, Archive));
		}

		public void MountArchive(string VirtualPath, string ArchivePath) {
			Console.WriteLine("Mounting {0} -> {1}", ArchivePath, VirtualPath);
			MountArchive(VirtualPath, Path.GetFileName(ArchivePath), OpenFile(ArchivePath));
		}

		public void AddProvider(IFileSystem Provider) {
			FileSystems.Add(Provider);
		}

		public override bool Exists(FileSystemPath path) {
			for (int i = FileSystems.Count - 1; i >= 0; i--)
				if (FileSystems[i].Exists(path))
					return true;

			return false;
		}

		public override ICollection<FileSystemPath> GetEntities(FileSystemPath path) {
			HashSet<FileSystemPath> Entities = new HashSet<FileSystemPath>();

			for (int i = FileSystems.Count - 1; i >= 0; i--) {
				FileSystemPath[] Ents = FileSystems[i].GetEntities(path).ToArray();

				for (int j = 0; j < Ents.Length; j++)
					if (!Entities.Contains(Ents[j]))
						Entities.Add(Ents[j]);
			}

			return Entities.ToArray();
		}

		public override Stream OpenFile(FileSystemPath path, FileAccess access) {
			for (int i = FileSystems.Count - 1; i >= 0; i--) {
				if (FileSystems[i].Exists(path))
					return FileSystems[i].OpenFile(path, access);
			}

			return null;
		}

		public IEnumerable<SharpArchiveFileSystem> GetMountedArchives() {
			for (int i = FileSystems.Count - 1; i >= 0; i--) {
				if (FileSystems[i] is FileSystemMounter Mounter)
					foreach (KeyValuePair<FileSystemPath, IFileSystem> Mount in Mounter.Mounts)
						if (Mount.Value is SharpArchiveFileSystem ArchiveFS)
							yield return ArchiveFS;
			}
		}

		public SharpArchiveFileSystem GetMountedArchive(string ArchiveName) {
			foreach (var ArchiveFS in GetMountedArchives())
				if (ArchiveFS.ArchiveName == ArchiveName)
					return ArchiveFS;

			return null;
		}
	}
}
