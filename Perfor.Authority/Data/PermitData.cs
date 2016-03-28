using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Perfor.Authority.Data
{
    /**
     * @ 权限类型
     * */
    public class PermitData
    {
        /**
         * @ 类型编号，主键
         * */
        public string ID { get; set; }
        /**
         * @ 标题
         * */
        public string Title { get; set; }
        /**
         * @ 内容
         */
        public string Content { get; set; }
        /**
         * @ 描述/备注
         * */
        public string Description { get; set; }
    }
}
