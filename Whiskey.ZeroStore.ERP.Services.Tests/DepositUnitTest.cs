using Autofac;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models.DTO;
using Whiskey.ZeroStore.ERP.Services;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Services.Implements.Notice;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Xunit;

namespace UnitTest
{
    public class DepositUnitTest : IClassFixture<AutofacFixture>
    {
        private AutofacFixture _fixture;

        public DepositUnitTest(AutofacFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void TestCalcuMemberLevel()
        {
            var depositSvc = _fixture.Container.Resolve<IMemberDepositContract>();
            var memberActivitySvc = _fixture.Container.Resolve<IMemberActivityContract>();
            var memberLevelSvc = _fixture.Container.Resolve<IMemberLevelContract>();
            var memberSvc = _fixture.Container.Resolve<IMemberContract>();


            //获取非企业会员,进行充值
            var noralVIPMembers = memberSvc.Members.Where(m => !m.IsDeleted && m.IsEnabled && m.MemberType.MemberTypeName != "企业会员")
                .Include(m => m.MemberLevel)
                .ToList();

            // 获取所有的充值活动
            var memberLevels = memberLevelSvc.MemberLevels.Where(m => !m.IsDeleted && m.IsEnabled
                                                            && m.UpgradeType != Whiskey.ZeroStore.ERP.Models.UpgradeType.企业)
                                                            .ToList();
            foreach (var member in noralVIPMembers)
            {

                foreach (var level in memberLevels)
                {
                   


                }
            }

        }








    }
}