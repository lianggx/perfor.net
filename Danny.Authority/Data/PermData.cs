using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Danny.Authority.Data
{
    /**
     * @ 权限类型
     * */
    public class PermData
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
         * @ 描述/备注
         * */
        public string Description { get; set; }
    }
}
