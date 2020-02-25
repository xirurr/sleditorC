using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using ConsoleApplication1.Base;
using ConsoleApplication1.DataAccess;
using ConsoleApplication1.DataAccess.FileReaders;
using ConsoleApplication1.DTO;
using ConsoleApplication1.Services;
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
            Console.WriteLine("project are ready");

            var projectControllers = new List<ProjectController>();
            projects.ForEach(o => { projectControllers.Add(new ProjectController(o)); });
            Parallel.ForEach(projectControllers, current => current.getStatistics());

            Send(projectControllers);
        }


        private void Send(IEnumerable<ProjectController> controllers)
        {
            var config = AllConfig.GetInstance();
            if (config.supervisorRecipientsList.Count <= 1)
            {
                if (String.IsNullOrWhiteSpace(config.supervisorRecipientsList[0]))
                {
                    Console.WriteLine("Отсутвует список рассылки супервайзера");
                    return;
                }
            }

            var outputDateFormat = "MMM yyyy";

            DateTime tmpDate = DateTime.ParseExact(config.date, "yyyyMMdd",
                CultureInfo.InvariantCulture);

            string fileBaseName = "статистика использования R4000 за " + tmpDate.ToString(outputDateFormat);

            List<Attachment> attachments = new List<Attachment>();
            foreach (var controller in controllers)
            {
                if (controller.excelAttachment != null)
                {
                    attachments.Add(controller.excelAttachment);
                }
            }

            var smtpEmailService = new SmtpEmailService(config.mailServer, int.Parse(config.mailPort),
                config.mailUser, config.mailPassword);

            smtpEmailService.Send(fileBaseName, "отчет в приложении",
                config.mailUser, config.supervisorRecipientsList.ToArray(), new string[0], new string[0],
                attachments);
        }
    }
}