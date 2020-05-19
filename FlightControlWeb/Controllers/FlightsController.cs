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
        public async Task<ActionResult<List<Flight>>> GetFlight(DateTime relative_to)
        {
            //Check if "sync_all" query string was put.
            string queryStr = Request.QueryString.Value;
            if (queryStr.Contains("sync_all"))
            {
                ///HandleExternalServers();
            }
            List<Flight> activeFlights = new List<Flight>();
            //Iterate all planned fligts and add relevant to list.
            foreach (var fp in _context.FlightPlan)
            {
                var fixedTime = TimeZoneInfo.ConvertTimeToUtc(relative_to);
                ///var newfix = fixedTime.ToString("yyy-MM-ddTHH-mm:ssZ");   redundant?
                var seg = await _context.Segments.Where(x => x.FlightID == fp.FlightID).ToListAsync();
                fp.Segments = seg;
                //Getting departure time from each flight plan.
                var location = await _context.InitialLocation.Where(x => x.FlightID == fp.FlightID).ToListAsync();
                fp.InitialLocation = location.FirstOrDefault();
                //If flight is active, add it to list of active flights, with current location.
                if (IsActiveFlight(fp, fixedTime))
                {
                    ///UpdateFlightLocation(fp);
                    Flight flight = FlightPlanToFlight(fp);
                    activeFlights.Add(flight);
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
            if (id != flight.FlightID)
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

            return CreatedAtAction("GetFlight", new { id = flight.FlightID }, flight);
        }

        // DELETE: api/Flights/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Flight>> DeleteFlight(long id)
        {
            var fp = await _context.FlightPlan.FindAsync(id);
            var flight = await _context.Flight.FindAsync(id);
            var locations = await _context.InitialLocation.Where(x => x.FlightID == id).ToListAsync();
            var segs = await _context.Segments.Where(x => x.FlightID == id).ToListAsync();
            var location = locations.FirstOrDefault();
            if (fp == null)
            {
                return NotFound();
            }
            //Remove all related segments from db.
            foreach (var segment in segs)
            {
                _context.Segments.Remove(segment);
            }
            _context.InitialLocation.Remove(location);
            _context.FlightPlan.Remove(fp);
            //_context.Flight.Remove(flight);
            await _context.SaveChangesAsync();

            return flight;
        }
        private bool FlightExists(long id)
        {
            return _context.Flight.Any(e => e.FlightID == id);
        }

        public bool IsActiveFlight(FlightPlan fp, DateTime fixedTime)
        {
            //If departure time precedes relative time.
            if (fp.InitialLocation.Date_Time.Ticks <= fixedTime.Ticks)
            {
                //Return true if flight hasn't finished yet.
                return InTimeSpan(fp, fixedTime);
            }
            return false;
        }
        public bool InTimeSpan(FlightPlan fp, DateTime fixedTime)
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
            if (DateTime.Compare(time, fixedTime) > 0)
            {
                return true;
            }
            return false;
        }
        //public void HandleExternalServers() {}

        //Update flight current location using linear interpulation.
        public void UpdateFlightLocation(FlightPlan fp)
        {
            //starting point: beginning of segment being in.
        }
        //Convert FlightLocation object to Flight object.
        public Flight FlightPlanToFlight(FlightPlan fp)
        {
            Flight flight = new Flight();
            flight.FlightID = fp.FlightID;
            flight.Longitude = fp.InitialLocation.Longitude;
            flight.Latitude = fp.InitialLocation.Latitude;
            flight.Passengers = fp.Passengers;
            flight.CompanyName = fp.CompanyName;
            flight.Date_Time = fp.InitialLocation.Date_Time;
            flight.Is_External = false;
            return flight;
        }
    }
}
