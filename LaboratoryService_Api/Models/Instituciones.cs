using System.ComponentModel.DataAnnotations;

namespace LaboratoryService_Api.Models
{
    public class Instituciones
    {
        [Key]
        public int InstitucionID { get; set; }

        [Required]
        [StringLength(150)]
        public string Nombre { get; set; }

        [StringLength(200)]
        public string Logo { get; set; }

        [StringLength(200)]
        public string Direccion { get; set; }

        [StringLength(50)]
        public string Telefono { get; set; }

        [StringLength(50)]
        public string Cuit { get; set; }

        [StringLength(50)]
        public string Abreviatura { get; set; }

        public bool Anulado { get; set; }
        public int? IdInstitucionHC { get; set; }
        public bool? Terminos { get; set; }
        public string LogoInforme { get; set; }
        public string NombreInforme { get; set; }
        public string Aplicacion { get; set; }
        public string CodigoRefes { get; set; }
        public string CodigoHospital { get; set; }
        public string DireccionCompleta { get; set; }
        public string DatosBanco { get; set; }
        public int? InstitucionIDIosep { get; set; }
        public bool? HPGD { get; set; }
        public bool? Clinica { get; set; }
        public bool? ValidaLaboratorio { get; set; }
        public bool? Psiquiatrico { get; set; }

        public int? IdTipoConsumirBono { get; set; }

        public int? InstitucionIDMenu { get; set; }
        public int? InstitucionIDLab { get; set; }
        public int? InstitucionIDMed { get; set; }
        public bool? PacienteProtegido { get; set; }
        public bool? AcompananteInternacion { get; set; }


        public bool? FirmaResponsableCargaLab { get; set; }
        public bool? AtencionTotem { get; set; }

        public bool? AdmisionaHasta10minAntes { get; set; }

        public bool? AutorizaPedidosWeb { get; set; }
        public bool? MuestraAislamiento { get; set; }
    }
}
