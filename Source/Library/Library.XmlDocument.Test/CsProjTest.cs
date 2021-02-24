using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BlackPearl.Library.Xml.Test
{
    [TestClass]
    public class CsProjTest
    {
        private static ICSProjectDocument doc, doc2;
        private const string XmlDataPath = "Data\\CsProject2TestData.xml";
        private const string XmlDataPath2 = "Data\\CsProjectTestData.xml";

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            doc = new CSProjectDocument() { ProjectPath = XmlDataPath };
            doc.Initialize();

            doc2 = new CSProjectDocument() { ProjectPath = XmlDataPath2 };
            doc2.Initialize();
        }

        [TestMethod]
        public void GetAllProjectReference_Test1()
        {
            //Arrange

            //Act
            IEnumerable<string> projects = doc.AllProjectReferences.Select(p => p.ProjectPath);

            //Assert
            Assert.IsTrue(projects.Any());
        }

        [TestMethod]
        public void GetPackageReferenceIds_Test1()
        {
            //Arrange

            //Act
            IEnumerable<CSPackageReference> packages = doc.Packages;

            //Assert
            Assert.IsTrue(packages.Count() == 4);
        }

        [TestMethod]
        public void GetPackageReferenceIds_Test2()
        {
            //Arrange

            //Act
            IEnumerable<CSPackageReference> packages = doc2.AllPackages;

            //Assert
            Assert.IsTrue(packages.Count() == 2);
        }

        [TestMethod]
        public void GetAllPackageReference_Test1()
        {
            //Arrange

            //Act
            IEnumerable<CSPackageReference> packages = doc.AllPackages;

            //Assert
            Assert.IsTrue(packages.Count() == 5);
        }

        [TestMethod]
        public void GetContentFiles_Test1()
        {
            //Arrange

            //Act
            IEnumerable<string> content1 = doc.ContentFiles;
            IEnumerable<string> content2 = doc2.ContentFiles;

            //Assert
            Assert.IsTrue(content1.Count() == 1);
            Assert.IsTrue(content2.Count() == 2);
        }
    }
}