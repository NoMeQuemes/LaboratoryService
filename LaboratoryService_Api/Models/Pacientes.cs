using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LaboratoryService_Api.Models
{
    public class Pacientes
    {
        [Key]
        [UIHint("Pacientes")]
        public int PacienteID { get; set; }

        [StringLength(11)]
        public string Cuil { get; set; }

        [Display(Name = "Tipo Documento")]
        public int IdTipoDocumento { get; set; }

        [Required]
        [StringLength(50)]
        public string Documento { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; }

        [StringLength(50)]
        public string Apellido { get; set; }

        [Display(Name = "Obra Social")]
        public int? ObraSocialID { get; set; }

        [Display(Name = "Nacionalidad")]
        public int? IdNacionalidad { get; set; }

        [Display(Name = "Sexo")]
        public byte? IdSexo { get; set; }

        [Display(Name = "Genero")]
        public byte? IdSexoGenero { get; set; }

        [Display(Name = "Estado Civil")]
        public int IdEstadoCivil { get; set; }

        [Column(TypeName = "date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Fecha de Nac.")]
        public DateTime? FechadeNacimiento { get; set; }

        [Display(Name = "Tipo Doc. Ref.")]
        public int? IdTipo_DocReferencia { get; set; }

        [StringLength(10)]
        [Display(Name = "Doc. Ref.")]
        public string Doc_Referencia { get; set; }

        [StringLength(100)]
        [Display(Name = "Residencia")]
        public string Residencia_Localidad { get; set; }

        [Display(Name = "Instrucción")]
        public int? TipoInstruccionID { get; set; }

        [Display(Name = "Laboral")]
        public int? TipoLaboralID { get; set; }

        [Display(Name = "Hijo")]
        public bool? ConDatodeMadre { get; set; }
        public string Barrio { get; set; }

        [StringLength(20)]
        public string Telefono { get; set; }

        [StringLength(20)]
        public string RNNroFicha { get; set; }

        [StringLength(11)]
        public string UsuarioCarga { get; set; }

        public DateTime? FechaCarga { get; set; }

        public DateTime? FechaConsulPuco { get; set; }

        public bool ConsultaPuco { get; set; }

        public int? PacienteIDRef { get; set; }

        public bool Anulado { get; set; }

        public bool Unificado { get; set; }

        [Display(Name = "Localidad")]
        public int? IdLocalidad { get; set; }

        [StringLength(11)]
        public string UsuarioMod { get; set; }

        public DateTime? UltimaMod { get; set; }

        public string NroAfiliado { get; set; }

        public int? TipoParentezcoID { get; set; }

        public int? TipoBeneficiarioID { get; set; }

        public string FotoDniFrente { get; set; }

        public string FotoDniDorso { get; set; }

        public bool? RecienNacidoHospital { get; set; }
        public int? PacienteIDHC { get; set; }
        public int? CProvinciaID { get; set; }
        public int? CDepartamentoID { get; set; }
        public int? CLocalidadID { get; set; }
        public string Correo { get; set; }

        public bool? VerificadoTotem { get; set; }

        public string Ejemplar { get; set; }

        public string NumTramite { get; set; }
        public string CodigoPedidosWeb { get; set; }

        public string Alias { get; set; }

        public int? IdMPI { get; set; }
        public string DatosAdicionales { get; set; }
    }
}
