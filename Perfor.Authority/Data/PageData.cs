using Perfor.Lib.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Perfor.Authority.Data
{
    /**
        * @ 页面元素实体类
        * */
    public class PageData
    {
        /**
         * @ 页面元素编号，自动主键，关联权限类型
         * */
        [SQLEntityKey(PrimaryKey = true)]
        public string ID { get; set; }
        /**
         * @ 页面元素名称
         * */
        public string Name { get; set; }
        /**
         * @ 页面元素编号，自编号
         * */
        public string Code { get; set; }
    }
}
