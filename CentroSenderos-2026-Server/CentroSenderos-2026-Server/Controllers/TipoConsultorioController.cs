using CentroSenderos_2026_Repositorio.Repositorios;
using CentroSenderos_2026_Shared.DTO;
using Microsoft.AspNetCore.Mvc;

namespace CentroSenderos_2026_Server.Controllers
{
    [ApiController]
    [Route("api/tipoconsultorio")]
    public class TipoConsultorioController : ControllerBase
    {
        private readonly ITipoConsultorioRepositorio repositorio;

        public TipoConsultorioController(ITipoConsultorioRepositorio repositorio)
        {
            this.repositorio = repositorio;
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TipoConsultorioDTO>> GetById(int id)
        {
            var entidad = await repositorio.SelectPorId(id);
            if (entidad == null) return NotFound($"No existe consultorio con id {id}.");
            return Ok(entidad);
        }

        [HttpGet("ListaTipoConsultorio")]
        public async Task<IActionResult> GetListaTipoConsultorio()
        {
            var lista = await repositorio.SelectListaTipoConsultorio();
            if (lista == null || !lista.Any())
                return NotFound("No hay consultorios registrados.");
            return Ok(lista);
        }

        [HttpGet("Tipo/{cod}")]
        public async Task<ActionResult<TipoConsultorioListadoDTO>> SelectByTipo(string cod)
        {
            var entidad = await repositorio.SelectByTipoConsultorio(cod);
            if (entidad is null)
                return NotFound($"No existe el registro con el tipo: {cod}.");
            return Ok(entidad);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] TipoConsultorioDTO dto)
        {
            try
            {
                int id = await repositorio.InsertarTipoConsultorio(dto);
                return Ok(id);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno del servidor", detalle = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, TipoConsultorioDTO dto)
        {
            try
            {
                var resultado = await repositorio.ActualizarTipoConsultorio(id, dto);
                if (!resultado)
                    return NotFound($"No existe el consultorio con el id: {id}.");
                return Ok($"El registro con el id: {id} fue actualizado correctamente.");
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno del servidor", detalle = ex.Message });
            }
        }
    }
}


