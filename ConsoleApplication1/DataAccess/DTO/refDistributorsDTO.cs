using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConsoleApplication1.DTO
{
    [Table("refDistributors", Schema = "dbo")]
    public class RefDistributorsDto
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long distributorId { get; set; }
        
        [Column("name")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string distributorName { get; set; }
        
        [Column("nodeId")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long distributorNodeId { get; set; }
    }
}