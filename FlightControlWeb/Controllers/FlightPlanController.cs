using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlightControlWeb.Models;
using System.Collections.Immutable;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

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
            var flightPlan = await _context.FlightPlan.Include(x => x.Segments).Include(x => x.InitialLocation).Where(x => String.Equals(id, x.FlightID)).FirstOrDefaultAsync();
            if (flightPlan != null)
            {
                return flightPlan;
            }
            //TODO: handle case where infinite loop may happen.
            var address = await Task.Run(() => FindFlightServer(id));
            var response = await Task.Run(() => GetFlightPlanFromExtServer(address, id));
            try
            {
                flightPlan = ParseFlightPlanFromResponse(response);
                if (flightPlan.FlightID.Equals("null"))
                {
                    flightPlan.FlightID = id;
                }
                return flightPlan;
            }
            catch
            {
                return NotFound();
            }
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

        private string FindFlightServer(string id)
        {
            var serverURL = _context.IdToServer.Find(id);
            return serverURL.ServerURL;
        }

        private HttpWebResponse GetFlightPlanFromExtServer(string address, string id)
        {
            var fullAddress = address + "api/FlightPlan/" + id;
            string extURL = String.Format(fullAddress);
            WebRequest request = WebRequest.Create(extURL);
            request.Method = "GET";
            HttpWebResponse response = null;
            //Getting a response from external API.
            response = (HttpWebResponse)request.GetResponse();
            return response;
        }

        private FlightPlan ParseFlightPlanFromResponse(HttpWebResponse response)
        {
            string jsonText = null;
            //Creating a stream object from external API response.
            using (Stream stream = response.GetResponseStream())
            {
                StreamReader sr = new StreamReader(stream);
                //Assigning JSON from external API to a string.
                jsonText = sr.ReadToEnd();
                sr.Close();
            }
            var dezerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                }
            };
            var flightPlan = JsonConvert.DeserializeObject<FlightPlan>(jsonText, dezerializerSettings);
            return flightPlan;
        }
    }
}
