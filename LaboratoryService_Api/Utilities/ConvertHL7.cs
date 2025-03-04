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
                string nombrePaciente = $"{registro.Pacientes.Apellido}^{registro.Pacientes.Nombre}";
                string documentoPaciente = registro.Pacientes.Documento;
                string fechaNacimiento = registro.Pacientes.FechadeNacimiento?.ToString("yyyyMMdd") ?? "";
                string sexoPaciente = registro.Pacientes.IdSexo == 1 ? "M" : (registro.Pacientes.IdSexo == 2 ? "F" : "O");

                // Datos del prestador
                string nombrePrestador = registro.Prestadores?.Nombre ?? "Sin Prestador";

                // Datos de las prácticas
                string practicas = string.Join("^", registro.LabRegistroDetalle
                    .Select(detalle => detalle.LaboratorioPracticas.Nombre)
                    .Distinct());

                // Datos de los grupos de práctica
                string gruposPractica = string.Join("^", registro.LabRegistroDetalle
                    .Where(detalle => detalle.LabGrupoPracticaID.HasValue)
                    .Select(detalle => detalle.LabGrupoPracticaID.ToString())
                    .Distinct());

                // Construcción de los segmentos
                string MSH = $"MSH|^~\\&|LIS||AMS||{fechaActual}||OML^O33|{fechaActual}|P|2.5|||AL|NE<CR>";
                string PID = $"PID|1||{documentoPaciente}||{nombrePaciente}||{fechaNacimiento}|{sexoPaciente}||||<CR>";
                string PV1 = $"PV1|1||A||||{nombrePrestador}<CR>";
                string SPM1 = $"SPM|1|{gruposPractica}||SUERO<CR>";
                string NTE = $"NTE|1|L|{registro.MotivoModificado ?? "Sin motivo"}<CR>";
                string ORC = $"ORC|NW||{gruposPractica}|||||||||||||||||||<CR>";
                string OBR = $"OBR|1|||{practicas}||||||||||{fechaActual}||||||||||LACHYBS|||^^^{fechaActual}^^R<CR>";
                string SPM2 = "SPM|2|A61906H|A61906H|SGRE EDTA<CR>";

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