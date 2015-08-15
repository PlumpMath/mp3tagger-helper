using System.Collections.Generic;

namespace tagRipper.Helpers
{
    public class Track
    {
        public string album_id { get; set; }

        public string album_title { get; set; }

        public string albumseokey { get; set; }

        public List<Artist> artist { get; set; }

        public string artwork { get; set; }

        public string artwork_large { get; set; }

        public string artwork_web { get; set; }

        public string content_source { get; set; }

        public int country { get; set; }

        public string display_global { get; set; }

        public string duration { get; set; }

        public List<Gener> gener { get; set; }

        public string http { get; set; }

        public string https { get; set; }

        public int is_most_popular { get; set; }

        public string isrc { get; set; }

        public string language { get; set; }

        public string lyrics_url { get; set; }

        public int mobile { get; set; }

        public string popularity { get; set; }

        public string rating { get; set; }

        public string rtmp { get; set; }

        public string rtsp { get; set; }

        public string seokey { get; set; }

        public string stream_type { get; set; }

        public TrackFormat track_format { get; set; }

        public string track_id { get; set; }

        public string track_title { get; set; }

        public string vendor { get; set; }

        public string video_url { get; set; }
    }
}