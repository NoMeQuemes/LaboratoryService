using NHapi.Model.V25.Message;
using NHapi.Model.V25.Segment;
using NHapi.Model.V25.Datatype;
using NHapi.Base.Parser;
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
                string MSH = "MSH|^~\\&|LIS||AMS||20241115120405||OML^O33|20241115120405|P|2.5|||AL|NE<CR>";
                string PID = "PID|1||283662-18845632||Mo Ri^Ji Iv||19800123|F|||||<CR >";
                string PV1 = "PV1|1||A||||Ku Ale<CR>";
                string SPM1 = "SPM|1|A61906|A61906|SUERO<CR>";
                string NTE = "NTE|1|L|anemia t.ovario plan o<CR>";
                string ORC = "ORC|NW||A61906|||||||||||||||||||||<CR>";
                string OBR = "OBR|1|||FAL||||||||||20241115120405||PADILLA||||||||LACHYBS|||^^^20241115120405^^R<CR>";
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