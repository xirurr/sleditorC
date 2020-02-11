using System;
using System.Collections.Generic;
using System.IO;
using ConsoleApplication1.DTO;

namespace ConsoleApplication1.DataAccess.FileCreators
{   
    [Obsolete("for future", true)]
    public class HTMLCreator : AbstractCreator
    {
        public HTMLCreator(string dbName) : base(dbName)
        {
        }

        public void CreateHTML(List<Statistic> statisticList)
        {
            var path = Directory.GetCurrentDirectory();
            var addpath = "\\templates\\";
            path = path + addpath;            
        }
    }
}