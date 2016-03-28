using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Perfor.Lib.Enums
{
    /**
     * @ 数据操作类型
     * */
    public enum SQLOption
    {
        /**
         * @ 普通操作
         * */
        NORMAL = 1000,
        /**
         * @ 创建
         * */
        CREATE = 1001,
        /**
         * @ 更新
         * */
        UPDATE = 2001,
        /**
         * @ 读取
         * */
        READ = 3001,
        /**
         * @ 删除
         * */
        DELETE = 4001
    }
}
