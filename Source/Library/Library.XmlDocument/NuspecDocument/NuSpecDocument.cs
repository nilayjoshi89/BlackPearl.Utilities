﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

using BlackPearl.Library.Extentions;

using NuGet.Frameworks;

namespace BlackPearl.Library.Xml
{
    public class NuSpecDocument
    {
        #region Constants
        public const string PACKAGE_NODE = "package";
        public const string INCLUDE_ATTRIBUTE = "include";
        public const string VERSION_NODE = "version";
        public const string FILES_NODE = "files";
        public const string FILE_NODE = "file";
        public const string SRC_ATTRIBUTE = "src";
        public const string BIN_RELEASE = "bin\\release\\";
        public const string DLL_EXT = ".dll";
        public const string TARGET_ATTRIBUTE = "target";
        public const string LIB = "lib\\";
        public const string REF = "ref\\";
        public const string CONTENT_FILES = "contentFiles";
        public const string CONTENT_ANY = "contentFiles\\any\\any\\";
        public const string BUILD_ACTION_ATTRIBUTE = "buildAction";
        public const string COPY_TO_OUTPUT_ATTRIBUTE = "copyToOutput";
        public const string ANY_PATH = "/any/any/";
        public const string NONE_VALUE = "None";
        public const string REFERENCE_NODE = "reference";
        public const string TARGET_FRAMEWORK_ATTRIBUTE = "targetFramework";
        public const string REFERENCES_NODE = "references";
        public const string GROUP_NODE = "group";
        public const string DEPENDENCIES_NODE = "dependencies";
        public const string DEPENDENCY_NODE = "dependency";
        public const string ID_ATTRIBUTE = "id";
        public const string METADATA_NODE = "metadata";
        public const string TITLE_NODE = "title";
        public const string AUTHORS_NODE = "authors";
        public const string OWNERS_NODE = "owners";
        public const string DESCRIPTION_NODE = "description";
        public const string RELEASE_NOTES_NODE = "releaseNotes";
        public const string COPYRIGHT_NODE = "copyright";
        public const string TAGS_NODE = "tags";

        private readonly IProjectDocument projectDocument;
        private readonly PackageMetaData nuspecMetaData;
        private XmlDocument document;
        #endregion

        #region Constructor
        public NuSpecDocument(IProjectDocument projectDocument, PackageMetaData nuspecMetaData)
        {
            this.projectDocument = projectDocument;
            this.nuspecMetaData = nuspecMetaData;
        }
        #endregion

        #region Nodes
        private XmlNode PackageNode => document?.SelectSingleNode(PACKAGE_NODE);
        private XmlNode MetaNode => document?.SelectSingleNode(PACKAGE_NODE + "/" + METADATA_NODE);
        #endregion

        #region Methods
        public Task<XmlDocument> Generate()
        {
            return Task.Run(() =>
            {
                try
                {
                    if (document != null)
                    {
                        return document;
                    }

                    if (!projectDocument.IsInitialized)
                    {
                        projectDocument.Initialize();
                    }

                    document = new XmlDocument();
                    document.AppendChild(document.CreateXmlDeclaration("1.0", "utf-8", ""));
                    document.AppendChild(PACKAGE_NODE);

                    CreateMetaBasic();
                    CreateDependencies();
                    CreateReferences();
                    CreateContentFiles();
                    CreateFiles();

                    return document;
                }
                catch (Exception ex)
                {
                    Debug.Print(ex.Message);
                }
                return null;
            });
        }
        private void CreateFiles()
        {
            XmlElement files = PackageNode.CreateChildNode(FILES_NODE);

            foreach (NuGetFramework fw in nuspecMetaData.TargetFramework)
            {
                foreach (string projRef in projectDocument.AllProjectReferences.Select(p => p.ProjectPath))
                {

                    files.CreateChildNode(FILE_NODE,
                        (SRC_ATTRIBUTE, BIN_RELEASE + Path.GetFileNameWithoutExtension(projRef) + DLL_EXT),
                        (TARGET_ATTRIBUTE, LIB + fw.GetShortFolderName()));
                    files.CreateChildNode(FILE_NODE,
                        (SRC_ATTRIBUTE, BIN_RELEASE + Path.GetFileNameWithoutExtension(projRef) + DLL_EXT),
                        (TARGET_ATTRIBUTE, REF + fw.GetShortFolderName()));
                }
            }

            foreach (NuGetFramework fw in nuspecMetaData.TargetFramework)
            {
                foreach (string cl in nuspecMetaData.CustomLibrary)
                {

                    files.CreateChildNode(FILE_NODE,
                    (SRC_ATTRIBUTE, BIN_RELEASE + Path.GetFileName(cl)),
                    (TARGET_ATTRIBUTE, LIB + fw.GetShortFolderName()));
                }
            }

            foreach (NuGetFramework fw in nuspecMetaData.TargetFramework)
            {
                foreach (string cr in nuspecMetaData.CustomReference)
                {
                    files.CreateChildNode(FILE_NODE,
                    (SRC_ATTRIBUTE, BIN_RELEASE + Path.GetFileName(cr)),
                    (TARGET_ATTRIBUTE, REF + fw.GetShortFolderName()));
                }
            }

            foreach (string cf in projectDocument.ContentFiles)
            {
                files.CreateChildNode(FILE_NODE,
                    (SRC_ATTRIBUTE, BIN_RELEASE + cf),
                    (TARGET_ATTRIBUTE, CONTENT_ANY + cf));
            }
        }
        private void CreateContentFiles()
        {
            if (!projectDocument.ContentFiles.Any())
            {
                return;
            }

            XmlElement contentFilesNode = MetaNode.CreateChildNode(CONTENT_FILES);
            foreach (string cf in projectDocument.ContentFiles)
            {
                contentFilesNode.CreateChildNode(FILES_NODE,
                    (INCLUDE_ATTRIBUTE, ANY_PATH + cf.Replace('\\', '/')),
                    (BUILD_ACTION_ATTRIBUTE, NONE_VALUE),
                    (COPY_TO_OUTPUT_ATTRIBUTE, bool.TrueString.ToLower()));
            }
        }
        private void CreateReferences()
        {
            if (!projectDocument.AllProjectReferences.Any() && !nuspecMetaData.CustomReference.Any())
            {
                return;
            }

            XmlElement references = MetaNode.CreateChildNode(REFERENCES_NODE);
            XmlElement refGroup = references.CreateChildNode(GROUP_NODE);

            CreateReferences(refGroup);

            foreach (NuGetFramework fw in nuspecMetaData.TargetFramework)
            {
                XmlNode refFrameworkNode = references.CreateChildNode(GROUP_NODE,
                    (TARGET_FRAMEWORK_ATTRIBUTE, fw.GetShortFolderName()));

                CreateReferences(refFrameworkNode);
            }
        }

        private void CreateReferences(XmlNode refGroupNode)
        {
            foreach (string projRef in projectDocument.AllProjectReferences.Select(p => p.ProjectPath))
            {
                refGroupNode.CreateChildNode(REFERENCE_NODE,
                    (FILE_NODE, Path.GetFileNameWithoutExtension(projRef) + DLL_EXT));
            }

            foreach (string cr in nuspecMetaData.CustomReference)
            {
                refGroupNode.CreateChildNode(REFERENCE_NODE,
                    (FILE_NODE, Path.GetFileName(cr)));
            }
        }

        private void CreateDependencies()
        {
            if (!projectDocument.AllPackages.Any())
            {
                return;
            }

            XmlElement dependencies = MetaNode.CreateChildNode(DEPENDENCIES_NODE);
            XmlElement group = dependencies.CreateChildNode(GROUP_NODE);

            CreateDependencyNodes(group);

            foreach (NuGetFramework fw in nuspecMetaData.TargetFramework)
            {
                XmlNode frameworkGroupNode = dependencies.CreateChildNode(GROUP_NODE, (TARGET_FRAMEWORK_ATTRIBUTE, fw.GetShortFolderName()));
                CreateDependencyNodes(frameworkGroupNode);
            }
        }

        private void CreateDependencyNodes(XmlNode groupNode)
        {
            foreach (PackageReference pRef in projectDocument.AllPackages)
            {
                groupNode.CreateChildNode(DEPENDENCY_NODE,
                    (ID_ATTRIBUTE, pRef.Name),
                    (VERSION_NODE, pRef.Version));
            }
        }

        private void CreateMetaBasic()
        {
            PackageNode.CreateChildNode(METADATA_NODE)
                .CreateChildNode(ID_ATTRIBUTE, nuspecMetaData.Id).ParentNode
                .CreateChildNode(VERSION_NODE, nuspecMetaData.Version).ParentNode
                .CreateChildNode(TITLE_NODE, nuspecMetaData.Title).ParentNode
                .CreateChildNode(AUTHORS_NODE, nuspecMetaData.Author).ParentNode
                .CreateChildNode(OWNERS_NODE, nuspecMetaData.Owner).ParentNode
                .CreateChildNode(DESCRIPTION_NODE, nuspecMetaData.Desciption).ParentNode
                .CreateChildNode(RELEASE_NOTES_NODE, nuspecMetaData.ReleaseNotes).ParentNode
                .CreateChildNode(COPYRIGHT_NODE, nuspecMetaData.Copyright).ParentNode
                .CreateChildNode(TAGS_NODE, nuspecMetaData.Tags);
        }
        #endregion
    }
}
