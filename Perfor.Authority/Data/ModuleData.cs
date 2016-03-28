using Perfor.Lib.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Perfor.Authority.Data
{
    /**
     * @ 模块操作实体类
     * */
    public class ModuleData
    {
        /**
         * @ 操作编号，主键
         * */
        [SQLEntityKey(PrimaryKey = true)]
        public string ID { get; set; }
        /**
         * @ 操作名称
         * */
        public string Name { get; set; }
        /**
         * @ 所属命名空间
         * */
        public string NameSpace { get; set; }
        /**
         * @ 函数名称
         * */
        public string Method { get; set; }
        /**
         * @ 操作编码
         * */
        public string Code { get; set; }
    }
}
