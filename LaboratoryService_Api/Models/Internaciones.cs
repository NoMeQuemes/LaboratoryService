using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LaboratoryService_Api.Models
{
    public class Internaciones
    {
        [Key]
        public int InternacionID { get; set; }

        [Display(Name = "Paciente")]
        [UIHint("Pacientes")]
        public int PacienteID { get; set; }


        [Display(Name = "Habitaci¾n")]
        public int HabitacionID { get; set; }
        [ForeignKey("HabitacionID")]
        public virtual Habitaciones_Hospital Habitaciones_Hospital { get; set; }

        [Display(Name = "Cama")]
        public int CamaID { get; set; }

        [Display(Name = "Obra Social")]
        [UIHint("DropDownList")]
        public int ObraSocialID { get; set; }

        public bool Hijo { get; set; }

        [Display(Name = "Nombre del hijo")]
        [StringLength(50)]
        public string HijoNombre { get; set; }

        [Display(Name = "Familiar referente")]
        public int? PacienteReferenciaID { get; set; }

        [Display(Name = "Familiar referente")]
        [StringLength(50)]
        public string Familiar_Referente { get; set; }

        [Display(Name = "TelÚfono referente")]
        [StringLength(50)]
        public string Telefono_Referente { get; set; }

        [Required]
        [Display(Name = "Fecha de Ingreso")]
        [Column(TypeName = "date")]
        [UIHint("Fecha")]
        public DateTime Fecha_ingreso { get; set; }

        [Required]
        [StringLength(4)]
        [UIHint("Hora")]
        [Display(Name = "Hora de Ingreso")]
        public string Hora_Ingreso { get; set; }

        [Required]
        [Display(Name = "MÚdico de Ingreso")]
        [UIHint("DropDownList")]
        public int PrestadorIngresoID { get; set; }

        [Required]
        [Display(Name = "Motivo de Ingreso")]
        [UIHint("DropDownList")]
        public int MotivoIngresoID { get; set; }

        [Display(Name = "Fecha Alta")]
        [Column(TypeName = "date")]
        [UIHint("Fecha")]
        public DateTime? Fecha_Alta { get; set; }

        [StringLength(4)]
        [UIHint("Hora")]
        [Display(Name = "Hora Alta")]
        public string Hora_alta { get; set; }

        [Display(Name = "MÚdico Alta")]
        [UIHint("DropDownList")]
        public int? PrestadorAltaID { get; set; }

        [Display(Name = "Motivo de Alta")]
        [UIHint("DropDownList")]
        public int? TipoAltaID { get; set; }

        [StringLength(100)]
        [Display(Name = "Derivado a")]
        public string Derivado_A { get; set; }

        [StringLength(100)]
        [Display(Name = "Ocupaci¾n")]
        public string Ocupacion_Habitual { get; set; }

        [StringLength(100)]
        public string Observaciones { get; set; }

        public bool Anulado { get; set; }


        [StringLength(11)]
        public string UsuarioCrea { get; set; }

        public DateTime? FechaCrea { get; set; }

        [StringLength(11)]
        public string UsuarioAlta { get; set; }
        public DateTime? FechaRegistraAlta { get; set; }

        public int? NomExpedienteID { get; set; }

        public int? InternacionReferenciaID { get; set; }

        public bool? CompartenCama { get; set; }

        [StringLength(11)]
        public string UsuarioMod { get; set; }

        public DateTime? FechaMod { get; set; }

        public int? InternacionIDMod { get; set; }

        [StringLength(100)]
        public string OtroMedioTraslado { get; set; }
        public int? CentroID { get; set; }

        public int? MovilID { get; set; }
        public string Motivo { get; set; }
        [StringLength(100)]
        public string OtroCentro { get; set; }

        public string EstudiosComp { get; set; }
        public string MedicacionSum { get; set; }
        public string Antecedentes { get; set; }
        public int? InstitucionID { get; set; }
        public int? IDInternacionHC { get; set; }
        public bool? EstadoAfiliado { get; set; }
        public bool? Upcn { get; set; }
        public string NumeroFamiliar { get; set; }

        public int? IdInternadoIosep { get; set; }

        public bool? Particular { get; set; }
        public bool? Consentimiento { get; set; }


        public int? DiagnosticoID { get; set; }

        public bool? CargaAcompanante { get; set; }

        public DateTime? FechaAnulaIosep { get; set; }
        public DateTime? Fecha_Alta_Admin { get; set; }

        public DateTime? Fecha_Forza { get; set; }
        public string Usuario_Forza { get; set; }
    }
}
