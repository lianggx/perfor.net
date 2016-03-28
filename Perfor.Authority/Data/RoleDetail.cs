using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Perfor.Authority.Data
{
    /**
     * @ 角色和权限类型关联表
     * */
    public class RoleDetail
    {
        /**
         * @ 角色ID
         * */
        public string Role_ID { get; set; }
        /**
         * @ 权限类型ID
         * */
        public string Detail_ID { get; set; }
        /**
         * @ 项目类型
         */
        public int Type { get; set; }
    }
}
