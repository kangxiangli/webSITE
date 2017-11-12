using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Whiskey.ZeroStore.ERP.Website.Models;
namespace UnitTest
{
    public class CalculatorTest : IClassFixture<AutofacFixture>
    {
        AutofacFixture _fixture;
        public CalculatorTest(AutofacFixture fixture)
        {
            _fixture = fixture;
        }


        [Fact]
        public void TestZero()
        {
            ICalculator cashCalculator = new CashConsumeCalculator();
            ICalculator swipCardCalculator = new SwipCardConsumeCalculator();
            var paramObj = new CalculatorParamObj()
            {
                TotalConsumeMoney = 0,
                Coupon = 0,
                EraseConsume = 0,
                CurrentCardMoney = 0,
                CurrentScore = 0,
                IsMember = false,
                CashConsume = 0,
                SwipCardConsume = 0
            };
            var res = cashCalculator.Compute(paramObj);
            Assert.Equal(0, res.cash_consum);
            Assert.Equal(0, res.swipcard_consum);
            Assert.Equal(0, res.cardmoney_consum);
            Assert.Equal(0, res.score_consum);
            Assert.Equal(0, res.retumoney_consum);
            var dict2 = swipCardCalculator.Compute(paramObj);
            Assert.Equal(0, dict2.cash_consum);
            Assert.Equal(0, dict2.swipcard_consum);
            Assert.Equal(0, dict2.cardmoney_consum);
            Assert.Equal(0, dict2.score_consum);
            Assert.Equal(0, dict2.retumoney_consum);
        }


        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(50)]
        [InlineData(99)]
        [InlineData(400)]
        [InlineData(489)]

        public void TestNoMember_NoEnoughCash(decimal cashConsume)
        {
            ICalculator cashCalculator = new CashConsumeCalculator();

            var paramObj = new CalculatorParamObj()
            {
                TotalConsumeMoney = 497.5M,
                Coupon = 0,
                EraseConsume = 7.5M,
                CashConsume = cashConsume,
                SwipCardConsume = 0,
                CurrentCardMoney = 0,
                CurrentScore = 0,
                IsMember = false,
            };
            var dict = cashCalculator.Compute(paramObj);

            Assert.True(dict.cash_consum == paramObj.CashConsume);
            Assert.True(dict.swipcard_consum == paramObj.TotalConsumeMoney - paramObj.Coupon - paramObj.EraseConsume - paramObj.CashConsume);
            Assert.True(dict.cardmoney_consum == 0);
            Assert.True(dict.score_consum == 0);
            Assert.True(dict.retumoney_consum == 0);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TestUseCash_CanReturnMoney(bool isMember)
        {
            ICalculator cashCalculator = new CashConsumeCalculator();

            var paramObj = new CalculatorParamObj()
            {
                TotalConsumeMoney = 497.5M,
                Coupon = 0,
                EraseConsume = 7.5M,
                CashConsume = 500,
                SwipCardConsume = 0,
                CurrentCardMoney = 0,
                CurrentScore = 0,
                IsMember = isMember,
            };
            var res = cashCalculator.Compute(paramObj);

            Assert.Equal(0, res.swipcard_consum);
            Assert.Equal(0, res.cardmoney_consum);
            Assert.Equal(0, res.score_consum);
            Assert.True(res.cash_consum == paramObj.CashConsume, "找零金额不对");
            Assert.Equal(10m, res.retumoney_consum);


        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 0)]
        [InlineData(0, 1)]
        [InlineData(1, 1)]
        [InlineData(100, 100)]

        public void TestUseOnlySwipCard(decimal coupon, decimal erase)
        {
            ICalculator cashCalculator = new CashConsumeCalculator();


            var paramObj = new CalculatorParamObj()
            {
                TotalConsumeMoney = 497.5M,
                Coupon = coupon,
                EraseConsume = erase,
                CashConsume = 0,
                SwipCardConsume = 0,
                CurrentCardMoney = 0,
                CurrentScore = 0,
                IsMember = true,
            };
            var res = cashCalculator.Compute(paramObj);
            Assert.Equal(res.swipcard_consum, paramObj.TotalConsumeMoney - paramObj.Coupon - paramObj.EraseConsume);
            Assert.Equal(0, res.cash_consum);
            Assert.Equal(0, res.cardmoney_consum);
            Assert.Equal(0, res.score_consum);
        }
        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 0)]
        [InlineData(0, 1)]
        [InlineData(1, 1)]
        [InlineData(100, 100)]
        public void TestUseOnlyCash(decimal coupon, decimal erase)
        {

            ICalculator swipCalculator = new SwipCardConsumeCalculator();

            var paramObj = new CalculatorParamObj()
            {
                TotalConsumeMoney = 497.5M,
                Coupon = coupon,
                EraseConsume = erase,
                CashConsume = 0,
                SwipCardConsume = 0,
                CurrentCardMoney = 0,
                CurrentScore = 0,
                IsMember = true,
            };
            var res = swipCalculator.Compute(paramObj);

            Assert.Equal(res.cash_consum, paramObj.TotalConsumeMoney - paramObj.Coupon - paramObj.EraseConsume);
            Assert.Equal(0, res.swipcard_consum);
            Assert.Equal(0, res.cardmoney_consum);
            Assert.Equal(0, res.score_consum);
        }

        [Theory]
        [InlineData(0, 0, 0)]
        [InlineData(1, 0, 1)]
        [InlineData(0, 1, 100)]
        [InlineData(1, 1, 400)]
        [InlineData(100, 100, 500)]
        public void TestUseOnlyCardMoney(decimal coupon, decimal erase, decimal cardMoney)
        {
            ICalculator cashCalculator = new CashConsumeCalculator();
            ICalculator swipcardCalculator = new SwipCardConsumeCalculator();

            var paramObj = new CalculatorParamObj()
            {
                TotalConsumeMoney = 497.5M,
                Coupon = coupon,
                EraseConsume = erase,
                CashConsume = 0,
                SwipCardConsume = 0,
                CurrentCardMoney = cardMoney,
                CurrentScore = 0,
                IsMember = true,
            };

            var res = cashCalculator.Compute(paramObj);
            Assert.Equal(paramObj.TotalConsumeMoney - coupon - erase, res.cardmoney_consum + res.swipcard_consum);
            var res2 = swipcardCalculator.Compute(paramObj);
            Assert.Equal(paramObj.TotalConsumeMoney - coupon - erase, res2.cardmoney_consum + res2.cash_consum);

        }


        [Theory]
        [InlineData(0, 0, 0)]
        [InlineData(1, 0, 1)]
        [InlineData(0, 1, 100)]
        [InlineData(1, 1, 400)]
        [InlineData(100, 100, 500)]
        public void TestUseOnlyScore(decimal coupon, decimal erase, decimal score)
        {
            ICalculator cashCalculator = new CashConsumeCalculator();
            ICalculator swipcardCalculator = new SwipCardConsumeCalculator();

            var paramObj = new CalculatorParamObj()
            {
                TotalConsumeMoney = 497.5M,
                Coupon = coupon,
                EraseConsume = erase,
                CashConsume = 0,
                SwipCardConsume = 0,
                CurrentCardMoney = 0,
                CurrentScore = score,
                IsMember = true,
            };

            var res = cashCalculator.Compute(paramObj);
            Assert.Equal(paramObj.TotalConsumeMoney - coupon - erase, res.score_consum + res.swipcard_consum);
            var res2 = swipcardCalculator.Compute(paramObj);
            Assert.Equal(paramObj.TotalConsumeMoney - coupon - erase, res2.score_consum + res2.cash_consum);

        }

        [Theory]
        [InlineData(0, 0, 0, 0)]
        [InlineData(1, 0, 100, 0)]
        [InlineData(0, 1, 0, 100)]
        [InlineData(1, 1, 100, 100)]
        [InlineData(100, 100, 300, 300)]
        [InlineData(100, 100, 500, 200)]
        [InlineData(100, 100, 200, 500)]
        public void TestMultiSource(decimal coupon, decimal erase, decimal cardMoney, decimal score)
        {
            ICalculator cashCalculator = new CashConsumeCalculator();
            ICalculator swipcardCalculator = new SwipCardConsumeCalculator();
            var paramObj = new CalculatorParamObj()
            {
                TotalConsumeMoney = 497.5M,
                Coupon = coupon,
                EraseConsume = erase,
                CashConsume = 0,
                SwipCardConsume = 0,
                CurrentCardMoney = cardMoney,
                CurrentScore = score,
                IsMember = true,
            };


            var res = cashCalculator.Compute(paramObj);
            Assert.Equal(paramObj.TotalConsumeMoney - coupon - erase, res.score_consum + res.cardmoney_consum + res.swipcard_consum);
            var res2 = swipcardCalculator.Compute(paramObj);
            Assert.Equal(paramObj.TotalConsumeMoney - coupon - erase, res2.score_consum + res2.cardmoney_consum + res2.cash_consum);


        }
    }
}
