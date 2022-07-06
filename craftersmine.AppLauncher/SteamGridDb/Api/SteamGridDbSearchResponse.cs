using Newtonsoft.Json;

namespace craftersmine.AppLauncher.SteamGridDb.Api
{
    public class SteamGridDbSearchResponse : SteamGridDbObject
    {
        [JsonProperty("data")]
        public SteamGridDbGame[] Data { get; set; }
    }
}
