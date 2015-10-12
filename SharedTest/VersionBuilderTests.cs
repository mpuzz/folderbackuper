using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FolderBackup.Shared;
using System.IO;


namespace FolderBackup.SharedTest
{
    [TestClass]
    public class FBVersionBuilderTest
    {
        [TestMethod]
        public void VersionBuilderGenerate()
        {
            string[] lines = { "First line", "Second line", "Third line" };
            System.IO.Directory.CreateDirectory("asd");
            System.IO.Directory.CreateDirectory(@"asd\ciao");
            DirectoryInfo rinfo = new DirectoryInfo("asd");
            System.IO.File.WriteAllLines(@"asd\uno.txt", lines);
            System.IO.File.WriteAllLines(@"asd\ciao\due.txt", lines);
            FileInfo finfo = new FileInfo(@"asd\ciao\due.txt");

            FBVersionBuilder vb = new FBVersionBuilder(rinfo.FullName);
            FolderBackup.Shared.FBVersion v = (FolderBackup.Shared.FBVersion)vb.generate();

            Assert.AreEqual(v.root.content.Count, (int)2);
            Assert.IsInstanceOfType(v.root.content["ciao"], v.root.GetType());
            Assert.AreEqual(((FolderBackup.Shared.FBDirectory)v.root.content["ciao"]).content.Count, 1);
            Assert.AreEqual(((FolderBackup.Shared.FBFile)((FolderBackup.Shared.FBDirectory)v.root.content["ciao"]).content["due.txt"]).dimension, finfo.Length);
            Assert.AreEqual(((FolderBackup.Shared.FBDirectory)v.root.content["ciao"]).content["due.txt"].isEqualTo(v.root.content["uno.txt"]), true);
            System.IO.Directory.Delete("asd", true);
        }

        [TestMethod]
        public void TwoVersionsAreEquals()
        {

            string[] lines = { "First line", "Second line", "Third line" };
            System.IO.Directory.CreateDirectory("asd");
            System.IO.Directory.CreateDirectory(@"asd\ciao");
            DirectoryInfo rinfo = new DirectoryInfo("asd");
            System.IO.File.WriteAllLines(@"asd\uno.txt", lines);
            System.IO.File.WriteAllLines(@"asd\ciao\due.txt", lines);
            FileInfo finfo = new FileInfo(@"asd\ciao\due.txt");

            FBVersionBuilder vb = new FBVersionBuilder(rinfo.FullName);
            FolderBackup.Shared.FBVersion v1 = (FolderBackup.Shared.FBVersion)vb.generate();
            FolderBackup.Shared.FBVersion v2 = (FolderBackup.Shared.FBVersion)vb.generate();

            Assert.AreEqual(v1, v2);
            System.IO.Directory.Delete("asd", true);
        }

        [TestMethod]
        public void TwoVersionsAreNotEquals()
        {
            string[] lines1 = { "First line", "Second line", "Third line" };
            string[] lines2 = { "First line", "Second line", "NO" };
            System.IO.Directory.CreateDirectory("asd");
            System.IO.Directory.CreateDirectory(@"asd\ciao");
            DirectoryInfo rinfo = new DirectoryInfo("asd");
            System.IO.File.WriteAllLines(@"asd\uno.txt", lines1);
            System.IO.File.WriteAllLines(@"asd\ciao\due.txt", lines1);
            FileInfo finfo = new FileInfo(@"asd\ciao\due.txt");

            FBVersionBuilder vb = new FBVersionBuilder(rinfo.FullName);
            FolderBackup.Shared.FBVersion v1 = (FolderBackup.Shared.FBVersion)vb.generate();
            System.IO.File.WriteAllLines(@"asd\ciao\due.txt", lines2);
            FolderBackup.Shared.FBVersion v2 = (FolderBackup.Shared.FBVersion)vb.generate();

            Assert.AreNotEqual(v1, v2);
            System.IO.Directory.Delete("asd", true);
        }
    }
}
