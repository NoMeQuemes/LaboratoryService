using System.ComponentModel.DataAnnotations;

namespace LaboratoryService_Api.Models
{
    public class Habitaciones_Hospital
    {
        [Key]
        public int HabitacionID { get; set; }

        [Required]
        [StringLength(30)]
        public string Nombre { get; set; }

        public int SectorID { get; set; }

        public int PisoId { get; set; }

        public bool Anulado { get; set; }

        public bool? MostrarHoraServicio { get; set; }

        public bool? MostrarMedidasGenerales { get; set; }

        public string InicioServicio { get; set; }
        public bool? MostrarIndicacionesRetiradas { get; set; }

        public bool AdmiteCuna { get; set; }
        public int? InstitucionID { get; set; }
        public int? TipoInternacionID { get; set; }
        public int? HabitacionIDHC { get; set; }
        public int? SectorIDHC { get; set; }
        public bool? Informa { get; set; }
        public bool? PedidoUrgente { get; set; }
        public int? SectorIosepID { get; set; }
        //  public string ColorSala { get; set; }

        public int? ColorID { get; set; }

        [StringLength(50)]
        public string ColorSala { get; set; }

        public bool? PermiteReserva { get; set; }

        public bool? EsNeo { get; set; }
    }
}
