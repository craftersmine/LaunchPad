using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using craftersmine.AppLauncher.SteamGridDb.Api;
using Newtonsoft.Json;

namespace craftersmine.AppLauncher.SteamGridDb
{
    public sealed class SteamGridDb
    {
        public const string ApiKey = "763883ca756f15cdf11a6712f9072e43";
        public const string ApiRoot = "https://www.steamgriddb.com/api/v2/";

        private HttpClient _httpClient;

        public static SteamGridDb Instance { get; private set; }

        public SteamGridDb()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(ApiRoot);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);


            Instance = this;
        }

        public async Task<SteamGridDbSearchResponse> SearchForGamesByName(string request)
        {
            try
            {
                string response = await _httpClient.GetStringAsync("search/autocomplete/" + request);

                var respObj = JsonConvert.DeserializeObject<SteamGridDbSearchResponse>(response);

                if (respObj.Success)
                    return respObj;
                return null;
            }
            catch (HttpRequestException e)
            {
                return null;
            }
        }

        public async Task<SteamGridDbGridsResponse> LoadGridsById(
            int gameId, 
            string dimensions = RequestParameters.Grids.Dimensions.W600H900, 
            string styles = "", 
            string mimes = RequestParameters.Grids.Mimes.Png, 
            string types = RequestParameters.Grids.Types.Static, 
            bool nsfw = false, 
            bool humor = false)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(styles))
                    styles = "&styles=" + styles;

                string reqUri =
                    $"grids/game/{gameId}?dimensions={dimensions}{styles}&mimes={mimes}&types={types}&nsfw={nsfw.ToString().ToLower()}&humor={humor.ToString().ToLower()}";

                string response = await _httpClient.GetStringAsync(reqUri);
                var respObj = JsonConvert.DeserializeObject<SteamGridDbGridsResponse>(response);

                return respObj;
            }
            catch (HttpRequestException e)
            {
                return null;
            }
        }
    }
}
