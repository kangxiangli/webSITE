using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Data;
using Whiskey.Utility.Logging;
using Whiskey.Utility.Secutiry;
using Whiskey.Web.Helper;
using Whiskey.Web.Mvc;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Base;
using Whiskey.RongCloudServer;
using Whiskey.ZeroStore.MobileApi.Extensions.Attribute;
using Newtonsoft.Json;
using Whiskey.ZeroStore.ERP.Models;

namespace Whiskey.ZeroStore.MobileApi.Areas.Message.Controllers
{
    public class IMController : BaseController
    {


        private IMemberContract _memberContract;
        private IMemberIMProfileContract _imContract;
        private IMemberIMFriendContract _friendContract;
        public IMController(IMemberContract memContract, IMemberIMProfileContract imContract,
            IMemberIMFriendContract friendContract)
        {
            _memberContract = memContract;
            _imContract = imContract;
            _friendContract = friendContract;
        }

        /// <summary>
        /// 发送好友请求
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddFriend(NtfMsg msg)
        {
            try
            {
                // param validate
                if (msg == null || string.IsNullOrEmpty(msg.sourceUserId)
                    || string.IsNullOrEmpty(msg.targetUserId)
                    )
                {
                    return Json(new OperationResult(OperationResultType.ValidError, "发送失败"));
                }


                // sender exist check
                var sourceIdInt = int.Parse(msg.sourceUserId);
                var sourceMember = _memberContract.Members.Where(m => m.Id == sourceIdInt && !m.IsDeleted && m.IsEnabled).FirstOrDefault();
                if (sourceMember == null)
                {
                    return Json(new OperationResult(OperationResultType.ValidError, "请求用户不存在"));
                }

                // receiver exist  check
                var targetMember = _memberContract.Members.Where(m => m.MobilePhone == msg.targetUserId && !m.IsDeleted && m.IsEnabled).FirstOrDefault();
                if (targetMember == null)
                {
                    return Json(new OperationResult(OperationResultType.ValidError, "目标用户不存在"));
                }


                // send msg
                //var res = RongCloudServerHelper.PublishContactNtfMsg(msg.sourceUserId, targetMember.Id.ToString(), msg);
                var res = RongCloudServerHelper.publishSystem(msg.sourceUserId, new string[] { targetMember.Id.ToString() }, JsonConvert.SerializeObject(msg), msg.GetMsgType(), "", "", "", 1, 1, 1);

                //ret
                if (res.code != 200)
                {
                    return Json(new OperationResult(OperationResultType.Error, "发送失败"));
                }

                //保存请求记录

                return Json(new OperationResult(OperationResultType.Success, string.Empty));
            }
            catch (Exception)
            {
                return Json(new OperationResult(OperationResultType.Error, "系统错误"));
            }
        }


        /// <summary>
        /// 同意添加好友
        /// </summary>
        /// <param name="memberId1"></param>
        /// <param name="memberId2"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AcceptFriend(int memberId1, int memberId2)
        {
            try
            {
                var res = _friendContract.AcceptFriend(memberId1, memberId2);
                return Json(res);
            }
            catch (Exception)
            {

                return Json(new OperationResult(OperationResultType.Error, "系统错误"));
            }

        }

        [HttpPost]
        public ActionResult GetMyFriend(int memberId)
        {
            try
            {
                var res = _friendContract.GetMyFriend(memberId);
                return Json(res);
            }
            catch (Exception)
            {

                return Json(new OperationResult(OperationResultType.Error, "系统错误"));
            }
        }

        [HttpPost]
        public ActionResult GetFriendDetail(int friendId)
        {
            try
            {
                int userId = int.Parse(Request.Params["memberId"]);
                var res = _friendContract.GetFriendDetail(userId,friendId);
                return Json(res);
            }
            catch (Exception)
            {

                return Json(new OperationResult(OperationResultType.Error, "系统错误"));
            }
        }

        [HttpPost]
        public ActionResult GetToken(string memberId, string name, string avatar)
        {
            try
            {
                var res = _imContract.GetToken(memberId, name, avatar);
                return Json(res);
            }
            catch (Exception)
            {

                return Json(new OperationResult(OperationResultType.Error, "系统错误"));
            }
        }


        [HttpPost]
        public ActionResult AddBlack(String userId, string[] blackUserIds)
        {
            try
            {
                var res = RongCloudServerHelper.Black(userId, blackUserIds);
                return Json(new OperationResult( OperationResultType.Success,string.Empty,res));
            }
            catch (Exception)
            {

                return Json(new OperationResult(OperationResultType.Error, "系统错误"));
            }
        }
        [HttpPost]
        public ActionResult RemoveBlack(String userId, string blackUserId)
        {
            try
            {
                var res = RongCloudServerHelper.UnBlack(userId, blackUserId);
                return Json(new OperationResult(OperationResultType.Success, string.Empty, res));
            }
            catch (Exception)
            {

                return Json(new OperationResult(OperationResultType.Error, "系统错误"));
            }
        }

        [HttpPost]
        public ActionResult QueryBlack(String userId)
        {
            try
            {
                var res = RongCloudServerHelper.QueryBlack(userId);
                return Json(new OperationResult(OperationResultType.Success, string.Empty, res));
            }
            catch (Exception)
            {

                return Json(new OperationResult(OperationResultType.Error, "系统错误"));
            }
        }
    }



}