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
        //[System.Text.Json.Serialization.JsonIgnore]
        [JsonPropertyName("flight_id")]
        public string FlightID { get; set; } = "null";
        [JsonPropertyName("passengers")]
        public int Passengers { get; set; }
        [JsonPropertyName("company_name")]
        public string CompanyName { get; set; }
        [JsonPropertyName("initial_location")]
        public InitialLocation InitialLocation { get; set; }
        [JsonPropertyName("segments")]
        public List<Segment> Segments { get; set; }
        //Generates unique key to flights. format: XX000 (X - Capital letter, 0 - digit).
        public static string GenerateFlightKey()
        {
            string key = "";
            int randForChar;
            Random random = new Random();
            //Generate two capital letters.
            randForChar = random.Next(0, 26);
            key += ((char)('A' + randForChar)).ToString();
            randForChar = random.Next(0, 26);
            key += ((char)('A' + randForChar)).ToString();
            //Generate three numbers.
            key += random.Next(0, 9).ToString();
            key += random.Next(0, 9).ToString();
            key += random.Next(0, 9).ToString();
            key += random.Next(0, 9).ToString();
            return key;
        }
    }
    public class InitialLocation
    {
        [Key]
        [System.Text.Json.Serialization.JsonIgnore]
        public long ID { get; set; }
        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }
        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }
        [JsonPropertyName("date_time")]
        public DateTime DateTime { get; set; }
    }
    public class Segment
    {
        [Key]
        [System.Text.Json.Serialization.JsonIgnore]
        public long ID { get; set; }
        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }
        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }
        [JsonPropertyName("timespan_seconds")]
        public int TimespanSeconds { get; set; }
    }
}
