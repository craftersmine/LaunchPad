using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace craftersmine.AppLauncher.SteamGridDb.Api
{
    public class SteamGridDbError : SteamGridDbObject
    {
        [JsonProperty("errors")]
        public string[] Errors { get; set; }
    }
}
