using System;
using System.IO;

namespace ConsoleApplication1.DataAccess.FileCreators
{
    public abstract class AbstractCreator
    {
        public string finalPath;
        public AbstractCreator() {
        }

        public AbstractCreator(String dbName) {
            String path = Directory.GetCurrentDirectory();
            finalPath = path + "\\" + dbName + "\\";
        }
    }
}