using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Perfor.Lib.Cacheing
{
    /**
     * @ SQLData缓存类型
     * */
    public enum SqlCacheOption
    {
        /**
         * @ 单个对象
         * */
        Single = 10,
        /**
         * @ 列表对象，如List
         * */
        List = 20
    }
}
