using System;
using Whiskey.ZeroStore.ERP.Models.Entities;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Models.DTO;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Stores.Models
{
    public class ProductCalculator
    {

        public float GetScore(ConsumeInfo consumeInfo, Retail retail, ScoreRule scoreRule)
        {
            if (scoreRule == null || scoreRule.ConsumeUnit <= 0 || scoreRule.ScoreUnit <= 0)
            {
                return 0;
            }
            var payMoney = consumeInfo.ConsumeCoun - GetNoMoney(consumeInfo, retail);

            // 消费/积分比例
            float unitConsu = scoreRule.ConsumeUnit;
            float unitScore = scoreRule.ScoreUnit;
            var score = (float)Math.Round(payMoney) / unitConsu * unitScore;
            return score;

        }

        public decimal GetNoMoney(ConsumeInfo consumeInfo, Retail retail)
        {

            //不找零的消费 积分+储值+抹除+优惠券+店铺活动优惠
            var holy = consumeInfo.Score + consumeInfo.CardMoney + consumeInfo.Erase + retail.CouponConsume + retail.StoreActivityDiscount;
            return holy;

        }
    }
}