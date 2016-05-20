using Perfor.Lib.Enums;
using Perfor.Lib.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Perfor.Forms.Test.example
{
    public partial class Customers
    {
        [JsonProperty(PropertyName = "CID")]
        [SQLEntityKey(PrimaryKey = true)]
        public string ID { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string UserName { get; set; }
        [JsonProperty(PropertyName = "sex")]
        public int Gender { get; set; }
        [JsonProperty(PropertyName = "phone")]
        public string Phone { get; set; }
        [JsonProperty(PropertyName = "time")]
        public DateTime LastModifyTime { get; set; }
        //[JsonProperty(PropertyName = "islock")]
        //public bool Enabled { get; set; }
        //public SQLOption SQLOption { get; set; }
    }
}
