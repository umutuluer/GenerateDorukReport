using Newtonsoft.Json;
using System;

namespace GenerateDorukReport
{
    class Stopover
    {
        [JsonProperty("reason")]
        public string Reason { get; set; }

        [JsonProperty("start")]
        public DateTime Start { get; set; }

        [JsonProperty("end")]
        public DateTime End { get; set; }
    }
}