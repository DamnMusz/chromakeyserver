using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChromaKeyServer.Models
{
    public class Init
    {
        public string ORIGINAL_VID_PATH { get; set; }
        public string BACKGROUNDS_PATH { get; set; }
        public string BACKGROUNDS_EXTENSION { get; set; }
        public string OUTPUT_VID_PATH { get; set; }
        public string OUTPUT_VID_EXTENSION { get; set; }
        public string OUTPUT_THUMBNAIL_RELATIVE_PATH { get; set; }
        public string FFMPEG_BIN_PATH { get; set; }
        public string COLORKEY { get; set; }
        public string SIMILARITY { get; set; }
        public string BLEND { get; set; }
        public string FRAME_RATE_RELATION { get; set; }
    }
}