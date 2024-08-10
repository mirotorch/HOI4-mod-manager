namespace WpfManager
{
    class Mod
    {
        public string Name { get; set; }
        public string? AvailableVersion { get; set; } = null;
        public string? InstalledVersion { get; set; } = null;
        public string SupportedVersion { get; set; }
        public string RemoteId { get; set; }
    }
}
