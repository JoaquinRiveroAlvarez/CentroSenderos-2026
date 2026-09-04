using CentroSenderos_2026_Repositorio.Repositorios;
using CentroSenderos_2026_Shared.DTO;
using Microsoft.AspNetCore.Mvc;

namespace CentroSenderos_2026_Server.Controllers
{
    [ApiController]
    [Route("api/tipoobrasocial")]
    public class TipoObraSocialController : ControllerBase
    {
        private readonly ITipoObraSocialRepositorio repositorio;

        public TipoObraSocialController(
            ITipoObraSocialRepositorio repositorio)
        {
            this.repositorio = repositorio;
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TipoObraSocialDTO>>
            GetById(int id)
        {
            var entidad =
                await repositorio.SelectPorId(id);

            if (entidad == null)
            {
                return NotFound(
                    $"No existe la obra social con el id {id}."
                );
            }

            return Ok(entidad);
        }

        [HttpGet("ListaTipoObraSocial")]
        public async Task<
            ActionResult<List<TipoObraSocialDTO>>>
            GetListaTipoObraSocial()
        {
            var lista =
                await repositorio
                    .SelectListaTipoObrasocial();

            return Ok(lista);
        }

        [HttpGet("Tipo/{tipo}")]
        public async Task<ActionResult<TipoObraSocialDTO>>
            SelectByTipo(string tipo)
        {
            var obraSocial =
                await repositorio
                    .SelectByTipoObraSocial(tipo);

            if (obraSocial == null)
            {
                return NotFound(
                    $"No existe la obra social '{tipo}'."
                );
            }

            return Ok(obraSocial);
        }

        [HttpPost]
        public async Task<ActionResult<int>> PostObraSocial(
            [FromBody] TipoObraSocialDTO dto)
        {
            try
            {
                var id =
                    await repositorio
                        .InsertarTipoObraSocial(dto);

                return Ok(id);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(
                    new { mensaje = ex.Message }
                );
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new
                    {
                        mensaje =
                            "Error interno del servidor",
                        detalle = ex.Message
                    }
                );
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(
            int id,
            [FromBody] TipoObraSocialDTO dto)
        {
            try
            {
                var resultado =
                    await repositorio
                        .ActualizarTipoObraSocial(
                            id,
                            dto
                        );

                if (!resultado)
                {
                    return NotFound(
                        $"No existe la obra social con el id {id}."
                    );
                }

                return Ok(new
                {
                    mensaje = "Actualizado correctamente",
                    id
                });
            }
            catch (ApplicationException ex)
            {
                return BadRequest(
                    new { mensaje = ex.Message }
                );
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new
                    {
                        mensaje =
                            "Error interno del servidor",
                        detalle = ex.Message
                    }
                );
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var resultado =
                await repositorio
                    .DeleteTipoObraSocial(id);

            if (!resultado)
            {
                return NotFound(
                    $"No existe la obra social con el id {id}."
                );
            }

            return Ok(new
            {
                mensaje = "Eliminado correctamente",
                id
            });
        }
    }
}