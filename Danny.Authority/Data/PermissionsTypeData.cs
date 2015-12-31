using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Danny.Authority.Data
{
    /**
     * @ 权限类型
     * */
    public class PermissionsTypeData
    {
        /**
         * @ 类型编号，主键
         * */
        public string ID { get; set; }
        /**
         * @ 类型名称
         * */
        public string Name { get; set; }
        /**
         * @ 类型编码
         * */
        public string Code { get; set; }
    }
}
