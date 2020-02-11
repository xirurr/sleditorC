using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ConsoleApplication1.Base;
using ConsoleApplication1.DataAccess;
using ConsoleApplication1.DataAccess.FileReaders;
using ConsoleApplication1.DTO;
using DbContext = ConsoleApplication1.DataAccess.DbDataContext;

namespace ConsoleApplication1.pro
{
    public class Start
    {
        private DbDataContext context;

        public static void Main(string[] args)
        {
            var start = new Start();
            start.Exec();
        }

        public void Exec()
        {
            var allConfig = AllConfig.GetInstance();
            var projects = allConfig.ProjectConfList;
            //   projects.ForEach(o => o.CreateConnectionUrl());
            Console.WriteLine("project are ready");
            var projectConfig = projects[0];
            new ProjectController(projectConfig).getStatistics();
            //Task.Factory.StartNew(() => new ProjectController(projectConfig).getStatistics());
        }


    

        public void Nevedomaya()
        {
            /*var test1 = context.CiceroneSessions
                   .Where(p => p.SessionCreateDate >= new DateTime(2019, 01, 01))
                   .GroupBy(p => p.distributorId)
                   .Select(p => new
                   {
                       distributorId = p.Key, firstSync = p.Min(s => s.SessionCreateDate),
                       lastSync = p.Max(s => s.SessionCreateDate)
                   });*/

            /*
            var test1 = context.rdTestDbo
                .Where(p => p.dateTimeStamp >= new DateTime(2019, 01, 01))
                .GroupBy(p => p.printDocNum)
                .Select(p => new RdTestModel()
                {
                    printDocNum = p.Key, firstSync = p.Min(s => s.dateTimeStamp),
                    lastSync = p.Max(s => s.dateTimeStamp)
                }).ToList();
                */


            Console.WriteLine(10);
        }
    }
}