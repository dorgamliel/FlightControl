using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace FlightControlWeb.Models
{
    public class IDToServer
    {
        [Key]
        public string FlightID { get; set; }
        public string ServerURL { get; set; }
    }
}
