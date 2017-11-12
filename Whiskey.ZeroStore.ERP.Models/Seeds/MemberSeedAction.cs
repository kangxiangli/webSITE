using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using Whiskey.Core.Data.Entity.Migrations;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class MemberSeedAction : ISeedAction
    {
        public int Order
        {
            get { return 10; }
        }

        public void Action(DbContext context)
        {
            #region 充值活动
            List<MemberActivity> listMemberActivity = new List<MemberActivity>()
            {
                new MemberActivity(){ActivityName="充100送50！",Score=50,IsForever=true,ActivityType=0,Price=100,StartDate=DateTime.Now,EndDate=DateTime.Now,Notes="这是一个充值活动"},
            };
            context.Set<MemberActivity>().AddOrUpdate(listMemberActivity.ToArray());
            context.SaveChanges();
            #endregion
            
            #region 签到奖品
            List<Prize> listPrize = new List<Prize>()
            {
                new Prize(){ PrizeName="1积分",Quantity=100,Score=1,RewardImagePath="/Content/Images/Prize/1@2x.png",ReceiveQuantity=0,GetQuantity=0,PrizeType=0,Sequence=0},
                new Prize(){ PrizeName="10积分",Quantity=100,Score=10,RewardImagePath="/Content/Images/Prize/10@2x.png",ReceiveQuantity=0,GetQuantity=0,PrizeType=0,Sequence=0},
                new Prize(){ PrizeName="20积分",Quantity=100,Score=20,RewardImagePath="/Content/Images/Prize/20@2x.png",ReceiveQuantity=0,GetQuantity=0,PrizeType=0,Sequence=0},
                new Prize(){ PrizeName="50积分",Quantity=100,Score=50,RewardImagePath="/Content/Images/Prize/50@2x.png",ReceiveQuantity=0,GetQuantity=0,PrizeType=0,Sequence=0},
                new Prize(){ PrizeName="100积分",Quantity=100,Score=100,RewardImagePath="/Content/Images/Prize/100@2x.png",ReceiveQuantity=0,GetQuantity=0,PrizeType=0,Sequence=0},
            };
            context.Set<Prize>().AddOrUpdate(listPrize.ToArray());
            context.SaveChanges();
            #endregion
            
            #region 会员热度

            var listMH = new MemberHeat[] {
                new MemberHeat() { HeatName="7天内购买过",DayStart=1,DayEnd=7,IconPath="/Content/UploadFiles/MemberHeatImg/20170315/o_0_1623223751_100_100.png"},
                new MemberHeat() { HeatName="3个月内购买过",DayStart=7,DayEnd=90,IconPath="/Content/UploadFiles/MemberHeatImg/20170315/o_0_1623386671_100_100.png"},
                new MemberHeat() { HeatName="1年内购买过",DayStart=90,DayEnd=365,IconPath="/Content/UploadFiles/MemberHeatImg/20170315/o_0_1623526124_100_100.png"},
                new MemberHeat() { HeatName="很久前买过",DayStart=365,IconPath="/Content/UploadFiles/MemberHeatImg/20170315/o_0_1624508810_100_100.png"},
                new MemberHeat() { HeatName="暂未购买",IconPath="/Content/UploadFiles/MemberHeatImg/20170315/o_0_1625105431_100_100.png"},
            };

            #endregion
        }
        
    }
}
