using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalGalleryDemo.Core.Models
{
    public class PictureResponse
    {
        public PictureListData[] data { get; set; }
        public string sql { get; set; }
        public int shown { get; set; }
        public string time { get; set; }
        public string total { get; set; }
        public int alt_total { get; set; }
    }

    public class PictureListData
    {
        public bool favorited { get; set; }
        public string id { get; set; }
        public string width { get; set; }
        public string height { get; set; }
        public string views { get; set; }
        public string colors { get; set; }
        public string favorites { get; set; }
    }

}
