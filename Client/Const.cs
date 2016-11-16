using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderBackup.Client
{
    public class Const<T>
    {
        private static T obj;
        private Const(){}
        private static Const<T> instance;
        public static Const<T> Instance()
        {
            if (instance==null)
            {
                instance = new Const<T>();
            }
            return instance;

        }
        public void set(T o)
        {
            if (obj!=null)
            {
                throw new Exception("The object has already been setted");
            }
            obj = o;
        }
        public T get()
        {
            return obj;
        }
    }
}
