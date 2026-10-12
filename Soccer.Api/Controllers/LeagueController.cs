
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Soccer.Api.Data;
using Soccer.Api.Models;

namespace Soccer.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeagueController : ControllerBase
    {
        private readonly SoccerContext _context;

        public LeagueController(SoccerContext context)
        {
            _context = context;
        }

        // GET: api/League
        /// <summary>
        ///  endpoint para obtener todas las ligas
        /// </summary>
        /// <returns>
        ///  una lista con todas las ligas existentes en la bd
        /// </returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<League>>> GetLeagues()
        {
            return await _context.Leagues.ToListAsync();
        }

        // GET: api/League/5
        /// <summary>
        /// endpoint para obtener una liga por su ID
        /// </summary>
        /// <param name="id">
        /// Numero entero que representa el ID de la liga que se desea obtener
        /// </param>
        /// <returns>
        /// La liga correspondiente al ID proporcionado, o un NotFound si no existe
        /// </returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<League>> GetLeague(int id)
        {
            var league = await _context.Leagues.FindAsync(id);

            if (league == null)
                return NotFound();

            return league;
        }

        // POST: api/League
        /// <summary>
        /// endpoint para crear una nueva liga
        /// </summary>
        /// <param name="league">
        /// Objeto de tipo League que contiene la información de la liga a crear
        /// </param>
        /// <returns>
        /// La liga creada
        /// </returns>
        [HttpPost]
        public async Task<ActionResult<League>> CreateLeague(League league)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Leagues.Add(league);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetLeague),
                new { id = league.Id },
                league
            );
        }

        
        // PUT: api/League/5
        /// <summary>
        /// Endpoint para actualizar una liga existente.
        /// </summary>
        /// <param name="id">ID de la liga que se desea actualizar.</param>
        /// <param name="updatedLeague">Datos nuevos de la liga.</param>
        /// <returns>204 si se actualiza; 404 si no existe.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLeague(
            int id,
            League updatedLeague)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var league = await _context.Leagues.FindAsync(id);

            if (league == null)
            {
                return NotFound();
            }

            league.Name = updatedLeague.Name;
            league.Country = updatedLeague.Country;
            league.StartDate = updatedLeague.StartDate;
            league.EndDate = updatedLeague.EndDate;
            league.Enabled = updatedLeague.Enabled;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        
        // DELETE: api/League/5
        /// <summary>
        /// Endpoint para eliminar una liga existente.
        /// </summary>
        /// <param name="id">ID de la liga que se desea eliminar.</param>
        /// <returns>204 si se elimina; 404 si no existe.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLeague(int id)
        {
            var league = await _context.Leagues.FindAsync(id);

            if (league == null)
            {
                return NotFound();
            }

            _context.Leagues.Remove(league);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}