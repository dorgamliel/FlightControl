using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Drawing;

namespace FlightControlWeb.Models
{
    public class Flight
    {
        [Key]
        [JsonPropertyName("flight_id")]

        public long FlightID { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public int Passengers { get; set; }
        [JsonPropertyName("company_name")]

        public string CompanyName { get; set; }
        public DateTime Date_Time { get; set; }
        public bool Is_External { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public Point Location { get; set; }
    }
}
