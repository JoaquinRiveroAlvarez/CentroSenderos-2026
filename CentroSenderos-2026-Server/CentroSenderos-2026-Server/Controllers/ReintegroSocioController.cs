using CentroSenderos_2026_Repositorio.Repositorios;
using CentroSenderos_2026_Shared.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace CentroSenderos_2026_Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/reintegrosocio")]
    public class ReintegroSocioController : ControllerBase
    {
        private readonly IReintegroSocioRepositorio repositorio;

        public ReintegroSocioController(
            IReintegroSocioRepositorio repositorio)
        {
            this.repositorio = repositorio;
        }

        [HttpPost("insertar")]
        public async Task<ActionResult<int>> InsertarReintegroSocio(
            [FromBody] ReintegroSocioDTO dto)
        {
            try
            {
                var id = await repositorio.InsertarReintegroSocio(dto);

                return Ok(id);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new RespuestaDTO
                {
                    mensaje = ex.Message
                });
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new RespuestaDTO
                    {
                        mensaje =
                            "No se pudo confirmar el registro del reintegro. " +
                            "Verificá los movimientos antes de volver a intentarlo."
                    });
            }
        }
    }
}