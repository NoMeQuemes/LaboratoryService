using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LaboratoryService_Api.Models
{
    public class Turnos
    {
        [Key]
        public int TurnoID { get; set; }

        public int PacienteID { get; set; }

        public int ServicioID { get; set; }

        public int ConsultorioID { get; set; }

        public int PrestadorID { get; set; }

        [Column(TypeName = "smalldatetime")]
        public DateTime Fecha_Hora { get; set; }

        [Required]
        [StringLength(4)]
        public string Hora_Hasta { get; set; }

        [StringLength(2)]
        public string Orden { get; set; }


        public DateTime? Llegada { get; set; }

        [Column(TypeName = "smalldatetime")]
        public DateTime? Llamado { get; set; }

        [Column(TypeName = "smalldatetime")]
        public DateTime? Atendido { get; set; }

        [Column(TypeName = "smalldatetime")]
        public DateTime? NoAtendido { get; set; }

        [StringLength(4)]
        public string TerminaAtencion { get; set; }

        [StringLength(4)]
        public string ComienzaAtencion { get; set; }

        public int? ConsultorioVisorID { get; set; }

        public bool? Primeravez { get; set; }

        public int? AnterirorID { get; set; }

        public int? ObraSocialID { get; set; }

        public bool? Emergencia { get; set; }

        public short? EmergenciaID { get; set; }

        public bool Admisionado { get; set; }
        public string OpCrea { get; set; }

        public string OpModifica { get; set; }

        public short? Edad { get; set; }

        public bool? Inyectable { get; set; }
        public int? TipoCuraciones { get; set; }

        public DateTime? FechaCrea { get; set; }
        public DateTime? FechaModifica { get; set; }
        public DateTime? FecModifica { get; set; }
        public bool Anulado { get; set; }
        public int? Curaciones { get; set; }
        public int? ShockRoom { get; set; }
        public int? NomExpedienteID { get; set; }
        public int? IdTurnoConsulta { get; set; }
        public int? InstitucionID { get; set; }
        public bool? TeleSalud { get; set; }
        public int? TipoTeleSaludID { get; set; }
        public bool? Replanificado { get; set; }
        public int? BonoID { get; set; }
        public bool? TurnoProtegido { get; set; }
        public DateTime? BloqueoTAT { get; set; }
        public DateTime? DatosCompletos { get; set; }
        public DateTime? DesbloqueoTAT { get; set; }
        [StringLength(11)]
        public string UserDesbloqueaTAT { get; set; }
        public int? LlamaAdmision { get; set; }
        [Column(TypeName = "smalldatetime")]
        public DateTime? LlamadoAdmision { get; set; }
        public int? ConsultorioIDAdmision { get; set; }
    }
}
