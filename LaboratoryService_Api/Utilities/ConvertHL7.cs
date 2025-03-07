using System;
using LaboratoryService_Api.Models;
using System.Diagnostics;

namespace LaboratoryService_Api.Utilities
{
    public class ConvertHL7
    {
        public static string ConvertToHL7(LaboratorioRegistro registro)
        {
            try
            {
                // Obtener fecha actual en el formato requerido
                string fechaActual = DateTime.Now.ToString("yyyyMMddHHmmss");

                // Datos del paciente
                string nombrePaciente = $"{registro.Pacientes.Apellido.Trim()}^{registro.Pacientes.Nombre.Trim()}";
                string documentoPaciente = registro.Pacientes.Documento.Trim();
                string fechaNacimiento = registro.Pacientes.FechadeNacimiento?.ToString("yyyyMMdd") ?? "";
                string sexoPaciente = registro.Pacientes.IdSexo == 1 ? "M" : (registro.Pacientes.IdSexo == 2 ? "F" : "O");
                string direccionPaciente = registro.Pacientes.Residencia_Localidad != null ? registro.Pacientes.Residencia_Localidad.Trim() : "";
                string telefonoPaciente = registro.Pacientes.Telefono ?? "|";
                string tipoPaciente = registro.InternacionID != null ? "I" : "O";
                string notasPaciente = registro.Internaciones.Observaciones.Trim() ?? "";

                // Datos del prestador

                string nombrePrestador = registro.Prestadores != null? $"{registro.Prestadores.PrestadorID}^{registro.Prestadores?.Nombre.Trim()}" : "";
                string prestadorSolicita = registro.PrestadorSolicita.ToString();

                // Datos de las prácticas
                string practicas = string.Join("^", registro.LabRegistroDetalle
                    .Select(detalle => detalle.LaboratorioPracticas.LaboratorioPracticasID)
                    .Distinct());

                // Datos de los grupos de práctica
                string gruposPractica = string.Join("^", registro.LabRegistroDetalle
                    .Where(detalle => detalle.LabGrupoPracticaID.HasValue)
                    .Select(detalle => detalle.LabGrupoPracticaID.ToString())
                    .Distinct());

                // Construcción de los segmentos
                string MSH = $"MSH|^~\\&|LIS||AMS||{fechaActual}||OML^O33|{fechaActual}|P|2.5|||AL|NE\x0D";
                string PID = $"PID|1||{documentoPaciente}||{nombrePaciente}||{fechaNacimiento}|{sexoPaciente}|||{direccionPaciente}||||{telefonoPaciente}|\x0D";
                if(registro.Internaciones.Observaciones != null)
                {
                    string NTEPID = $"NTE|1|L1|{notasPaciente}\x0D";
                }
                string PV1 = $"PV1|1|{tipoPaciente}|||{prestadorSolicita}|||{nombrePrestador}|\x0D";
                string SPM1 = $"SPM|1|{gruposPractica}||SUERO\x0D";
                string NTE = $"NTE|1|L|{registro.MotivoModificado ?? "Sin motivo"}\x0D";
                string ORC = $"ORC|NW||{gruposPractica}|||||||||||||||||||\x0D";
                string OBR = $"OBR|1|||{practicas}||||||||||{fechaActual}||||||||||LACHYBS|||^^^{fechaActual}^^R\x0D";
                string SPM2 = "SPM|2|A61906H|A61906H|SGRE EDTA\x0D";

                string hl7Message = $"{MSH}{PID}{PV1}{SPM1}{NTE}{ORC}{OBR}{SPM2}";
                string mllpMessage = $"\x0B{hl7Message}\x1C\r";

                Debug.WriteLine($"Mensaje HL7 generado: {mllpMessage}");
                return mllpMessage;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al convertir a HL7: " + ex.Message);
            }
        }
    }
}