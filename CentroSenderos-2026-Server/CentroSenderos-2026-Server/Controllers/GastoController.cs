using CentroSenderos_2026_Repositorio.Repositorios;
using CentroSenderos_2026_Shared.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

namespace CentroSenderos_2026_Server.Controllers
{
    [ApiController]
    [Route("api/gasto")]
    public class GastoController : ControllerBase
    {
        private readonly IGastoRepositorio repositorio;

        public GastoController(IGastoRepositorio repositorio)
        {
            this.repositorio = repositorio;
        }

        [HttpGet("ListaGastos")]
        public async Task<ActionResult<List<GastoListadoDTO>>> GetListaGastos()
        {
            try
            {
                var lista = await repositorio.SelectListaGastos();

                return Ok(lista);
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        mensaje = "Ocurrió un error inesperado al consultar los gastos."
                    }
                );
            }
        }

        [HttpPost("insertar")]
        public async Task<ActionResult<int>> InsertarGasto(
            [FromBody] GastoCrearDTO dto)
        {
            try
            {
                var id = await repositorio.InsertarGasto(dto);

                return Ok(id);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(
                    new { mensaje = ex.Message }
                );
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        mensaje = "Ocurrió un error inesperado al registrar el gasto."
                    }
                );
            }
        }
    }
}