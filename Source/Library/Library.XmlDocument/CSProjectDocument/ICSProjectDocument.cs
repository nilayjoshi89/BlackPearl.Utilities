using System.Collections.Generic;

namespace BlackPearl.Library.Xml
{
    public interface ICSProjectDocument
    {
        IList<CSPackageReference> Packages { get; }
        IList<CSPackageReference> AllPackages { get; }
        IList<ICSProjectDocument> ProjectReferences { get; }
        IList<ICSProjectDocument> AllProjectReferences { get; }
        IList<string> ContentFiles { get; }
        string ProjectPath { get; set; }
        bool IsInitialized { get; }
        void Initialize();
    }
}