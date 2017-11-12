using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.jpush.api;
using Xunit;

namespace Whiskey.ZeroStore.ERP.Services.Tests
{
    public class MobileAPIPushHelperUnitTests
    {
        [Fact]
        public void PushTest()
        {
            var registrationId = "1114a897929a21d8ad2";
            var res = MobileAPIPushHelper.PushConfirmLogin(registrationId);
            Assert.True(res);
        }
    }
}
