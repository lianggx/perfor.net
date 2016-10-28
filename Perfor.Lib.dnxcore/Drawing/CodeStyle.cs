using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dywebsdk.Drawing
{
    /// <summary>
    ///  验证码样式类型
    /// </summary>
    public enum CodeStyle
    {
        /// <summary>
        /// 英文字母
        /// </summary>
        Char = 1,
        /// <summary>
        ///  中文字符
        /// </summary>
        Zh_cn = 2,
        /// <summary>
        ///  数字
        /// </summary>
        Number = 4
    }
}
