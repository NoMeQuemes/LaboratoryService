using NHapi.Base.Parser;
using NHapi.Model.V25.Message;
using NHapi.Model.V25.Segment;
using NHapi.Model.V25.Group;
using NHapi.Model.V25.Datatype;
using System;
using System.Text.Json;

namespace LaboratoryService_Api.Utilities
{
    public class ParserHL7
    {
        public static string ObtenerTipoMensaje(string hl7)
        {
            PipeParser parser = new PipeParser();
            var genericMessage = parser.Parse(hl7);
            var mshSegment = genericMessage.GetStructure("MSH") as NHapi.Model.V25.Segment.MSH;
            return mshSegment.MessageType.MessageCode.Value + "^" + mshSegment.MessageType.TriggerEvent.Value;
        }

        public static string DecodificarOULR22(string hl7)
        {
            PipeParser parser = new PipeParser();
            OUL_R22 mensaje = parser.Parse(hl7) as OUL_R22;

            //Segmento MSH
            MSH msh = mensaje.MSH;

            string nombreRemitente = msh.SendingApplication.NamespaceID.Value;
            string versionRemitente = msh.VersionID.VersionID.Value;

            string fechaRaw = msh.DateTimeOfMessage.Time.Value;
            DateTime fechaMensaje = DateTime.ParseExact(fechaRaw, "yyyyMMddHHmmss", null);

            string tipoMensaje = $"{msh.MessageType.MessageCode.Value}^{msh.MessageType.TriggerEvent.Value}";
            string mensajeID = msh.MessageControlID.Value;

            _MSH infoMSH = new _MSH
            {
                NombreRemitente = nombreRemitente,
                VersionRemitente = versionRemitente,
                FechaMensaje = fechaMensaje,
                TipoMensaje = tipoMensaje,
                MensajeID = mensajeID
            };

            //Segmento PID
            PID pid = mensaje.PATIENT.PID;

            string patientID = pid.GetPatientIdentifierList(0).IDNumber.Value;

            string apellido = pid.GetPatientName(0).FamilyName.Surname.Value;
            string nombre = pid.GetPatientName(0).GivenName.Value;
            string patientName = $"{nombre} {apellido}";

            string birthdateRaw = pid.DateTimeOfBirth.Time.Value;
            DateTime birthdate = DateTime.ParseExact(birthdateRaw, "yyyyMMdd", null);

            string sex = pid.AdministrativeSex.Value;

            _PID paciente = new _PID
            {
                Id = patientID,
                NombreCompleto = patientName,
                FechaNacimiento = birthdate,
                Sexo = sex
            }; ;

            //Segmento SPM
            List<_SPM> muestras = new List<_SPM>();
            List<_SAC> contenedores = new List<_SAC>();

            int totalSpecimens = mensaje.SPECIMENRepetitionsUsed;
            for (int i = 0; i < totalSpecimens; i++)
            {
                var specimenGroup = mensaje.GetSPECIMEN(i);
                var spm = specimenGroup.SPM;

                string setId = spm.SetIDSPM.Value;

                string specimenID = spm.SpecimenID?.PlacerAssignedIdentifier?.EntityIdentifier?.Value;
                string specimenParentID = spm.GetSpecimenParentIDs().Length > 0
                    ? spm.GetSpecimenParentIDs()[0].PlacerAssignedIdentifier?.EntityIdentifier?.Value
                    : null;
                string specimenType = spm.SpecimenType?.Identifier?.Value;

                _SPM muestra = new _SPM
                {
                    SetId = setId,
                    SpecimenID = specimenID,
                    SpecimenParentID = specimenParentID,
                    SpecimenType = specimenType
                };

                muestras.Add(muestra);

                // Obtener cada grupo CONTAINER dentro del SPECIMEN
                int containerCount = specimenGroup.CONTAINERRepetitionsUsed;
                for (int j = 0; j < containerCount; j++)
                {
                    var containerGroup = specimenGroup.GetCONTAINER(j);
                    var sac = containerGroup.SAC;

                    string accessionId = sac.AccessionIdentifier?.EntityIdentifier?.Value;
                    string containerId = sac.ContainerIdentifier?.EntityIdentifier?.Value;
                    string carrierId = sac.CarrierIdentifier?.EntityIdentifier?.Value;
                    string positionInCarrier = $"{sac.PositionInCarrier?.Value1}.{sac.PositionInCarrier?.Value2}";
                    string rackLocation = sac.LocationRepetitionsUsed > 0
                                            ? sac.GetLocation(0).Identifier.Value
                                            : null;
                    string bayNumber = ((NM)sac.GetField(17, 0))?.Value;

                    _SAC containerInfo = new _SAC
                    {
                        AccessionIdentifier = accessionId,
                        ContainerIdentifier = containerId,
                        CarrierIdentifier = carrierId,
                        PositionInCarrier = positionInCarrier,
                        RackLocation = rackLocation,
                        BayNumber = bayNumber
                    };

                    contenedores.Add(containerInfo);
                }
            }

            //Segmento NTE

            //Se convierte todos los segmentos en un JSON
            OULR22 OulR22Parseado = new OULR22
            {
                MSH = infoMSH,
                PID = paciente,
                SPM = muestras
            };

            var Json = new JsonSerializerOptions
            {
                WriteIndented = true, //Para formato bonito
            };

            return JsonSerializer.Serialize(OulR22Parseado, Json);
        }



        //Objetos
        public class OULR22
        {
            public _MSH MSH { get; set; }
            public _PID PID { get; set; }
            public List<_SPM> SPM { get; set; }
            public List<_SAC> SAC { get; set; }
        }
        public class _MSH
        {
            public string NombreRemitente { get; set; }
            public string VersionRemitente { get; set; }
            public DateTime FechaMensaje { get; set; }
            public string TipoMensaje { get; set; }
            public string MensajeID { get; set; }
        }
        public class _PID
        {
            public string Id { get; set; }
            public string NombreCompleto { get; set; }
            public DateTime FechaNacimiento { get; set; }
            public string Sexo { get; set; }
        }
        public class _SPM
        {
            public string SetId { get; set; }
            public string SpecimenID { get; set; }
            public string SpecimenParentID { get; set; }
            public string SpecimenType { get; set; }

        }
        public class _SAC
        {
            public string AccessionIdentifier { get; set; }
            public string ContainerIdentifier { get; set; }
            public string CarrierIdentifier { get; set; }
            public string PositionInCarrier { get; set; }
            public string RackLocation { get; set; }
            public string BayNumber { get; set; }
        }
        public class NTE
        {
            public string SetId { get; set; }
            public string SourceOfComment { get; set; }
            public string CommentText { get; set; }
        }

    }
}
