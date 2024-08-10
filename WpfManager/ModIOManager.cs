using System.IO;
using System.IO.Compression;
using ICSharpCode.SharpZipLib.BZip2;

namespace WpfManager
{
    class ModIOManager
    {
        const string WorkshopPath = """D:\Steam\steamapps\workshop\content\394360""";
        const string InstalledPath = """C:\Users\user\Documents\Paradox Interactive\Hearts of Iron IV\mod""";

		#region Public methods
		public List<Mod> ReadMods()
        {
            if (!Directory.Exists(InstalledPath) || !Directory.Exists(WorkshopPath))
            {
                throw new Exception("Path error: path does not exist");   
            }

			var mods = new List<Mod>();
            List<string> workshopModDirs = new(Directory.GetDirectories(WorkshopPath));

            Dictionary<string, Mod> modNames = new();

            foreach (string dir in workshopModDirs)
            {
                Mod? mod = null;
                string? archivePath = GetArchiveIfExists(dir);
                if (archivePath != null)
                {
					ZipIO.ExtractFile(archivePath, "descriptor.mod", Path.Combine(dir, "tmp.mod"));
					mod = ParseDescriptor(Path.Combine(dir, "tmp.mod"), false);
					File.Delete(Path.Combine(dir, "tmp.mod"));
				}
                else
                {
					var files = Directory.GetFiles(dir, "*.mod");
					if (files.Length > 0)
					{
						foreach (var file in files)
						{
							if (file.EndsWith("descriptor.mod"))
								mod = ParseDescriptor(file, false);
						}
					}
				}
                if (mod == null) 
					throw new Exception($"Descriptor not found, path={dir}");
                mods.Add(mod);
                modNames.Add(mod.Name, mod);
            }

            List<string> installedMods = new(Directory.GetFiles(InstalledPath, "*.mod"));
            foreach (var installed in installedMods)
            {
                if (modNames.ContainsKey(Path.GetFileName(installed)))
                {
                    using var sr = new StreamReader(installed);
                    while (!sr.EndOfStream)
                    {
                        string? line = sr.ReadLine();
						if (line == null) break;
						if (line.Length == 0) continue;
						if (line.StartsWith("version"))
						{
                            modNames[Path.GetFileName(installed)].InstalledVersion = line.Split("\"")[1];
                            break;
						}
					}
                }
                else
                {
                    Mod mod = ParseDescriptor(installed, true);
                    mods.Add(mod);
                }
            }
			return mods;
        }

        public void InstallMod(Mod mod)
        {
			ArgumentNullException.ThrowIfNull(mod?.AvailableVersion);
            string sourcePath = Path.Combine(WorkshopPath, mod.RemoteId);
            string destModPath = Path.Combine(InstalledPath, $"{mod.Name}.mod");
			if (!Directory.Exists(sourcePath)) throw new Exception("Mod directory doesn't exist");

            string? archivePath = GetArchiveIfExists(sourcePath);
			if (archivePath != null)
			{
				ZipIO.ExtractFile(archivePath, "descriptor.mod", destModPath);
			}
			else
			{
				var files = Directory.GetFiles(sourcePath, ".*.mod");
				if (files.Length > 0)
				{
					foreach (var file in files)
					{
						if (file.EndsWith("descriptor.mod"))
							mod = ParseDescriptor(file, false);
					}
				}
			}
            if (!File.Exists(destModPath)) throw new Exception("Error occured during .mod file copying");
            
            if (archivePath != null)
            {
                File.Copy(archivePath, Path.Combine(InstalledPath, $"{mod.Name}.zip"));
                using var sw = new StreamWriter(destModPath);
                sw.WriteLine($"archive=\"{Path.Combine(InstalledPath, mod.Name)}.zip\"");
            }
            else
            {
                var dirPath = Path.Combine(InstalledPath, mod.Name);
				CopyDirectory(Path.Combine(WorkshopPath, mod.RemoteId), dirPath, true);
				using var sw = new StreamWriter(destModPath);
				sw.WriteLine($"path=\"{dirPath}\"");
			}
            mod.InstalledVersion = mod.AvailableVersion;
		}

		public void UninstallMod(Mod mod)
		{
			ArgumentNullException.ThrowIfNull(mod?.InstalledVersion);
			string archivePath = Path.Combine(InstalledPath, mod.Name + ".zip");
			if (File.Exists(archivePath))
			{
				File.Delete(archivePath);
			}
			else
			{
				string dirPath = Path.Combine(InstalledPath, mod.Name);
				if (Directory.Exists(dirPath))
				{
					Directory.Delete(dirPath, true);
				}
			}
			string descriptorPath = Path.Combine(InstalledPath, mod.Name + ".mod");
			if (File.Exists(descriptorPath))
			{
				File.Delete(descriptorPath);
			}
			mod.InstalledVersion = null;
		}

		#endregion

		#region Utils
		private string? GetArchiveIfExists(string path)
		{
			var archivePath = Directory.GetFiles(path, "*.zip");
			if (archivePath.Length == 1) return archivePath[0];
			else return null;
		}

		private void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
		{
			var dir = new DirectoryInfo(sourceDir);

			if (!dir.Exists)
				throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

			DirectoryInfo[] dirs = dir.GetDirectories();

			Directory.CreateDirectory(destinationDir);

			foreach (FileInfo file in dir.GetFiles())
			{
				string targetFilePath = Path.Combine(destinationDir, file.Name);
				file.CopyTo(targetFilePath);
			}

			if (recursive)
			{
				foreach (DirectoryInfo subDir in dirs)
				{
					string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
					CopyDirectory(subDir.FullName, newDestinationDir, true);
				}
			}
		}

		private Mod ParseDescriptor(string path, bool installed)
		{
			Mod mod = new Mod();
			using (var sr = new StreamReader(path))
			{
				while (!sr.EndOfStream)
				{
					string? line = sr.ReadLine();
					if (line == null) break;
					if (line.Length == 0) continue;
					if (line.StartsWith("version"))
					{
						if (installed) mod.InstalledVersion = line.Split("\"")[1];
						else mod.AvailableVersion = line.Split("\"")[1];
					}
					else if (line.StartsWith("name"))
					{
						mod.Name = line.Split("\"")[1];
					}
					else if (line.StartsWith("supported_version"))
					{
						mod.SupportedVersion = line.Split("\"")[1];
					}
					else if (line.StartsWith("remote_file_id"))
					{
						mod.RemoteId = line.Split("\"")[1];
					}
				}
			}
			if (mod.InstalledVersion ==  null && mod.AvailableVersion == null) 
			{
				if (installed)
				{
					mod.InstalledVersion = "1.0";
				}
				else
				{
					mod.AvailableVersion = "1.0";
				}
			}
			return mod;
		}
		#endregion
	}
}
