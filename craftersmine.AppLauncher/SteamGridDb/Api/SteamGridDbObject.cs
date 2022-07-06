using Newtonsoft.Json;

namespace craftersmine.AppLauncher.SteamGridDb.Api
{
    public abstract class SteamGridDbObject
    {
        [JsonProperty("success")]
        public bool Success { get; set; }
        [JsonProperty("errors")]
        public string[] Errors { get; set; }
    }
}
