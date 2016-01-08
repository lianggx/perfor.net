using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Danny.Authority.Data
{
    /**
     * @ 权限和功能关系
     * */
    public class PermToFunc
    {
        /**
         * @ 权限编号，主键
         * */
        public string Perm_ID { get; set; }
        /**
         * @ 功能编号，主键
         * */
        public string Func_ID { get; set; }
        /**
         * @ 模块类型
         * */
        public FuncDataType DataType { get; set; }
    }
}
