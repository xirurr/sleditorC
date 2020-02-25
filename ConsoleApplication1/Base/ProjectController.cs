using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using ConsoleApplication1.DataAccess;
using ConsoleApplication1.DataAccess.FileCreators;
using ConsoleApplication1.DTO;
using ConsoleApplication1.Services;
using Microsoft.Xrm.Sdk.Metadata;
using NPOI.HSSF.Record;

namespace ConsoleApplication1.Base
{
    public class ProjectController
    {
        private DbDataContext context;
        private AllConfig instance = AllConfig.GetInstance();
        private ProjectConfig projectConfig { get; set; }
        private List<Statistic> statisticList { get; set; } = new List<Statistic>();
        public CSVCreator csvCreator { get; private set; }
        public ExcelCreator excelCreator { get; private set; }
        private const string outputDateFormat = "MMM yyyy";


        public Attachment csvAttachment { get; private set; }
        public Attachment excelAttachment { get; private set; }

        public ProjectController(ProjectConfig projectConfig)
        {
            this.projectConfig = projectConfig;
        }

        private void PrepareConnectionToBd()
        {
            projectConfig.CreateConnectionUrl();
            context = new DbDataContext(projectConfig.sqlBuilder.ToString());
            
        }

        public List<Statistic> getStatistics()
        {
            PrepareConnectionToBd();
            if (!GetR4000Data())
            {
                return null;
            }
            GetCiceroneData();
            csvCreator = new CSVCreator(projectConfig.dataBase);
            excelCreator = new ExcelCreator(projectConfig.dataBase);
            csvCreator.CreatCSV(statisticList);
            excelCreator.CreateExcel(statisticList);
            SendMail();
            Console.WriteLine(projectConfig.dataBase + " работа выполнена");
            return statisticList;
        }

        private bool GetR4000Data()
        {
            Console.WriteLine("получение данных по протоколу R4000 для " + projectConfig.dataBase + " за "+ DateTime.ParseExact(instance.date,"yyyyMMdd",CultureInfo.InvariantCulture));
            var query = context.Database.SqlQuery<StatisticModel>(
                "with tmpSe as (\n"+
                "select eal.TenantId, MIN(LastSync) firstSync, MAX(LastSync) lastSync \n"+
                "from hub.ExchangeAuditLog eal \n"+
                "where Date >=\'"+ instance.date+"\' \n"+
                "group by TenantId)\n"+
                
                "select t.firstSync, t.lastSync, rd.NodeID, rd.id \n"+
                "into #sessions\n"+
                "from tmpSe t\n"+
                "join refDistributors rd on t.TenantId = rd.NodeID\n"+
                
                "select rd.NodeID, rde.UseReplicator4000, rd.id distrId\n"+
                "into #current\n"+
                "from refDistributorsExt rde\n"+
                "join refDistributors rd on rd.id = rde.id\n"+
                "where rde.UseReplicator4000 = 1\n"+
                
                "select firstSync,lastSync,UseReplicator4000, COALESCE(se.NodeID,cu.NodeID ) NodeID,COALESCE(se.id, cu.distrid) DistributorId \n"+
                "into #combinedData\n"+
                "from #sessions se\n"+
                "full outer join #current cu on cu.NodeID = se.NodeID\n"+

                "select cd.firstSync, cd.lastSync,CAST (rde.UseReplicator4000 AS VARCHAR) UseReplicator4000, rd.Nodeid statisticNodeId, rd.name statisticName,COALESCE(cd.DistributorId,ld.idDistr) statistcDistr, ld.changeDate dateOfChange from #combinedData cd\n"+
                "full outer join \n"+
                "(select MAX(ChangeDate) changeDate, idRecord idDistr\n"+
                "from v_LogDataChange where TableName = 'refDistributorsExt' and FieldName = 'UseReplicator4000'\n"+
                "and ChangeDate >=\'"+ instance.date+"\' \n"+
                "and idRecord in (select id from refDistributors)\n"+
                "group by idRecord) ld on ld.idDistr = cd.DistributorId\n"+
                "join refDistributorsExt rde on rde.id = COALESCE(cd.DistributorId,ld.idDistr)\n"+
                "join refDistributors rd on rd.id = rde.id\n"+
                
                "drop table #sessions,#current, #combinedData\n"
            );
            try
            {
                foreach (var statisticModel in query)
                {
                    var statistic = new Statistic();
                    statistic.dateOfChange = statisticModel.dateOfChange;

                    if (statisticModel.useReplicator4000.Equals("0"))
                    {
                        statistic.status = "Disabled";
                    }
                    else
                    {
                        statistic.status = "Enabled";
                    }
                    statistic.nameOfDistr = statisticModel.statisticName;
                    statistic.nodeId = statisticModel.statisticNodeId;
                    statistic.distrId = statisticModel.statistcDistr;
                    statistic.dateOfChange = statisticModel.dateOfChange;
                    statistic.firstSession = statisticModel.firstSync;
                    statistic.lastSession = statisticModel.lastSync;
                    statistic.protocol = Protocol.R4000;
                    statisticList.Add(statistic);
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("0x80131904") || e.Message.Contains("Login failed."))
                {
                    Console.WriteLine("Ошибка логина " + projectConfig.server);
                    return false;
                }
                Console.WriteLine(e.Message);
                throw;
            }
            if (statisticList.Count == 0)
            {
                Console.WriteLine(projectConfig.dataBase + " отсутствуют данные по R4000");
            }
            return true;
        }

        private void GetCiceroneData()
        {
            List<Statistic> tmpListCicerone = new List<Statistic>();
            Console.WriteLine("получение данных по протоколу Cicerone " + projectConfig.dataBase);
            var query = context.Database.SqlQuery<StatisticModel>(
                "select st.DistributorId statistcDistr, cd.id enabledDistr, rd.name enabledName, rd.NodeID enabledNodeID, rd2.name statisticName, rd2.NodeID statisticNodeID, firstSync,lastSync,Protocol from cicerone.Distributors cd \n" +
                "full outer join ( \n" +
                "select DistributorId, MIN(SessionCreateDate) firstSync, MAX(SessionCreateDate) lastSync \n" +
                "from cicerone.Sessions \n" +
                "where SessionCreateDate >=\'" + instance.date + "\' \n" +
                "group by DistributorId)  st on st.DistributorId = cd.id \n" +
                "left join refDistributors rd on cd.id = rd.id \n" +
                "left join refDistributors rd2 on st.DistributorId = rd2.id;"
            );


            try
            {
                foreach (var statisticModel in query)
                {
                    var statistic = new Statistic();
                    statistic.nameOfDistr = statisticModel.enabledName ?? statisticModel.statisticName;
                    statistic.nodeId = statisticModel.enabledNodeId ?? statisticModel.statisticNodeId;
                    statistic.distrId = statisticModel.enabledDistr ?? statisticModel.statistcDistr;

                    if (string.IsNullOrWhiteSpace(statisticModel.Protocol))
                    {
                        statistic.status = "Disabled";
                    }
                    else statistic.status = "Enabled";
                    
                    if (!string.IsNullOrWhiteSpace(statisticModel.firstSync.ToString()))
                    {
                        statistic.firstSession = statisticModel.firstSync;
                        statistic.lastSession = statisticModel.lastSync;
                    }

                    statistic.protocol = Protocol.Cicerone;
                    tmpListCicerone.Add(statistic);
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("cicerone.Distributors"))
                {
                    Console.WriteLine(projectConfig.dataBase + " :отсутствуют данные по Cicerone");
                }
                else
                {
                    Console.WriteLine(e.Message);
                    throw;
                }
            }

            CombineR4000AndCiceroneData(tmpListCicerone);
            statisticList = DeleteMarkedElements(statisticList);
        }

        private void CombineR4000AndCiceroneData(List<Statistic> tmpListCicerone)
        {
            if (tmpListCicerone.Count < 1) return;
            Console.WriteLine("CombiningR4000AndCiceroneData");
            foreach (var tmpStatistic in tmpListCicerone)
            {
                foreach (var statistic in statisticList.Where(statistic =>
                    statistic.distrId.Equals(tmpStatistic.distrId)))
                {
                    if (tmpStatistic.status.ToLower().Equals("enabled") &&
                        statistic.status.ToLower().Equals("disabled"))
                    {
                        tmpStatistic.dateOfChange = statistic.dateOfChange;
                        statistic.deletedMark = true;
                    }

                    if (tmpStatistic.status.ToLower().Equals("disabled") &&
                        statistic.status.ToLower().Equals("enabled"))
                    {
                        tmpStatistic.deletedMark = true;
                    }

                    if (tmpStatistic.status.ToLower().Equals("disabled") &&
                        statistic.status.ToLower().Equals("disabled"))
                    {
                        if (tmpStatistic.firstSession > statistic.firstSession)
                        {
                            tmpStatistic.dateOfChange = statistic.dateOfChange;
                            statistic.deletedMark = true;
                        }
                        else
                        {
                            tmpStatistic.deletedMark = true;
                        }
                    }
                }
            }

            statisticList.AddRange(tmpListCicerone);
        }

        private List<Statistic> DeleteMarkedElements(List<Statistic> tmpList)
        {
            Console.WriteLine("удаление дубликатов");
            Console.WriteLine(tmpList.Count + " размер до " + projectConfig.dataBase);
            var deleteMarkedElements = tmpList.Where(x => !x.deletedMark).ToList();
            Console.WriteLine(deleteMarkedElements.Count + "размер после " + projectConfig.dataBase);
            return deleteMarkedElements;
        }


        private void SendMail()

        {
            DateTime tmpDate = DateTime.ParseExact(instance.date, "yyyyMMdd",
                CultureInfo.InvariantCulture);
            string fileBaseName = projectConfig.dataBase + " " + tmpDate.ToString(outputDateFormat);

            csvAttachment = new Attachment(csvCreator.filePath);
            excelAttachment = new Attachment(excelCreator.filePath);
            csvAttachment.Name = fileBaseName + ".csv";
            excelAttachment.Name = fileBaseName + ".xls";
            
            if (projectConfig.recipientsForOneList.Count <= 1)
            {
                if (string.IsNullOrWhiteSpace(projectConfig.recipientsForOneList[0]))
                {
                    Console.WriteLine("отсутвует список рассылки по " + projectConfig.server);
                    return;
                }
            }

            var smtpEmailService = new SmtpEmailService(instance.mailServer, int.Parse(instance.mailPort),
                instance.mailUser,
                instance.mailPassword);
            
            List<Attachment> attachments = new List<Attachment>();
            attachments.Add(csvAttachment);
            attachments.Add(excelAttachment);
            
            smtpEmailService.Send("статистика использования по " + projectConfig.dataBase, "отчет в приложении",
                instance.mailUser, projectConfig.recipientsForOneList.ToArray(), new string[0], new string[0],
                attachments);
        }
    }
}