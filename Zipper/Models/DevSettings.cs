using System;

namespace Zipper.Models
{
    [Serializable]
    public class DevSettings
    {
        public string? DefaultFolderToEncodePath { get; set; }

        public string? DefaultFolderToDecodePath { get; set; }

        public string? DefaultFaacFilePath { get; set; }
    }
}
