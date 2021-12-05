using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace DictionaryBruteForce
{
    static class Serializer
    {
        public static FileInfo SerializeObject(this List<string> list, string fileName)
        {
            var serializer = new XmlSerializer(typeof(List<string>));
            using (var stream = File.OpenWrite(fileName))
            {
                serializer.Serialize(stream, list);
            }
            return new FileInfo(fileName);
        }

        public static List<string> Deserialize(string fileName)
        {
            List<string> list = new List<string>();
            var serializer = new XmlSerializer(typeof(List<string>));
            using (var stream = File.OpenRead(fileName))
            {
                var other = (List<string>)(serializer.Deserialize(stream));
                list.Clear();
                list.AddRange(other);
            }

            return list;
        }
    }

}
