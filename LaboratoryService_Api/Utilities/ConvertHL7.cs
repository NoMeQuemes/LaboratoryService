using System;
using LaboratoryService_Api.Models;
using System.Diagnostics;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace LaboratoryService_Api.Utilities
{
    public class ConvertHL7
    {
        public static string ConvertToHL7(LaboratorioRegistro registro)
        {
            try
            {
                // Fecha del mensaje
                string fechaMensaje = DateTime.Now.ToString("yyyyMMddHHmmss");
                string idMensaje = registro.LaboratorioRegistroID.ToString();

                //Datoso de la orden
                string idMensaje = registro.LaboratorioRegistroID.ToString(); //Se usa el id del pedido cómo código univoco del mensaje

                // Datos del paciente
                string nombrePaciente = $"{registro.Pacientes.Apellido.Trim()}^{registro.Pacientes.Nombre.Trim()}";
                string documentoPaciente = registro.Pacientes.Documento?.Trim() ?? "";
                string fechaNacimiento = registro.Pacientes.FechadeNacimiento?.ToString("yyyyMMdd") ?? "";
                string sexoPaciente = registro.Pacientes.IdSexo == 1 ? "M" : (registro.Pacientes.IdSexo == 2 ? "F" : "O");
                string direccionPaciente = registro.Pacientes.Residencia_Localidad?.Trim() ?? "";
                string telefonoPaciente = registro.Pacientes.Telefono?.Trim() ?? "";
                string notasPaciente = registro.Internaciones?.Observaciones?.Trim() ?? "";

                // Datos de la internación
                string tipoPaciente = registro.InternacionID != null ? "1" : "0";
                string fechaAdmision = registro.InternacionID != null
                    ? registro.Internaciones.FechaCrea?.ToString("yyyyMMddHHmmss") ?? ""
                    : registro.TurnoID != null
                        ? registro.Turnos.FechaCrea?.ToString("yyyyMMddHHmmss") ?? ""
                        : "";
                string ubicacionPaciente = "CHIR"; //Esto hay que hacerlo dínamico
                string institucionPaciente = registro.Instituciones.Nombre ?? "|";
                string nombrePrestador = registro.Prestadores != null ? $"{registro.Prestadores.PrestadorID}^{registro.Prestadores?.Nombre.Trim()}" : "";
                string prestadorSolicita = registro.PrestadorSolicita.ToString();

                // Datos del pedido
                string codigoBarraPedido = registro.CodigoBarra ?? "|";
                string tipoMuestra = "SUERO";
                string codigoTubo = "123456";
                string controlSolicitud = "NW";
                string fechaPedido = registro.FechaCrea?.ToString("yyyyMMddHHmmss") ?? "|";
                string laboratorioProcesa = "LAB";
                string urgenciaPedido = Convert.ToInt32(registro.Urgente) == 1 ? "S" : "R";

                var detallesPedido = registro.LabRegistroDetalle.ToList();
                var gruposEnPedido = detallesPedido
                    .Where(d => d.LabGrupoPracticaID.HasValue && d.LabGrupoPracticaID.Value > 0)
                    .Select(d => d.LabGrupoPracticaID.Value)
                    .Distinct()
                    .ToList();

                // Construcción de los segmentos
                string MSH = $"MSH|^~\\&|LIS||Abbott||{fechaMensaje}||OML^O33|{idMensaje}|P|2.5|||AL|NE\x0D";
                string PID = $"PID|1||{documentoPaciente}||{nombrePaciente}||{fechaNacimiento}|{sexoPaciente}|||{direccionPaciente}||||||\x0D";
                string NTEPID = $"NTE|1|L|{notasPaciente}\x0D";
                string PV1 = $"PV1|1||{ubicacionPaciente}|||{fechaAdmision}|||{nombrePrestador}|||{institucionPaciente}||||||||{tipoPaciente}\x0D";
                string SPM1 = $"SPM|1|{codigoBarraPedido}|{codigoTubo}|{tipoMuestra}\x0D";

                var messageBuilder = new StringBuilder();
                messageBuilder.Append(MSH)
                              .Append(PID);
                if (registro.Internaciones != null && registro.Internaciones.Observaciones != null)
                {
                    messageBuilder.Append(NTEPID);
                }
                messageBuilder.Append(PV1)
                              .Append(SPM1);

                // Construcción de ORC y OBR
                if (gruposEnPedido.Any())
                {
                    foreach (var grupoId in gruposEnPedido)
                    {
                        var practicasDelGrupo = gruposPracticas
                            .Where(gp => gp.LaboratorioPracticasIDGrupo == grupoId)
                            .Select(gp => gp.LaboratorioPracticasID)
                            .Distinct()
                            .ToList();

                        foreach (var practicaId in practicasDelGrupo)
                        {
                            string ORC = $"ORC|{controlSolicitud}||{grupoId}|||||||||||||||||||||\x0D";
                            string OBR = $"OBR|1|||{practicaId}||||||||||{fechaPedido}||{prestadorSolicita}||||||||{laboratorioProcesa}|||^^^20241115120405^^{urgenciaPedido}\r";
                            messageBuilder.Append(ORC).Append(OBR);
                        }
                    }
                }
                else
                {
                    var practicasIndividuales = detallesPedido
                        .Where(d => d.LabGrupoPracticaID == 0)
                        .Select(d => d.LaboratorioPracticasID)
                        .Distinct()
                        .ToList();

                    foreach (var practicaId in practicasIndividuales)
                    {
                        string ORC = $"ORC|{controlSolicitud}||{practicaId}|||||||||||||||||||||\x0D";
                        string OBR = $"OBR|1|||{practicaId}||||||||||{fechaPedido}||{prestadorSolicita}||||||||{laboratorioProcesa}|||^^^20241115120405^^{urgenciaPedido}\r";
                        messageBuilder.Append(ORC).Append(OBR);
                    }
                }

                // Formatear mensaje en protocolo MLLP
                string hl7Message = messageBuilder.ToString();
                string mllpMessage = $"\x0B{hl7Message}\x1C\r";

                Debug.WriteLine($"Mensaje HL7 generado:\n{mllpMessage}");
                return mllpMessage;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al generar HL7: {ex.Message}");
                throw new Exception("Error al convertir a HL7: " + ex.Message);
            }
        }
    }
}
