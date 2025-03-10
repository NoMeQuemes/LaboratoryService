using System;
using LaboratoryService_Api.Models;
using System.Diagnostics;

namespace LaboratoryService_Api.Utilities
{
    public class ConvertHL7
    {
        public static string ConvertToHL7(LaboratorioRegistro registro, List<LabGrupoPractica> gruposPracticas)
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
                string SPM2 = "SPM|2|A61906H|A61906H|SGRE EDTA\x0D";

                string hl7Message = $"{MSH}{PID}{PV1}{SPM1}{NTE}{SPM2}";

                var detallesPedido = registro.LabRegistroDetalle.ToList();
                var gruposEnPedido = detallesPedido
                    .Where(d => d.LabGrupoPracticaID.HasValue && d.LabGrupoPracticaID.Value > 0)
                    .Select(d => d.LabGrupoPracticaID.Value)
                    .Distinct()
                    .ToList();

                if (gruposEnPedido.Any())
                {
                    foreach (var grupoId in gruposEnPedido)
                    {
                        var practicasDelGrupo = gruposPracticas
                            .Where(gp => gp.LaboratorioPracticasIDGrupo == grupoId)
                            .Select(gp => gp.LaboratorioPracticasID)
                            .Distinct()
                            .ToList();

                        if (practicasDelGrupo.Any())
                        {
                            string SPM = $"SPM|1|{grupoId}||SUERO\x0D";
                            hl7Message += SPM;

                            foreach (var practicaId in practicasDelGrupo)
                            {
                                string ORC = $"ORC|NW||{grupoId}|||||||||||||||||||\x0D";
                                string OBR = $"OBR|1|||{practicaId}||||||||||{fechaActual}||||||||||LACHYBS|||^^^{fechaActual}^^R\x0D";
                                hl7Message += $"{ORC}{OBR}";
                            }
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

                    if (practicasIndividuales.Any())
                    {
                        string SPM = $"SPM|1|SIN_GRUPO||SUERO\x0D";
                        hl7Message += SPM;

                        foreach (var practicaId in practicasIndividuales)
                        {
                            string ORC = $"ORC|NW||{practicaId}|||||||||||||||||||\x0D";
                            string OBR = $"OBR|1|||{practicaId}||||||||||{fechaActual}||||||||||LACHYBS|||^^^{fechaActual}^^R\x0D";
                            hl7Message += $"{ORC}{OBR}";
                        }
                    }
                }


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