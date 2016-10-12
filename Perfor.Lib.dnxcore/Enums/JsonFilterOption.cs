using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Perfor.Lib.Enums
{
    /**
     * @ Json过滤操作
     * */
    public enum JsonFilterOption
    {
        /**
         * @ 不作过滤
         * */
        Normal = 0,
        /**
         * @ 过滤
         * */
        Fileter = 10,
        /**
         * @ 保留
         * */
        Reservation = 20
    }

    /**
     * @ 序列化时，对属性名的操作
     * */
    public enum JsonCharOption
    {
        /**
         * @ 不作过滤
         * */
        Normal = 0,
        /**
         * @ 给属性名加上前缀
         * */
        Prefix = 10,
        /**
         * @ 移除属性名中指定的字符
         * */
        Remove = 20
    }

    /**
     * @ 序列化时，对属性进行大小写的操作
     * */
    public enum JsonLowerUpper
    {
        /**
         * @ 不作过滤
         * */
        Normal = 0,
        /**
         * @ 转换成小写
         * */
        Lower = 10,
        /**
         * @ 转换成大写
         * */
        Upper = 20
    }
}
