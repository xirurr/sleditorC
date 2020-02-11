using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using ConsoleApplication1.DataAccess;
using ConsoleApplication1.DataAccess.FileCreators;
using ConsoleApplication1.DTO;
using ConsoleApplication1.Services;
using Microsoft.Xrm.Sdk.Metadata;

namespace ConsoleApplication1.Base
{
    public class ProjectController
    {
        private DbDataContext context;
        private AllConfig instance = AllConfig.GetInstance();
        private ProjectConfig projectConfig { get; set; }
        private List<Statistic> statisticList { get; set; } = new List<Statistic>();
        private CSVCreator csvCreator;
        private ExcelCreator excelCreator;

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
            GetR4000Data();
            GetCiceroneData();
            csvCreator = new CSVCreator(projectConfig.dataBase);
            excelCreator = new ExcelCreator(projectConfig.dataBase);
            csvCreator.CreatCSV(statisticList);
            excelCreator.CreateExcel(statisticList);
            SendMail();
            return statisticList;
        }

        private void GetR4000Data()
        {
            Console.WriteLine("получение данных по протоколу R4000 для " + projectConfig.dataBase);
            List<Statistic> tmpListCicerone = new List<Statistic>();
            var query = context.Database.SqlQuery<StatisticModel>(
                "select MAX(ChangeDate) ChangeDate, idRecord\n" +
                "into #cd1\n" +
                "from v_LogDataChange \n" +
                "where tableName = 'refDistributorsExt' \n" +
                "and idRecord in (select id from refDistributors) and FieldName = 'usereplicator4000'\n" +
                "group by idRecord;\n" +
                "select eal.TenantId, MIN(LastSync) firstSync, MAX(LastSync) lastSync\n" +
                "into #statistc\n" +
                "from hub.ExchangeAuditLog eal\n" +
                "where Date >=\'" + instance.date + "\' \n" +
                "group by TenantId;\n" +
                "with st as(\n" +
                "select ChangeDate, idRecord, NewValue useReplicator4000Log from v_LogDataChange where (ChangeDate in (select ChangeDate from #cd1) and idRecord in (select idRecord from #cd1))\n" +
                "),\n" +
                "finalStat as \n" +
                "(select rde.UseReplicator4000 useReplicator4000, rde.id idDistr,st.ChangeDate ChangeDate, st.idRecord, st.useReplicator4000Log useReplicator4000Log from refDistributorsExt  rde\n" +
                "left join st on st.idRecord = rde.id\n" +
                ")\n" +
                "select rd.NodeID statisticNodeId, rd.id statistcDistr,  rd.Name statisticName, firstSync, lastSync, fs.ChangeDate dateOfChange, CAST (fs.useReplicator4000 as VARCHAR) useReplicator4000, fs.useReplicator4000Log from #statistc st\n" +
                "join refDistributors rd on rd.NodeID = st.TenantId\n" +
                "join finalStat fs on fs.idDistr = rd.id \n" +
                "drop table #cd1,#statistc"
            );


            foreach (var statisticModel in query)
            {
                var statistic = new Statistic();
                if (!statisticModel.useReplicator4000.Equals(statisticModel.useReplicator4000Log))
                {
                    statistic.dateOfChange = null;
                }

                if (String.IsNullOrWhiteSpace(statisticModel.useReplicator4000) ||
                    statisticModel.useReplicator4000.Equals("0"))
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
                /*statistic.dateOfChange = DateTime.ParseExact(statisticModel.dateOfChange, "yyyy-MM-dd HH:mm:ss",
                    CultureInfo.InvariantCulture);
                statistic.firstSession = DateTime.ParseExact(statisticModel.firstSync, "yyyy-MM-dd HH:mm:ss",
                    CultureInfo.InvariantCulture);
                statistic.lastSession = DateTime.ParseExact(statisticModel.lastSync, "yyyy-MM-dd HH:mm:ss",
                    CultureInfo.InvariantCulture);*/
                statistic.protocol = Protocol.R4000;
                statisticList.Add(statistic);
            }

            if (statisticList.Count == 0)
            {
                Console.WriteLine(projectConfig.dataBase + " не использовал R4000 в этом периоде");
            }
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
                if (e.Message.Contains("Invalid object name \'cicerone.Distributors\'"))
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
            Console.WriteLine("CombiningR4000AndCiceroneData");
            foreach (var tmpStatistic in tmpListCicerone)
            {
                foreach (var statistic in statisticList.Where(statistic =>
                    statistic.distrId.Equals(tmpStatistic.distrId)))
                {
                    if (tmpStatistic.status.ToLower().Equals("Enabled") &&
                        tmpStatistic.status.ToLower().Equals("disabled"))
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
            return tmpList.Where(x => !x.deletedMark).ToList();
        }


        private void SendMail()
        {
            var smtpEmailService = new SmtpEmailService(instance.mailServer, int.Parse(instance.mailPort),
                instance.mailUser,
                instance.mailPassword);
            const string outputDateFormat = "MMM yyyy";
            DateTime tmpDate = DateTime.ParseExact(instance.date, "yyyy-MM-dd HH:mm:ss",
                CultureInfo.InvariantCulture);

            string fileBaseName = projectConfig.dataBase + " " + tmpDate.ToString(outputDateFormat);

            List<Attachment> attachments = new List<Attachment>();
            var csvAttachment = new Attachment(csvCreator.fileInfo.ToString());
            var excelAttachment = new Attachment(excelCreator.fileInfo.ToString());
            csvAttachment.Name = fileBaseName + ".csv";
            excelAttachment.Name = fileBaseName + ".xls";

            attachments.Add(csvAttachment);
            attachments.Add(excelAttachment);


            smtpEmailService.Send("статистика использования по " + projectConfig.dataBase, "отчет в приложении",
                instance.mailUser, projectConfig.recipientsForOneList.ToArray(), null, null, attachments);
        }
    }
}