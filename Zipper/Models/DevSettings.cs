using System;

namespace Zipper.Models
{
    [Serializable]
    public class DevSettings
    {
        [NonSerialized]
        public static string FileName = "dev-settings.xml";

        public string? DefaultFolderToEncodePath { get; set; }

        public string? DefaultFolderToDecodePath { get; set; }

        public string? DefaultFaacFilePath { get; set; }
    }
}
