using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sandbox.Server.Data;
using Sandbox.Shared.ControllerModels;
using Sandbox.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sandbox.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Staff")]
    public class FilesController : ControllerBase
    {
        ISQL _sql;
        public FilesController(ISQL sql)
        {
            _sql = sql;
        }

        [HttpPost("CreateFile")]
        [Authorize(Roles = "Admin")]
        public int? CreateFile([FromBody] FileCreateBody body)
        {
            var res = _sql.StoredProc("cam.box_files_createrequest", new
            {
                filename = body.Filename,
                comment = "",
                requesterid = 0,
                uploaderid = 0,
                uploaderidsource = 0
            }).FirstOrDefault();


            return (int?)(res?["FileID"]);

        }


        //todo: use IActionResult instead of bool
        [RequestSizeLimit(FilesSettings.MaxSizeBytes)]
        [HttpPost()]
        public bool Post([FromBody] FilePostBody body)
        {
            try
            {
                var user = HttpContext.User.ToIndividual();
                //user.ThrowIfCantAccessFile(_sql, body.FileID, true, false);
                _sql.StoredProc("[cam].boxv2_files_upload",
                    new
                    {
                        fileid = body.FileID,
                        filetype = body.FileType,
                        fileext = body.FileExt,
                        payload = body.Data,
                        payloadsize = (int)body.Data.Length
                    }).FirstOrDefault();

                return true;
            }
            catch
            {
                return false;
            }
        }

        [HttpPost("downloadtoken")]
        public FilesAccessTokenResponse CreateDownloadToken([FromQuery] int fileid)
        {
            var user = HttpContext.User.ToIndividual();

            //user.ThrowIfCantAccessFile(_sql, fileid, true, false);
            var results = _sql.StoredProc(
                "[cam].boxv2_token_createorupdate",
                new
                {
                    id = user.ID,
                    idsource = (int)user.IDType,
                    useby = DateTime.Now.AddMinutes(1),
                    fileid = fileid
                }).FirstOrDefault();
            return
                new FilesAccessTokenResponse
                {
                    Token = (string)results["key"],
                    FileName = (string)results["filename"]
                };
        }

        [AllowAnonymous]
        [HttpGet()]
        public IActionResult Get([FromQuery] int fileid, [FromQuery] string access_token)
        {
            var user = HttpContext.User.ToIndividual();
            try
            {
                //user.ThrowIfCantAccessFile(_sql, fileid, false, true);
            }
            catch (Exception e)
            {
                Forbid(e.Message);
            }
            var result = _sql.StoredProc("[cam].boxv2_files_download",
                new
                {
                    userid = user.ID,
                    useridsource = user.IDType,
                    access_token = access_token
                }).FirstOrDefault();
            if (result is null)
                throw new Exception("no file");
            //call a stored procedure that does the following:
            //  delete from database where user/access_token exists
            //  if deleted a row, get the bytes
            return File((byte[])result["payload"], (string)result["filetype"], (string)result["filename"]);
        }


    }
}
