using System.Collections.Generic;

using NuGet.Frameworks;

namespace BlackPearl.Library.Xml
{

    public class PackageMetaData
    {
        public PackageMetaData()
        {
            CustomLibrary = new List<string>();
            CustomReference = new List<string>();
            TargetFramework = new List<NuGetFramework>() { FrameworkConstants.CommonFrameworks.Net461 };
        }

        public string Id { get; set; }
        public string Version { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Owner { get; set; }
        public string Desciption { get; set; }
        public string ReleaseNotes { get; set; }
        public string Copyright { get; set; }
        public string Tags { get; set; }
        public List<string> CustomLibrary { get; set; }
        public List<string> CustomReference { get; set; }
        public List<NuGetFramework> TargetFramework { get; set; }
    }
}
