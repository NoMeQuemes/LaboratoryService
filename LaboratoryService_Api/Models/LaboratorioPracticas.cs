using System.ComponentModel.DataAnnotations;

namespace LaboratoryService_Api.Models
{
    public class LaboratorioPracticas
    {
        [Key]
        public int LaboratorioPracticasID { get; set; }

        [Required]
        public string Nombre { get; set; }
        public string Codigo { get; set; }

        public int? LabTipoPracticaID { get; set; }

        public bool EsGrupo { get; set; }

        [StringLength(500)]
        public string ValorNormal { get; set; }

        [StringLength(25)]
        public string UnidadMedida { get; set; }

        [StringLength(25)]
        public string TipoValorResultado { get; set; }


        public bool? Observacion { get; set; }

        public bool Anulado { get; set; }

        public bool Ambulatorio { get; set; }

        public decimal? ValorMaximo { get; set; }
        public decimal? ValorMinimo { get; set; }
        public int? LaboratorioAgrupaSumaID { get; set; }
        public bool? InformeMensual { get; set; }
        public bool Realizandose { get; set; }
        public bool MostrarMedAmbulatorio { get; set; }
        public bool MostrarGuardia { get; set; }
        public bool MostrarInternacion { get; set; }
        public bool Habilitada { get; set; }
        public bool Privado { get; set; }
        public int? InstitucionID { get; set; }
        public int? LaboratorioMetodoID { get; set; }
        public int? LaboratorioSeccionID { get; set; }
        public int? SexoID { get; set; }
        public int? LaboratorioFormulaID { get; set; }
        public string Abreviatura { get; set; }
        public int? Vigencia { get; set; }
        public bool EsRutina { get; set; }
    }
}
