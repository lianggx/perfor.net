﻿using Danny.Authority.Data;
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
     * @ 权限类型数据管理
     * */
    public class PermDataService : DataOperation<PermData>
    {
        #region Identity
        public PermDataService() { }
        #endregion

        /**
         * @ 修改
         * */
        public override bool Update(IEnumerable<PermData> menu)
        {
            bool succeess = true;
            if (menu.IsNullOrEmpty())
                return succeess;

            MssqlUpdate update = new MssqlUpdate(TableName);
            foreach (var item in menu)
            {
                update.UpdateObject<PermData>(item);
                update.AddWhere(Primarykey, item.ID);
                update.SaveChange();
            }

            return succeess;
        }

        /**
         * @ 新增项
         * */
        public override bool Add(IEnumerable<PermData> menu)
        {
            bool succeess = true;
            if (menu == null)
                return succeess;

            MssqlInsert insert = new MssqlInsert(TableName);
            foreach (var item in menu)
            {
                item.ID = Guid.NewGuid().ToString("N");
                insert.InsertObject<PermData>(item);
            }
            insert.SaveChange();

            return succeess;
        }

        /**
         * @ 表名称
         * */
        public override string TableName
        {
            get
            {
                return "PermData";
            }
        }
    }
}
