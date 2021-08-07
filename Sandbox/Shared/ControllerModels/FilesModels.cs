using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox.Shared.ControllerModels
{
    public class FileCreateBody
    {
        public string Filename { get; set; }
    }
    public class FilePostResponse
    {
        public bool Worked { get; set; }
    }

    public class FilePostBody
    {
        public int FileID { get; set; }
        public byte[] Data { get; set; }
        public string FileType { get; set; }
        public string FileExt { get; set; }

    }

    public class FilesAccessTokenResponse
    {
        public string Token { get; set; }
        public string FileName { get; set; }
    }


    //todo: use app config?
    public static class FilesSettings
    {
        public const int MaxSizeBytes = 25 * 1000000;  //25mb
        public static string ToStringBytes(this long sizebytes)
        {
                return
                    sizebytes < 1000 ? $"{(int)(sizebytes)} B" :
                    sizebytes < 1000000 ? $"{(int)(sizebytes / (1000.0))} KB" :
                    sizebytes < 1000000000 ? $"{(int)(sizebytes / (1000000.0))} MB" :
                    $"{(int)(sizebytes / 1000000000.0)} GB";
        }
    }
}
