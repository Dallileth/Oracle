using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox.Shared.Models
{
    public class ProblemException : Exception
    {
        public HttpStatusCode Status { get; init; }
        public string Detail
        {
            get; init;
        }
        public string Title
        {
            get; init;
        }

        public ProblemException(HttpStatusCode code, string detail, string title = null) : base()
        {
            Status = code;
            Detail = detail;
            Title = title;
        }
        public ProblemException(string detail, string title = null) : base()
        {
            Status = HttpStatusCode.BadRequest;
            Detail = detail;
            Title = title;
        }
    }
}
