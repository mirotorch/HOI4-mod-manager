using ICSharpCode.SharpZipLib.BZip2;
using ICSharpCode.SharpZipLib.Zip;
using System.IO.Compression;
using System.Text.Json;
using System.Text.RegularExpressions;

using var fs = File.Open("Settings.json", FileMode.OpenOrCreate);
Settings settings;
try
{
	settings = JsonSerializer.Deserialize<Settings>(fs) ?? new Settings();
}
catch
{
	settings = new();
}

Regex regexName = new Regex(@"name\s*=\s*""([^""]*)""");
Regex regexVersion = new Regex(@"version\s*=\s*""([^""]*)""");

List<Mod> mods = new List<Mod>();
Dictionary<string, Mod> namesMods = new();

foreach (var directory in Directory.GetDirectories(settings.WorkshopPath))
{
	if (Directory.GetFileSystemEntries(directory).Length > 1)
	{
		Mod mod = GetModDirectory(directory);
		mods.Add(mod);
		namesMods.Add(mod.Name, mod);
	}
	else
	{
		try
		{
			Mod mod = GetModZip(Directory.GetFileSystemEntries(directory)[0]);
			mods.Add(mod);
			namesMods.Add(mod.Name, mod);
			Console.WriteLine($"Mod {directory.Split("\\").Last()}: succesed to read archive");
		}
		catch
		{
			Console.WriteLine($"Mod {directory.Split("\\").Last()}: failed to read archive");
		}
	}
}

foreach (var directory in Directory.GetDirectories(settings.ModsPath))
{
	string description = File.ReadAllText(directory + "\\descriptor.mod");
	Match matchName = regexName.Match(description);
	if (matchName.Success)
	{
		if (namesMods.TryGetValue(matchName.Groups[1].Value, out Mod mod))
		{
			Match matchVersion = regexVersion.Match(description);
			if (matchVersion.Success)
			{
				mod.InstalledVersion = matchVersion.Groups[1].Value;
			}
		}
	}
}








File.WriteAllText("Settings.json", JsonSerializer.Serialize(settings));


Mod GetModDirectory(string directory)
{
	Mod mod = new Mod();
	mod.WorkshopPath = directory;
	string description = File.ReadAllText(directory + "\\descriptor.mod");
	Match matchName = regexName.Match(description);
	if (matchName.Success)
	{
		mod.Name = matchName.Groups[1].Value;
	}
	Match matchVersion = regexVersion.Match(description);
	if (matchVersion.Success)
	{
		mod.WorkshopVersion = matchVersion.Groups[1].Value;
	}
	mod.ModType = ModType.Directory;
	return mod;
}

Mod GetModZip(string zipFilePath)
{
	Mod mod = new Mod();
	mod.WorkshopPath = zipFilePath;
	using (FileStream fs = File.OpenRead(zipFilePath))
	using (ICSharpCode.SharpZipLib.Zip.ZipFile zipFile = new(fs))
	{
		ZipEntry entry = zipFile.GetEntry("descriptor.mod");
		using (Stream zipStream = zipFile.GetInputStream(entry))
		using (BZip2InputStream bZip2InputStream = new BZip2InputStream(zipStream))
		using (StreamReader reader = new StreamReader(bZip2InputStream))
		{
			string description = reader.ReadToEnd();
			Match matchName = regexName.Match(description);
			if (matchName.Success)
			{
				mod.Name = matchName.Groups[1].Value;
			}
			Match matchVersion = regexVersion.Match(description);
			if (matchVersion.Success)
			{
				mod.WorkshopVersion = matchVersion.Groups[1].Value;
			}
		}
	}
	mod.ModType = ModType.Archive;
	return mod;
}

class Settings
{
	public string WorkshopPath { get; set; } = "D:\\Steam\\steamapps\\workshop\\content\\394360";
	public string ModsPath { get; set; } = "C:\\Users\\rusko\\Documents\\Paradox Interactive\\Hearts of Iron IV\\mod";
	public string DowserPath { get; set; } = "D:\\Games\\Hearts of Iron IV\\dowser.exe";
}


enum ModType
{
	Directory,
	Archive
}

class Mod
{
	public string WorkshopVersion { get; set; } = "n/a";
	public string InstalledVersion { get; set; } = "";
	public string Name { get; set; } = "not defined";
	public string WorkshopPath { get; set; } = "n/a";
	public bool Installed => InstalledVersion == string.Empty;
	public ModType ModType { get; set; }
}