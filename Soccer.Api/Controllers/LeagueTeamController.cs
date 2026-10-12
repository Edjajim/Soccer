
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Soccer.Api.Data;
using Soccer.Api.Models;
using Soccer.Api.DTOs;

namespace Soccer.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeagueTeamController : ControllerBase
    {
        private readonly SoccerContext _context;

        public LeagueTeamController(SoccerContext context)
        {
            _context = context;
        }

        
        // GET: api/LeagueTeam/League/5
        /// <summary>
        /// Obtiene todos los equipos relacionados con una liga.
        /// </summary>
        /// <param name="id">
        /// Identificador de la liga.
        /// </param>
        /// <returns>
        /// Lista de equipos relacionados con la liga o NotFound si no existe la liga.
        /// </returns>
        [HttpGet("League/{id}")]
        public async Task<ActionResult<IEnumerable<Team>>> GetLeagueTeam(
            int id,
            CancellationToken cancellationToken)
        {
            var leagueExists = await _context.Leagues
                .AnyAsync(l => l.Id == id, cancellationToken);

            if (!leagueExists)
                return NotFound($"No existe una liga con el ID {id}.");

            var teams = await _context.Teams
                .AsNoTracking()
                .Where(t => t.LeagueTeams.Any(lt => lt.LeagueId == id))
                .ToListAsync(cancellationToken);

            return Ok(teams);
        }

        
        // GET: api/LeagueTeam/Team/5
        /// <summary>
        /// Obtiene todas las ligas en las que está inscrito un equipo.
        /// </summary>
        /// <param name="id">
        /// Identificador del equipo.
        /// </param>
        /// <returns>
        /// Lista de ligas a las que está inscrito el equipo
        /// </returns>
        [HttpGet("Team/{id}")]
        public async Task<ActionResult<IEnumerable<League>>> GetLeaguesByTeam(int id)
        {
            var teamExists = await _context.Teams
                .AnyAsync(t => t.Id == id);

            if (!teamExists)
                return NotFound($"No existe un equipo con el ID {id}.");

            var leagues = await _context.LeagueTeams
                .Where(lt => lt.TeamId == id)
                .Select(lt => lt.League)
                .ToListAsync();

            return Ok(leagues);
        }

        
        // POST: api/LeagueTeam
        /// <summary>
        /// Relaciona un equipo con una liga.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<LeagueTeam>> CreateLeagueTeam(
            CreateLeagueTeamDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Verificar que la liga exista
            var leagueExists = await _context.Leagues
                .AnyAsync(l => l.Id == dto.LeagueId);

            if (!leagueExists)
                return NotFound(
                    $"No existe una liga con el ID {dto.LeagueId}.");

            // Verificar que el equipo exista
            var teamExists = await _context.Teams
                .AnyAsync(t => t.Id == dto.TeamId);

            if (!teamExists)
                return NotFound(
                    $"No existe un equipo con el ID {dto.TeamId}.");

            // Evitar relaciones duplicadas
            var relationExists = await _context.LeagueTeams
                .AnyAsync(lt =>
                    lt.LeagueId == dto.LeagueId &&
                    lt.TeamId == dto.TeamId);

            if (relationExists)
                return Conflict(
                    "El equipo ya está inscrito en esta liga.");

            // Crear la relación
            var leagueTeam = new LeagueTeam
            {
                LeagueId = dto.LeagueId,
                TeamId = dto.TeamId
            };

            _context.LeagueTeams.Add(leagueTeam);
            await _context.SaveChangesAsync();

            return Created(
                $"/api/LeagueTeam/{dto.LeagueId}",
                new
                {
                    message = "Equipo inscrito correctamente en la liga.",
                    leagueTeam.LeagueId,
                    leagueTeam.TeamId
                });
        }
        
        // DELETE: api/LeagueTeam/5
        /// <summary>
        /// Endpoint para eliminar un LeagueTeam existente.
        /// </summary>
        /// <param name="id">ID del LeagueTeam que se desea eliminar.</param>
        /// <returns>204 si se elimina; 404 si no existe.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLeagueTeam(int id)
        {
            var LeagueTeam = await _context.LeagueTeams.FindAsync(id);

            if (LeagueTeam == null)
            {
                return NotFound();
            }

            _context.LeagueTeams.Remove(LeagueTeam);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}