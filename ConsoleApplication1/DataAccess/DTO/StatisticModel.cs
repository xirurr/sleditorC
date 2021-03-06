﻿using System;
using System.Globalization;
using ConsoleApplication1.Exceptions;

namespace ConsoleApplication1.DTO
{
    public class StatisticModel

    {
        public string Protocol { get; set; }
        public long? statistcDistr { get; set; }
        public string statisticName { get; set; }
        public short? statisticNodeId { get; set; }
        
        public long? enabledDistr { get; set; }
        public string enabledName { get; set; }
        public short? enabledNodeId { get; set; }


        public DateTime? dateOfChange { get; set; }
        public DateTime? lastSync { get; set; }
        public DateTime? firstSync { get; set; }
        
        public string status { get; set; }
      
    
        public string useReplicator4000 { get; set; }
        public string useReplicator4000Log { get; set; }
    }
}