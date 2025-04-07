namespace LaboratoryService_Api.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;


    [Table("LabRegistroXEstado")]
    public class LabRegistroXEstado
    {
        public int LabRegistroXEstadoID { get; set; }

        public int LaboratorioRegistroID { get; set; }
        [ForeignKey("LaboratorioRegistroID")]
        public virtual LaboratorioRegistro LaboratorioRegistro { get; set; }

        public int PracticasEstadoID { get; set; }

        public DateTime FechaHora { get; set; }

        [StringLength(11)]
        public string Usuario { get; set; }
    }
}
