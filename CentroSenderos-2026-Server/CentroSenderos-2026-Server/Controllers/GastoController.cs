using CentroSenderos_2026_Repositorio.Repositorios;
using CentroSenderos_2026_Shared.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
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
        public async Task<ActionResult<int>> InsertarGasto([FromBody] GastoCrearDTO dto)
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

        [Authorize]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<GastoCrearDTO>> GetGastoPorId(int id)
        {
            try
            {
                var gasto = await repositorio.SelectGastoPorId(id);

                if (gasto == null)
                {
                    return NotFound(new
                    {
                        mensaje = "El gasto no existe o está dado de baja."
                    });
                }

                return Ok(gasto);
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        mensaje = "Ocurrió un error inesperado al consultar el gasto."
                    });
            }
        }

        [Authorize]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> ActualizarGasto(
            int id,
            [FromBody] GastoEditarDTO dto)
        {
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(usuarioId))
            {
                return Unauthorized(new
                {
                    mensaje = "Iniciá sesión para modificar el gasto."
                });
            }

            try
            {
                var actualizado = await repositorio.ActualizarGasto(
                    id,
                    dto,
                    usuarioId);

                if (!actualizado)
                {
                    return NotFound(new
                    {
                        mensaje = "El gasto no existe o está dado de baja."
                    });
                }

                return Ok(new RespuestaDTO
                {
                    mensaje = "Gasto actualizado correctamente."
                });
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
                        mensaje = "No se pudo completar la actualización del gasto."
                    });
            }
        }

        [Authorize]
        [HttpGet("{id:int}/historial")]
        public async Task<ActionResult<List<GastoHistorialDTO>>> GetHistorialGasto(int id)
        {
            try
            {
                var historial = await repositorio.SelectHistorialGasto(id);

                if (historial == null)
                {
                    return NotFound(new RespuestaDTO
                    {
                        mensaje = "El gasto no existe."
                    });
                }

                return Ok(historial);
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new RespuestaDTO
                    {
                        mensaje = "Ocurrió un error inesperado al consultar el historial del gasto."
                    });
            }
        }

        [Authorize]
        [HttpGet("{id:int}/detalle")]
        public async Task<ActionResult<GastoListadoDTO>> GetDetalleGasto(int id)
        {
            try
            {
                var gasto = await repositorio.SelectDetalleGasto(id);

                if (gasto == null)
                {
                    return NotFound(new RespuestaDTO
                    {
                        mensaje = "El gasto no existe o está dado de baja."
                    });
                }

                return Ok(gasto);
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new RespuestaDTO
                    {
                        mensaje = "Ocurrió un error inesperado al consultar el detalle del gasto."
                    });
            }
        }





    }
}