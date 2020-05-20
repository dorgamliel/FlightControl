using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlightControlWeb.Models;
using System.Drawing;

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
                    Flight flight = FlightPlanToFlight(fp);
                    var tupleLocation = UpdateFlightLocation(fp, fixedTime);
                    flight.Longitude = tupleLocation.Item1;
                    flight.Latitude = tupleLocation.Item2;
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
        //Checks if flight is active at given time.
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
        //Checks if given flight is in given time span.
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
        public Tuple<double, double> UpdateFlightLocation(FlightPlan fp, DateTime time)
        {
            //Current segment and end of current segment.
            int index = getCurrentSegment(fp, time);
            //Calculates the number of ticks until arriving to current segment.
            var difference = time.Ticks - FromDepatruteToSeg(fp, index).Ticks;
            //Distance (in seconds).
            var distance = TimeSpan.FromTicks(difference).TotalSeconds;
            Tuple<double, double> relativePoint = Interpolation(fp, index, distance);
            return relativePoint;
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
        //Get segment which plane is in (relative to time).
        public int getCurrentSegment(FlightPlan fp, DateTime time)
        {
            var difference = time.Ticks - fp.InitialLocation.Date_Time.Ticks;
            var seconds = TimeSpan.FromTicks(difference).TotalSeconds;
            int i = 0;
            foreach (var seg in fp.Segments)
            {
                //If remaining seconds are greater than current timespan seconds.
                if (seconds > seg.Timespan_Seconds)
                {
                    //Reduce remaining seconds (distance) from segment's seconds (distance).
                    seconds -= seg.Timespan_Seconds;
                } else
                //Else, return index of desired segment.
                {
                    return i;
                }
                i++;
            }
            return -1;
        }
        //Get a point based on interpolation of two segments and x axis of desired point.
        public Tuple<double, double> Interpolation (FlightPlan fp, int index, double distance)
        {
            Segment currSeg = null;
            if (index == 0)
            {
                currSeg = InitLocationToSeg(fp);
            }
            else
            {
                currSeg = fp.Segments[index - 1];
            }
            var endSeg = fp.Segments[index];
            //All variables for interpolation.
            var x0 = currSeg.Longitude;
            var y0 = currSeg.Latitude;
            var x1 = endSeg.Longitude;
            var y1 = endSeg.Latitude;
            var x = x0 + distance / endSeg.Timespan_Seconds;
            var y = y0 + (y1 - y0) * ((x - x0) / (x1 - x0));
            //Longitude and latitude of current flight.
            var point = Tuple.Create(x, y);
            return point;
        }
        //Measurement of number of seconds since departure until reaching current segment.
        public DateTime FromDepatruteToSeg(FlightPlan fp, int index)
        {
            int i;
            DateTime time = fp.InitialLocation.Date_Time;
            for (i = 0; i < index; i++)
            {
                time = time.AddSeconds(fp.Segments[i].Timespan_Seconds);
            }
            return time;
        }
        //Create a segment based on flight plan initial location.
        public Segment InitLocationToSeg(FlightPlan fp)
        {
            Segment seg = new Segment();
            seg.FlightID = fp.FlightID;
            seg.ID = -1;
            seg.Latitude = fp.InitialLocation.Latitude;
            seg.Longitude = fp.InitialLocation.Longitude;
            seg.Timespan_Seconds = 0;
            return seg;
        }
    }
}
