using System;
using System.IO;
using FolderBackup.Shared;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Runtime.Serialization.Formatters.Binary;

namespace SharedTest
{
    [TestClass]
    public class SerializationTests
    {
        [TestMethod]
        public void PersistAndDeserialize()
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

            Stream TestFileStream = File.Create("version.bin");
            BinaryFormatter serializer = new BinaryFormatter();
            serializer.Serialize(TestFileStream, v);
            TestFileStream.Close();

            TestFileStream = File.OpenRead("version.bin");
            BinaryFormatter deserializer = new BinaryFormatter();
            FBVersion v2 = (FBVersion)deserializer.Deserialize(TestFileStream);
            TestFileStream.Close();

            File.Delete("version.bin");

            Assert.AreEqual(v2, v);
        }
    }
}
