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

        ///delete this.
        // GET: api/FlightPlan
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FlightPlan>>> GetFlightPlan()
        {
            return await _context.FlightPlan.Include(x => x.Segments).Include(x => x.InitialLocation).ToListAsync();
        }

        // GET: api/FlightPlan/5
        [HttpGet("{id}")]
        public async Task<ActionResult<FlightPlan>> GetFlightPlan(string id)
        {
            List<Segment> segments = new List<Segment>();
            var flightPlan = await _context.FlightPlan.Include(x => x.Segments).Include(x => x.InitialLocation).Where(x => String.Equals(id, x.FlightID)).FirstOrDefaultAsync();
            if (flightPlan == null)
            {
                return NotFound();
            }
            return flightPlan;
        }

        // POST: api/FlightPlan
        [HttpPost]
        public async Task<ActionResult<FlightPlan>> PostFlightPlan(FlightPlan flightPlan)
        {
            _context.FlightPlan.Add(flightPlan);
            //Generate a unique key for new flight.
            do
            {
                flightPlan.FlightID = FlightPlan.GenerateFlightKey();
                //As long as generated key is identical to a key in DB, generate a new one.
            } while (_context.FlightPlan.Count(x => x.FlightID == flightPlan.FlightID) > 0);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetFlightPlan", new { id = flightPlan.FlightID }, flightPlan);
        }

        private bool FlightPlanExists(string id)
        {
            return _context.FlightPlan.Any(e => String.Equals(e.FlightID, id));
        }
    }
}
