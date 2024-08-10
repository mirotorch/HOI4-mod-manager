using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
