using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FolderBackup.Shared;
using System.IO;
using System.Collections.Generic;

namespace FolderBackup.SharedTest
{
    [TestClass]
    public class VersionsCompareTests
    {
        [TestMethod]
        public void NecessaryFilesToUpgradeTest()
        {
            string[] lines1 = { "First line", "Second line", "Third line" };
            string[] lines2 = { "First line", "Second line", "NO" };
            System.IO.Directory.CreateDirectory("asd");
            System.IO.Directory.CreateDirectory(@"asd\ciao");
            DirectoryInfo rinfo = new DirectoryInfo("asd");
            System.IO.File.WriteAllLines(@"asd\uno.txt", lines1);
            System.IO.File.WriteAllLines(@"asd\ciao\due.txt", lines1);
            System.IO.File.WriteAllLines(@"asd\ciao\tre.txt", lines1);
            System.IO.File.WriteAllLines(@"asd\ciao\quattro.txt", lines1);

            FBVersionBuilder vb = new FBVersionBuilder(rinfo.FullName);
            FolderBackup.Shared.FBVersion vold = (FolderBackup.Shared.FBVersion)vb.generate();
            System.IO.File.WriteAllLines(@"asd\ciao\cinque.txt", lines2);
            System.IO.File.WriteAllLines(@"asd\sei.txt", lines2);
            System.IO.File.WriteAllLines(@"asd\sette.txt", lines1);
            FolderBackup.Shared.FBVersion vnew = (FolderBackup.Shared.FBVersion)vb.generate();

            FBVersion diff = vnew - vold;

            List<FBFile> fl = FBVersion.getNecessaryFilesToUpgrade(vnew, vold.fileList);

            Assert.AreEqual(fl.Count, 1);
            FBFileBuilder fb = new FBFileBuilder(@"asd\ciao\cinque.txt");
            FBFile necessary = (FBFile) fb.generate();
            Assert.AreEqual(fl[0], necessary);

            System.IO.Directory.Delete("asd", true);
        }
    }
}
