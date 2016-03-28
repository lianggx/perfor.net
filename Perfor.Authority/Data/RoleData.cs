using Perfor.Lib.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Perfor.Authority.Data
{
    /**
     * @ 角色实体
     * */
    public class RoleData
    {
        /**
         * @ 角色编号，主键
         * */
        [SQLEntityKey(PrimaryKey = true)]
        public string ID { get; set; }
        /**
         * @ 角色名称
         * */
        public string Name { get; set; }
        /**
         * @ 角色编码
         * */
        public string Description { get; set; }
        /**
         * @ 排序号
         * */
        public int Sort { get; set; }
    }
}
