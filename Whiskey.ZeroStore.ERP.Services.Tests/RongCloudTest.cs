using Newtonsoft.Json;
using Whiskey.RongCloudServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Autofac;
using Whiskey.ZeroStore.ERP.Services.Contracts;

namespace UnitTest
{
    public class RongCloudTest : IClassFixture<AutofacFixture>
    {
        private AutofacFixture _fixture;
        public RongCloudTest(AutofacFixture fixture)
        {
            _fixture = fixture;
        }
        [Fact]
        public void TestGetToken()
        {
            var userId = "4242";
            var userName = "邓亚魁";
            var avatar = "http://www.rongcloud.cn/images/logo.png";
            var result = RongCloudServerHelper.GetToken(userId, userName, avatar);
            Assert.Equal(200, result.code);
            Assert.Equal(userId, result.userId);
            Assert.NotNull(result.token);
        }

        [Fact]
        public void TestRefreshInfo()
        {
            var userId = "4242";
            var userName = "邓亚魁";
            var avatar = "https://www.baidu.com/img/bd_logo1.png";
            var res = RongCloudServerHelper.RefreshUser(userId, userName, avatar);
            Assert.Equal(200, res.code);
        }

        [Fact]
        public void TestCheckOnline()
        {
            var userId = "4242";
            var res = RongCloudServerHelper.CheckOnline(userId);
            Assert.Equal(200, res.code);
            Assert.NotEmpty(res.status);
        }

        [Fact]
        public void TestBlockUser()
        {
            var userId = "4242";
            var res = RongCloudServerHelper.Block(userId, "1");
            Assert.Equal(200, res.code);


            var res2 = RongCloudServerHelper.QueryBlock();
            Assert.Equal(200, res2.code);
            var blockUsers = res2.users;
            Assert.True(blockUsers.Count(u => u.userId == userId) == 1);

            var res3 = RongCloudServerHelper.Unblock(userId);
            Assert.Equal(200, res3.code);
            blockUsers = RongCloudServerHelper.QueryBlock().users;
            Assert.True(blockUsers.Count(u => u.userId == userId) == 0);

        }

        [Fact]
        public void TestSendContactMsg()
        {
            var fromUserId = "26";
            var toUserId = "5275";

            //用户头像
            var uinfo = new UserInfo()
            {
                id = "26",
                name = "xxx",
                icon = "https://www.baidu.com/img/bd_logo1.png"
            };


            //加好友
            NtfMsg msg = new NtfMsg()
            {
                sourceUserId = fromUserId,
                targetUserId = toUserId,
                operation = "addFriend",
                message = "加我吧",
                extra = "加好友",
                user = uinfo
            };




            //文字
            var plainMsg = new PlainMsg
            {
                content = "hhelele",
                extra = "",
                user = uinfo
            }; ;

            var noti = new InfoMsg() { message = "noti", user = uinfo };

            //var res = RongCloudServerHelper.publishPrivate(fromUserId, new string[] { toUserId }, JsonConvert.SerializeObject(plainMsg), plainMsg.GetMsgType(), "hi", "hello","1", 1, 1, 1);
            //var res2 = RongCloudServerHelper.publishPrivate(fromUserId, new string[] { toUserId }, JsonConvert.SerializeObject(noti), noti.GetMsgType(), "hi", "hello","1", 1, 1, 1);
            var res3 = RongCloudServerHelper.publishSystem(fromUserId, new string[] { toUserId }, JsonConvert.SerializeObject(msg), msg.GetMsgType(), "hi", "hello", "1", 1, 1, 1);
            //var res = RongCloudServerHelper.publishPrivate(fromUserId, new string[] { toUserId }, JsonConvert.SerializeObject(msg), NtfMsg.GetMsgType(), "hi", "hello","1", 0, 1, 1,JsonConvert.SerializeObject(uinfo));
            //Assert.Equal(200, res.code);

        }

        [Fact]
        public void TestGetMyFriend()
        {
            var memberId = 26;
            var service = _fixture.Container.Resolve<IMemberIMFriendContract>();
            var res = service.GetMyFriend(memberId);
            Assert.True(res.ResultType == Whiskey.Utility.Data.OperationResultType.Success);

        }

        [Fact]
        public void TestAcceptFriend()
        {
            var memberId1 = 26;
            var memberId2 = 2;
            var service = _fixture.Container.Resolve<IMemberIMFriendContract>();
            var res = service.AcceptFriend(memberId1, memberId2);
            Assert.True(res.ResultType == Whiskey.Utility.Data.OperationResultType.Success);

            var res2 = service.AcceptFriend(memberId2, memberId1);
            Assert.True(res.ResultType == Whiskey.Utility.Data.OperationResultType.Success);

        }



        [Fact]
        public void TestBlackList()
        {
            var userId = "26";
            var blackUserId = new string[] { "5275" };
            var res = RongCloudServerHelper.Black(userId, blackUserId);
            Assert.Equal(200, res.code);

            var res2 = RongCloudServerHelper.QueryBlack(userId);
            Assert.Equal(200, res2.code);
            var blackUsers = res2.users;
            Assert.True(blackUsers.Count() == 1);


            var res3 = RongCloudServerHelper.UnBlack(userId, blackUserId.First());
            Assert.Equal(200, res3.code);
            blackUsers = RongCloudServerHelper.QueryBlack(userId).users;
            Assert.True(blackUsers.Count() == 0);

        }

        //[Fact]
        //public void TestPublishPrivateMsg()
        //{
        //    var userId = "4242";
        //    var toUserId = new string[]
        //    {
        //        "muzi"
        //    };
        //    var textMsg = new TxtMessage("hello", "world");
        //    var msg = new
        //    {
        //        content = "hellllo",
        //        extra = "msmmmg",
        //        user = new
        //        {
        //            id = "4242",
        //            name = "邓亚魁",
        //            icon = "https://www.baidu.com/img/bd_logo1.png"
        //        }
        //    };
        //    var content = JsonConvert.SerializeObject(msg);
        //    //var contactMsg = new ContactNtfMessage("op1", "extra", "1", "-1", "约吗");
        //    //var content2 = JsonConvert.SerializeObject(contactMsg);
        //    //var msg = new { content = "hello" };
        //    //var res = _client.message.publishPrivate(userId, toUserId, "RC:TxtMsg", content, string.Empty, string.Empty, string.Empty, 0, 0, 0);
        //    var res2 = _client.message.publishPrivate(userId, toUserId, "RC:TxtMsg", content, string.Empty, string.Empty, string.Empty, 0, 0, 0);
        //}
        //[Fact]
        //public void ttt()
        //{

        //    var userId = "4242";
        //    var toUserId = new string[]
        //    {
        //        "muzi"
        //    };
        //    var msg = new InfoNtfMessage("info...", "extra...");

        //    var content = JsonConvert.SerializeObject(msg);
        //    var res2 = _client.message.publishPrivate(userId, toUserId, "RC:InfoNtf", content, string.Empty, string.Empty, string.Empty, 0, 0, 0);

        //}

        //[Fact]
        //public void tttt()
        //{
        //    var userId = "4242";
        //    var toUserId = new string[]
        //    {
        //        "-1"
        //    };
        //    var user = new UserInfo()
        //    {
        //        id = "4242",
        //        icon = "https://www.baidu.com/img/bd_logo1.png",
        //        name = "dengyakui"
        //    };
        //    var msg = new TxtMessage("hhh","extra",user);
        //    var res = _client.message.publishPrivate(userId, toUserId, "", msg.toString(),string.Empty, string.Empty, string.Empty, 0, 0,0);


        //}

    }
}
