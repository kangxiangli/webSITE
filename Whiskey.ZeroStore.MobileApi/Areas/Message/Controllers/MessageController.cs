using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Data;
using Whiskey.Utility.Extensions;
using Whiskey.ZeroStore.ERP.Services.Contracts;

namespace Whiskey.ZeroStore.MobileApi.Areas.Message.Controllers
{
    public class MessageController : Controller
    {
        protected readonly IMessagerContract _messagerContract;
        protected readonly INotificationContract _notificationContract;
        protected readonly IAdministratorContract _adminContract;
        protected readonly IMemberContract _memberContract;
        protected readonly IMsgNotificationContract _msgNotificationContract;
        protected readonly IDepartmentContract _departmentContract;
        protected readonly ITrainintBlogContract _trainintBlogContract;
        protected readonly IExamRecordContract _examRecordContract;
        protected readonly INotificationQASystemContract _notificationQASystemContract;

        public MessageController(IMessagerContract messageContract, IMsgNotificationContract msgNotificationContract,
            IAdministratorContract administratorContract, INotificationContract notificationContract,
            IDepartmentContract departmentContract,
            ITrainintBlogContract trainintBlogContract,
            IExamRecordContract examRecordContract,
            INotificationQASystemContract notificationQASystemContract)
        {
            _notificationContract = notificationContract;
            _messagerContract = messageContract;
            _msgNotificationContract = msgNotificationContract;
            _adminContract = administratorContract;
            _departmentContract = departmentContract;
            _trainintBlogContract = trainintBlogContract;
            _examRecordContract = examRecordContract;
            _notificationQASystemContract = notificationQASystemContract;
        }

        // GET: Message/Message
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GetUnRead(int AdminId, int? Status)
        {
            try
            {
                //暂时只支持查找未读，如果以后要查找已读的情况，将该句删除
                Status = 0;

                var query = _messagerContract.Messagers.Where(m => m.IsDeleted == false && m.IsEnabled == true && m.ReceiverId == AdminId);
                if (Status.HasValue)
                {
                    query = query.Where(m => m.Status == Status.Value);
                }
                var list = query.OrderBy(m => m.Status)
                    .ThenByDescending(m => m.CreatedTime)
                    .Select(m => new
                    {
                        m.Id,
                        Title = m.MessageTitle,
                        Content = m.Description,
                        Sender = m.Sender.Member.RealName,
                        SenderPhoto = m.Sender.Member.UserPhoto,
                        SendTime = m.CreatedTime,
                        Status = m.Status,
                        Type = 1
                    });


                var queryN = _msgNotificationContract.MsgNotificationReaders.Where(r => r.IsDeleted == false && r.IsEnabled == true && !r.Notification.IsDeleted && r.Notification.IsEnabled && r.AdministratorId == AdminId);
                if (Status.HasValue)
                {
                    var isRead = Status == 0 ? false : true;
                    queryN = queryN.Where(r => r.IsRead == isRead);
                }

                var total = list.Union(queryN.OrderBy(n => n.IsRead)
                    .ThenByDescending(n => n.CreatedTime)
                    .Select(n => new
                    {
                        n.Id,
                        n.Notification.Title,
                        Content = n.Notification.Description,
                        Sender = n.Notification.Operator == null ? "" : n.Notification.Operator.Member.RealName,
                        SenderPhoto = n.Notification.Operator.Member.UserPhoto,
                        SendTime = n.Notification.CreatedTime,
                        Status = n.IsRead ? 1 : 0,
                        Type = 2
                    })).ToList();

                var res = total.Select(s => new
                {
                    s.Id,
                    s.Title,
                    Content = s.Content,
                    RealName = s.Sender,
                    SenderPhoto = s.SenderPhoto,
                    SendTime = s.SendTime.ToUnixTime(),
                    Status = s.Status,
                    s.Type
                }).ToList();

                return Json(new OperationResult(OperationResultType.Success, string.Empty, res), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new OperationResult(OperationResultType.Error, "系统错误"), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult GetExamRecord(int adminId, int pageIndex = 1, int pageSize = 10)
        {
            if (!_adminContract.CheckExists(a => a.Id == adminId))
            {
                return Json(new OperationResult(OperationResultType.Error, "用户未登录"), JsonRequestBehavior.AllowGet);
            }
            var query = _examRecordContract.Entities;
            query = query.Where(e => !e.IsDeleted && e.IsEnabled && e.AdminId == adminId);
            var list = query.OrderByDescending(e => e.UpdatedTime)
                            .Skip((pageIndex - 1) * pageSize)
                            .Take(pageSize)
                            .Select(e => new
                            {
                                examRecordId = e.Id,
                                e.TraingBlogId,
                                e.TrainingBlog.Title,
                                e.GetScore,
                                e.IsPass,
                                e.CreatedTime,
                                e.RewardMemberScore,
                                State = e.State.ToString()
                            }).ToList()
                            .Select(e => new
                            {
                                e.examRecordId,
                                BlogTitle = e.Title,
                                BlogId = e.TraingBlogId,
                                e.GetScore,
                                e.IsPass,
                                BlogUrl = "/msg/viewBlog?blogId=" + e.TraingBlogId,
                                e.RewardMemberScore,
                                State = e.State.ToString()
                            }).ToList();


            var res = new OperationResult(OperationResultType.Success, string.Empty, list);
            return Json(res, JsonRequestBehavior.AllowGet);
        }
    }
}