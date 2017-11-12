using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web;
using Whiskey.Core.Data.Entity.Migrations;
using Whiskey.ZeroStore.ERP.Models;

namespace Whiskey.ZeroStore.ERP.Models.Seeds
{
    public class OfficeSeedAction:ISeedAction
    {
         /// <summary>
        /// 获取 操作排序，数值越小越先执行
        /// </summary>
        public int Order { get { return 3; } }

        /// <summary>
        /// 定义种子数据初始化过程
        /// </summary>
        /// <param name="context">数据上下文</param>
        public void Action(DbContext context)
        {
            

        }
    }
}
