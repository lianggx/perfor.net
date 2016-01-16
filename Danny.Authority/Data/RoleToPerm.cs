using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Danny.Authority.Data
{
    /**
     * @ 角色和权限类型关联表
     * */
    public class RoleToPerm
    {
        /**
         * @ 角色ID
         * */
        public string Role_ID { get; set; }
        /**
         * @ 权限类型ID
         * */
        public string Perm_ID { get; set; }
    }
}
