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
            return await _context.FlightPlan.ToListAsync();
        }

        // GET: api/FlightPlan/5
        [HttpGet("{id}")]
        public async Task<ActionResult<FlightPlan>> GetFlightPlan(long id)
        {
            List<Segment> segments = new List<Segment>();
            var flightPlan = await _context.FlightPlan.FindAsync(id);
            //Add segments and initial location which have the same Flight_ID as flight plan.
            var seg = await _context.Segments.Where(x => x.Flight_ID == id).ToListAsync();
            var loc = await _context.InitialLocation.Where(x => x.Flight_ID == id).ToListAsync();
            if (flightPlan == null)
            {
                return NotFound();
            }
            //Adding segment to flight plan.
            flightPlan.Segments = seg;
            //Adding initial location to flight plan
            flightPlan.InitialLocation = loc.FirstOrDefault();

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
            //DateTime dt = flightPlan.InitialLocation.Date_Time.ToString("");
            //Adding flight ID for each segment which is related to flight plan.
            foreach (Segment seg in flightPlan.Segments)
            {
                seg.Flight_ID = flightPlan.FlightID;
            }
            //Adding flight plan to current flights.
            //_context.Flight.Add(new Flight()); ///add parameters.
            flightPlan.InitialLocation.Flight_ID = flightPlan.FlightID;
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
