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
     * @ 模块管理
     * */
    public class PageDataService : DataOperation<PageData>
    {
        #region Identity
        public PageDataService() { }
        #endregion

        /**
         * @ 修改
         * */
        public override bool Update(IEnumerable<PageData> menu)
        {
            bool succeess = true;
            if (menu.IsNullOrEmpty())
                return succeess;

            MssqlUpdate update = new MssqlUpdate(TableName);
            foreach (var item in menu)
            {
                update.UpdateObject<PageData>(item);
                update.AddWhere(Primarykey, item.ID);
                update.SaveChange();
            }

            return succeess;
        }

        /**
         * @ 新增项
         * */
        public override bool Add(IEnumerable<PageData> menu)
        {
            bool succeess = true;
            if (menu == null)
                return succeess;

            MssqlInsert insert = new MssqlInsert(TableName);
            foreach (var item in menu)
            {
                item.ID = Guid.NewGuid().ToString("N");
                insert.InsertObject<PageData>(item);
            }
            insert.SaveChange();

            return succeess;
        }

        /**
         * @ 表名称
         * */
        public override string TableName
        {
            get { return "PageData"; }
        }
    }
}