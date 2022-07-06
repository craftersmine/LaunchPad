using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace craftersmine.AppLauncher.SteamGridDb
{
    public class SteamGridDbGridCover
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("score")]
        public int Score { get; set; }
        [JsonProperty("style")]
        public string Style { get; set; }
        [JsonProperty("width")]
        public int Width { get; set; }
        [JsonProperty("height")]
        public int Height { get; set; }
        [JsonProperty("nsfw")]
        public bool IsNsfw { get; set; }
        [JsonProperty("humor")]
        public bool IsHumorous { get; set; }
        [JsonProperty("notes")]
        public string Notes { get; set; }
        [JsonProperty("mime")]
        public string Mime { get; set; }
        [JsonProperty("language")]
        public string Language { get; set; }
        [JsonProperty("url")]
        public string FullImageUrl { get; set; }
        [JsonProperty("thumb")]
        public string ThumbnailImageUrl { get; set; }
        [JsonProperty("lock")]
        public bool Locked { get; set; }
        [JsonProperty("epilepsy")]
        public bool EpilepsyWarning { get; set; }
        [JsonProperty("upvotes")]
        public int Upvotes { get; set; }
        [JsonProperty("downvotes")]
        public int Downwotes { get; set; }
        [JsonProperty("author")]
        public SteamAuthor Author { get; set; }

        public async Task DownloadToAsync(string fullPath)
        {
            await Task.Run(async () => 
            {
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        var data = await client.GetByteArrayAsync(FullImageUrl);
                        using (FileStream file = new FileStream(fullPath, FileMode.Create, FileAccess.ReadWrite))
                        {
                            await file.WriteAsync(data, 0, data.Length);
                        }
                    }
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e);
                    throw;
                }
            });
        }
    }
}
