using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Perfor.Lib.Enums
{
    /**
     * @ 事务状态
     * */
    public enum TranSactionState
    {
        /**
         *  事务未启用
         * */
        Off = 0,
        /**
         * @ 开始事务
         * */
        Begin = 10,
        /**
         * 提交事务
         * */
        Commit = 20,
        /**
         *  回滚
         * */
        Rollback = 30
    }
}
