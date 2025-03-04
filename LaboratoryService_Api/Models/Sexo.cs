using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LaboratoryService_Api.Models
{
    public class Sexo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public byte IdSexo { get; set; }

        [Required]
        [StringLength(30)]
        public string Nombre { get; set; }

        public bool Anulado { get; set; }

        [StringLength(1)]
        public string Abreviatura { get; set; }
    }
}
