using System;

namespace UniversalGalleryDemo.Core.Server
{
    public class HttpGalleryRequest : EventArgs
    {
        public string Query { get; set; }

        public string Sort { get; set; }

        public int DelayInMilliseconds { get; set; }

        public string Additional { get; set; }
    }
}
