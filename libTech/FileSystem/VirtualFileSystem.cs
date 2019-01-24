using SourceUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libTech.FileSystem {
	public interface FileSystemProvider : IResourceProvider {
		/*IEnumerable<string> GetFiles(string Directory);
		IEnumerable<string> GetDirectories(string Directory);

		bool ContainsFile(string FileName);
		Stream OpenFile(string FileName);*/
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
			string[] VPKs = Directory.GetFiles(RootDir, "*_dir.vpk", SearchOption.AllDirectories);

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

		public bool ContainsFile(string FileName) {
			return Res.ContainsFile(FileName);
		}

		public IEnumerable<string> GetDirectories(string Directory) {
			return Res.GetDirectories(Directory);
		}

		public IEnumerable<string> GetFiles(string Directory) {
			return Res.GetFiles(Directory);
		}

		public Stream OpenFile(string FileName) {
			return Res.OpenFile(FileName);
		}
	}

	public class FileProvider : FileSystemProvider {
		public readonly string RootDirectory;
		public readonly string VirtualDirectory;

		public FileProvider(string RootDir, string VirtualDirectory = "") {
			this.VirtualDirectory = PathUtils.CleanUp(VirtualDirectory);
			RootDirectory = PathUtils.GetFullPath(RootDir);
		}

		public bool ContainsFile(string FileName) {
			if (Path.IsPathRooted(FileName))
				return false;

			if (VirtualDirectory.Length != 0) {
				if (PathUtils.RemoveVirtualPrefix(ref FileName, VirtualDirectory))
					return File.Exists(PathUtils.Combine(RootDirectory, FileName));

				return false;
			}


			return File.Exists(PathUtils.Combine(RootDirectory, FileName));
		}

		public IEnumerable<string> GetDirectories(string Dir) {
			if (!Path.IsPathRooted(Dir)) {
				if (VirtualDirectory.Length != 0) {
					if (PathUtils.RemoveVirtualPrefix(ref Dir, VirtualDirectory)) {
						if (Directory.Exists(Dir))
							return Directory.EnumerateDirectories(Dir).Select(S => PathUtils.GetFullPath(S).Replace(RootDirectory, ""));
					}
				} else if (Directory.Exists(Dir))
					return Directory.EnumerateDirectories(Dir).Select(S => PathUtils.GetFullPath(S).Replace(RootDirectory, ""));
			}


			return new string[] { };
		}

		public IEnumerable<string> GetFiles(string Dir) {
			if (!Path.IsPathRooted(Dir)) {
				if (VirtualDirectory.Length != 0) {
					if (PathUtils.RemoveVirtualPrefix(ref Dir, VirtualDirectory)) {
						if (Directory.Exists(Dir))
							return Directory.GetFiles(Dir).Select(S => PathUtils.GetFullPath(S).Replace(RootDirectory, ""));
					}
				} else if (Directory.Exists(Dir))
					return Directory.GetFiles(Dir).Select(S => PathUtils.GetFullPath(S).Replace(RootDirectory, ""));
			}

			return new string[] { };
		}

		public Stream OpenFile(string FileName) {
			if (ContainsFile(FileName)) {
				if (VirtualDirectory.Length != 0)
					PathUtils.RemoveVirtualPrefix(ref FileName, VirtualDirectory);

				return File.OpenRead(PathUtils.Combine(RootDirectory, FileName));
			}

			return null;
		}
	}

	public class VirtualFileSystem : FileSystemProvider {
		List<FileSystemProvider> Providers;

		public VirtualFileSystem() {
			Providers = new List<FileSystemProvider>();
		}

		public void AddProvider(FileSystemProvider Provider) {
			Providers.Insert(0, Provider);
		}

		public bool ContainsFile(string FileName) {
			foreach (var P in Providers) {
				if (P.ContainsFile(FileName))
					return true;
			}

			return false;
		}

		public IEnumerable<string> GetDirectories(string Directory) {
			return Providers.SelectMany(P => P.GetDirectories(Directory));
		}

		public IEnumerable<string> GetFiles(string Directory) {
			return Providers.SelectMany(P => P.GetFiles(Directory));
		}

		public IEnumerable<string> GetFiles(string Pth, string Ext) {
			return GetFiles(Pth).Where(P => Path.GetExtension(P) == Ext);
		}

		public Stream OpenFile(string FileName) {
			Stream S = null;

			foreach (var P in Providers) {
				if (P.ContainsFile(FileName)) {
					S = P.OpenFile(FileName);
					break;
				}
			}

			// TODO
			if (S != null)
				Console.WriteLine("Opening '{0}'", FileName);

			return S;
		}
	}
}
