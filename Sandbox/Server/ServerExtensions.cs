using Sandbox.Server.Data;
using Sandbox.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sandbox.Server
{
    public static class ServerExtensions
    {

        public static void ThrowIfCantAccessFile(this Individual individual, ISQL sql, int fileid, bool check_upload, bool check_download)
        {
            var result = sql.StoredProc("[cam].box_files_permission",
                new
                {
                    userid = individual.ID,
                    useridsource = individual.IDType,
                    fileid
                }).First();
            if (check_upload && !(bool)result["IsUploader"])
                throw new Exception("Can't upload");
            if (check_download && !((bool)result["IsRequester"] || (bool)result["IsUploader"] || (bool)result["IsAudience"]))
                throw new Exception("Can't download");
        }
    }
}
