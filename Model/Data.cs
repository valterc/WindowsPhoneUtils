using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace WindowsPhoneUtils.Model
{
    public abstract class Data
    {

        public static T LoadData<T>(IsolatedStorageFile isf, String fileName)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            T t = default(T);

            using (IsolatedStorageFileStream stream = new IsolatedStorageFileStream(fileName, System.IO.FileMode.OpenOrCreate, isf))
            {
                if (stream.Length > 0)
                {
                    t = (T)serializer.Deserialize(stream);
                }
            }

            return t;
        }

        public static void SaveData<T>(IsolatedStorageFile isf, T data, String fileName)
        {
            MemoryStream ms = new MemoryStream();
            XmlSerializer xs = new XmlSerializer(typeof(T));

            xs.Serialize(ms, data);

            ms.Seek(0, SeekOrigin.Begin);
            using (ms)
            {
                using (IsolatedStorageFileStream file_stream = isf.CreateFile(fileName))
                {
                    ms.WriteTo(file_stream);
                }
            }
        }

    }
}
