using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Perfor.Lib.Helpers;

namespace Perfor.Authority.Data
{
    /**
     * @ 菜单实体类
     * */
    public class MenuData
    {
        /**
         * @ 上级菜单ID
         * */
        public string PID { get; set; }
        /**
         * @ 菜单ID
         * */
        [SQLEntityKey(PrimaryKey = true)]
        public string ID { get; set; }
        /**
         * @ 菜单名称
         * */
        public string Name { get; set; }
        /**
         * @ 上级路径链，如1.2.3.4
         * */
        public string ParentPath { get; set; }
        /**
         * @ 菜单URL
         * */
        public string Url { get; set; }
        /**
         * @ 层级
         * */
        public int Level { get; set; }
        /**
         * @ 排序号
         * */
        public int Sort { get; set; }
    }
}
