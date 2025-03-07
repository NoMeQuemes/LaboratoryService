using LaboratoryService_Api.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using LaboratoryService_Api.Models;
using System.Net;
using LaboratoryService_Api.Utilities;

namespace LaboratoryService_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LaboratorioComunicacionController : ControllerBase
    {

        private readonly ApplicationDbContext _context;
        private readonly ApiResponse _response;
        private readonly ConvertHL7 _parsearHL7;
        private readonly TcpManager _tcpManager;

        public LaboratorioComunicacionController(ApplicationDbContext context, ConvertHL7 convertHL7, TcpManager tcpManager)
        {
            _context = context;
            _response = new ApiResponse();
            _parsearHL7 = convertHL7;
            _tcpManager = tcpManager;
        }

        [HttpPost]
        [Route("enviarPedidoHl7")]
        public async Task<ActionResult<ApiResponse>> EnviarPedidoHL7([FromBody] OrderRequest request)
        {
            if (request == null || request.orderID <= 0)
            {
                _response.statusCode = HttpStatusCode.BadRequest;
                _response.IsExitoso = false;
                _response.ErrorMessages = new List<string>() { "Id del pedidido invalido" };
                return _response;
            }

            try
            {
                var registro = await _context.LaboratorioRegistro
                                        .Where(o => o.LaboratorioRegistroID == request.orderID)
                                        .Include(p => p.Pacientes)
                                            .ThenInclude(s => s.Sexo)
                                        .Include(d => d.Prestadores)
                                        .Include(i => i.Instituciones)
                                        .Include(l => l.LabRegistroDetalle)
                                            .ThenInclude(a => a.LaboratorioPracticas)
                                        .Include(z => z.Internaciones)
                                        .FirstOrDefaultAsync();

                if (registro == null)
                {
                    _response.statusCode = HttpStatusCode.NotFound;
                    _response.IsExitoso = false;
                    _response.ErrorMessages = new List<string>() { "Pedido no encontrado." };
                    return (_response);
                }

                
                string hl7Message = ConvertHL7.ConvertToHL7(registro); // Convierte el resultado de la BD en un mensaje HL7
                _tcpManager.SendMessage(hl7Message); //Envía el mensaje HL7 al servidor


                _response.Resultado = hl7Message;
                _response.statusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.statusCode = HttpStatusCode.InternalServerError;
                _response.IsExitoso = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
                return (_response);
            }
        }
    }

    public class OrderRequest
    {
        public int orderID { get; set; }
    }
}
