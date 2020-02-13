using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using ConsoleApplication1.DTO;

namespace ConsoleApplication1.DataAccess
{
    public class DbDataContext :DbContext
    {
        public DbDataContext(string connectionString) : base(connectionString)
        {
            if (Debugger.IsAttached)
                Database.Log = m => Debug.WriteLine(m);
            ((IObjectContextAdapter)this).ObjectContext.CommandTimeout = 400;
        }
       
        public DbSet<CiceroneSessionsDto> csesDto { get; set; }
        public DbSet<CiceroneDistributorsDto> cdistrDto { get; set; }
        public DbSet<RefDistributorsDto> rdistrDto { get; set; }
        
    }
}