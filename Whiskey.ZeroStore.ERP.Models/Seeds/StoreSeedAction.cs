using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using Whiskey.Core.Data.Entity.Migrations;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class StoreSeedAction : ISeedAction
    {
        public int Order { get { return 20; } }

        public void Action(DbContext context)
        {
            #region 店铺等级

            var listSL = new StoreLevel[] {
                new StoreLevel() { LevelName="一星店铺",Discount=1f,UpgradeCondition=0f },
                new StoreLevel() { LevelName="二星店铺",Discount=0.9f,UpgradeCondition=30000f, },
                new StoreLevel() { LevelName="三星店铺",Discount=0.8f,UpgradeCondition=100000f, },
            };

            context.Set<StoreLevel>().AddOrUpdate(listSL);
            context.SaveChanges();
            #endregion
        }
    }
}
