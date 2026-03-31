using System.Collections.Generic;
using Newtonsoft.Json;

namespace MP3Player.Models
{
    public partial class AcoustIdResponse
    {
        [JsonProperty("status")]
        public string Status { get; set; }
        
        [JsonProperty("results")]
        public List<AcoustIdResult> Results { get; set; }

        public string GetBestTitle()
        {
            if (Results?.Count > 0 && Results[0].Recordings?.Count > 0)
            {
                return Results[0].Recordings[0].Title ?? string.Empty;
            }
            return string.Empty;
        }
        
        public string GetBestArtist()
        {
            if (Results?.Count > 0 && Results[0].Recordings?.Count > 0)
            {
                var recording = Results[0].Recordings[0];
                if (recording.Artists?.Count > 0)
                {
                    return recording.Artists[0].Name ?? string.Empty;
                }
            }
            return string.Empty;
        }
        
        public string GetBestAlbum()
        {
            if (Results?.Count > 0 && Results[0].Recordings?.Count > 0)
            {
                var recording = Results[0].Recordings[0];
                if (recording.ReleaseGroups?.Count > 0)
                {
                    return recording.ReleaseGroups[0].Title ?? string.Empty;
                }
            }
            return string.Empty;
        }
        
        public int GetBestYear()
        {
            if (Results?.Count > 0 && Results[0].Recordings?.Count > 0)
            {
                var recording = Results[0].Recordings[0];
                if (recording.ReleaseGroups?.Count > 0)
                {
                    var release = recording.ReleaseGroups[0];
                    if (!string.IsNullOrEmpty(release.SecondaryTypes?.FirstOrDefault()))
                    {
                        // 尝试从信息中提取年份
                        var yearStr = release.SecondaryTypes.First();
                        if (int.TryParse(yearStr, out int year) && year > 1900 && year <= DateTime.Now.Year + 1)
                        {
                            return year;
                        }
                    }
                }
            }
            return 0;
        }
        
        public string GetBestGenre()
        {
            if (Results?.Count > 0 && Results[0].Recordings?.Count > 0)
            {
                var recording = Results[0].Recordings[0];
                if (recording.ReleaseGroups?.Count > 0)
                {
                    var release = recording.ReleaseGroups[0];
                    return release.Type ?? string.Empty;
                }
            }
            return string.Empty;
        }

    }

    public class AcoustIdResult
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        
        [JsonProperty("recordings")]
        public List<Recording> Recordings { get; set; }
        
        [JsonProperty("score")]
        public double Score { get; set; }
    }

    public class Recording
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        
        [JsonProperty("title")]
        public string Title { get; set; }
        
        [JsonProperty("artists")]
        public List<Artist> Artists { get; set; }
        
        [JsonProperty("releasegroups")]
        public List<ReleaseGroup> ReleaseGroups { get; set; }
        
        [JsonProperty("duration")]
        public int? Duration { get; set; }
    }

    public class Artist
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class ReleaseGroup
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        
        [JsonProperty("title")]
        public string Title { get; set; }
        
        [JsonProperty("type")]
        public string Type { get; set; }
        
        [JsonProperty("secondarytypes")]
        public List<string> SecondaryTypes { get; set; }
    }
}