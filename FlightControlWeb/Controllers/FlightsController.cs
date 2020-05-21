using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlightControlWeb.Models;
using System.Drawing;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

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
            List<Flight> activeFlights = new List<Flight>();
            //Check if "sync_all" query string was put.
            string queryStr = Request.QueryString.Value;
            if (queryStr.Contains("sync_all"))
            {
                var externalFlights = HandleExternalServers(relative_to);
                activeFlights.AddRange(externalFlights);
                //return x (just like  "return flight" in flightscontroller).
                //return a LIST of all relevant flights rfom server. pay attention to relative time.
            }
            var flightPlans = await _context.FlightPlan.Include(x => x.Segments).Include(x => x.InitialLocation).ToListAsync();
            //Iterate all planned fligts and add relevant to list.
            foreach (var fp in flightPlans)
            {
                var fixedTime = TimeZoneInfo.ConvertTimeToUtc(relative_to);
                //Getting departure time from each flight plan.
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
        // DELETE: api/Flights/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Flight>> DeleteFlight(long id)
        {
            var fp = await _context.FlightPlan.FindAsync(id);
            var flight = await _context.Flight.FindAsync(id);
            if (fp == null)
            {
                return NotFound();
            }
            _context.FlightPlan.Remove(fp);
            _context.Flight.Remove(flight);
            await _context.SaveChangesAsync();

            return flight;
        }
        private bool FlightExists(long id)
        {
            return _context.Flight.Any(e => String.Equals(e.FlightID, id));
        }
        //Checks if flight is active at given time.
        public bool IsActiveFlight(FlightPlan fp, DateTime fixedTime)
        {
            //If departure time precedes relative time.
            if (fp.InitialLocation.DateTime.Ticks <= fixedTime.Ticks)
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
                totalTime += seg.TimespanSeconds;
            }
            //Add sum of segments timespan to initial time of departure.
            DateTime time = fp.InitialLocation.DateTime.AddSeconds(totalTime);
            //Check if a specific flight is in the required timespan.
            if (DateTime.Compare(time, fixedTime) > 0)
            {
                return true;
            }
            return false;
        }
        //Calling an external API.
        public List<Flight> HandleExternalServers(DateTime time)
        {
            List<Flight> allExtFlights = new List<Flight>();
            var set = _context.Server.Select(x => x.ServerURL).ToList();
            foreach (string address in set)
            {
                allExtFlights.AddRange(GetFlightsFromExtServer(address, time));
            }
            return allExtFlights;
        }
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
            flight.DateTime = fp.InitialLocation.DateTime;
            flight.IsExternal = false;
            return flight;
        }
        //Get segment which plane is in (relative to time).
        public int getCurrentSegment(FlightPlan fp, DateTime time)
        {
            var difference = time.Ticks - fp.InitialLocation.DateTime.Ticks;
            var seconds = TimeSpan.FromTicks(difference).TotalSeconds;
            int i = 0;
            foreach (var seg in fp.Segments)
            {
                //If remaining seconds are greater than current timespan seconds.
                if (seconds > seg.TimespanSeconds)
                {
                    //Reduce remaining seconds (distance) from segment's seconds (distance).
                    seconds -= seg.TimespanSeconds;
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
            var x = x0 + distance / endSeg.TimespanSeconds;
            var y = y0 + (y1 - y0) * ((x - x0) / (x1 - x0));
            //Longitude and latitude of current flight.
            var point = Tuple.Create(x, y);
            return point;
        }
        //Measurement of number of seconds since departure until reaching current segment.
        public DateTime FromDepatruteToSeg(FlightPlan fp, int index)
        {
            int i;
            DateTime time = fp.InitialLocation.DateTime;
            for (i = 0; i < index; i++)
            {
                time = time.AddSeconds(fp.Segments[i].TimespanSeconds);
            }
            return time;
        }
        //Create a segment based on flight plan initial location.
        public Segment InitLocationToSeg(FlightPlan fp)
        {
            Segment seg = new Segment();
            seg.ID = -1;
            seg.Latitude = fp.InitialLocation.Latitude;
            seg.Longitude = fp.InitialLocation.Longitude;
            seg.TimespanSeconds = 0;
            return seg;
        }
        //Get all relevant flights according to given time, from a given address.
        public List<Flight> GetFlightsFromExtServer(string address, DateTime time)
        {
            List<Flight> allExtFlights = new List<Flight>();
            //Making a string as a request for external server with a relative time.
            string fullAdd = address + "api/Flights/?relative_to=" + time.ToString();
            //Get json string from external server.
            string jsonText = GetJsonFromServer(fullAdd);
            //Handling difference between class prop. names and json prop. names.
            var dezerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                }
            };
            var extSrvFlights = JsonConvert.DeserializeObject<List<Flight>>(jsonText, dezerializerSettings);
            //Update flight as external and add to all active flights list.
            foreach (Flight flight in extSrvFlights)
            {
                flight.IsExternal = true;
                allExtFlights.Add(flight);
            }
            return allExtFlights;
        }
        //Getting Json string from given url address.
        public string GetJsonFromServer(string fullAdd)
        {
            //Assigning API path.
            string extURL = String.Format(fullAdd);
            WebRequest request = WebRequest.Create(extURL);
            request.Method = "GET";
            HttpWebResponse response = null;
            //Getting a response from external API.
            response = (HttpWebResponse)request.GetResponse();
            string jsonText = null;
            //Creating a stream object from external API response.
            using (Stream stream = response.GetResponseStream())
            {
                StreamReader sr = new StreamReader(stream);
                //Assigning JSON from external API to a string.
                jsonText = sr.ReadToEnd();
                sr.Close();
            }
            return jsonText;
        }
    }
}
