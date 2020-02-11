using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ConsoleApplication1.Base;
using ConsoleApplication1.DTO;
using NPOI.HSSF.UserModel;

namespace ConsoleApplication1.DataAccess.FileCreators
{
    public class ExcelCreator : AbstractCreator

    {
        int rownum = 0;
        public FileInfo fileInfo { get; }
       
        public ExcelCreator(string dbName) : base(dbName)
        {
        }

        public void CreateExcel(List<Statistic> statisticList)
        {
            var fileInfo = new FileInfo(finalPath+"statistic.csv");
            var workbook = new HSSFWorkbook();
            var sheet = workbook.CreateSheet("DistributorStatistic");
            var rowhead = sheet.CreateRow((short) rownum++);
            List<string> characteristics = new List<string>(statisticList[0].listElements().Keys);
            for (int i = 0; i < characteristics.Count; i++)
            {
                rowhead.CreateCell(i).SetCellValue(characteristics[i]);
            }
            foreach (var statistic in statisticList)
            {
                var row = sheet.CreateRow((short) rownum++);
                List<string> listOfElements = new List<string>(statistic.listElements().Values);

                for (int i = 0; i < listOfElements.Count; i++)
                {
                    row.CreateCell(i).SetCellValue(listOfElements[i]);
                }
            }
            var fileStream = fileInfo.Create();
            workbook.Write(fileStream);
        }
    }
}