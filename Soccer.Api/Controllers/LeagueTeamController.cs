
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
        /// Obtiene todos los equipos inscritos en una liga específica.
        /// </summary>
        /// <param name="id">Identificador de la liga de la que se desea consultar los equipos registrados</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Una lista de los equipos inscritos en la liga</returns>
        [HttpGet("League/{id}")]
        public async Task<ActionResult<IEnumerable<TeamDto>>> GetLeagueTeam(
        int id,
        CancellationToken cancellationToken)
        {
            var leagueExists = await _context.Leagues
                .AnyAsync(l => l.Id == id, cancellationToken);

            if (!leagueExists)
                return NotFound($"No existe una liga con el ID {id}.");

            var teams = await _context.LeagueTeams
                .AsNoTracking()
                .Where(lt => lt.LeagueId == id)
                .Select(lt => new TeamDto
                {
                    Id = lt.Team.Id,
                    Name = lt.Team.Name,
                    Country = lt.Team.Country,
                    PlayersQuantity = lt.Team.PlayersQuantity,
                    Enabled = lt.Team.Enabled
                })
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
        /// Endpoint para inscribir un equipo en una liga.
        /// </summary>
        /// <param name="dto">Utiliza el DTO para crear la relación entre la liga y el equipo.</param>
        /// <returns>200 si se crea la relación correctamente; 400 si los datos son inválidos; 404 si la liga o el equipo no existen.</returns>
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