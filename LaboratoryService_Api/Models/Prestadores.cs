using System.ComponentModel.DataAnnotations;

namespace LaboratoryService_Api.Models
{
    public class Prestadores
    {
        [Key]
        public int PrestadorID { get; set; }

        [Required]
        [StringLength(10)]
        public string Matricula { get; set; }
        [Required]
        [StringLength(10)]
        public string Documento { get; set; }

        [Required]
        [StringLength(50)]
        public string Nombre { get; set; }

        public int EspecialidadID { get; set; }

        [StringLength(50)]
        public string Telefono { get; set; }

        [StringLength(50)]
        public string Email { get; set; }

        public bool Anulado { get; set; }

        public bool? Guardia { get; set; }

        public bool? Ambulatorio { get; set; }

        public bool? Internacion { get; set; }

        public bool? TiempoAtencionReal { get; set; }

        public int? TipoGuardiaID { get; set; }
        [StringLength(11)]
        public string UsuarioCarga { get; set; }
        [StringLength(11)]
        public string UsuarioMod { get; set; }
        [StringLength(11)]
        public string UsuarioBaja { get; set; }

        public DateTime? UltimaMod { get; set; }

        public bool EsPrestadorImagen { get; set; }

        public int? InstitucionID { get; set; }

        public int? UsuarioID { get; set; }
        public string Usuario { get; set; }
        public int? PrestadorIDHC { get; set; }
        public string Cuil { get; set; }
        public bool? IOSEPddjj { get; set; }
        public DateTime? Vencimientoddjj { get; set; }
        public int? idIOSEPddjj { get; set; }
    }
}
