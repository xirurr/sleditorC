using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Xml.Serialization;

namespace ConsoleApplication1.Base
{
    [XmlType("project")]
    public class ProjectConfig
    {
        [XmlElement("server")] public string server { get; set; }
        [XmlElement("port")] public string port { get; set; }
        [XmlElement("dataBase")] public string dataBase { get; set; }
        [XmlElement("dataBaseUser")] public string dataBaseUser { get; set; } = "";
        [XmlElement("dataBasePassword")] public string dataBasePassword { get; set; } = "";
        public List<string> recipientsForOneList { get; set; } = new List<string>();
        [XmlElement("recipientsForOne")]
        public string recipientsForOne
        {
            get => String.Join(";", recipientsForOneList);
            set => recipientsForOneList.AddRange(value.Split(';'));
        }
        [XmlIgnoreAttribute]
        public SqlConnectionStringBuilder sqlBuilder { get; private set; } = new SqlConnectionStringBuilder();

        public void CreateConnectionUrl()
        {
            var tmpPort = 0;
            sqlBuilder.InitialCatalog = dataBase;
            sqlBuilder.IntegratedSecurity = true;
            
            if (int.TryParse(port, out tmpPort))
            {
                sqlBuilder.DataSource = server + "," + port;
            }
            else
            {
                sqlBuilder.DataSource = server;
            }

            if (!dataBaseUser.Equals("") && !dataBasePassword.Equals(""))
            {
                sqlBuilder.Password = dataBasePassword;
                sqlBuilder.UserID = dataBaseUser;
                sqlBuilder.IntegratedSecurity = false;
            }
        }
    }
}