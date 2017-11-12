using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Models;
using Autofac;
using Whiskey.ZeroStore.ERP.Services.Content;

namespace UnitTest
{
    public class OrderblankServiceUnitTest : IClassFixture<AutofacFixture>
    {
        AutofacFixture _fixture;
        public OrderblankServiceUnitTest(AutofacFixture fixture)
        {
            _fixture = fixture;
        }
    }
}
