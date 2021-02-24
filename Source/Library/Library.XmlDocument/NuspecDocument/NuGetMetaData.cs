using System.Collections.Generic;

namespace BlackPearl.Library.Xml
{

    public class NuspecMetaData
    {
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
    }
}
