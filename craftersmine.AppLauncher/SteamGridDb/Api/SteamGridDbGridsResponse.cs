using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace craftersmine.AppLauncher.SteamGridDb.Api
{
    public class SteamGridDbGridsResponse : SteamGridDbObject
    {
        [JsonProperty("data")]
        public SteamGridDbGridCover[] Covers { get; set; }
    }
}
