using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Perfor.Lib.Common
{
    /// <summary>
    ///  分页处理类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PagedList<T> : List<T>
    {
        /// <summary>
        ///  默认构造函数
        /// </summary>
        public PagedList() { }

        /// <summary>
        ///  构造函数第一次重载
        /// </summary>
        /// <param name="source">分页的内容</param>
        /// <param name="page">页码</param>
        /// <param name="size">页大小</param>
        public PagedList(IEnumerable<T> source, int page, int size)
        {
            Page = page;
            Size = size;
            TotalCount = source.Count();
            TotalPage = (int)Math.Ceiling(TotalCount / (decimal)size);
            page = page > 0 ? page - 1 : page;
            this.AddRange(source.Skip(page * size).Take(size));
        }

        /// <summary>
        ///  获取或者设置页码
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        ///  获取或者设置页大小
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        ///  获取或者设置总数量
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        ///  获取或者设置总页数
        /// </summary>
        public int TotalPage { get; set; }

        /// <summary>
        ///  获取是否可向前翻页
        /// </summary>
        public bool HasPreviousPage { get { return Page > 0; } }

        /// <summary>
        ///  获取是否可向后翻页
        /// </summary>
        public bool HasNextPage { get { return Page + 1 < TotalPage; } }
    }
}
