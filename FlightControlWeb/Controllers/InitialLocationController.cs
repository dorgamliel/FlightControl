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
    public class InitialLocationController : ControllerBase
    {
        private readonly FlightDbContext _context;

        public InitialLocationController(FlightDbContext context)
        {
            _context = context;
        }

        // GET: api/InitialLocation
        [HttpGet]
        public async Task<ActionResult<IEnumerable<InitialLocation>>> GetInitialLocation()
        {
            return await _context.InitialLocation.ToListAsync();
        }

        // GET: api/InitialLocation/5
        [HttpGet("{id}")]
        public async Task<ActionResult<InitialLocation>> GetInitialLocation(int id)
        {
            var initialLocation = await _context.InitialLocation.FindAsync(id);

            if (initialLocation == null)
            {
                return NotFound();
            }

            return initialLocation;
        }

        // PUT: api/InitialLocation/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutInitialLocation(int id, InitialLocation initialLocation)
        {
            if (id != initialLocation.ID)
            {
                return BadRequest();
            }

            _context.Entry(initialLocation).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InitialLocationExists(id))
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

        // POST: api/InitialLocation
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<InitialLocation>> PostInitialLocation(InitialLocation initialLocation)
        {
            _context.InitialLocation.Add(initialLocation);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetInitialLocation", new { id = initialLocation.ID }, initialLocation);
        }

        // DELETE: api/InitialLocation/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<InitialLocation>> DeleteInitialLocation(int id)
        {
            var initialLocation = await _context.InitialLocation.FindAsync(id);
            if (initialLocation == null)
            {
                return NotFound();
            }

            _context.InitialLocation.Remove(initialLocation);
            await _context.SaveChangesAsync();

            return initialLocation;
        }

        private bool InitialLocationExists(int id)
        {
            return _context.InitialLocation.Any(e => e.ID == id);
        }
    }
}
