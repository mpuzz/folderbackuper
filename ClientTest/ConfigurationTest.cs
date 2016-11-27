using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FolderBackup.Client;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace ClientTest
{
    [TestClass]
    public class ConfigurationTestCreateFile
    {
        String fileName = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"/FolderBackuper/config.conf";
        [TestCleanup]
        public void clean()
        {
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

        }
        [TestInitialize]
        public void TestInitialize()
        {
            clean();
        }
        [TestMethod]
        public void CheckFileCreated()
        {
            Config conf = Config.Instance();
            Assert.IsTrue(File.Exists(fileName));
           
        }
        [TestMethod]
        public void VariableSetted()
        {
            Config conf = Config.Instance();
            conf.userName.set("pluto");
            Assert.AreEqual(conf.userName.get(), "pluto");

            Stream br = File.OpenRead(fileName);

            BinaryFormatter deserializer = new BinaryFormatter();
            Config instance = (Config)deserializer.Deserialize(br);
            Assert.AreEqual(instance.userName.get(),"pluto");
            br.Close();
        }

    }
}
