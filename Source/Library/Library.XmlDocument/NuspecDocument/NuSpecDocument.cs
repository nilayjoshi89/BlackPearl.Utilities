using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

using BlackPearl.Library.Extentions;

namespace BlackPearl.Library.Xml
{
    public class NuSpecDocument
    {
        #region Constants
        public const string PACKAGE_NODE = "package";
        public const string INCLUDE_ATTRIBUTE = "Include";
        public const string VERSION_NODE = "Version";
        public const string FILES_NODE = "files";
        public const string FILE_NODE = "file";
        public const string SRC_ATTRIBUTE = "src";
        public const string BIN_RELEASE = "bin\\release\\";
        public const string DLL_EXT = ".dll";
        public const string TARGET_ATTRIBUTE = "target";
        public const string LIB_461 = "lib\\net461";
        public const string REF_461 = "ref\\net461";
        public const string CONTENT_FILES = "contentFiles";
        public const string CONTENT_ANY = "contentFiles\\any\\any\\";
        public const string BUILD_ACTION_ATTRIBUTE = "buildAction";
        public const string COPY_TO_OUTPUT_ATTRIBUTE = "copyToOutput";
        public const string ANY_PATH = "/any/any/";
        public const string NONE_VALUE = "None";
        public const string REFERENCE_NODE = "reference";
        public const string TARGET_FRAMEWORK_ATTRIBUTE = "targetFramework";
        public const string NET461_VALUE = "net461";
        public const string REFERENCES_NODE = "references";
        public const string GROUP_NODE = "group";
        public const string DEPENDENCIES_NODE = "dependencies";
        public const string DEPENDENCY_NODE = "dependency";
        public const string NETFRAMEWORK_461 = ".NETFramework4.6.1";
        public const string ID_ATTRIBUTE = "id";
        public const string METADATA_NODE = "metadata";
        public const string TITLE_NODE = "title";
        public const string AUTHORS_NODE = "authors";
        public const string OWNERS_NODE = "owners";
        public const string DESCRIPTION_NODE = "description";
        public const string RELEASE_NOTES_NODE = "releaseNotes";
        public const string COPYRIGHT_NODE = "copyright";
        public const string TAGS_NODE = "tags";
        #endregion

        #region Methods
        public Task<XmlDocument> GenerateForProject(ICSProjectDocument projectDocument, NuspecMetaData nuspecMetaData)
        {
            return Task.Run(() =>
            {
                try
                {
                    if (!projectDocument.IsInitialized)
                    {
                        projectDocument.Initialize();
                    }

                    IList<ICSProjectDocument> projectReferences = projectDocument.AllProjectReferences;
                    IList<CSPackageReference> packageReferences = projectDocument.AllPackages;
                    IList<string> contentFiles = projectDocument.ContentFiles;

                    var doc = new XmlDocument();
                    XmlDeclaration declaration = doc.CreateXmlDeclaration("1.0", "utf-8", "");
                    doc.AppendChild(declaration);
                    doc.CreateChildNode(PACKAGE_NODE, doc, out XmlElement packageNode);
                    CreateMetaData(projectReferences, packageReferences, contentFiles, nuspecMetaData, doc);
                    CreateFiles(projectReferences, contentFiles, nuspecMetaData, doc);

                    return doc;
                }
                catch { }
                return null;
            });
        }
        private void CreateMetaData(IList<ICSProjectDocument> projectReferences, IList<CSPackageReference> packageReferences, IList<string> contentFiles, NuspecMetaData metaData, XmlDocument doc)
        {
            CreateMetaBasic(metaData, doc);
            CreateDependencies(packageReferences, doc);
            CreateReferences(projectReferences, metaData.CustomReference, doc);
            CreateContentFiles(contentFiles, doc);
        }
        private void CreateFiles(IList<ICSProjectDocument> projectReferences, IList<string> contentFiles, NuspecMetaData metaData, XmlDocument doc)
        {
            doc.CreateChildNode(FILES_NODE, doc.SelectSingleNode(PACKAGE_NODE), out XmlElement files);

            foreach (string projRef in projectReferences.Select(p => p.ProjectPath))
            {
                doc.CreateChildNodeWithAttribute(FILE_NODE, files, out _,
                    (SRC_ATTRIBUTE, BIN_RELEASE + Path.GetFileNameWithoutExtension(projRef) + DLL_EXT),
                    (TARGET_ATTRIBUTE, LIB_461))

                    .CreateChildNodeWithAttribute(FILE_NODE, files, out _,
                    (SRC_ATTRIBUTE, BIN_RELEASE + Path.GetFileNameWithoutExtension(projRef) + DLL_EXT),
                    (TARGET_ATTRIBUTE, REF_461));
            }

            foreach (string cl in metaData.CustomLibrary)
            {
                doc.CreateChildNodeWithAttribute(FILE_NODE, files, out _,
                    (SRC_ATTRIBUTE, BIN_RELEASE + Path.GetFileName(cl)),
                    (TARGET_ATTRIBUTE, LIB_461));
            }

            foreach (string cr in metaData.CustomReference)
            {
                doc.CreateChildNodeWithAttribute(FILE_NODE, files, out _,
                    (SRC_ATTRIBUTE, BIN_RELEASE + Path.GetFileName(cr)),
                    (TARGET_ATTRIBUTE, REF_461));
            }

            foreach (string cf in contentFiles)
            {
                doc.CreateChildNodeWithAttribute(FILE_NODE, files, out _,
                    (SRC_ATTRIBUTE, BIN_RELEASE + cf),
                    (TARGET_ATTRIBUTE, CONTENT_ANY + cf));
            }
        }
        private void CreateContentFiles(IList<string> contentFiles, XmlDocument doc)
        {
            if (!contentFiles.Any())
            {
                return;
            }

            doc.CreateChildNode(CONTENT_FILES, doc.SelectSingleNode(PACKAGE_NODE + "/" + METADATA_NODE), out XmlElement contentFilesNode);
            foreach (string cf in contentFiles)
            {
                doc.CreateChildNodeWithAttribute(FILES_NODE, contentFilesNode, out _,
                    (INCLUDE_ATTRIBUTE, ANY_PATH + cf.Replace('\\', '/')),
                    (BUILD_ACTION_ATTRIBUTE, NONE_VALUE),
                    (COPY_TO_OUTPUT_ATTRIBUTE, bool.TrueString.ToLower()));
            }
        }
        private void CreateReferences(IList<ICSProjectDocument> projectReferences, List<string> customReference, XmlDocument doc)
        {
            if (!projectReferences.Any() && !customReference.Any())
            {
                return;
            }

            XmlNode metaNode = doc.SelectSingleNode(PACKAGE_NODE + "/" + METADATA_NODE);
            doc.CreateChildNode(REFERENCES_NODE, metaNode, out XmlElement references)
                .CreateChildNode(GROUP_NODE, references, out XmlElement refGroup)
                .CreateChildNodeWithAttribute(GROUP_NODE, references, out XmlElement refTarget461Group,
                    (TARGET_FRAMEWORK_ATTRIBUTE, NET461_VALUE));

            foreach (string projRef in projectReferences.Select(p => p.ProjectPath))
            {
                doc.CreateChildNodeWithAttribute(REFERENCE_NODE, refTarget461Group, out _,
                    (FILES_NODE, Path.GetFileNameWithoutExtension(projRef) + DLL_EXT))
                   .CreateChildNodeWithAttribute(REFERENCE_NODE, refGroup, out _,
                    (FILE_NODE, Path.GetFileNameWithoutExtension(projRef) + DLL_EXT));
            }

            foreach (string cr in customReference)
            {
                doc.CreateChildNodeWithAttribute(REFERENCE_NODE, refTarget461Group, out _,
                    (FILES_NODE, Path.GetFileName(cr)))
                   .CreateChildNodeWithAttribute(REFERENCE_NODE, refGroup, out _,
                    (FILE_NODE, Path.GetFileName(cr)));
            }
        }
        private void CreateDependencies(IList<CSPackageReference> packageReferences, XmlDocument doc)
        {
            if (!packageReferences.Any())
            {
                return;
            }

            XmlNode metaNode = doc.SelectSingleNode(PACKAGE_NODE + "/" + METADATA_NODE);
            doc.CreateChildNode(DEPENDENCIES_NODE, metaNode, out XmlElement dependencies)
                .CreateChildNode(GROUP_NODE, dependencies, out XmlElement group)
                .CreateChildNodeWithAttribute(GROUP_NODE, dependencies, out XmlElement group461,
                    (TARGET_FRAMEWORK_ATTRIBUTE, NETFRAMEWORK_461));

            foreach (CSPackageReference pRef in packageReferences)
            {
                doc.CreateChildNodeWithAttribute(DEPENDENCY_NODE, group, out _,
                    (ID_ATTRIBUTE, pRef.Name),
                    (VERSION_NODE.ToLower(), pRef.Version))
                   .CreateChildNodeWithAttribute(DEPENDENCY_NODE, group461, out _,
                    (ID_ATTRIBUTE, pRef.Name),
                    (VERSION_NODE.ToLower(), pRef.Version));
            }
        }
        private XmlElement CreateMetaBasic(NuspecMetaData metaData, XmlDocument doc)
        {
            XmlNode packageNode = doc.SelectSingleNode(PACKAGE_NODE);
            doc.CreateChildNode(METADATA_NODE, packageNode, out XmlElement metaNode)
                .CreateChildNode(ID_ATTRIBUTE, metaData.Id, metaNode, out _)
                .CreateChildNode(VERSION_NODE.ToLower(), metaData.Version, metaNode, out _)
                .CreateChildNode(TITLE_NODE, metaData.Title, metaNode, out _)
                .CreateChildNode(AUTHORS_NODE, metaData.Author, metaNode, out _)
                .CreateChildNode(OWNERS_NODE, metaData.Owner, metaNode, out _)
                .CreateChildNode(DESCRIPTION_NODE, metaData.Desciption, metaNode, out _)
                .CreateChildNode(RELEASE_NOTES_NODE, metaData.ReleaseNotes, metaNode, out _)
                .CreateChildNode(COPYRIGHT_NODE, metaData.Copyright, metaNode, out _)
                .CreateChildNode(TAGS_NODE, metaData.Tags, metaNode, out _);
            return metaNode;
        }
        #endregion
    }
}
