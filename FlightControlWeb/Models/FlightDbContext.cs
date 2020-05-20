using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using FlightControlWeb.Models;

namespace FlightControlWeb.Models
{
    public class FlightDbContext : DbContext
    {
        public FlightDbContext(DbContextOptions<FlightDbContext> options)
            : base(options)
        {
        }
        public DbSet<FlightPlan> FlightPlan { get; set; }
        public DbSet<Flight> Flight { get; set; }
    }
}
