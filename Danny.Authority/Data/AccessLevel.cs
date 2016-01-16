using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Danny.Authority.Data
{
    /**
     * @ 访问级别
     * */
    public enum AccessLevel
    {
        /**
         * @ 无权限
         * */
        Nothing = 0,
        /**
         * @ 读取权限
         * */
        Read = 10,
        /**
         * @ 写入权限
         * */
        Write = 20,
        /**
         * @ 读写都可
         * */
        ReadAndWrite = 30
    }
}
