using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using FolderBackup.Shared;


namespace FolderBackup.Client
{
    [Serializable()]
    public class ConfigAttribute<T>
    {
        private T obj;
        public void set(T newValue)
        {
            this.obj = newValue;
            Config.Instance().Serialize();
        }
        public T get()
        {
            return obj;
        }


    }

    [Serializable()]
    public class Config
    {   

        private static String basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"/FolderBackuper/";
        private static String fileName = basePath + @"config.conf";
        public ConfigAttribute<String> userName;
        public ConfigAttribute<String> targetPath;
        private static Config instance;

        private Config() {
            userName = new ConfigAttribute<String>();
            targetPath = new ConfigAttribute<String>();
        }
        public void Serialize()
        {
            MemoryStream ms = new MemoryStream();
            BinaryFormatter serializer = new BinaryFormatter();
            serializer.Serialize(ms, this);
            ms.Seek(0,SeekOrigin.Begin);
            UsefullMethods.SaveStreamToFile(ms, fileName);

        }

        public static Config Instance()
        {
            if (instance == null)
            {


                Stream br;
                try
                {
                    br = File.OpenRead(fileName);

                }
                catch (DirectoryNotFoundException e)
                {
                    Directory.CreateDirectory(basePath);
                    return Instance();
                }
                catch (FileNotFoundException e)
                {
                    instance = new Config();
                    instance.Serialize();
                    return instance;
                }
                BinaryFormatter deserializer = new BinaryFormatter();
                instance = (Config)deserializer.Deserialize(br);
                br.Close();
            }
            return instance;
        }
        

    }
}
