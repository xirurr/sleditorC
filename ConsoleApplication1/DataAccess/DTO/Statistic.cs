using System;
using System.Collections.Generic;
using System.Globalization;
using ConsoleApplication1.Exceptions;

namespace ConsoleApplication1.DTO
{
    public class Statistic

    {
        public string nameOfDistr { get; set; }
        public short? nodeId { get; set; }
        public long? distrId { get; set; }
        public DateTime? dateOfChange { get; set; }
        public DateTime? lastSession { get; set; }
        public DateTime? firstSession { get; set; }
        public string status { get; set; }
        public bool deletedMark { get; set; } = false;
        public Protocol protocol { get; set; }


        public Statistic(string nameOfDistr, string nodeId, string distrId,
            string dateOfChange, string fSessionTime, string lSessionTime, string status, Protocol r4000)
        {
            if (string.IsNullOrWhiteSpace(nameOfDistr))
            {
                this.nameOfDistr = "empty";
            }
            else
            {
                this.nameOfDistr = nameOfDistr;
            }

            if (string.IsNullOrWhiteSpace(nodeId))
            {
                throw new LackOfInformationException("nodeId cannot be empty");
            }

            this.nodeId = short.Parse(nodeId);

            if (string.IsNullOrWhiteSpace(distrId))
            {
                throw new LackOfInformationException("distrId cannot be empty");
            }

            this.distrId = long.Parse(distrId);


            if (string.IsNullOrWhiteSpace(dateOfChange))
            {
                this.dateOfChange = null;
            }
            else
            {
                this.dateOfChange =
                    DateTime.ParseExact(dateOfChange, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            }


            if (!string.IsNullOrWhiteSpace(fSessionTime))
            {
                firstSession =
                    DateTime.ParseExact(fSessionTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                lastSession =
                    DateTime.ParseExact(lSessionTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            }

            this.status = status;
            protocol = r4000;
        }


        public Statistic(string name, string distrId, string nodeId, DateTime firstSync, DateTime lastSync,
            string status, Protocol cicerone)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                nameOfDistr = "empty";
            }
            else
            {
                nameOfDistr = name;
            }

            if (string.IsNullOrWhiteSpace(distrId))
            {
                throw new LackOfInformationException("distrId cannot be empty");
            }

            this.distrId = long.Parse(distrId);

            if (string.IsNullOrWhiteSpace(nodeId))
            {
                throw new LackOfInformationException("nodeId cannot be empty");
            }

            this.nodeId = short.Parse(nodeId);

            firstSession = firstSync;
            lastSession = lastSync;
            protocol = cicerone;
            this.status = status;
        }

        public Statistic()
        {
        }
        
        
        public IDictionary<string, string> listElements() {
            IDictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("Имя дистрибьютора", nameOfDistr);
            parameters.Add("NodeId", nodeId.ToString());
            parameters.Add("ID дистрибьютора", distrId.ToString());
            parameters.Add(
                "Дата включения/отключения R4000",dateOfChange!=null? dateOfChange.ToString(): "вне периода сверки");
            parameters.Add("протокол", protocol.ToString());
            parameters.Add("состояние", status);
            parameters.Add(
                "первая сессия периода", value: firstSession != null ? firstSession.ToString() : "нет сессий за период");
            parameters.Add(
                "последняя сессия периода", value: lastSession != null ? lastSession.ToString() : "нет сессий за период");
            return parameters;
        }
    }
}