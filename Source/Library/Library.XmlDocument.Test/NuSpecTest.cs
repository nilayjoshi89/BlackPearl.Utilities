using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

namespace BlackPearl.Library.Xml.Test
{
    [TestClass]
    public class NuSpecTest
    {
        private Mock<ICSProjectDocument> projectADocument, projectBDocument, projectCDocument;
        private readonly CSPackageReference packageOne = new CSPackageReference() { Name = "PackageOne", Version = "1.0.0.0" };
        private readonly CSPackageReference packageTwo = new CSPackageReference() { Name = "PackageTwo", Version = "1.0.0.0" };
        private readonly CSPackageReference packageTwo2 = new CSPackageReference() { Name = "PackageTwo", Version = "2.0.0.0" };
        private readonly CSPackageReference packageThree = new CSPackageReference() { Name = "PackageThree", Version = "1.0.0.0" };
        private readonly CSPackageReference packageFour = new CSPackageReference() { Name = "PackageFour", Version = "3.0.0.0" };
        private readonly CSPackageReference packageFour2 = new CSPackageReference() { Name = "PackageFour", Version = "2.5.0.0" };
        private readonly CSPackageReference packageFive = new CSPackageReference() { Name = "PackageFive", Version = "1.0.0.0" };

        [TestInitialize]
        public void Initialize()
        {
            projectADocument = new Mock<ICSProjectDocument>(MockBehavior.Strict);
            projectBDocument = new Mock<ICSProjectDocument>(MockBehavior.Strict);
            projectCDocument = new Mock<ICSProjectDocument>(MockBehavior.Strict);

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
                            .Returns(new List<CSPackageReference>
                            {
                                packageOne,
                                packageTwo2,
                                packageThree,
                                packageFour,
                                packageFive
                            });
            projectADocument.SetupGet(p => p.AllProjectReferences)
                            .Returns(new List<ICSProjectDocument> { projectBDocument.Object, projectCDocument.Object });
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
        public void TestNuspec()
        {
            var nuspecDoc = new NuSpecDocument();
            var metaData = new NuspecMetaData()
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
                CustomReference = new List<string>()
            };

            XmlDocument doc = nuspecDoc.GenerateForProject(projectADocument.Object, metaData).Result;

            Assert.IsNotNull(doc);
            CheckDependencies(doc);
            CheckReferences(doc, metaData);
            CheckContentFile(doc);
            CheckFiles(doc, metaData);
        }

        private void CheckFiles(XmlDocument doc, NuspecMetaData metaData)
        {
            string queryPath = NuSpecDocument.PACKAGE_NODE + "/" + NuSpecDocument.FILES_NODE + "/"
              + NuSpecDocument.FILE_NODE + "[@src = \"bin\\release\\{0}\" and @target= \"lib\\net461\"]";
            string queryPath2 = NuSpecDocument.PACKAGE_NODE + "/" + NuSpecDocument.FILES_NODE + "/"
              + NuSpecDocument.FILE_NODE + "[@src = \"bin\\release\\{0}\" and @target= \"ref\\net461\"]";

            foreach (ICSProjectDocument r in projectADocument.Object.AllProjectReferences)
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
              + NuSpecDocument.CONTENT_FILES + "/" + NuSpecDocument.FILES_NODE + "[@Include = \"/any/any/{0}\"]";

            foreach (string cf in projectADocument.Object.ContentFiles)
            {
                Assert.IsTrue(doc.SelectNodes(string.Format(queryPath, cf.Replace('\\', '/'))).Count == 1);
            }
        }
        private void CheckReferences(XmlDocument doc, NuspecMetaData metaData)
        {
            string queryPath = NuSpecDocument.PACKAGE_NODE + "/" + NuSpecDocument.METADATA_NODE + "/"
                + NuSpecDocument.REFERENCES_NODE + "/" + NuSpecDocument.GROUP_NODE + "/" + NuSpecDocument.REFERENCE_NODE + "[@file = \"{0}\"]";

            foreach (ICSProjectDocument r in projectADocument.Object.AllProjectReferences)
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

            foreach (CSPackageReference dp in projectADocument.Object.AllPackages)
            {
                Assert.IsTrue(doc.SelectNodes(string.Format(groupDependenciesPath, dp.Name, dp.Version)).Count == 2);
            }
        }
    }
}