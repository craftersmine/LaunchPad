using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace craftersmine.AppLauncher.SteamGridDb
{
    public class SteamAuthor
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("steam64")]
        public string Steam64Id { get; set; }
        [JsonProperty("avatar")]
        public string AvatarUrl { get; set; }
    }
}
