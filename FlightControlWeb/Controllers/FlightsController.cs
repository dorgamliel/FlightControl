using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlightControlWeb.Models;

namespace FlightControlWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FlightsController : ControllerBase
    {
        private readonly FlightDbContext _context;

        public FlightsController(FlightDbContext context)
        {
            _context = context;
        }
        // GET: api/Flights?relative_to=<DATE_TIME>
        [HttpGet]
        public async Task<ActionResult<List<FlightPlan>>> GetFlight(DateTime relative_to)
        {
            List<FlightPlan> activeFlights = new List<FlightPlan>();
            foreach (var fp in _context.FlightPlan)
            {
                var seg = await _context.Segments.Where(x => x.Flight_ID == fp.FlightID).ToListAsync();
                fp.Segments = seg;
                //Getting departure time from each flight plan.
                var location = await _context.InitialLocation.Where(x => x.Flight_ID == fp.FlightID).ToListAsync();
                fp.InitialLocation = location.FirstOrDefault();
                //If flight is active, add it to list of active flights.
                if (IsActiveFlight(fp, relative_to))
                {
                    activeFlights.Add(fp);
                }
            }

            if (activeFlights == null)
            {
                return NotFound();
            }

            return activeFlights;
        }

        // PUT: api/Flights/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutFlight(long id, Flight flight)
        {
            if (id != flight.Flight_ID)
            {
                return BadRequest();
            }

            _context.Entry(flight).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FlightExists(id))
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

        // POST: api/Flights
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<Flight>> PostFlight(Flight flight)
        {
            _context.Flight.Add(flight);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetFlight", new { id = flight.Flight_ID }, flight);
        }

        // DELETE: api/Flights/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Flight>> DeleteFlight(long id)
        {
            var fp = await _context.FlightPlan.FindAsync(id);
            var flight = await _context.Flight.FindAsync(id);
            var loc = await _context.InitialLocation.Where(x => x.Flight_ID == id).ToListAsync();
            var segs = await _context.Segments.Where(x => x.Flight_ID == id).ToListAsync();
            var loc1 = loc.FirstOrDefault();
            if (fp == null)
            {
                return NotFound();
            }
            //Remove all related segments from db.
            foreach (var segment in segs)
            {
                _context.Segments.Remove(segment);
            }
            _context.InitialLocation.Remove(loc1);
            _context.FlightPlan.Remove(fp);
            //_context.Flight.Remove(flight);
            await _context.SaveChangesAsync();

            return flight;
        }

        private bool FlightExists(long id)
        {
            return _context.Flight.Any(e => e.Flight_ID == id);
        }

        public bool IsActiveFlight(FlightPlan fp, DateTime relative_to)
        {
            //If departure time precedes relative time.
            if (fp.InitialLocation.Date_Time.Ticks <= relative_to.Ticks)
            {
                //Return true if flight hasn't finished yet.
                return InTimeSpan(fp, relative_to);
            }
            return false;
        }
        public bool InTimeSpan(FlightPlan fp, DateTime relative_to)
        {
            long totalTime = 0;
            //Sum all segments timespan.
            foreach (var seg in fp.Segments)
            {
                totalTime += seg.Timespan_Seconds;
            }
            //Add sum of segments timespan to initial time of departure.
            DateTime time = fp.InitialLocation.Date_Time.AddSeconds(totalTime);
            //Check if a specific flight is in the required timespan.
            if (DateTime.Compare(time, relative_to) > 0)
            {
                return true;
            }
            return false;
        }
    }
}
