using NHapi.Model.V251.Message;
using NHapi.Model.V251.Segment;
using NHapi.Model.V251.Datatype;
using NHapi.Base.Parser;
using System;
using System.Collections.Generic;
using LaboratoryService_Api.Models;

namespace LaboratoryService_Api.Utilities
{
    public class ConvertHL7
    {
        public static string ConvertToHL7(LaboratorioRegistro registro)
        {
            try
            {
                // Crear el mensaje HL7
                var omlMessage = new OML_O33();

                // MSH Segment
                var msh = omlMessage.MSH;
                msh.FieldSeparator.Value = "|";
                msh.EncodingCharacters.Value = "^~\\&";
                msh.SendingApplication.NamespaceID.Value = "LaboratoryService";
                msh.SendingFacility.NamespaceID.Value = "Hospital";
                msh.ReceivingApplication.NamespaceID.Value = "AMSAlinIQ";
                msh.ReceivingFacility.NamespaceID.Value = "Laboratory";
                msh.DateTimeOfMessage.Time.Value = DateTime.Now.ToString("yyyyMMddHHmmss");
                msh.MessageType.MessageCode.Value = "OML"; // Tipo de mensaje OML
                msh.MessageType.TriggerEvent.Value = "O33"; // Evento O33
                msh.MessageControlID.Value = "1";
                msh.ProcessingID.ProcessingID.Value = "P";
                msh.VersionID.VersionID.Value = "2.5.1";

                // PID Segment
                var pid = omlMessage.PATIENT.PID;
                pid.SetIDPID.Value = "1";
                pid.PatientID.IDNumber.Value = registro.PacienteID.ToString();
                var patientName = pid.GetPatientName(0);
                patientName.GivenName.Value = registro.Pacientes.Nombre.Trim();
                patientName.FamilyName.Surname.Value = registro.Pacientes.Apellido.Trim();
                pid.DateTimeOfBirth.Time.Value = registro.Pacientes.FechadeNacimiento?.ToString("yyyyMMdd");
                pid.AdministrativeSex.Value = registro.Pacientes.IdSexo == 1 ? "M" : "F"; // Asumiendo 1 para masculino y 2 para femenino

                // PV1 Segment (Información de visita del paciente)
                var pv1 = omlMessage.PATIENT.PATIENT_VISIT.PV1;
                //pv1.PatientClass.Value = "O"; // Ambulatorio
                pv1.PatientClass.Value = registro.InternacionID == 0 ? "0" : "1"; // Ambulatorio
                pv1.AssignedPatientLocation.PointOfCare.Value = "AMB";
                pv1.AdmissionType.Value = "E";
                var attendingDoctor = pv1.GetAttendingDoctor(0);
                attendingDoctor.IDNumber.Value = "123456";
                //attendingDoctor.FamilyName.Surname.Value = "Doe";
                attendingDoctor.FamilyName.Surname.Value = registro.Prestadores.Nombre.Trim();
                //attendingDoctor.GivenName.Value = "John";
                pv1.HospitalService.Value = "CAR";
                pv1.AdmitDateTime.Time.Value = DateTime.Now.ToString("yyyyMMddHHmmss");

                // ORC Segment (Grupos)
                var orc = omlMessage.GetSPECIMEN(0).GetORDER(0).ORC;
                orc.OrderControl.Value = "NW"; // Nuevo pedido
                orc.PlacerOrderNumber.EntityIdentifier.Value = registro.LaboratorioRegistroID.ToString();
                orc.FillerOrderNumber.EntityIdentifier.Value = registro.CodigoBarra;

                // NTE Segment (Notas y comentarios)
                var nte = omlMessage.GetNTE(0);
                nte.SetIDNTE.Value = "1";
                nte.SourceOfComment.Value = "L"; // Laboratorio
                nte.GetComment(0).Value = "Nota de prueba para el mensaje HL7";

                // Agregar grupos al ORC
                foreach (var detalle in registro.LabRegistroDetalle)
                {
                    if (detalle.LabGrupoPracticaID != null && detalle.LabGrupoPracticaID > 0)
                    {
                        var groupIdentifier = orc.GetOrderingFacilityName(orc.OrderingFacilityNameRepetitionsUsed);
                        groupIdentifier.OrganizationIdentifier.Value = detalle.LabGrupoPracticaID.ToString();
                        groupIdentifier.OrganizationName.Value = detalle.LaboratorioPracticas.Nombre;
                    }
                }

                // Iterar sobre cada muestra (specimen)
                var specimen = omlMessage.GetSPECIMEN(omlMessage.SPECIMENRepetitionsUsed);

                // Agregar el segmento SPM en SPECIMEN (y no en ORDER)
                var spm = specimen.SPM;
                spm.SetIDSPM.Value = "1";
                spm.SpecimenID.PlacerAssignedIdentifier.EntityIdentifier.Value = registro.CodigoBarra;
                spm.SpecimenType.Identifier.Value = "BLD"; // Sangre (Ejemplo)
                spm.SpecimenType.Text.Value = "Sangre";
                spm.SpecimenCollectionDateTime.RangeStartDateTime.Time.Value = DateTime.Now.ToString("yyyyMMddHHmmss");

                // Iterar sobre cada práctica y agregar un nuevo ORDER con su propio OBR
                foreach (var detalle in registro.LabRegistroDetalle)
                {
                    // Agregar un nuevo ORDER para cada práctica
                    var order = omlMessage.GetSPECIMEN(0).AddORDER();
                    var obr = order.OBSERVATION_REQUEST.OBR; // Obtener el OBR dentro del OBSERVATION_REQUEST

                    obr.SetIDOBR.Value = (omlMessage.SPECIMENRepetitionsUsed).ToString(); // ID único por práctica
                    obr.PlacerOrderNumber.EntityIdentifier.Value = registro.LaboratorioRegistroID.ToString();
                    obr.FillerOrderNumber.EntityIdentifier.Value = registro.CodigoBarra;
                    obr.UniversalServiceIdentifier.Identifier.Value = detalle.LaboratorioPracticasID.ToString();
                    obr.UniversalServiceIdentifier.Text.Value = detalle.LaboratorioPracticas.Nombre;
                    obr.RequestedDateTime.Time.Value = registro.FechaCrea?.ToString("yyyyMMddHHmmss");

                }

                // Convertir el mensaje a string HL7
                var parser = new PipeParser();
                string hl7Message = parser.Encode(omlMessage);

                // Asegurar que los segmentos estén separados por \r y el mensaje termine con \n
                hl7Message = hl7Message.Replace("\r\n", "\r").Replace("\n", "\r"); // Normalizar saltos de línea
                if (!hl7Message.EndsWith("\r"))
                {
                    hl7Message += "\r"; // Asegurar que el mensaje termine con \r
                }
                hl7Message += "\n"; // Añadir \n al final del mensaje

                string mllpMessage = $"\x0B{hl7Message}\x1C\r"; // Encapsulado con MLLP

                return mllpMessage;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al convertir a HL7: " + ex.Message);
            }
        }
    }
}