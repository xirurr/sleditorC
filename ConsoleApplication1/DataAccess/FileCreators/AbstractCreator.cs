using System;
using System.IO;

namespace ConsoleApplication1.DataAccess.FileCreators
{
    public abstract class AbstractCreator
    {
        public string finalDir;
        public AbstractCreator() {
        }

        public AbstractCreator(String dbName) {
            String path = Directory.GetCurrentDirectory();
            finalDir = path + "\\" + dbName + "\\";
        }
    }
}