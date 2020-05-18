using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FlightControlWeb.Models
{
    public class FlightPlan
    {
        [Key]
        [Newtonsoft.Json.JsonIgnore]
        [JsonPropertyName("flight_id")]
        public long FlightID { get; set; }
        public int Passengers { get; set; }
        [JsonPropertyName("company_name")]
        public string CompanyName { get; set; }
        public List<Segment> Segments { get; set; }
        [JsonPropertyName("initial_location")]
        public InitialLocation InitialLocation { get; set; }
    }
   
    public class Segment
    {
        [Key]
        [Newtonsoft.Json.JsonIgnore]
        public int ID { get; set; }
        public long Flight_ID { get; set; }
        //public Coordinates Coordinates { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public int Timespan_Seconds { get; set; }
    }
    public class InitialLocation
    {
        [Key]
        [Newtonsoft.Json.JsonIgnore]
        public int ID { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public long Flight_ID { get; set; }
        //public Coordinates Coordinates { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public DateTime Date_Time { get; set; }
    }
    public class Coordinates ///check if initial and segments coordinates should be two different properties.
    {
        [Key]
        [Newtonsoft.Json.JsonIgnore]
        public int ID { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public long Flight_ID { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
    }
}
