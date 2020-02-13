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
        public string filePath { get; private set; }
        public ExcelCreator(string dbName) : base(dbName)
        {
        }

        public void CreateExcel(List<Statistic> statisticList)
        {
            FileInfo fileInfo = new FileInfo(finalDir+"statistic.xls");
            var workbook = new HSSFWorkbook();
            var sheet = workbook.CreateSheet("DistributorStatistic");
            var rowhead = sheet.CreateRow((short) rownum++);
            if (statisticList.Count > 0)
            {
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
              
            }
            else
            {
                rowhead.CreateCell(0).SetCellValue("Отсутвуют данные за период");

            }
            var fileStream = fileInfo.Create();
            workbook.Write(fileStream);
            fileStream.Close();
            filePath = fileInfo.ToString();
        }
    }
}