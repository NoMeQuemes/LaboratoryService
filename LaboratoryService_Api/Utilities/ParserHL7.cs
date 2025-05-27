using NHapi.Base.Parser;
using NHapi.Model.V25.Message;
using NHapi.Model.V25.Segment;
using NHapi.Model.V25.Group;
using NHapi.Model.V25.Datatype;
using System;
using System.Text.Json;
using NLog;
using System.ComponentModel;
using LaboratoryService_Api.Models;
using System.Diagnostics;

namespace LaboratoryService_Api.Utilities
{
    public class ParserHL7
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

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

                // Segmento SAC
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

            // Segmento OBR
            List<_OBR> obrList = new List<_OBR>();

            // Iterar sobre los grupos SPECIMEN
            int totalSpecimenss = mensaje.SPECIMENRepetitionsUsed;
            for (int i = 0; i < totalSpecimens; i++)
            {
                var specimenGroup = mensaje.GetSPECIMEN(i);

                // Iterar sobre los grupos ORDER dentro de cada SPECIMEN
                int totalOrders = specimenGroup.ORDERRepetitionsUsed; // Aquí usamos ORDERRepetitionsUsed dentro de SPECIMEN
                for (int j = 0; j < totalOrders; j++)
                {
                    var orderGroup = specimenGroup.GetORDER(j);
                    var obr = orderGroup.OBR;

                    string setId = obr.SetIDOBR.Value ?? "";
                    string placerOrderNumber = obr.PlacerOrderNumber.EntityIdentifier.Value ?? "";
                    string fillerOrderNumber = obr.FillerOrderNumber.EntityIdentifier.Value ?? "";
                    string universalTestId = obr.UniversalServiceIdentifier.Identifier.Value ?? "";

                    DateTime fechaMuestraRecibida;
                    try
                    {
                        string fechaMuestraRaw = obr.SpecimenReceivedDateTime.Time.Value;
                        fechaMuestraRecibida = DateTime.ParseExact(fechaMuestraRaw, "yyyyMMddHHmmss", null);
                    }
                    catch (FormatException)
                    {
                        logger.Warn("Invalid OBR Specimen Received Date format, using default date.");
                        fechaMuestraRecibida = DateTime.MinValue;
                    }

                    string requestingProvider = obr.GetOrderingProvider().Length > 0
                        ? $"{obr.GetOrderingProvider(0).GivenName.Value} {obr.GetOrderingProvider(0).FamilyName.Surname.Value}".Trim()
                        : "";
                    string diagnosticService = obr.DiagnosticServSectID.Value ?? "";

                    DateTime fechaExtraccion;
                    try
                    {
                        string fechaExtraccionRaw = obr.ObservationDateTime?.Time?.Value;

                        if (!string.IsNullOrEmpty(fechaExtraccionRaw))
                        {
                            fechaExtraccion = DateTime.ParseExact(fechaExtraccionRaw, "yyyyMMddHHmmss", null);
                        }
                        else
                        {
                            logger.Warn("OBR Observation Date is null or empty, using default date.");
                            fechaExtraccion = DateTime.MinValue;
                        }
                    }
                    catch (FormatException)
                    {
                        logger.Warn("Invalid OBR Observation Date format, using default date.");
                        fechaExtraccion = DateTime.MinValue;
                    }

                    string prioridad = obr.PriorityOBR.Value ?? "";

                    _OBR obrInfo = new _OBR
                    {
                        SetId = setId,
                        PlacerOrderNumber = placerOrderNumber,
                        FillerOrderNumber = fillerOrderNumber,
                        UniversalTestId = universalTestId,
                        FechaMuestraRecibida = fechaMuestraRecibida,
                        RequestingProvider = requestingProvider,
                        DiagnosticService = diagnosticService,
                        FechaExtraccion = fechaExtraccion,
                        Prioridad = prioridad
                    };

                    obrList.Add(obrInfo);
                }
            }
            //Segmento OBX
            List<_OBX> obxList = new List<_OBX>();

            for (int i = 0; i < mensaje.SPECIMENRepetitionsUsed; i++)
            {
                var specimenGroup = mensaje.GetSPECIMEN(i);

                for (int j = 0; j < specimenGroup.OBXRepetitionsUsed; j++)
                {
                    var obx = specimenGroup.GetOBX(j);

                    var obxInfo = new _OBX
                    {
                        SetId = obx.SetIDOBX?.Value,
                        ValueType = obx.ValueType?.Value,
                        ObserationIdentifier = obx.ObservationIdentifier?.Identifier?.Value,
                        ObservationSubId = obx.ObservationSubID?.Value,
                        ObservationValue = obx.ObservationValueRepetitionsUsed > 0
                            ? obx.GetObservationValue(0).Data.ToString()
                            : null,
                        Unit = obx.Units?.Identifier?.Value,
                        ReferenceRanges = obx.ReferencesRange?.Value,
                        AbnormalFlags = obx.AbnormalFlagsRepetitionsUsed > 0
                            ? obx.GetAbnormalFlags(0).Value
                            : null,
                        NatureOfAbnormalTest = obx.NatureOfAbnormalTestRepetitionsUsed > 0
                            ? obx.GetNatureOfAbnormalTest(0).Value
                            : null,
                        ObservationResultStatus = obx.ObservationResultStatus?.Value,
                        ValidationUserInAms = obx.ResponsibleObserverRepetitionsUsed > 0
                            ? obx.GetResponsibleObserver(0).IDNumber.Value
                            : null,
                        ValidationUserOnTheInstrument = obx.ResponsibleObserverRepetitionsUsed > 1
                            ? obx.GetResponsibleObserver(1).IDNumber.Value
                            : null,
                        OperatorLogged = obx.ProducerSID?.Identifier?.Value,
                        SecondValidationUserInAms = obx.ResponsibleObserverRepetitionsUsed > 2
                            ? obx.GetResponsibleObserver(2).IDNumber.Value
                            : null,

                        // Parte 18: EquipmentInstanceIdentifier
                        EditingField = obx.EquipmentInstanceIdentifierRepetitionsUsed > 0
                            ? obx.GetEquipmentInstanceIdentifier(0).EntityIdentifier.Value
                            : null,
                        EditOrQplOrAchitect = obx.EquipmentInstanceIdentifierRepetitionsUsed > 1
                            ? obx.GetEquipmentInstanceIdentifier(1).EntityIdentifier.Value
                            : null,
                        EmptyOrArchitect = obx.EquipmentInstanceIdentifierRepetitionsUsed > 2
                            ? obx.GetEquipmentInstanceIdentifier(2).EntityIdentifier.Value
                            : null,
                        InstrumentSerialNro = obx.EquipmentInstanceIdentifierRepetitionsUsed > 3
                            ? obx.GetEquipmentInstanceIdentifier(3).EntityIdentifier.Value
                            : null,
                        ProcessPathId = obx.EquipmentInstanceIdentifierRepetitionsUsed > 4
                            ? obx.GetEquipmentInstanceIdentifier(4).EntityIdentifier.Value
                            : null,
                        ProcessLaneId = obx.EquipmentInstanceIdentifierRepetitionsUsed > 5
                            ? obx.GetEquipmentInstanceIdentifier(5).EntityIdentifier.Value
                            : null,

                        // Parte 19: Fechas
                        ResultValidationDate = DateTime.TryParseExact(
                            obx.DateTimeOfTheAnalysis?.Time?.Value,
                            "yyyyMMddHHmmss",
                            null,
                            System.Globalization.DateTimeStyles.None,
                            out DateTime resultValidationDate) ? resultValidationDate : DateTime.MinValue,

                        InstrumentResultDate = DateTime.TryParseExact(
                            obx.DateTimeOfTheObservation?.Time?.Value,
                            "yyyyMMddHHmmss",
                            null,
                            System.Globalization.DateTimeStyles.None,
                            out DateTime instrumentResultDate) ? instrumentResultDate : DateTime.MinValue,

                        SecondResultValidationDate = DateTime.TryParseExact(
                            obx.EffectiveDateOfReferenceRange?.Time?.Value,
                            "yyyyMMddHHmmss",
                            null,
                            System.Globalization.DateTimeStyles.None,
                            out DateTime secondValidationDate) ? secondValidationDate : DateTime.MinValue,
                    };

                    obxList.Add(obxInfo);
                }
            }

            //Segmento TCD
            List<_TCD> tcdList = new List<_TCD>();

            for (int i = 0; i < mensaje.SPECIMENRepetitionsUsed; i++)
            {
                var specimenGroup = mensaje.GetSPECIMEN(i);

                for (int j = 0; j < specimenGroup.ORDERRepetitionsUsed; j++)
                {
                    var orderGroup = specimenGroup.GetORDER(j);

                    int resultCount = orderGroup.RESULTRepetitionsUsed;
                    for (int k = 0; k < resultCount; k++)
                    {
                        var resultGroup = orderGroup.GetRESULT(k);
                        var tcd = resultGroup.TCD;

                        if (tcd != null)
                        {
                            // Obtener el número desde el campo SN correctamente
                            string dilutionFactor = null;
                            var sn = tcd.AutoDilutionFactor;
                            if (sn != null && sn.Components.Length > 1)
                            {
                                var numberComponent = sn.Components[1] as NM; // SN.2 es el número
                                dilutionFactor = numberComponent?.Value;
                            }

                            var tcdObj = new _TCD
                            {
                                UniversalServiceIdentifier = tcd.UniversalServiceIdentifier?.Identifier?.Value,
                                DilutionFactor = dilutionFactor
                            };

                            tcdList.Add(tcdObj);
                        }
                    }
                }
            }

            //Segmento INV
            List<_INV> invList = new List<_INV>();

            for (int i = 0; i < mensaje.SPECIMENRepetitionsUsed; i++)
            {
                var specimenGroup = mensaje.GetSPECIMEN(i);

                for (int j = 0; j < specimenGroup.ORDERRepetitionsUsed; j++)
                {
                    var orderGroup = specimenGroup.GetORDER(j);

                    int resultCount = orderGroup.RESULTRepetitionsUsed;
                    for (int k = 0; k < resultCount; k++)
                    {
                        var resultGroup = orderGroup.GetRESULT(k);

                        // Intenta obtener el segmento INV
                        var invSegment = resultGroup.GetStructure("INV") as NHapi.Model.V25.Segment.INV;

                        if (invSegment != null)
                        {
                            var invObj = new _INV
                            {
                                SubstanceId = invSegment.SubstanceIdentifier?.Identifier?.Value,

                                // Solo tomamos la primera repetición del campo repeating INV-2
                                SubstanceStatus = invSegment.SubstanceStatusRepetitionsUsed > 0
                                    ? invSegment.GetSubstanceStatus(0)?.Identifier?.Value
                                    : null,

                                // Parte 3
                                SubstanceType = invSegment.SubstanceType?.Identifier?.Value,
                                PossibleValuesIdentifier = invSegment.SubstanceType?.Identifier?.Value,
                                Text = invSegment.SubstanceType?.Text?.Value,
                                Hl7ReferenceTable = invSegment.SubstanceType?.NameOfCodingSystem?.Value,

                                InventoryContainerIdentifier = invSegment.InventoryContainerIdentifier?.Identifier?.Value,

                                ExpirationDate = invSegment.ExpirationDateTime?.Time?.Value,
                                FirstUsedDate = invSegment.FirstUsedDateTime?.Time?.Value,
                                ManufacturerLotNumber = invSegment.ManufacturerLotNumber?.Value
                            };

                            invList.Add(invObj);
                        }
                    }
                }
            }

            //Segmento NTE

            //Se convierte todos los segmentos en un JSON
            OULR22 OulR22Parseado = new OULR22
            {
                MSH = infoMSH,
                PID = paciente,
                SPM = muestras,
                SAC = contenedores,
                OBX = obxList,
                TCD = tcdList,
                INV = invList
            };

            var Json = new JsonSerializerOptions
            {
                WriteIndented = true, //Para formato bonito
            };

            logger.Info("Mensaje OUL^R22 recibido y decodificado correctamente.");
            logger.Debug(JsonSerializer.Serialize(OulR22Parseado, Json));
            return JsonSerializer.Serialize(OulR22Parseado, Json);
        }

        public static string DecodificarSSUUO3(string hl7)
        {
            PipeParser parser = new PipeParser();
            SSU_U03 mensaje = parser.Parse(hl7) as SSU_U03;

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


            // Segmento EQU
            var equ = mensaje.EQU;

            _EQU equipo = new _EQU
            {
                EquipmentInstanceIdentifier = equ.EquipmentInstanceIdentifier?.EntityIdentifier?.Value,
                EventDate = equ.EventDateTime?.Time?.Value
            };

            //Segmento SAC
            List<_SAC> contenedores = new List<_SAC>();

            for (int i = 0; i < mensaje.SPECIMEN_CONTAINERRepetitionsUsed; i++)
            {
                var containerGroup = mensaje.GetSPECIMEN_CONTAINER(i);
                var sac = containerGroup.SAC;

                string evento = sac.EquipmentContainerIdentifier?.EntityIdentifier?.Value; // SAC-5

                _SAC contenedor = new _SAC
                {
                    AccessionIdentifier = sac.AccessionIdentifier?.EntityIdentifier?.Value,     // SAC-2
                    ContainerIdentifier = sac.ContainerIdentifier?.EntityIdentifier?.Value,     // SAC-3
                    EquipmentContainerIdentifier = evento,                                      // SAC-5
                    SpecimenSource = sac.SpecimenSource?.SpecimenSourceNameOrCode?.Identifier?.Value, // SAC-6
                    RegistrationDate = sac.RegistrationDateTime?.Time?.Value,                   // SAC-7
                    ContainerStatus = sac.ContainerStatus?.Identifier?.Value,                   // SAC-8
                    CarrierIdentifier = sac.CarrierIdentifier?.EntityIdentifier?.Value,         // SAC-10
                    PositionInCarrier = $"{sac.PositionInCarrier?.Value1}.{sac.PositionInCarrier?.Value2}", // SAC-11
                    Location = sac.LocationRepetitionsUsed > 0 ? sac.GetLocation(0).Identifier.Value : null, // SAC-15
                    RackLocation = sac.LocationRepetitionsUsed > 0 ? sac.GetLocation(0).Identifier?.Value : null,
                    BayNumber = sac.LocationRepetitionsUsed > 0 ? sac.GetLocation(0).Text?.Value : null,
                    AvailableSpecimenVolume = sac.AvailableSpecimenVolume?.Value                // SAC-22
                };

                switch (evento)
                {
                    case "CK": // Sample Check-In
                    case "SE": // Sample Seen
                    case "HQ": // Host Query
                    case "SM": // Sample Removed
                    case "SD": // Sample Disposed
                               // Solo campos comunes ya están asignados arriba
                        break;

                    case "SS": // Sample Storage
                               // Ya incluye: CarrierIdentifier, PositionInCarrier, Location
                        break;

                    case "AL": // Aliquot Notification
                        contenedor.SpecimenSource = sac.SpecimenSource?.SpecimenSourceNameOrCode?.Identifier?.Value;
                        contenedor.AvailableSpecimenVolume = sac.AvailableSpecimenVolume?.Value;
                        break;

                    case "PT": // Primary Tube Info
                        contenedor.ContainerStatus = sac.ContainerStatus?.Identifier?.Value;
                        break;

                    case "SU": // Sample Status Update
                               // Campos ya mapeados en estructura base
                        break;

                    case "TU": // Test Status Update
                        contenedor.SpecimenSource = sac.SpecimenSource?.SpecimenSourceNameOrCode?.Identifier?.Value;
                        break;

                    default:
                        logger.Warn($"Tipo de evento no reconocido: {evento}");
                        break;
                }

                contenedores.Add(contenedor);
            }





            //Se convierte todos los segmentos en un JSON
            SSUUO3 SsuUo3Parseado = new SSUUO3
            {
                MSH = infoMSH
            };

            var Json = new JsonSerializerOptions
            {
                WriteIndented = true, //Para formato bonito
            };

            logger.Info($"Json del mensaje recibido: {JsonSerializer.Serialize(SsuUo3Parseado, Json)}");
            return JsonSerializer.Serialize(SsuUo3Parseado, Json);
        }
        //Objetos
        public class OULR22
        {
            public _MSH MSH { get; set; }
            public _PID PID { get; set; }
            public List<_SPM> SPM { get; set; }
            public List<_SAC> SAC { get; set; }
            public List<_OBX> OBX { get; set; }
            public List<_TCD> TCD { get; set; }
            public List<_INV> INV { get; set; }

        }
        public class SSUUO3
        {
            public _MSH MSH { get; set; }
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
            public string AccessionIdentifier { get; set; } //2
            public string ContainerIdentifier { get; set; } //3
            public string EquipmentContainerIdentifier { get; set; } //5
            public string SpecimenSource {  get; set; } //6
            public string RegistrationDate { get; set; } //7
            public string ContainerStatus { get; set; } //8
            public string CarrierIdentifier { get; set; } //10
            public string PositionInCarrier { get; set; } //11
            public string Location { get; set; } //15
            public string RackLocation { get; set; } //15.1
            public string BayNumber { get; set; } //15.2
            public string AvailableSpecimenVolume { get; set; } //22
        }
        public class _OBR
        {
            public string SetId { get; set; }
            public string PlacerOrderNumber { get; set; }
            public string FillerOrderNumber { get; set; }
            public string UniversalTestId { get; set; }
            public DateTime FechaMuestraRecibida { get; set; }
            public string RequestingProvider { get; set; }
            public string DiagnosticService { get; set; }
            public DateTime FechaExtraccion { get; set; }
            public string Prioridad { get; set; }
        }
        public class _OBX
        {
            public string SetId { get; set; }
            public string ValueType { get; set; }
            public string ObserationIdentifier { get; set; }
            public string ObservationSubId { get; set; }
            public string ObservationValue  { get; set; }
            public string Unit { get; set; }
            public string ReferenceRanges { get; set; }
            public string AbnormalFlags { get; set; }
            public string NatureOfAbnormalTest { get; set; }
            public string ObservationResultStatus { get; set; }
            public string ValidationUserInAms { get; set; }
            public string ValidationUserOnTheInstrument { get; set; }
            public string OperatorLogged { get; set; }
            public string SecondValidationUserInAms { get; set; }

            //Parte 18 del segmento:
            public string EditingField { get; set; }
            public string EditOrQplOrAchitect { get; set; }
            public string EmptyOrArchitect { get; set; }
            public string InstrumentSerialNro { get; set; }
            public string ProcessPathId { get; set; }
            public string ProcessLaneId { get; set; }

            //Parte 19 del segmento:
            public DateTime ResultValidationDate { get; set; }
            public DateTime InstrumentResultDate { get; set; }
            public DateTime SecondResultValidationDate { get; set; }
        }
        public class _TCD
        {
            public string UniversalServiceIdentifier { get; set; }
            public string DilutionFactor { get; set; }
        }
        public class _INV
        {
            public string SubstanceId { get; set; }
            public string SubstanceStatus { get; set; }

            //Inicio de la parte 3 del segmento
            public string SubstanceType { get; set; }
            public string PossibleValuesIdentifier { get; set; }
            public string Text { get; set; }
            public string Hl7ReferenceTable { get; set; }
            //---------------------------------------------------
            public string InventoryContainerIdentifier { get; set; }
            public string ExpirationDate { get; set; }
            public string FirstUsedDate { get; set; }
            public string ManufacturerLotNumber { get; set; }
        }
        public class NTE
        {
            public string SetId { get; set; }
            public string SourceOfComment { get; set; }
            public string CommentText { get; set; }
        }
        public class _EQU
        {
            public string EquipmentInstanceIdentifier { get; set; }
            public string EventDate { get; set; }
        }

    }
}
