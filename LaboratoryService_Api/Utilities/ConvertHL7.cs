using System;
using LaboratoryService_Api.Models;
using System.Diagnostics;
using System.Text;

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

                //Datoso de la orden
                string idMensaje = registro.LaboratorioRegistroID.ToString(); //Se usa el id del pedido cómo código univoco del mensaje

                // Datos del paciente
                string nombrePaciente = $"{registro.Pacientes.Apellido.Trim()}^{registro.Pacientes.Nombre.Trim()}";
                string documentoPaciente = registro.Pacientes.Documento.Trim();
                string fechaNacimiento = registro.Pacientes.FechadeNacimiento?.ToString("yyyyMMdd") ?? "";
                string sexoPaciente = registro.Pacientes.IdSexo == 1 ? "M" : (registro.Pacientes.IdSexo == 2 ? "F" : "O");
                string direccionPaciente = registro.Pacientes.Residencia_Localidad != null ? registro.Pacientes.Residencia_Localidad.Trim() : "";
                string telefonoPaciente = registro.Pacientes.Telefono ?? "|";
                string tipoPaciente = registro.InternacionID != null ? "I" : "O";
                string notasPaciente = "";
                if (registro.Internaciones != null )
                {
                    notasPaciente = registro.Internaciones.Observaciones.Trim() ?? "";
                }
                string fechaAdmision = "";
                if ( registro.InternacionID != null )
                {
                    fechaAdmision = registro.Internaciones.FechaCrea.ToString();
                }else if( registro.TurnoID != null )
                {
                    fechaAdmision = registro.Turnos.FechaCrea.ToString();
                }

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
                string MSH = $"MSH|^~\\&|LIS||AMS||{fechaActual}||OML^O33|{idMensaje}|P|2.5|||AL|NE\x0D";
                string PID = $"PID|1||{documentoPaciente}||{nombrePaciente}||{fechaNacimiento}|{sexoPaciente}|||{direccionPaciente}||||{telefonoPaciente}|\x0D";
                string NTEPID = $"NTE|1|L|{notasPaciente}\x0D";
                string PV1 = $"PV1|1|||{fechaAdmision}||||{nombrePrestador}||||||||||||{tipoPaciente}\x0D";
                string SPM1 = $"SPM|1|{gruposPractica}||SUERO\x0D";
                string ORC = $"ORC|NW||{gruposPractica}|||||||||||||||||||\x0D";
                string OBR = $"OBR|1|||{practicas}||||||||||{fechaActual}||||||||||LACHYBS|||^^^{fechaActual}^^R\x0D";

                //Se unen todos los segmentos en un solo mensaje
                var messageBuilder = new StringBuilder();
                messageBuilder.Append(MSH)
                              .Append(PID);
                if(registro.Internaciones != null && registro.Internaciones.Observaciones != null)
                {
                    messageBuilder.Append(NTEPID);
                }
                messageBuilder.Append(PV1)
                              .Append(SPM1)
                              .Append(ORC)
                              .Append(OBR);

                string hl7Message = messageBuilder.ToString();
                string mllpMessage = $"\x0B{hl7Message}\x1C\r";
                return mllpMessage;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al convertir a HL7: " + ex.Message);
            }
        }
    }
}