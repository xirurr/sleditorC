﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using ConsoleApplication1.Base;
using ConsoleApplication1.DTO;
using ConsoleApplication1.Services;
using Jitbit.Utils;

namespace ConsoleApplication1.DataAccess.FileCreators
{
    public class CSVCreator : AbstractCreator
    {
       public string filePath { get; private set; }

        public CSVCreator(string dbName) : base(dbName)
        {
        }

        public void CreatCSV(List<Statistic> statisticList)
        {
            var allConfig = AllConfig.GetInstance();
            var myExport = new CsvExport(allConfig.delimiter,true);
            foreach (var statistic in statisticList)
            {
                myExport.AddRow();
                var listElements = statistic.listElements();
                foreach (var listElementsKey in listElements.Keys)
                {
                    myExport[listElementsKey] = listElements[listElementsKey];
                }
            }

           
            
            filePath = Path.Combine(finalDir, "statistic.csv");
            string dir = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            
            
            var data = Encoding.GetEncoding(1251).GetBytes(myExport.Export());
            var array = Encoding.GetEncoding(1251).GetPreamble().Concat(data).ToArray();
            
            File.WriteAllBytes(filePath,array);
            
            
            
        }
    }
}