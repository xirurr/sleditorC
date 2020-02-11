using System;

namespace ConsoleApplication1.DTO
{
    public class CiceroneSessionsModel
    {
        public long distributorId { get; set; }
        public DateTime firstSync { get; set; }
        public DateTime lastSync { get; set; }
    }
}