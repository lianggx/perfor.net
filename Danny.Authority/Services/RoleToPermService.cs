using Danny.Authority.Data;
using Danny.Lib.Helpers;
using Danny.Lib.Helpers.Mssql;
using Danny.Lib.Extension;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Danny.Authority.Services
{
    /**
     * @ 数据管理
     * */
    public class RoleToPermService : DataOperation<RoleToPerm>
    {
        #region Identity
        public RoleToPermService() { }
        #endregion

        /**
         * @ 修改
         * */
        public override bool Update(IEnumerable<RoleToPerm> menu)
        {
            throw new NotImplementedException("不允许修改");
        }

        /**
         * @ 删除
         * */
        public override bool Delete(IEnumerable<string> id)
        {
            throw new NotImplementedException("未实现，请调用重载方法");
        }

        /**
         * @ 删除
         * */
        public bool Delete(IEnumerable<RoleToPerm> id)
        {
            if (id.IsNullOrEmpty())
                return true;

            MssqlDelete delete = new MssqlDelete(TableName);
            foreach (var item in id)
            {
                delete.AddWhere("Perm_ID", item.Perm_ID);
                delete.AddWhere("Role_ID", item.Role_ID);
                delete.SaveChange();
            }
            return true;
        }

        /**
         * @ 获取单条记录
         * */
        public override RoleToPerm Get(string id)
        {
            throw new NotImplementedException();
        }

        /**
         * @ 新增项
         * */
        public override bool Add(IEnumerable<RoleToPerm> menu)
        {
            bool succeess = true;
            if (menu == null)
                return succeess;

            MssqlInsert insert = new MssqlInsert(TableName);
            foreach (var item in menu)
            {
                if (item.Perm_ID.IsNullOrEmpty() || item.Role_ID.IsNullOrEmpty())
                    throw new ArgumentNullException("不能将空数据插入 RoleToPerm 表中");
                insert.InsertObject<RoleToPerm>(item);
            }
            insert.SaveChange();

            return succeess;
        }

        /**
         * @ 表名称
         * */
        public override string TableName
        {
            get { return "RoleToPerm"; }
        }
    }
}
