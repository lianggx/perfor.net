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
     * @ 数据管理
     * */
    public class RoleDetailService : DataOperation<RoleDetail>
    {
        #region Identity
        public RoleDetailService() { }
        #endregion

        /**
         * @ 修改
         * */
        public override bool Update(IEnumerable<RoleDetail> menu)
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
        public bool Delete(IEnumerable<RoleDetail> id)
        {
            if (id.IsNullOrEmpty())
                return true;

            MssqlDelete delete = new MssqlDelete(TableName);
            foreach (var item in id)
            {
                delete.AddWhere("Detail_ID", item.Detail_ID);
                delete.AddWhere("Role_ID", item.Role_ID);
                delete.SaveChange();
            }
            return true;
        }

        /**
         * @ 获取单条记录
         * */
        public override RoleDetail Get(string id)
        {
            throw new NotImplementedException();
        }

        /**
         * @ 新增项
         * */
        public override bool Add(IEnumerable<RoleDetail> menu)
        {
            bool succeess = true;
            if (menu == null)
                return succeess;

            MssqlInsert insert = new MssqlInsert(TableName);
            foreach (var item in menu)
            {
                if (item.Detail_ID.IsNullOrEmpty() || item.Role_ID.IsNullOrEmpty() || item.Type.IsEnum<DetailType, int>() == false)
                    throw new ArgumentNullException("不能将空数据插入 RoleDetail 表中");
                insert.InsertObject<RoleDetail>(item);
            }
            insert.SaveChange();

            return succeess;
        }

        /**
         * @ 表名称
         * */
        public override string TableName
        {
            get { return "RoleDetail"; }
        }
    }
}
