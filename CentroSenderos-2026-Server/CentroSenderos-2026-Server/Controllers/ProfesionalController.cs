using CentroSenderos_2026_BD.Datos.Entity;
using CentroSenderos_2026_Repositorio.Repositorios;
using CentroSenderos_2026_Shared.DTO;
using Microsoft.AspNetCore.Mvc;

namespace CentroSenderos_2026_Server.Controllers
{
    [ApiController]
    [Route("api/profesional")]
    public class ProfesionalController : ControllerBase
    {
        private readonly IProfesionalRepositorio repositorio;

        public ProfesionalController(IProfesionalRepositorio repositorio)
        {
            this.repositorio = repositorio;
        }

        [HttpGet("ListaProfesional")]
        public async Task<IActionResult> GetListaProfesionales()
        {
            var lista = await repositorio.SelectListaProfesional();
            if (lista == null || !lista.Any())
                return NotFound("No hay profesionales registrados.");

            return Ok(lista);
        }

        [HttpGet("Id/{id:int}")]
        public async Task<ActionResult<ProfesionalListadoDTO>> GetById(int id)
        {
            var tipoProvincia = await repositorio.SelectById(id);
            if (tipoProvincia is null)
            {
                return NotFound($"No existe el registro con el id: {id}.");
            }

            return Ok(tipoProvincia);
        }

        [HttpPost]
        public async Task<IActionResult> PostEvento([FromBody] ProfesionalDTO dto)
        {
            try
            {
                int id = await repositorio.InsertarProfesional(dto);
                return Ok(id);
            }
            catch (ApplicationException ex)
            {
                // Esto devuelve el mensaje controlado al cliente
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                // Errores no esperados
                return StatusCode(500, new { mensaje = "Error interno del servidor", detalle = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, ProfesionalListadoDTO DTO)
        {
            var entidad = new Profesional
            {
                Id = id,
                Nombre = DTO.Nombre,
                Area = DTO.Area,
                Cuit = DTO.Cuit,
                MP = DTO.MP,
                RNP = DTO.RNP,
                Telefono = DTO.Telefono
            };

            var resultado = await repositorio.Update(id, entidad);

            if (!resultado)
            {
                return BadRequest("Datos no válidos");
            }

            return Ok($"El registro con el id: {id} fue actualizado correctamente.");
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var resultado = await repositorio.Delete(id);
            if (!resultado)
            {
                return BadRequest("Datos no validos");
            }
            return Ok($"El registro con el id: {id} fue eliminado correctamente.");
        }

        //[HttpGet("ListaProfesionales")]
        //public async Task<ActionResult<List<ProfesionalListadoDTO>>> GetListaProfesional()
        //{
        //    var lista = await repositorio.SelectListaProfesional();
        //    if (lista == null)
        //    {
        //        return NotFound("No se encontro la lista, VERIFICAR.");
        //    }
        //    if (lista.Count == 0)
        //    {
        //        return Ok("No existen items en la lista en este momento");
        //    }
        //    return Ok(lista);
        //}
    }
}
