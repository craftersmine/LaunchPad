using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace craftersmine.AppLauncher.SteamGridDb
{
    public sealed class SteamGridDbGame
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("types")]
        public string[] Types { get; set; }
        [JsonProperty("verified")]
        public bool Verified { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
