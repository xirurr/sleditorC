
using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Xml.Serialization;
using ConsoleApplication1.Base;

namespace ConsoleApplication1.DataAccess.FileReaders
{
    public class XmlReader : IXmlReader
    {
        private readonly ConcurrentDictionary<Type, XmlSerializer> serializers = new ConcurrentDictionary<Type, XmlSerializer>();

        public AllConfig GetObject(string fileName)
        {
            if (Contains(fileName, "config.xml"))
            {
                return GetObject(typeof(AllConfig), fileName);
            }
            return null;

            bool Contains(string source, string value) => CultureInfo.InvariantCulture.CompareInfo.IndexOf(source, value, CompareOptions.IgnoreCase) > -1;
        }

        private AllConfig GetObject(Type type, string filePath)
        {
            var serializer = serializers.GetOrAdd(type, t => new XmlSerializer(t, new XmlRootAttribute("config")));
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                return (AllConfig)serializer.Deserialize(fileStream);
            }
        }
    }
}