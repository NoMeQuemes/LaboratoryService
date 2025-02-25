using System.ComponentModel.DataAnnotations.Schema;

namespace LaboratoryService_Api.Models
{

    [Table("LabRegistroDetalle")]
    public class LabRegistroDetalle
    {
        public int LabRegistroDetalleID { get; set; }

        public int LaboratorioRegistroID { get; set; }
        [ForeignKey("LaboratorioRegistroID")]
        public virtual LaboratorioRegistro LaboratorioRegistro { get; set; }

        public int? LabGrupoPracticaID { get; set; }


        public int LaboratorioPracticasID { get; set; }
        [ForeignKey("LaboratorioPracticasID")]
        public virtual LaboratorioPracticas LaboratorioPracticas { get; set; }


        public string Resultado { get; set; }

        public string CodigoTubo { get; set; }

        public string UsuarioCargaCodigoTubo { get; set; }

        public DateTime? FechaCargaCodigoTubo { get; set; }

        public int? OrdenImprimir { get; set; }

        public bool Anulado { get; set; }

        public string UsuarioCarga { get; set; }

        public int? PrestadorID { get; set; }

        public DateTime? FechaCarga { get; set; }

        public bool NoSolicitado { get; set; }

        public bool? Imprimir { get; set; }

        public bool? Impreso { get; set; }

        //para identificar los pedidos de practicas cuando hay grupos dentro de grupos
        public int? LabGrupoPralPracticaID { get; set; }
        public bool? Confirmado { get; set; }

    }
}
