using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Perfor.Lib.Models
{
    public class CallResult
    {
        public int Code { get; set; }
        public string Message { get; set; }
        public bool RetSucceed { get; set; }
        public object Source { get; set; }
        public bool Succeed { get; set; }
    }
}
