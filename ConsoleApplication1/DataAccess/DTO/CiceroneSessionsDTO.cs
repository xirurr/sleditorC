using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConsoleApplication1.DTO
{
    [Table("Sessions", Schema = "cicerone")]
    public class CiceroneSessionsDto
    {   [Key]
        [Column("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long distributorId { get; set; }
        
        [Column("SessionCreateDate")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public DateTime sessionCreateDate { get; set; }

    }
}