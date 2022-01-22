using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

using NuGet.Versioning;

namespace BlackPearl.Library.Xml
{
    public class CSharpProjectDocument : IProjectDocument
    {
        #region Constants
        private const string PROJECT_NODE = "Project";
        private const string ITEMGROUP_NODE = "ItemGroup";
        private const string PROJECT_REFERENCE_NODE = "ProjectReference";
        private const string INCLUDE_ATTRIBUTE = "Include";
        private const string PACKAGE_REFERENCE_NODE = "PackageReference";
        private const string VERSION_NODE = "Version";
        private const string NONE_VALUE = "None";
        private const string CONTENT_VALUE = "Content";
        private const string COPY_TO_OUT_DIR = "CopyToOutputDirectory";
        private const string XMLNS_REGEX = @" xmlns="".*?""";
        #endregion

        #region Members
        private string csProjectPath;
        private readonly XmlDocument doc;
        private readonly List<PackageReference> packages = new List<PackageReference>();
        private readonly List<PackageReference> allPackages = new List<PackageReference>();
        private readonly List<IProjectDocument> projectReferences = new List<IProjectDocument>();
        private readonly List<IProjectDocument> allProjectReferences = new List<IProjectDocument>();
        private readonly List<string> contentFiles = new List<string>();
        #endregion

        #region Constructor
        public CSharpProjectDocument()
        {
            doc = new XmlDocument();
            packages = new List<PackageReference>();
            allPackages = new List<PackageReference>();
            projectReferences = new List<IProjectDocument>();
            contentFiles = new List<string>();
        }
        #endregion

        #region Properties
        public string ProjectPath { get => csProjectPath; set => csProjectPath = Path.GetFullPath(value); }
        public IList<PackageReference> Packages => packages;
        public IList<PackageReference> AllPackages => allPackages;
        public IList<IProjectDocument> ProjectReferences => projectReferences;
        public IList<IProjectDocument> AllProjectReferences => allProjectReferences;
        public IList<string> ContentFiles => contentFiles;
        public bool IsInitialized { get; private set; }
        private string XPATH_PROJECT_REF => $"{PROJECT_NODE}/{ITEMGROUP_NODE}/{PROJECT_REFERENCE_NODE}/@{INCLUDE_ATTRIBUTE}";
        private string XPATH_PACKAGE_REF => $"{PROJECT_NODE}/{ITEMGROUP_NODE}/{PACKAGE_REFERENCE_NODE}";
        private string XPATH_CONTENT => $"{PROJECT_NODE}/{ITEMGROUP_NODE}/{NONE_VALUE}[{COPY_TO_OUT_DIR}]/@{INCLUDE_ATTRIBUTE}"
                                                          + $"| {PROJECT_NODE}/{ITEMGROUP_NODE}/{CONTENT_VALUE}[{COPY_TO_OUT_DIR}]/Link"
                                                          + $"| {PROJECT_NODE}/{ITEMGROUP_NODE}/{CONTENT_VALUE}[{COPY_TO_OUT_DIR}]/@{INCLUDE_ATTRIBUTE}";
        #endregion

        #region Methods
        public void Initialize()
        {
            if (IsInitialized)
            {
                throw new InvalidOperationException("Already initialized");
            }

            LoadXmlFromFile();

            LoadProjectRefernces();
            LoadContentFile();
            LoadPackages();

            IsInitialized = true;
        }
        private IEnumerable<string> GetProjectReferences() =>
                    doc.SelectNodes(XPATH_PROJECT_REF)
                        .Cast<XmlNode>()
                        .Select(n => Path.GetFullPath(Path.GetDirectoryName(csProjectPath) + "\\" + n.Value));
        private IEnumerable<PackageReference> GetPackageReferences() =>
                    doc.SelectNodes(XPATH_PACKAGE_REF)
                        .Cast<XmlNode>()
                        .Select(n => new PackageReference
                        {
                            Name = n.SelectSingleNode($"@{INCLUDE_ATTRIBUTE}").Value.ToLower(),
                            Version = n.SelectSingleNode(VERSION_NODE)?.InnerText
                                                                ?? n.SelectSingleNode($"@{VERSION_NODE}").Value
                        });
        private IEnumerable<string> GetContentFiles() =>
                    doc.SelectNodes(XPATH_CONTENT)
                        .Cast<XmlNode>()
                        .Select(n => n.InnerText ?? n.Value);
        private void LoadContentFile() => contentFiles.AddRange(GetContentFiles());
        private void LoadProjectRefernces()
        {
            projectReferences.AddRange(GetProjectReferences().Select(p => new CSharpProjectDocument() { ProjectPath = p }));
            allProjectReferences.AddRange(projectReferences);

            Parallel.ForEach(projectReferences, (p) =>
            {
                p.Initialize();
            });

            foreach (CSharpProjectDocument p in projectReferences)
            {
                foreach (CSharpProjectDocument r in p.AllProjectReferences)
                {
                    if (!allProjectReferences.Any(pr => pr.ProjectPath == r.ProjectPath))
                    {
                        allProjectReferences.Add(r);
                    }
                }
            }
        }
        private void LoadPackages()
        {
            packages.AddRange(GetPackageReferences());

            var result = new Dictionary<string, PackageReference>();
            foreach (PackageReference p in packages)
            {
                result.Add(p.Name, p);
            }

            foreach (CSharpProjectDocument p in allProjectReferences)
            {
                IEnumerable<PackageReference> refs = p.GetPackageReferences();

                foreach (PackageReference r in refs)
                {
                    if (!result.ContainsKey(r.Name))
                    {
                        result.Add(r.Name, r);
                        continue;
                    }

                    PackageReference existingEntry = result[r.Name];

                    var newVersion = new NuGetVersion(r.Version);
                    var existingVersion = new NuGetVersion(existingEntry.Version);

                    if (existingVersion < newVersion)
                    {
                        existingEntry.Version = r.Version;
                    }
                }
            }

            allPackages.AddRange(result.Values.ToList());
        }
        private void LoadXmlFromFile()
        {
            string xmlContent = File.ReadAllText(csProjectPath);
            xmlContent = new Regex(XMLNS_REGEX).Replace(xmlContent, string.Empty);
            doc.LoadXml(xmlContent);
        }
        #endregion
    }
}
