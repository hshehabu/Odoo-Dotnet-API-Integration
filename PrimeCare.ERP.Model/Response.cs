using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimeCare.ERP.Model
{

    // { "Ex":null, "Message":"Successfully Posted.", "Success":true, "Content":"I-0000004" }
    public class Response
    {
        public Exception Ex { get; set; }
        public string Message { get; set; }
        public bool Success { get; set; }
        public string Content { get; set; }
    }
}