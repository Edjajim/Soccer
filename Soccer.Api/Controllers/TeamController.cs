
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Soccer.Api.Data;
using Soccer.Api.Models;

namespace Soccer.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TeamController : ControllerBase
    {
        private readonly SoccerContext _context;

        public TeamController(SoccerContext context)
        {
            _context = context;
        }

        // GET: api/Team
        /// <summary>
        ///  endpoint para obtener todas los teams
        /// </summary>
        /// <returns>
        ///  una lista con todos los temas existentes en la bd
        /// </returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Team>>> GetTeams()
        {
            return await _context.Teams.ToListAsync();
        }

        // GET: api/Team/5
        /// <summary>
        /// endpoint para obtener un team por su ID
        /// </summary>
        /// <param name="id">
        /// Numero entero que representa el ID del tema que se desea obtener
        /// </param>
        /// <returns>
        /// El team correspondiente al ID proporcionado, o un NotFound si no existe
        /// </returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Team>> GetTeam(int id)
        {
            var Team = await _context.Teams.FindAsync(id);

            if (Team == null)
                return NotFound();

            return Team;
        }

        // POST: api/Team
        /// <summary>
        /// endpoint para crear un nuevo team
        /// </summary>
        /// <param name="Team">
        /// Objeto de tipo Team que contiene la información del team a crear
        /// </param>
        /// <returns>
        /// El team creada
        /// </returns>
        [HttpPost]
        public async Task<ActionResult<Team>> CreateTeam(Team Team)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Teams.Add(Team);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetTeam),
                new { id = Team.Id },
                Team
            );
        }

        
        // PUT: api/Team/5
        /// <summary>
        /// Endpoint para actualizar un team existente.
        /// </summary>
        /// <param name="id">ID del team que se desea actualizar.</param>
        /// <param name="updatedTeam">Datos nuevos del team.</param>
        /// <returns>204 si se actualiza; 404 si no existe.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTeam(
            int id,
            Team updatedTeam)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var Team = await _context.Teams.FindAsync(id);

            if (Team == null)
            {
                return NotFound();
            }

            Team.Name = updatedTeam.Name;
            Team.Country = updatedTeam.Country;
            Team.PlayersQuantity = updatedTeam.PlayersQuantity;
            Team.Enabled = updatedTeam.Enabled;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        
        // DELETE: api/Team/5
        /// <summary>
        /// Endpoint para eliminar un team existente.
        /// </summary>
        /// <param name="id">ID del team que se desea eliminar.</param>
        /// <returns>204 si se elimina; 404 si no existe.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTeam(int id)
        {
            var Team = await _context.Teams.FindAsync(id);

            if (Team == null)
            {
                return NotFound();
            }

            _context.Teams.Remove(Team);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}