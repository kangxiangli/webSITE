using Autofac;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Models.DTO;
using Whiskey.ZeroStore.ERP.Models.Enums;
using Whiskey.ZeroStore.ERP.Services;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Services.Implements.Notice;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Xunit;

namespace UnitTest
{
    public class StatUnitTest : IClassFixture<AutofacFixture>
    {
        private AutofacFixture _fixture;

        public StatUnitTest(AutofacFixture fixture)
        {
            _fixture = fixture;
        }

        //[Fact]
        public void TestQueryOpetion()
        {
            IStatisticsContract service = _fixture.Container.Resolve<IStatisticsContract>();
            var res = service.QueryOptions();
            Assert.NotNull(res.Data);

            var storeId = 14;
            var bigProductNum = "BH001A2";
            res = service.QueryOptions(storeId, bigProductNum);
            Assert.NotNull(res.Data);

        }

        //[Fact]
        public void TestSaleStat()
        {
            IStatisticsContract service = _fixture.Container.Resolve<IStatisticsContract>();
            var adminContract = _fixture.Container.Resolve<IAdministratorContract>();
            var req = new SaleStatReq()
            {
                AdminId = 9,
                StoreId = 15,
                CategoryId = 9,
                StartDate = DateTime.Now.AddDays(-30).Date,
                EndDate = DateTime.Now.Date,
                StatType = StatTypeEnum.SaleStat,
                SizeId = 9
            };
            var res = service.SaleStat(req);
            Assert.NotNull(res.Data);
        }

        //[Fact]
        public void TestBrandStat()
        {
            IStatisticsContract service = _fixture.Container.Resolve<IStatisticsContract>();
            var adminContract = _fixture.Container.Resolve<IAdministratorContract>();
            var req = new BrandStatReq()
            {
                AdminId = 9,
                StoreId = 15,
                CategoryId = 1,
                StartDate = DateTime.Now.AddDays(-30).Date,
                EndDate = DateTime.Now.Date,
                StatType = StatTypeEnum.SaleStat,
                SizeId = 1
            };
            var res = service.BrandStat(req);
            Assert.NotNull(res.Data);
        }


        //[Fact]
        public void TestCategoryStat()
        {
            IStatisticsContract service = _fixture.Container.Resolve<IStatisticsContract>();
            var adminContract = _fixture.Container.Resolve<IAdministratorContract>();
            var req = new SaleStatReq()
            {
                AdminId = 9,
                //StoreId = 15,
                //CategoryId = 1,
                StartDate = DateTime.Now.AddDays(-6).Date,
                EndDate = DateTime.Now.Date,
                StatType = StatTypeEnum.SaleStat
            };
            var res = service.CategoryStat(req);
            Assert.NotNull(res.Data);
        }


        //[Fact]
        public void TestBigProductNumStat()
        {
            IStatisticsContract service = _fixture.Container.Resolve<IStatisticsContract>();
            var adminContract = _fixture.Container.Resolve<IAdministratorContract>();
            var req = new BigProductNumStatReq()
            {
                BigProductNum = "BH00DBA",
                SizeId = 22,
                //ColorId = 1,
                AdminId = 9,
                StoreId = 23,
                Days = 30,
                StatType = StatTypeEnum.SaleStat
            };
            var res = service.BigProductNumStat(req);
            Assert.NotNull(res.Data);
        }


        //[Fact]
        public void TestMemberStat()
        {
            IStatisticsContract service = _fixture.Container.Resolve<IStatisticsContract>();
            var adminContract = _fixture.Container.Resolve<IAdministratorContract>();
            var req = new MemberStatReq()
            {
                AdminId = 9,
                StoreId = 19,
                MemberStatType = MemberStatTypeEnum.会员类型
            };
            var res = service.MemberStat(req);
            Assert.NotNull(res.Data);
        }

        //[Fact]
        public void TestMemberLevelStat()
        {
            IStatisticsContract service = _fixture.Container.Resolve<IStatisticsContract>();
            var adminContract = _fixture.Container.Resolve<IAdministratorContract>();
            var req = new MemberStatReq()
            {
                AdminId = 9,
                StoreId = 14,
                MemberStatType = MemberStatTypeEnum.等级
            };
            var res = service.MemberStat(req);
            Assert.NotNull(res.Data);
        }

        //[Fact]
        public void TestMemberColorStat()
        {
            IStatisticsContract service = _fixture.Container.Resolve<IStatisticsContract>();
            var adminContract = _fixture.Container.Resolve<IAdministratorContract>();
            var req = new MemberStatReq()
            {
                AdminId = 9,
                StoreId = 15,
                MemberStatType = MemberStatTypeEnum.偏好颜色,

            };
            var res = service.MemberStat(req);
            Assert.NotNull(res.Data);
        }

        //[Fact]
        public void TestMemberHeightStat()
        {
            IStatisticsContract service = _fixture.Container.Resolve<IStatisticsContract>();
            var adminContract = _fixture.Container.Resolve<IAdministratorContract>();
            var req = new MemberStatReq()
            {
                AdminId = 9,
                StoreId = 15,
                MemberStatType = MemberStatTypeEnum.身高,
                HeightRanges = JsonHelper.ToJson(new RangeEntry[]{
                   new RangeEntry { MinValue = 151,MaxValue=155},
                   new RangeEntry{ MinValue= 156,MaxValue = 160},
                   new RangeEntry{ MinValue= 161,MaxValue = 170},
                   new RangeEntry{ MinValue= 171,MaxValue = 180},

               })
            };
            var res = service.MemberStat(req);
            Assert.NotNull(res.Data);
        }

        //[Fact]
        public void TestMemberWeightStat()
        {
            IStatisticsContract service = _fixture.Container.Resolve<IStatisticsContract>();
            var adminContract = _fixture.Container.Resolve<IAdministratorContract>();
            var req = new MemberStatReq()
            {
                AdminId = 9,
                StoreId = 15,
                MemberStatType = MemberStatTypeEnum.体重,
                WeightRanges = JsonHelper.ToJson(new RangeEntry[]{
                   new RangeEntry { MinValue = 40,MaxValue=60},
                   new RangeEntry{ MinValue= 61,MaxValue = 65},
                   new RangeEntry{ MinValue= 66,MaxValue = 70},
                   new RangeEntry{ MinValue= 70,MaxValue = 100},
                   new RangeEntry{ MinValue= 101,MaxValue = 120},
                   new RangeEntry{ MinValue= 121,MaxValue = 180},
               })
            };
            var res = service.MemberStat(req);
            Assert.NotNull(res.Data);
        }

        //[Fact]
        public void TestMemberShoulderStat()
        {
            IStatisticsContract service = _fixture.Container.Resolve<IStatisticsContract>();
            var adminContract = _fixture.Container.Resolve<IAdministratorContract>();
            var req = new MemberStatReq()
            {
                AdminId = 9,
                StoreId = 15,
                MemberStatType = MemberStatTypeEnum.肩宽,
                ShoudlerRanges = JsonHelper.ToJson(new RangeEntry[]{
                   new RangeEntry { MinValue = 40,MaxValue=60},
                   new RangeEntry{ MinValue= 61,MaxValue = 65},
                   new RangeEntry{ MinValue= 66,MaxValue = 70},
                   new RangeEntry{ MinValue= 70,MaxValue = 100},
                   new RangeEntry{ MinValue= 101,MaxValue = 120},
                   new RangeEntry{ MinValue= 121,MaxValue = 180},
               })
            };
            var res = service.MemberStat(req);
            Assert.NotNull(res.Data);
        }

        //[Fact]
        public void TestMemberBustStat()
        {
            IStatisticsContract service = _fixture.Container.Resolve<IStatisticsContract>();
            var adminContract = _fixture.Container.Resolve<IAdministratorContract>();
            var req = new MemberStatReq()
            {
                AdminId = 9,
                StoreId = 15,
                MemberStatType = MemberStatTypeEnum.胸围,
                BustRanges = JsonHelper.ToJson(new RangeEntry[]{
                   new RangeEntry { MinValue = 40,MaxValue=60},
                   new RangeEntry{ MinValue= 61,MaxValue = 65},
                   new RangeEntry{ MinValue= 66,MaxValue = 70},
                   new RangeEntry{ MinValue= 70,MaxValue = 100},
                   new RangeEntry{ MinValue= 101,MaxValue = 120},
                   new RangeEntry{ MinValue= 121,MaxValue = 180},
               })
            };
            var res = service.MemberStat(req);
            Assert.NotNull(res.Data);
        }

        //[Fact]
        public void TestMemberWaistLineStat()
        {
            IStatisticsContract service = _fixture.Container.Resolve<IStatisticsContract>();
            var adminContract = _fixture.Container.Resolve<IAdministratorContract>();
            var req = new MemberStatReq()
            {
                AdminId = 9,
                StoreId = 15,
                MemberStatType = MemberStatTypeEnum.腰围,
                WaistLineRanges = JsonHelper.ToJson(new RangeEntry[]{
                   new RangeEntry { MinValue = 40,MaxValue=60},
                   new RangeEntry{ MinValue= 61,MaxValue = 65},
                   new RangeEntry{ MinValue= 66,MaxValue = 70},
                   new RangeEntry{ MinValue= 70,MaxValue = 100},
                   new RangeEntry{ MinValue= 101,MaxValue = 120},
                   new RangeEntry{ MinValue= 121,MaxValue = 180},
               })
            };
            var res = service.MemberStat(req);
            Assert.NotNull(res.Data);
        }

        //[Fact]
        public void TestMemberHipStat()
        {
            IStatisticsContract service = _fixture.Container.Resolve<IStatisticsContract>();
            var adminContract = _fixture.Container.Resolve<IAdministratorContract>();
            var req = new MemberStatReq()
            {
                AdminId = 9,
                StoreId = 15,
                MemberStatType = MemberStatTypeEnum.臀围,
                HipRanges = JsonHelper.ToJson(new RangeEntry[]{
                   new RangeEntry { MinValue = 40,MaxValue=60},
                   new RangeEntry{ MinValue= 61,MaxValue = 65},
                   new RangeEntry{ MinValue= 66,MaxValue = 70},
                   new RangeEntry{ MinValue= 70,MaxValue = 100},
                   new RangeEntry{ MinValue= 101,MaxValue = 120},
                   new RangeEntry{ MinValue= 121,MaxValue = 180},
               })
            };
            var res = service.MemberStat(req);
            Assert.NotNull(res.Data);
        }

        [Fact]

        public void TestMemberFigure()
        {
            IStatisticsContract service = _fixture.Container.Resolve<IStatisticsContract>();
            var adminContract = _fixture.Container.Resolve<IAdministratorContract>();
            var req = new MemberStatReq()
            {
                AdminId = 9,
                StoreId = 15,
                MemberStatType = MemberStatTypeEnum.体型,
            };
            var res = service.MemberStat(req);
            Assert.NotNull(res.Data);
        }



        //[Fact]
        public void SeedPartnerData()
        {
            var seedData = new List<PartnerStatistics>();
            for (int i = 1; i <= 12; i++)
            {
                seedData.Add(
                new PartnerStatistics { CreatedTime = new DateTime(2017, i, 1), MemberCount = 100, OrderCount = 100, OrderMoney = 100, PartnerCount = 100, SaleCount = 100, SaleMoney = 100 }
                );
            }

            var contract = _fixture.Container.Resolve<IPartnerStatisticsContract>();
            contract.Insert(seedData.ToArray());
        }



        //[Fact]
        public void SeedPartnerDailyData()
        {
            var seedData = new List<PartnerStatistics>();
            for (int i = 2; i <= 31; i++)
            {
                seedData.Add(
                new PartnerStatistics { CreatedTime = new DateTime(2017, 1, i), MemberCount = 100, OrderCount = 100, OrderMoney = 100, PartnerCount = 100, SaleCount = 100, SaleMoney = 100 }
                );
            }

            var contract = _fixture.Container.Resolve<IPartnerStatisticsContract>();
            contract.Insert(seedData.ToArray());
        }

    }
}