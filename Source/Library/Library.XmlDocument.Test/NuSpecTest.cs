using System.Collections.Generic;
using System.IO;
using System.Xml;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

namespace BlackPearl.Library.Xml.Test
{
    [TestClass]
    public class NuSpecTest
    {
        private Mock<IProjectDocument> projectADocument, projectBDocument, projectCDocument;
        private readonly PackageReference packageOne = new PackageReference() { Name = "PackageOne", Version = "1.0.0.0" };
        private readonly PackageReference packageTwo = new PackageReference() { Name = "PackageTwo", Version = "1.0.0.0" };
        private readonly PackageReference packageTwo2 = new PackageReference() { Name = "PackageTwo", Version = "2.0.0.0" };
        private readonly PackageReference packageThree = new PackageReference() { Name = "PackageThree", Version = "1.0.0.0" };
        private readonly PackageReference packageFour = new PackageReference() { Name = "PackageFour", Version = "3.0.0.0" };
        private readonly PackageReference packageFour2 = new PackageReference() { Name = "PackageFour", Version = "2.5.0.0" };
        private readonly PackageReference packageFive = new PackageReference() { Name = "PackageFive", Version = "1.0.0.0" };

        [TestInitialize]
        public void Initialize()
        {
            projectADocument = new Mock<IProjectDocument>(MockBehavior.Strict);
            projectBDocument = new Mock<IProjectDocument>(MockBehavior.Strict);
            projectCDocument = new Mock<IProjectDocument>(MockBehavior.Strict);

            SetupProjectAMoq();
            SetupProjectBMoq();
            SetupProjectCMoq();
        }
        private void SetupProjectAMoq()
        {

            projectADocument.SetupGet(p => p.IsInitialized)
                                        .Returns(true);
            //projectADocument.SetupGet(p => p.ProjectPath)
            //                .Returns("C:\\MySolution\\ProjectA\\A.csproj");
            //projectADocument.SetupGet(p => p.ProjectReferences)
            //                .Returns(new List<ICSProjectDocument> { projectBDocument.Object });
            //projectADocument.SetupGet(p => p.Packages)
            //                .Returns(new List<CSPackageReference> { packageOne, packageTwo });
            projectADocument.SetupGet(p => p.ContentFiles)
                            .Returns(new List<string> { "Config\\CNF.xml", "Data\\MyData.json" });
            projectADocument.SetupGet(p => p.AllPackages)
                            .Returns(new List<PackageReference>
                            {
                                packageOne,
                                packageTwo2,
                                packageThree,
                                packageFour,
                                packageFive
                            });
            projectADocument.SetupGet(p => p.AllProjectReferences)
                            .Returns(new List<IProjectDocument> { projectBDocument.Object, projectCDocument.Object });
        }
        private void SetupProjectBMoq()
        {

            //projectBDocument.SetupGet(p => p.IsInitialized)
            //                            .Returns(true);
            projectBDocument.SetupGet(p => p.ProjectPath)
                            .Returns("..\\..\\ProjectB\\B.csproj");
            //projectBDocument.SetupGet(p => p.ProjectReferences)
            //                .Returns(new List<ICSProjectDocument> { projectCDocument.Object });
            //projectBDocument.SetupGet(p => p.Packages)
            //                .Returns(new List<CSPackageReference> { packageTwo2, packageThree, packageFour });
            //projectBDocument.SetupGet(p => p.ContentFiles)
            //                .Returns(new List<string> { "Config\\INVALID_CNF.xml", "Data\\INVALID_MyData.json" });
            //projectBDocument.SetupGet(p => p.AllPackages)
            //                .Returns(new List<CSPackageReference>
            //                {
            //                    packageTwo2,
            //                    packageThree,
            //                    packageFour,
            //                    packageFive
            //                });
            //projectBDocument.SetupGet(p => p.AllProjectReferences)
            //                .Returns(new List<ICSProjectDocument> { projectCDocument.Object });
        }
        private void SetupProjectCMoq()
        {

            //projectCDocument.SetupGet(p => p.IsInitialized)
            //                            .Returns(true);
            projectCDocument.SetupGet(p => p.ProjectPath)
                            .Returns("..\\..\\ProjectC\\C.csproj");
            //projectCDocument.SetupGet(p => p.ProjectReferences)
            //                .Returns(new List<ICSProjectDocument> { });
            //projectCDocument.SetupGet(p => p.Packages)
            //                .Returns(new List<CSPackageReference> { packageFour2, packageFive });
            //projectCDocument.SetupGet(p => p.ContentFiles)
            //                .Returns(new List<string> { "Config\\INVALID_C_CNF.xml", "Data\\INVALID_C_MyData.json" });
            //projectCDocument.SetupGet(p => p.AllPackages)
            //                .Returns(new List<CSPackageReference>
            //                {
            //                    packageFour2,
            //                    packageFive
            //                });
            //projectCDocument.SetupGet(p => p.AllProjectReferences)
            //                .Returns(new List<ICSProjectDocument> { });
        }

        [TestCleanup]
        public void CleanUp()
        {
            projectADocument.VerifyAll();
            projectBDocument.VerifyAll();
            projectCDocument.VerifyAll();
        }

        [TestMethod]
        public void TestNuspec_Content()
        {
            var metaData = new PackageMetaData()
            {
                Id = "Id1",
                Version = "1.0.0.0",
                Title = "Title",
                Author = "Auth1",
                Owner = "Owner1",
                Copyright = "CP1",
                Desciption = "Desc1",
                Tags = "T1",
                ReleaseNotes = "RN1",
                CustomLibrary = new List<string>() { "A.exe" },
                CustomReference = new List<string>(),
            };

            var nuspecDoc = new NuSpecDocument(projectADocument.Object, metaData);
            XmlDocument doc = nuspecDoc.Generate().Result;

            string expectedValue = @"<?xml version=""1.0"" encoding=""utf-8""?><package><metadata><id>Id1</id><version>1.0.0.0</version><title>Title</title><authors>Auth1</authors><owners>Owner1</owners><description>Desc1</description><releaseNotes>RN1</releaseNotes><copyright>CP1</copyright><tags>T1</tags><dependencies><group><dependency id=""PackageOne"" version=""1.0.0.0"" /><dependency id=""PackageTwo"" version=""2.0.0.0"" /><dependency id=""PackageThree"" version=""1.0.0.0"" /><dependency id=""PackageFour"" version=""3.0.0.0"" /><dependency id=""PackageFive"" version=""1.0.0.0"" /></group><group targetFramework=""net461""><dependency id=""PackageOne"" version=""1.0.0.0"" /><dependency id=""PackageTwo"" version=""2.0.0.0"" /><dependency id=""PackageThree"" version=""1.0.0.0"" /><dependency id=""PackageFour"" version=""3.0.0.0"" /><dependency id=""PackageFive"" version=""1.0.0.0"" /></group></dependencies><references><group><reference file=""B.dll"" /><reference file=""C.dll"" /></group><group targetFramework=""net461""><reference file=""B.dll"" /><reference file=""C.dll"" /></group></references><contentFiles><files include=""/any/any/Config/CNF.xml"" buildAction=""None"" copyToOutput=""true"" /><files include=""/any/any/Data/MyData.json"" buildAction=""None"" copyToOutput=""true"" /></contentFiles></metadata><files><file src=""bin\release\B.dll"" target=""lib\net461"" /><file src=""bin\release\B.dll"" target=""ref\net461"" /><file src=""bin\release\C.dll"" target=""lib\net461"" /><file src=""bin\release\C.dll"" target=""ref\net461"" /><file src=""bin\release\A.exe"" target=""lib\net461"" /><file src=""bin\release\Config\CNF.xml"" target=""contentFiles\any\any\Config\CNF.xml"" /><file src=""bin\release\Data\MyData.json"" target=""contentFiles\any\any\Data\MyData.json"" /></files></package>";
            Assert.IsNotNull(doc.InnerXml, expectedValue);
        }

        [TestMethod]
        public void TestNuspec()
        {
            var metaData = new PackageMetaData()
            {
                Id = "Id1",
                Version = "1.0.0.0",
                Title = "Title",
                Author = "Auth1",
                Owner = "Owner1",
                Copyright = "CP1",
                Desciption = "Desc1",
                Tags = "T1",
                ReleaseNotes = "RN1",
                CustomLibrary = new List<string>() { "A.exe" },
                CustomReference = new List<string>(),
            };

            var nuspecDoc = new NuSpecDocument(projectADocument.Object, metaData);
            XmlDocument doc = nuspecDoc.Generate().Result;

            Assert.IsNotNull(doc);
            CheckDependencies(doc);
            CheckReferences(doc, metaData);
            CheckContentFile(doc);
            CheckFiles(doc, metaData);
        }

        private void CheckFiles(XmlDocument doc, PackageMetaData metaData)
        {
            string queryPath = NuSpecDocument.PACKAGE_NODE + "/" + NuSpecDocument.FILES_NODE + "/"
              + NuSpecDocument.FILE_NODE + "[@src = \"bin\\release\\{0}\" and @target= \"lib\\net461\"]";
            string queryPath2 = NuSpecDocument.PACKAGE_NODE + "/" + NuSpecDocument.FILES_NODE + "/"
              + NuSpecDocument.FILE_NODE + "[@src = \"bin\\release\\{0}\" and @target= \"ref\\net461\"]";

            foreach (IProjectDocument r in projectADocument.Object.AllProjectReferences)
            {
                Assert.IsTrue(doc.SelectNodes(string.Format(queryPath, Path.GetFileNameWithoutExtension(r.ProjectPath) + ".dll")).Count == 1);
                Assert.IsTrue(doc.SelectNodes(string.Format(queryPath2, Path.GetFileNameWithoutExtension(r.ProjectPath) + ".dll")).Count == 1);
            }

            foreach (string r in metaData.CustomLibrary)
            {
                Assert.IsTrue(doc.SelectNodes(string.Format(queryPath, r)).Count == 1);
            }

            string queryPath3 = NuSpecDocument.PACKAGE_NODE + "/" + NuSpecDocument.FILES_NODE + "/"
              + NuSpecDocument.FILE_NODE + "[@src = \"bin\\release\\{0}\" and @target= \"contentFiles\\any\\any\\{0}\"]";
            foreach (string cf in projectADocument.Object.ContentFiles)
            {
                Assert.IsTrue(doc.SelectNodes(string.Format(queryPath3, cf)).Count == 1);
            }
        }
        private void CheckContentFile(XmlDocument doc)
        {
            string queryPath = NuSpecDocument.PACKAGE_NODE + "/" + NuSpecDocument.METADATA_NODE + "/"
              + NuSpecDocument.CONTENT_FILES + "/" + NuSpecDocument.FILES_NODE + "[@include = \"/any/any/{0}\"]";

            foreach (string cf in projectADocument.Object.ContentFiles)
            {
                Assert.IsTrue(doc.SelectNodes(string.Format(queryPath, cf.Replace('\\', '/'))).Count == 1);
            }
        }
        private void CheckReferences(XmlDocument doc, PackageMetaData metaData)
        {
            string queryPath = NuSpecDocument.PACKAGE_NODE + "/" + NuSpecDocument.METADATA_NODE + "/"
                + NuSpecDocument.REFERENCES_NODE + "/" + NuSpecDocument.GROUP_NODE + "/" + NuSpecDocument.REFERENCE_NODE + "[@file = \"{0}\"]";

            foreach (IProjectDocument r in projectADocument.Object.AllProjectReferences)
            {
                Assert.IsTrue(doc.SelectNodes(string.Format(queryPath, Path.GetFileNameWithoutExtension(r.ProjectPath) + ".dll")).Count == 2);
            }

            foreach (string r in metaData.CustomReference)
            {
                Assert.IsTrue(doc.SelectNodes(string.Format(queryPath, r)).Count == 2);
            }
        }

        private void CheckDependencies(XmlDocument doc)
        {
            string groupDependenciesPath = NuSpecDocument.PACKAGE_NODE + "/" + NuSpecDocument.METADATA_NODE + "/"
                + NuSpecDocument.DEPENDENCIES_NODE + "/" + NuSpecDocument.GROUP_NODE + "/" + NuSpecDocument.DEPENDENCY_NODE + "[@id = \"{0}\" and @version = \"{1}\"]";

            foreach (PackageReference dp in projectADocument.Object.AllPackages)
            {
                Assert.IsTrue(doc.SelectNodes(string.Format(groupDependenciesPath, dp.Name, dp.Version)).Count == 2);
            }
        }
    }
}