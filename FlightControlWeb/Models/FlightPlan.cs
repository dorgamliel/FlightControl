using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace FlightControlWeb.Models
{
    public class FlightPlan
    {
        [Key]
        [JsonIgnore]
        public long Flight_ID { get; set; }
        public int Passengers { get; set; }
        public string Company_Name { get; set; }
        public List<Segment> Segments { get; set; }
        public Initial_Location Initial_Location { get; set; }
    }
    public class Segment
    {
        public Coordinates Coordinates { get; set; }
        public int Timespan_Seconds { get; set; }
    }
    public class Initial_Location
    {
        public Coordinates Coordinates { get; set; }
        public DateTime Date_Time { get; set; }
    }
    public class Coordinates ///check if initial and segments coordinates should be two different properties.
    {
        public double Longitude { get; set; }
        public double Latitude { get; set; }
    }
}
