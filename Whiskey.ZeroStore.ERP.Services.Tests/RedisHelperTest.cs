using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Utility.Data;
using Xunit;

namespace Whiskey.ZeroStore.ERP.Services.Tests
{
    public class RedisHelperTest
    {
        //[Fact]
        public void TestSetNX()
        {
            var res = RedisCacheHelper.SetNX("foo", "bar");
            Assert.True(res);
            var value = RedisCacheHelper.Get<string>("foo");
            Assert.True(value == "bar");
        }

        //[Fact]
        public void TestSetAll()
        {
            var dict = new Dictionary<string, string> {
                {"foo","bar" },
                {"baz","no" }
            };

            RedisCacheHelper.SetAll(dict);
            var valueDict = RedisCacheHelper.GetAll<string>(dict.Keys);
            Assert.NotNull(valueDict);
            Assert.True(valueDict.ContainsKey("foo"));
            Assert.True(valueDict.ContainsKey("baz"));

            Assert.True(valueDict["foo"] == "bar");
            Assert.True(valueDict["baz"] == "no");

        }


        [Fact]
        public void TestReduce()
        {

        }

        [Fact]
        public void TestInitOption()
        {

        }

        
       

    }
}
