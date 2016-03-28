using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Perfor.Lib.Cacheing
{
    /**
     * @ 指定是缓存文件数量还是缓存文件名称
     * */
    public enum DirectoryOption
    {
        /**
         * @ 仅缓存文件的数量
         * */
        FileAmountOnly = 0,
        /**
         * @ 缓存文件名称数组列表
         * */
        FileNameArray = 1,
    }
}
