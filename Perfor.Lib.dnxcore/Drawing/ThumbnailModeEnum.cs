using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Perfor.Lib.Drawing
{
    /// <summary>
    ///  缩略图生成样式
    /// </summary>
    public enum ThumbnailModeEnum
    {
        /// <summary>
        ///  根据指定的长和宽生成，可能会丢失图片的原始宽高比例
        /// </summary>
        WidthAndHeight,
        /// <summary>
        ///  根据指定的宽度生成，高度按照指定的宽度值依据原始宽高比例获得，此时忽略指定的高度值。可以保持原始宽高比例
        /// </summary>
        ByWidth,
        /// <summary>
        ///  根据指定的高度生成，宽度按照指定的高度值依据原始宽高比例获得，此时忽略指定的宽度值。可以保持原始宽高比例
        /// </summary>
        ByHeight,
        /// <summary>
        ///  根据指定的百分比来生成，可以保持原始宽高比例
        /// </summary>
        ByPercent
    }
}
