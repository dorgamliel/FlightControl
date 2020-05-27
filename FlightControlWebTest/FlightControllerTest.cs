using Microsoft.VisualStudio.TestTools.UnitTesting;
using FlightControlWeb.Controllers;
using Microsoft.EntityFrameworkCore;
using FlightControlWeb.Models;
using System.Collections.Generic;

namespace FlightControlWebTest
{
    [TestClass]
    public class FlightControllerTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            //Arrage
            var options = new DbContextOptionsBuilder<FlightDbContext>().
                UseInMemoryDatabase(databaseName: "dummy").Options;
            List<Segment> segs1 = new List<Segment>();
            segs1.Add(new Segment()
            {
                ID = 1,
                Latitude = 10,
                Longitude = 10,
                TimespanSeconds = 100
            }) ;
            List<Segment> segs2 = new List<Segment>();
            List<Segment> segs3 = new List<Segment>();
            List<Segment> segs4 = new List<Segment>();
            InitialLocation init1 = new InitialLocation()
            {
                ID = 1,
                Latitude = 10,
                Longitude = 10,
                DateTime = new System.DateTime()
            };
            InitialLocation init2 = new InitialLocation();
            InitialLocation init3 = new InitialLocation();
            InitialLocation init4 = new InitialLocation();
            FlightPlan fp1 = new FlightPlan()
            {
                CompanyName = "OneLine",
                InitialLocation = init1,
                Passengers = 123,
                Segments = segs1
            };
            FlightPlan fp2 = new FlightPlan()
            {
                CompanyName = "TwoLine",
                InitialLocation = init2,
                Passengers = 456,
                Segments = segs2
            };
            FlightPlan fp3 = new FlightPlan()
            {
                CompanyName = "ThreeLine",
                InitialLocation = init3,
                Passengers = 679,
                Segments = segs3
            };
            FlightPlan fp4 = new FlightPlan()
            {
                CompanyName = "FourLine",
                InitialLocation = init4,
                Passengers = 84,
                Segments = segs4
            };

            //Act
            using (var context = new FlightDbContext(options))
            {   
                context.FlightPlan.Add(fp1);
                context.SaveChanges();
            }

            //Assert
            using (var context = new FlightDbContext(options))
            {
                var controller = new FlightsController(context);
                var flights = controller.GetFlight(new System.DateTime(2020, 01, 01));
                Assert.Equals(1, flights.Result.Value.Count);
            }
        }
    }
}
