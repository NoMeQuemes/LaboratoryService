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
            hl7 = hl7.Trim((char)0x0B, (char)0x1C, (char)0x0D);
            PipeParser parser = new PipeParser();
            var genericMessage = parser.Parse(hl7);
            var mshSegment = genericMessage.GetStructure("MSH") as NHapi.Model.V25.Segment.MSH;
            return mshSegment.MessageType.MessageCode.Value + "^" + mshSegment.MessageType.TriggerEvent.Value;
        }

        public static string DecodificarOULR22(string hl7)
        {
            PipeParser parser = new PipeParser();
            OUL_R22 mensaje = parser.Parse(hl7) as OUL_R22;

            // Helper para fechas HL7
            static DateTime ParseHl7Date(string value, string format = "yyyyMMddHHmmss")
            {
                return DateTime.TryParseExact(value?.Trim(), format, null, System.Globalization.DateTimeStyles.None, out DateTime result)
                    ? result
                    : DateTime.MinValue;
            }

            //Segmento MSH
            MSH msh = mensaje.MSH;
            string nombreRemitente = msh.SendingApplication.NamespaceID.Value;
            string versionRemitente = msh.VersionID.VersionID.Value;
            DateTime fechaMensaje = ParseHl7Date(msh.DateTimeOfMessage?.Time?.Value);
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
            DateTime birthdate = ParseHl7Date(pid.DateTimeOfBirth?.Time?.Value, "yyyyMMdd");
            string sex = pid.AdministrativeSex.Value;

            _PID paciente = new _PID
            {
                Id = patientID,
                NombreCompleto = patientName,
                FechaNacimiento = birthdate,
                Sexo = sex
            };

            List<_SPM> muestras = new();
            List<_SAC> contenedores = new();
            List<_OBR> obrList = new();
            List<_OBX> obxList = new();
            List<_TCD> tcdList = new();
            List<_INV> invList = new();

            int totalSpecimens = mensaje.SPECIMENRepetitionsUsed;
            for (int i = 0; i < totalSpecimens; i++)
            {
                var specimenGroup = mensaje.GetSPECIMEN(i);
                var spm = specimenGroup.SPM;

                _SPM muestra = new()
                {
                    SetId = spm.SetIDSPM.Value,
                    SpecimenID = spm.SpecimenID?.PlacerAssignedIdentifier?.EntityIdentifier?.Value,
                    SpecimenParentID = spm.GetSpecimenParentIDs().Length > 0
                        ? spm.GetSpecimenParentIDs()[0].PlacerAssignedIdentifier?.EntityIdentifier?.Value
                        : null,
                    SpecimenType = spm.SpecimenType?.Identifier?.Value
                };
                muestras.Add(muestra);

                // Segmento SAC
                for (int j = 0; j < specimenGroup.CONTAINERRepetitionsUsed; j++)
                {
                    var sac = specimenGroup.GetCONTAINER(j).SAC;

                    _SAC contenedor = new()
                    {
                        AccessionIdentifier = sac.AccessionIdentifier?.EntityIdentifier?.Value,
                        ContainerIdentifier = sac.ContainerIdentifier?.EntityIdentifier?.Value,
                        CarrierIdentifier = sac.CarrierIdentifier?.EntityIdentifier?.Value,
                        PositionInCarrier = $"{sac.PositionInCarrier?.Value1}.{sac.PositionInCarrier?.Value2}",
                        RackLocation = sac.LocationRepetitionsUsed > 0 ? sac.GetLocation(0).Identifier.Value : null,
                        BayNumber = ((NM)sac.GetField(17, 0))?.Value
                    };
                    contenedores.Add(contenedor);
                }

                // Segmento OBR, TCD, INV y OBX dentro del grupo ORDER
                for (int j = 0; j < specimenGroup.ORDERRepetitionsUsed; j++)
                {
                    var orderGroup = specimenGroup.GetORDER(j);
                    var obr = orderGroup.OBR;

                    _OBR obrInfo = new()
                    {
                        SetId = obr.SetIDOBR.Value,
                        PlacerOrderNumber = obr.PlacerOrderNumber?.EntityIdentifier?.Value,
                        FillerOrderNumber = obr.FillerOrderNumber?.EntityIdentifier?.Value,
                        UniversalTestId = obr.UniversalServiceIdentifier?.Identifier?.Value,
                        FechaMuestraRecibida = ParseHl7Date(obr.SpecimenReceivedDateTime?.Time?.Value),
                        FechaExtraccion = ParseHl7Date(obr.ObservationDateTime?.Time?.Value),
                        RequestingProvider = obr.GetOrderingProvider().Length > 0
                            ? $"{obr.GetOrderingProvider(0).GivenName.Value} {obr.GetOrderingProvider(0).FamilyName.Surname.Value}".Trim()
                            : "",
                        DiagnosticService = obr.DiagnosticServSectID?.Value,
                        Prioridad = obr.PriorityOBR?.Value
                    };
                    obrList.Add(obrInfo);

                    for (int k = 0; k < orderGroup.RESULTRepetitionsUsed; k++)
                    {
                        var resultGroup = orderGroup.GetRESULT(k);

                        // OBX
                        try
                        {
                            var obx = resultGroup.OBX;

                            _OBX obxInfo = new()
                            {
                                SetId = obx.SetIDOBX?.Value,
                                ValueType = obx.ValueType?.Value,
                                ObserationIdentifier = obx.ObservationIdentifier?.Identifier?.Value,
                                ObservationSubId = obx.ObservationSubID?.Value,
                                ObservationValue = obx.ObservationValueRepetitionsUsed > 0 ? obx.GetObservationValue(0).Data.ToString() : null,
                                Unit = obx.Units?.Identifier?.Value,
                                ReferenceRanges = obx.ReferencesRange?.Value,
                                AbnormalFlags = obx.AbnormalFlagsRepetitionsUsed > 0 ? obx.GetAbnormalFlags(0).Value : null,
                                NatureOfAbnormalTest = obx.NatureOfAbnormalTestRepetitionsUsed > 0 ? obx.GetNatureOfAbnormalTest(0).Value : null,
                                ObservationResultStatus = obx.ObservationResultStatus?.Value,
                                ValidationUserInAms = obx.ResponsibleObserverRepetitionsUsed > 0 ? obx.GetResponsibleObserver(0).IDNumber.Value : null,
                                ValidationUserOnTheInstrument = obx.ResponsibleObserverRepetitionsUsed > 1 ? obx.GetResponsibleObserver(1).IDNumber.Value : null,
                                OperatorLogged = obx.ProducerSID?.Identifier?.Value,
                                SecondValidationUserInAms = obx.ResponsibleObserverRepetitionsUsed > 2 ? obx.GetResponsibleObserver(2).IDNumber.Value : null,
                                EditingField = obx.EquipmentInstanceIdentifierRepetitionsUsed > 0 ? obx.GetEquipmentInstanceIdentifier(0).EntityIdentifier.Value : null,
                                EditOrQplOrAchitect = obx.EquipmentInstanceIdentifierRepetitionsUsed > 1 ? obx.GetEquipmentInstanceIdentifier(1).EntityIdentifier.Value : null,
                                EmptyOrArchitect = obx.EquipmentInstanceIdentifierRepetitionsUsed > 2 ? obx.GetEquipmentInstanceIdentifier(2).EntityIdentifier.Value : null,
                                InstrumentSerialNro = obx.EquipmentInstanceIdentifierRepetitionsUsed > 3 ? obx.GetEquipmentInstanceIdentifier(3).EntityIdentifier.Value : null,
                                ProcessPathId = obx.EquipmentInstanceIdentifierRepetitionsUsed > 4 ? obx.GetEquipmentInstanceIdentifier(4).EntityIdentifier.Value : null,
                                ProcessLaneId = obx.EquipmentInstanceIdentifierRepetitionsUsed > 5 ? obx.GetEquipmentInstanceIdentifier(5).EntityIdentifier.Value : null,
                                ResultValidationDate = ParseHl7Date(obx.DateTimeOfTheAnalysis?.Time?.Value),
                                InstrumentResultDate = ParseHl7Date(obx.DateTimeOfTheObservation?.Time?.Value),
                                SecondResultValidationDate = ParseHl7Date(obx.EffectiveDateOfReferenceRange?.Time?.Value)
                            };

                            obxList.Add(obxInfo);
                        }
                        catch (Exception ex)
                        {
                            logger.Warn($"Error al parsear OBX dentro de ORDER[{j}] RESULT[{k}]: {ex.Message}");
                        }

                        // TCD
                        var tcd = resultGroup.TCD;
                        if (tcd != null)
                        {
                            tcdList.Add(new _TCD
                            {
                                UniversalServiceIdentifier = tcd.UniversalServiceIdentifier?.Identifier?.Value,
                                DilutionFactor = (tcd.AutoDilutionFactor?.Components.Length > 1 && tcd.AutoDilutionFactor.Components[1] is NM nm) ? nm.Value : null
                            });
                        }

                        // INV
                        var inv = resultGroup.GetStructure("INV") as NHapi.Model.V25.Segment.INV;
                        if (inv != null)
                        {
                            invList.Add(new _INV
                            {
                                SubstanceId = inv.SubstanceIdentifier?.Identifier?.Value,
                                SubstanceStatus = inv.SubstanceStatusRepetitionsUsed > 0 ? inv.GetSubstanceStatus(0)?.Identifier?.Value : null,
                                SubstanceType = inv.SubstanceType?.Identifier?.Value,
                                PossibleValuesIdentifier = inv.SubstanceType?.Identifier?.Value,
                                Text = inv.SubstanceType?.Text?.Value,
                                Hl7ReferenceTable = inv.SubstanceType?.NameOfCodingSystem?.Value,
                                InventoryContainerIdentifier = inv.InventoryContainerIdentifier?.Identifier?.Value,
                                ExpirationDate = inv.ExpirationDateTime?.Time?.Value,
                                FirstUsedDate = inv.FirstUsedDateTime?.Time?.Value,
                                ManufacturerLotNumber = inv.ManufacturerLotNumber?.Value
                            });
                        }
                    }
                }
            }

            var OulR22Parseado = new OULR22
            {
                MSH = infoMSH,
                PID = paciente,
                SPM = muestras,
                SAC = contenedores,
                OBR = obrList,
                OBX = obxList,
                TCD = tcdList,
                INV = invList
            };

            var Json = new JsonSerializerOptions { WriteIndented = true };
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
            public List<_OBR> OBR { get; set; }
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
