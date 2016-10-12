using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Perfor.Lib.Common
{
    /**
     * @ 分页处理类
     * */
    public class PagedList<T> : List<T>
    {
        /**
         * @ 默认构造函数
         * */
        public PagedList() { }

        /**
         * @ 构造函数第一次重载
         * @ source 分页的内容
         * @ page 页码
         * @ size 页大小
         * */
        public PagedList(IEnumerable<T> source, int page, int size)
        {
            Page = page;
            Size = size;
            TotalCount = source.Count();
            TotalPage = (int)Math.Ceiling(TotalCount / (decimal)size);
            page = page > 0 ? page - 1 : page;
            this.AddRange(source.Skip(page * size).Take(size));
        }

        /**
         * @ 页码
         * */
        public int Page { get; set; }

        /**
         * @ 页大小 
         * */
        public int Size { get; set; }

        /**
         * @ 总数量
         * */
        public int TotalCount { get; set; }

        /**
         * @ 总页数
         * */
        public int TotalPage { get; set; }

        /*
         * @ 是否可向前翻页 
         * */
        public bool HasPreviousPage { get { return Page > 0; } }

        /**
         * @ 是否可向后翻页
         * */
        public bool HasNextPage { get { return Page + 1 < TotalPage; } }
    }
}
