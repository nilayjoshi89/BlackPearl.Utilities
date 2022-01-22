using System.Collections.Generic;

namespace BlackPearl.Library.Xml
{
    public interface IProjectDocument
    {
        IList<PackageReference> Packages { get; }
        IList<PackageReference> AllPackages { get; }
        IList<IProjectDocument> ProjectReferences { get; }
        IList<IProjectDocument> AllProjectReferences { get; }
        IList<string> ContentFiles { get; }
        string ProjectPath { get; set; }
        bool IsInitialized { get; }
        void Initialize();
    }
}