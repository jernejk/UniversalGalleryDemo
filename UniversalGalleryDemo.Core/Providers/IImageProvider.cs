using System.Collections.Generic;
using System.Threading.Tasks;

namespace UniversalGalleryDemo.Core.Providers
{
    public interface IImageProvider
    {
        Task<List<string>> GetImages(string query, string sort, int startFrom, string additional);
    }
}
