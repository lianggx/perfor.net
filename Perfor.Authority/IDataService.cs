using Perfor.Authority.Data;
using Perfor.Lib.Helpers;
using Perfor.Lib.Helpers.Mssql;
using Perfor.Lib.Extension;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Perfor.Authority
{
    /**
     * @ 数据管理接口
     * */
    public interface IDataService<T>
    {
        /**
         * @ 修改
         * */
        bool Update(IEnumerable<T> data);

        /**
         * @ 删除
         * */
        bool Delete(IEnumerable<string> id);

        /**
         * @ 获取单条记录
         * */
        T Get(string id);

        /**
         * @ 新增项
         * */
        bool Add(IEnumerable<T> menu);
    }
}
