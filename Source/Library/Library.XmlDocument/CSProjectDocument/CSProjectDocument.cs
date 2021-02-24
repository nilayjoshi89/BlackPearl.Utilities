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
    public class CSProjectDocument : ICSProjectDocument
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
        private readonly List<CSPackageReference> packages = new List<CSPackageReference>();
        private readonly List<CSPackageReference> allPackages = new List<CSPackageReference>();
        private readonly List<ICSProjectDocument> projectReferences = new List<ICSProjectDocument>();
        private readonly List<ICSProjectDocument> allProjectReferences = new List<ICSProjectDocument>();
        private readonly List<string> contentFiles = new List<string>();
        #endregion

        #region Constructor
        public CSProjectDocument()
        {
            doc = new XmlDocument();
            packages = new List<CSPackageReference>();
            allPackages = new List<CSPackageReference>();
            projectReferences = new List<ICSProjectDocument>();
            contentFiles = new List<string>();
        }
        #endregion

        #region Properties
        public string ProjectPath { get => csProjectPath; set => csProjectPath = Path.GetFullPath(value); }
        public IList<CSPackageReference> Packages => packages;
        public IList<CSPackageReference> AllPackages => allPackages;
        public IList<ICSProjectDocument> ProjectReferences => projectReferences;
        public IList<ICSProjectDocument> AllProjectReferences => allProjectReferences;
        public IList<string> ContentFiles => contentFiles;
        public bool IsInitialized { get; private set; }
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
        private IEnumerable<string> GetProjectReferences()
        {
            return doc.SelectNodes($"{PROJECT_NODE}/{ITEMGROUP_NODE}/{PROJECT_REFERENCE_NODE}/@{INCLUDE_ATTRIBUTE}")
                                                   .Cast<XmlNode>()
                                                   .Select(n => Path.GetFullPath(Path.GetDirectoryName(csProjectPath) + "\\" + n.Value));
        }
        private IEnumerable<CSPackageReference> GetPackageReferences()
        {
            return doc.SelectNodes($"{PROJECT_NODE}/{ITEMGROUP_NODE}/{PACKAGE_REFERENCE_NODE}")
                                                  .Cast<XmlNode>()
                                                  .Select(n => new CSPackageReference
                                                  {
                                                      Name = n.SelectSingleNode($"@{INCLUDE_ATTRIBUTE}").Value.ToLower(),
                                                      Version = n.SelectSingleNode(VERSION_NODE)?.InnerText
                                                                ?? n.SelectSingleNode($"@{VERSION_NODE}").Value
                                                  });

        }
        private IEnumerable<string> GetContentFiles()
        {
            return doc.SelectNodes($"{PROJECT_NODE}/{ITEMGROUP_NODE}/{NONE_VALUE}[{COPY_TO_OUT_DIR}]/@{INCLUDE_ATTRIBUTE}"
                                                          + $"| {PROJECT_NODE}/{ITEMGROUP_NODE}/{CONTENT_VALUE}[{COPY_TO_OUT_DIR}]/Link"
                                                          + $"| {PROJECT_NODE}/{ITEMGROUP_NODE}/{CONTENT_VALUE}[{COPY_TO_OUT_DIR}]/@{INCLUDE_ATTRIBUTE}")
                                                  .Cast<XmlNode>()
                                                  .Select(n => n.InnerText ?? n.Value);

        }
        private void LoadContentFile() => contentFiles.AddRange(GetContentFiles());
        private void LoadProjectRefernces()
        {
            projectReferences.AddRange(GetProjectReferences().Select(p => new CSProjectDocument() { ProjectPath = p }));
            allProjectReferences.AddRange(projectReferences);

            Parallel.ForEach(projectReferences, (p) =>
            {
                p.Initialize();
            });

            foreach (CSProjectDocument p in projectReferences)
            {
                foreach (CSProjectDocument r in p.AllProjectReferences)
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

            var result = new Dictionary<string, CSPackageReference>();
            foreach (CSPackageReference p in packages)
            {
                result.Add(p.Name, p);
            }

            foreach (CSProjectDocument p in allProjectReferences)
            {
                IEnumerable<CSPackageReference> refs = p.GetPackageReferences();

                foreach (CSPackageReference r in refs)
                {
                    if (!result.ContainsKey(r.Name))
                    {
                        result.Add(r.Name, r);
                        continue;
                    }

                    CSPackageReference existingEntry = result[r.Name];

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
