using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LaboratoryService_Api.Models
{
    [Table("LaboratorioRegistro")]
    public class LaboratorioRegistro
    {
        public int LaboratorioRegistroID { get; set; }

        public bool Urgente { get; set; }

        public int? TurnoID { get; set; }
        [ForeignKey("TurnoID")]
        public virtual Turnos Turnos { get; set; }
        public int? InternacionID { get; set; }
        [ForeignKey("InternacionID")]
        public virtual Internaciones Internaciones { get; set; }

        public int? GuardiaRegistroID { get; set; }

        public int PracticasOrigenID { get; set; }

        public int PracticasEstadoID { get; set; }

        public int NumeroIdentificador { get; set; }

        public int PacienteID { get; set; }
        [ForeignKey("PacienteID")]
        public virtual Pacientes Pacientes { get; set; }


        public int? PrestadorSolicita { get; set; }
        [ForeignKey("PrestadorSolicita")]
        public virtual Prestadores Prestadores { get; set; }

        public int? PrestadorRealiza { get; set; }
        public int? TurnoReferenciaID { get; set; }

        public DateTime Fecha { get; set; }

        public bool Anulado { get; set; }

        [StringLength(300)]
        public string MotivoModificado { get; set; }

        public DateTime? FechaCrea { get; set; }

        [StringLength(11)]
        public string UsuarioCrea { get; set; }
        public int? HabitacionID { get; set; }
        public int? GuardiaSectorID { get; set; }

        public int? InstitucionID { get; set; }
        [ForeignKey("InstitucionID")]
        public virtual Instituciones Instituciones { get; set; }

        public int? ServicioID { get; set; }
        public bool? Emergente { get; set; }
        public bool? Validado { get; set; }
        public string CodigoBarra { get; set; }


        // Relación con LabRegistroDetalle ya que no hay una FK explícita
        public ICollection<LabRegistroDetalle> LabRegistroDetalle { get; set; }


    }
}
