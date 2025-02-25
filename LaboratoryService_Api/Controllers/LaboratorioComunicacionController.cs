using LaboratoryService_Api.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NHapi.Base.Parser;
using NHapi.Base;
using NHapi.Model.V251.Message;
using NHapi.Model.V251.Segment;
using NHapi.Model.V251.Group;
using LaboratoryService_Api.Models;
using System.Net;

namespace LaboratoryService_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LaboratorioComunicacionController : ControllerBase
    {

        private readonly ApplicationDbContext _context;
        private readonly ApiResponse _response;

        public LaboratorioComunicacionController(ApplicationDbContext context)
        {
            _context = context;
            _response = new ApiResponse();
        }

        [HttpPost("buscar-pedido")]
        public async Task<ActionResult<ApiResponse>> BuscarPedido([FromBody] OrderRequest request)
        {
            if (request == null || request.orderID <= 0)
                return BadRequest("ID del pedido inválido."); //Verifica que venga algún id desde Zismed

            //Busca en la base de datos el pedido al cual se le quiere cargar los resulSearados
            try
            {
                _response.Resultado = await _context.LaboratorioRegistro
                                        .Where(o => o.LaboratorioRegistroID == request.orderID)
                                        .Include(p => p.Pacientes)
                                        .Include(d => d.Prestadores)
                                        .Include(i => i.Instituciones)
                                        .Include(l => l.LabRegistroDetalle)
                                            .ThenInclude(a => a.LaboratorioPracticas)
                                        .Select(o => new
                                        {
                                            o.LaboratorioRegistroID,
                                            Paciente = o.Pacientes.Nombre.Trim(),
                                            Prestador = o.Prestadores.Nombre.Trim(),
                                            Fecha = o.FechaCrea,
                                            Institucion = o.Instituciones.Nombre,
                                            o.CodigoBarra,
                                            Practicas = o.LabRegistroDetalle
                                                .Where(s => s.LaboratorioRegistroID == request.orderID)
                                                .Select(s => new
                                                {
                                                    s.LabGrupoPracticaID,
                                                    NombreGrupoPractica = s.LabGrupoPracticaID != 0 ? s.LaboratorioPracticas.Nombre : null,
                                                    s.LaboratorioPracticasID,
                                                    NombrePractica = s.LaboratorioPracticas.Nombre,
                                                    s.CodigoTubo
                                                })
                                                .DefaultIfEmpty()
                                                .ToList()
                                        })
                                        .FirstOrDefaultAsync();

                _response.statusCode = HttpStatusCode.OK;
                return Ok(_response);

            }
            catch (Exception ex)
            {
                _response.IsExitoso = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }

            return _response;
        }


    }

    public class OrderRequest
    {
        public int orderID { get; set; }
    }
}
