using System;
using System.Collections.Generic;

namespace Whiskey.ZeroStore.ERP.Website.Models
{

    public interface ICalculator
    {
        CalculatorRes Compute(CalculatorParamObj paramDto);
    }

    public class CalculatorParamObj
    {
        public bool IsMember { get; set; }

        /// <summary>
        /// 总消费金额
        /// </summary>
        public decimal TotalConsumeMoney { get; set; }

        /// <summary>
        /// 会员储值余额
        /// </summary>
        public decimal CurrentCardMoney { get; set; }

        /// <summary>
        /// 会员积分余额
        /// </summary>
        public decimal CurrentScore { get; set; }

        /// <summary>
        /// 现金消费
        /// </summary>
        public decimal CashConsume { get; set; }

        /// <summary>
        /// 刷卡消费
        /// </summary>
        public decimal SwipCardConsume { get; set; }

        /// <summary>
        /// 抹去
        /// </summary>
        public decimal EraseConsume { get; set; }

        /// <summary>
        /// 优惠券
        /// </summary>
        public decimal Coupon { get; set; }
    }

    public class CalculatorRes
    {
        public CalculatorRes()
        {
            cash_consum = 0;
            swipcard_consum = 0;
            cardmoney_consum = 0;
            score_consum = 0;
            retumoney_consum = 0;
        }
        /// <summary>
        /// 现金消费
        /// </summary>
        public decimal cash_consum { get; set; }

        /// <summary>
        /// 刷卡消费
        /// </summary>
        public decimal swipcard_consum { get; set; }

        /// <summary>
        /// 储值消费
        /// </summary>
        public decimal cardmoney_consum { get; set; }

        /// <summary>
        /// 积分消费
        /// </summary>
        public decimal score_consum { get; set; }

        /// <summary>
        /// 找零
        /// </summary>
        public decimal retumoney_consum { get; set; }
    }

    /// <summary>
    /// 现金消费计算器
    /// </summary>
    public class CashConsumeCalculator : ICalculator
    {
        public CalculatorRes Compute(CalculatorParamObj paramDto)
        {

            //实际需支付金额 = 总金额-优惠券-抹零
            var realConsumeCount = paramDto.TotalConsumeMoney - paramDto.Coupon - paramDto.EraseConsume;

            //支付金额<0的情况
            if (realConsumeCount <= 0)
            {
                return new CalculatorRes();
            }

            //现金消费=消费金额,则不需要使用刷卡和找零
            if (paramDto.CashConsume == realConsumeCount)
            {
                return new CalculatorRes()
                {
                    cash_consum = paramDto.CashConsume
                };
            }
            else if (paramDto.CashConsume > realConsumeCount)  //超过实际支付金额,需要找零
            {
                decimal returnMoney = paramDto.CashConsume - realConsumeCount;

                return new CalculatorRes()
                {
                    cash_consum = paramDto.CashConsume,
                    retumoney_consum = returnMoney
                };
            }
            else   //现金不够
            {
                decimal leftMoney = realConsumeCount - paramDto.CashConsume;

                //非会员,刷卡来凑
                if (!paramDto.IsMember)
                {
                    return new CalculatorRes()
                    {
                        cash_consum = paramDto.CashConsume,
                        swipcard_consum = leftMoney
                    };
                }



                //会员,优先使用会员的储值和积分来凑,还不够的话,就由刷卡来补充

                //1.现金不够,用储值来凑
                if (paramDto.CurrentCardMoney >= leftMoney)  //储值足够,不需要用积分或刷卡了
                {
                    return new CalculatorRes()
                    {
                        cash_consum = paramDto.CashConsume,
                        cardmoney_consum = leftMoney
                    };
                }
                //2储值不够,积分来凑
                decimal leftMoneyToScore = leftMoney - paramDto.CurrentCardMoney;
                if (paramDto.CurrentScore >= leftMoneyToScore) //积分可以凑够
                {
                    return new CalculatorRes()
                    {
                        cash_consum = paramDto.CashConsume,
                        cardmoney_consum = paramDto.CurrentCardMoney,
                        score_consum = leftMoneyToScore
                    };
                }

                //3积分不够,剩余由刷卡补充
                decimal leftMoneyToSwipeCard = leftMoneyToScore - paramDto.CurrentScore;
                return new CalculatorRes()
                {
                    cash_consum = paramDto.CashConsume,
                    cardmoney_consum = paramDto.CurrentCardMoney,
                    score_consum = paramDto.CurrentScore,
                    swipcard_consum = leftMoneyToSwipeCard
                };
            }
        }
    }

    /// <summary>
    /// 刷卡消费计算器
    /// </summary>
    public class SwipCardConsumeCalculator : ICalculator
    {
        public CalculatorRes Compute(CalculatorParamObj paramDto)
        {
            //实际需支付金额 = 总金额-优惠券-抹零
            var realConsumeCount = paramDto.TotalConsumeMoney - paramDto.Coupon - paramDto.EraseConsume;
            //支付金额<0的情况
            if (realConsumeCount <= 0)
            {
                return new CalculatorRes();
            }
            //刷卡消费=消费金额,则不需要使用其它支付
            if (paramDto.SwipCardConsume == realConsumeCount)
            {
                return new CalculatorRes()
                {
                    swipcard_consum = paramDto.SwipCardConsume
                };
            }
            else if (paramDto.SwipCardConsume > realConsumeCount) //刷卡金额>支付金额,需要找零
            {
                var returnMoney = paramDto.SwipCardConsume - realConsumeCount;
                return new CalculatorRes()
                {
                    swipcard_consum = paramDto.SwipCardConsume,
                    retumoney_consum = returnMoney
                };
            }
            else //刷卡金额不够
            {
                decimal leftMoney = realConsumeCount - paramDto.SwipCardConsume;
                //非会员,现金来凑
                if (!paramDto.IsMember)
                {
                    return new CalculatorRes()
                    {
                        swipcard_consum = paramDto.SwipCardConsume,
                        cash_consum = leftMoney
                    };
                }
                //会员,优先使用会员的储值和积分来凑,还不够的话,就由现金来补充
                //1.刷卡不够,用储值来凑
                if (paramDto.CurrentCardMoney >= leftMoney)  //储值足够,不需要用积分或现金了
                {
                    return new CalculatorRes()
                    {
                        swipcard_consum = paramDto.SwipCardConsume,
                        cardmoney_consum = leftMoney
                    };
                }
                //2.储值不够,积分来凑
                decimal leftMoneyToScore = leftMoney - paramDto.CurrentCardMoney;
                if (paramDto.CurrentScore >= leftMoneyToScore) //积分足够
                {
                    return new CalculatorRes()
                    {
                        swipcard_consum = paramDto.SwipCardConsume,
                        cardmoney_consum = paramDto.CurrentCardMoney,
                        score_consum = leftMoneyToScore
                    };
                }

                //3.积分不够,剩余由现金来凑齐
                decimal leftMoneyToCash = leftMoneyToScore - paramDto.CurrentScore;
                return new CalculatorRes()
                {
                    swipcard_consum = paramDto.SwipCardConsume,
                    cardmoney_consum = paramDto.CurrentCardMoney,
                    score_consum = paramDto.CurrentScore,
                    cash_consum = leftMoneyToCash
                };
            }
        }
    }
}