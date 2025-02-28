using NHapi.Model.V25.Message;
using NHapi.Model.V25.Segment;
using NHapi.Model.V25.Datatype;
using NHapi.Base.Parser;
using System;
using LaboratoryService_Api.Models;

namespace LaboratoryService_Api.Utilities
{
    public class ConvertHL7
    {
        public static string ConvertToHL7(LaboratorioRegistro registro)
        {
            try
            {
                // Crear el mensaje HL7 versión 2.5
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
                msh.MessageType.MessageCode.Value = "OML";
                msh.MessageType.TriggerEvent.Value = "O33";
                msh.MessageControlID.Value = "1";
                msh.ProcessingID.ProcessingID.Value = "P";
                msh.VersionID.VersionID.Value = "2.5";

                // PID Segment (Asegurar que PID viene inmediatamente después de MSH)
                var pid = omlMessage.PATIENT.PID;
                pid.SetIDPID.Value = "1";
                pid.PatientID.IDNumber.Value = registro.PacienteID.ToString();
                var patientName = pid.GetPatientName(0);
                patientName.GivenName.Value = registro.Pacientes.Nombre.Trim();
                patientName.FamilyName.Surname.Value = registro.Pacientes.Apellido.Trim();
                pid.DateTimeOfBirth.Time.Value = registro.Pacientes.FechadeNacimiento?.ToString("yyyyMMdd");
                pid.AdministrativeSex.Value = registro.Pacientes.IdSexo == 1 ? "M" : "F";

                // Agregar NTE después de PID
                var nte = omlMessage.PATIENT.AddNTE();
                nte.SetIDNTE.Value = "1";
                nte.SourceOfComment.Value = "L";
                nte.GetComment(0).Value = "Nota de prueba para el paciente";

                // PV1 Segment
                var pv1 = omlMessage.PATIENT.PATIENT_VISIT.PV1;
                pv1.PatientClass.Value = registro.InternacionID == 0 ? "O" : "I";
                pv1.AssignedPatientLocation.PointOfCare.Value = "AMB";
                pv1.AdmissionType.Value = "E";
                var attendingDoctor = pv1.GetAttendingDoctor(0);
                attendingDoctor.IDNumber.Value = "123456";
                attendingDoctor.FamilyName.Surname.Value = registro.Prestadores.Nombre.Trim();
                pv1.HospitalService.Value = "CAR";
                pv1.AdmitDateTime.Time.Value = DateTime.Now.ToString("yyyyMMddHHmmss");

                // ORC Segment
                var orc = omlMessage.GetSPECIMEN(0).GetORDER(0).ORC;
                orc.OrderControl.Value = "NW";
                orc.PlacerOrderNumber.EntityIdentifier.Value = registro.LaboratorioRegistroID.ToString();
                orc.FillerOrderNumber.EntityIdentifier.Value = registro.CodigoBarra;

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

                // Agregar el segmento SPM en SPECIMEN
                var spm = specimen.SPM;
                spm.SetIDSPM.Value = "1";
                spm.SpecimenID.PlacerAssignedIdentifier.EntityIdentifier.Value = registro.CodigoBarra;
                spm.SpecimenType.Identifier.Value = "BLD";
                spm.SpecimenType.Text.Value = "Sangre";
                spm.SpecimenCollectionDateTime.RangeStartDateTime.Time.Value = DateTime.Now.ToString("yyyyMMddHHmmss");

                // Iterar sobre cada práctica y agregar un nuevo ORDER con su propio OBR
                foreach (var detalle in registro.LabRegistroDetalle)
                {
                    var order = omlMessage.GetSPECIMEN(0).AddORDER();
                    var obr = order.OBSERVATION_REQUEST.OBR;

                    obr.SetIDOBR.Value = (omlMessage.SPECIMENRepetitionsUsed).ToString();
                    obr.PlacerOrderNumber.EntityIdentifier.Value = registro.LaboratorioRegistroID.ToString();
                    obr.FillerOrderNumber.EntityIdentifier.Value = registro.CodigoBarra;
                    obr.UniversalServiceIdentifier.Identifier.Value = detalle.LaboratorioPracticasID.ToString();
                    obr.UniversalServiceIdentifier.Text.Value = detalle.LaboratorioPracticas.Nombre;
                    obr.RequestedDateTime.Time.Value = registro.FechaCrea?.ToString("yyyyMMddHHmmss");
                }

                // Convertir el mensaje a string HL7
                var parser = new PipeParser();
                string hl7Message = parser.Encode(omlMessage);

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