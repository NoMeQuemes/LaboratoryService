using System.ComponentModel.DataAnnotations;

namespace LaboratoryService_Api.Models
{
    public class Servicios
    {
        [Key]
        public int ServicioID { get; set; }

        [Required]
        [StringLength(50)]
        public string Nombre { get; set; }

        public bool Guardia { get; set; }

        public bool Internado { get; set; }

        [Display(Name = "Especialidad")]
        public int? EspecilidadID { get; set; }

        [Display(Name = "Médico")]
        public int PrestadorID { get; set; }

        [Display(Name = "Pédido")]
        public int TipoPedidoID { get; set; }

        public bool Anulado { get; set; }
        [StringLength(11)]
        public string UsuarioCarga { get; set; }
        [StringLength(11)]
        public string UsuarioMod { get; set; }
        [StringLength(11)]
        public string UsuarioBaja { get; set; }

        public DateTime? UltimaMod { get; set; }

        [Display(Name = "Tiempo de Atención (Apertura ventana) ")]
        public bool TiempoAtencionAperturaVentana { get; set; }


        public bool SePlanificaSolo { get; set; }
        public bool Planificable { get; set; }

        public int? InstitucionID { get; set; }
        public int? TipoServicioAmbID { get; set; }
        public bool? MarcaAtendido { get; set; }

        public bool? ServAdmisiona { get; set; }

        public bool? MostrarFiltrado { get; set; }
        public string ColorFilas { get; set; }
        public bool? EsImagen { get; set; }
        public bool? PorPrestador { get; set; }
        public bool? PedidoUrgente { get; set; }

        public bool? ConfirmacionAutomatica { get; set; }
        public bool? ServicioTotem { get; set; }
        public int? SexoID { get; set; }
        public int? TipoPacienteID { get; set; }
        public int? DiasMostradosTotem { get; set; }
        public bool? AdmisionarTotem { get; set; }
        public int? TipoPracticaID { get; set; }
        public int? EdadMax { get; set; }
        public int? EdadMin { get; set; }
        public int? ProximoControlMeses { get; set; }
        public bool? Transversal { get; set; }

        public bool? MultiTurno { get; set; }
    }
}
