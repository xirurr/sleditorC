using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConsoleApplication1.DTO
{
    [Table("Distributors", Schema = "cicerone")]
    public class CiceroneDistributorsDto
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] 
        public long distributorId { get; set; }
        
        [Column("Protocol")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] 
        public long protocol { get; set; }
    }
}