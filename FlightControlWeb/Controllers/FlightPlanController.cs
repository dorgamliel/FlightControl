using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlightControlWeb.Models;
using System.Collections.Immutable;

namespace FlightControlWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FlightPlanController : ControllerBase
    {
        private readonly FlightDbContext _context;

        public FlightPlanController(FlightDbContext context)
        {
            _context = context;
        }

        // GET: api/FlightPlan
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FlightPlan>>> GetFlightPlan()
        {
            return await _context.FlightPlan.Include(x => x.Segments).Include(x => x.InitialLocation).ToListAsync();
        }

        // GET: api/FlightPlan/5
        [HttpGet("{id}")]
        public async Task<ActionResult<FlightPlan>> GetFlightPlan(long id)
        {
            List<Segment> segments = new List<Segment>();
            var flightPlan = await _context.FlightPlan.Include(x => x.Segments).Include(x => x.InitialLocation).Where(x => x.FlightID == id).FirstOrDefaultAsync();
            if (flightPlan == null)
            {
                return NotFound();
            }
            return flightPlan;
        }

        // PUT: api/FlightPlan/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutFlightPlan(long id, FlightPlan flightPlan)
        {
            if (id != flightPlan.FlightID)
            {
                return BadRequest();
            }

            _context.Entry(flightPlan).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FlightPlanExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/FlightPlan
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<FlightPlan>> PostFlightPlan(FlightPlan flightPlan)
        {
            _context.FlightPlan.Add(flightPlan);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetFlightPlan", new { id = flightPlan.FlightID }, flightPlan);
        }

        // DELETE: api/FlightPlan/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<FlightPlan>> DeleteFlightPlan(long id)
        {
            var flightPlan = await _context.FlightPlan.FindAsync(id);
            if (flightPlan == null)
            {
                return NotFound();
            }

            _context.FlightPlan.Remove(flightPlan);
            await _context.SaveChangesAsync();

            return flightPlan;
        }

        private bool FlightPlanExists(long id)
        {
            return _context.FlightPlan.Any(e => e.FlightID == id);
        }
    }
}
