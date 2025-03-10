using System.ComponentModel.DataAnnotations.Schema;

namespace LaboratoryService_Api.Models
{
    [Table("LabGrupoPractica")]
    public partial class LabGrupoPractica
    {
        public int LabGrupoPracticaID { get; set; }

        public int LaboratorioPracticasIDGrupo { get; set; }

        public int LaboratorioPracticasID { get; set; }

        public int? OrdenGrupo { get; set; }

        public bool Anulado { get; set; }

        public virtual LaboratorioPracticas LaboratorioPracticasGrup { get; set; }

        public virtual LaboratorioPracticas LaboratorioPracticasPrac { get; set; }
    }
}
