using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Perfor.Lib.Helpers
{
    /**
     * @ 实体键类
     * */
    public class SQLEntityKey : Attribute
    {
        /**
         * @ 是否主键
         * */
        public bool PrimaryKey { get; set; }
    }
}
