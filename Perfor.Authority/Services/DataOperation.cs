using Perfor.Authority.Data;
using Perfor.Lib.Helpers;
using Perfor.Lib.Helpers.Mssql;
using Perfor.Lib.Extension;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Perfor.Authority.Services
{
    /**
     * @ 模块管理
     * */
    public abstract class DataOperation<T> : IDataService<T> where T : class,new()
    {
        #region Identity
        /**
         * @ 要更新的表名称
         * */
        public abstract string TableName { get; }

        private string primarykey = "ID";
        /**
         * @ 表的主键字段
         * */
        public virtual string Primarykey
        {
            get { return primarykey; }
            set { primarykey = value; }
        }

        public DataOperation() { }
        #endregion

        /**
         * @ 修改
         * */
        public abstract bool Update(IEnumerable<T> menu);

        /**
         * @ 删除
         * */
        public virtual bool Delete(IEnumerable<string> id)
        {
            if (id.IsNullOrEmpty())
                return true;

            MssqlDelete delete = new MssqlDelete(TableName);
            foreach (var item in id)
            {
                delete.AddWhere(Primarykey, item);
                delete.SaveChange();
            }
            return true;
        }

        /**
         * @ 获取单条记录
         * */
        public virtual T Get(string id)
        {
            MssqlGetSomeOne pager = new MssqlGetSomeOne(TableName);
            pager.AddWhere(Primarykey, id);
            List<T> list = pager.Select<T>();
            if (list.IsNotNullAndGtEq(1))
                return list[0];
            return null;
        }

        /**
         * @ 新增项
         * */
        public abstract bool Add(IEnumerable<T> menu);
    }
}
