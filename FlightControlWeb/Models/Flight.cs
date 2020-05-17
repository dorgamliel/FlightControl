using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace FlightControlWeb.Models
{
    public class Flight
    {
        [Key]
        public long Flight_ID { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public int Passengers { get; set; }
        public string Company_Name { get; set; }
        public DateTime Date_Time { get; set; }
        public bool Is_External { get; set; }
    }
}
