using System.Collections.Generic;
using Newtonsoft.Json;

namespace tagRipper.Helpers
{
    public class raagaMeta
    {
        public int count { get; set; }

        public List<Track> tracks { get; set; }

        public int user_token_status { get; set; }

        [JsonProperty("user-token-status")]
        public int user_token_status_hyphen { get; set; }
    }
}