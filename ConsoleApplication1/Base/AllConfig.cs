using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using ConsoleApplication1.DataAccess.FileReaders;

namespace ConsoleApplication1.Base
{
    public class AllConfig
    {
        private static AllConfig _instance;

        static XmlReader _xmlReader = new XmlReader();

        private IDictionary<string, Object> fields =
            new Dictionary<string, Object>();

        [XmlArray("custom")] [XmlArrayItem("project")]
        public List<ProjectConfig> ProjectConfList = new List<ProjectConfig>();

        [XmlElement("supervisorRecipients")]
        public string supervisorRecipients
        {
            get => String.Join(";", supervisorRecipientsList);
            set => supervisorRecipientsList.AddRange(value.Split(';'));
        }

        [XmlElement("delimiter")]
        public string delimiter
        {
            get => (string) fields["delimiter"];
            set
            {
                if (value.Equals(""))
                    fields["delimiter"] = "\t";
                else
                    fields["delimiter"] = value;
            }
        }

        public List<string> supervisorRecipientsList { get; set; }

        [XmlElement("date")] public string date { get; set; }

        [XmlElement("mailServer")] public string mailServer { get; set; }

        [XmlElement("mailPort")] public string mailPort { get; set; }

        [XmlElement("mailUser")] public string mailUser { get; set; }

        [XmlElement("mailPassword")] public string mailPassword { get; set; }


        private AllConfig()
        {
        }

        public static AllConfig GetInstance()
        {
            try
            {
                return _instance ?? (_instance = _xmlReader.GetObject("config.xml"));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Проблемы с чтением файла config.xml");
                Console.WriteLine($"Исключение: {ex.Message}");
                Console.WriteLine($"Метод: {ex.TargetSite}");
                Console.WriteLine($"Трассировка стека: {ex.StackTrace}");
                Environment.Exit(1);
            }

            return _instance;
        }

        private void ReadConfig()
        {
            _xmlReader.GetObject("config.xml");
        }
    }
}