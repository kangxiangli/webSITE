using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Data;
using Whiskey.Utility.Extensions;
using Whiskey.Web.Extensions;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Notices;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;

namespace Whiskey.ZeroStore.ERP.Website.Controllers
{
    [AllowCross]
    public class MsgController : BaseController
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

        public MsgController(IMessagerContract messageContract, IMsgNotificationContract msgNotificationContract,
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


        [HttpPost]
        public ActionResult GetNotice(int adminId, int? status, int PageIndex = 1, int PageSize = 10)
        {
            try
            {
                var query = _msgNotificationContract.MsgNotificationReaders.Where(r => r.IsDeleted == false && r.IsEnabled == true && !r.Notification.IsDeleted && r.Notification.IsEnabled && r.AdministratorId == adminId);
                if (status.HasValue)
                {
                    var isRead = status == 0 ? true : false;
                    query = query.Where(r => r.IsRead == isRead);
                }

                var list = query.OrderBy(n => n.IsRead)
                    .ThenByDescending(n => n.CreatedTime)
                    .Select(n => new
                    {
                        n.Id,
                        n.Notification.Title,
                        n.Notification.Description,
                        n.Notification.Operator.Member.RealName,
                        n.Notification.Operator.Member.UserPhoto,
                        n.Notification.CreatedTime,
                        n.IsRead
                    }).Skip((PageIndex - 1) * PageSize).Take(PageSize).ToList();
                var res = list.Select(s => new
                {
                    s.Id,
                    s.Title,
                    Content = s.Description,
                    RealName = s.RealName,
                    SenderPhoto = s.UserPhoto,
                    SendTime = s.CreatedTime.ToUnixTime(),
                    Status = s.IsRead ? 1 : 0
                }).ToList();

                return Json(new OperationResult(OperationResultType.Success, string.Empty, res));
            }
            catch (Exception e)
            {
                return Json(new OperationResult(OperationResultType.Error, "系统错误"));
            }



        }

        [HttpPost]
        public ActionResult SendMsg(int senderId, int receiverId, string title, string content)
        {
            try
            {
                //校验接收者是否存在
                var entity = _adminContract.Administrators.FirstOrDefault(a => !a.IsDeleted && a.IsEnabled && a.Id == receiverId);
                if (entity == null)
                {
                    return Json(new OperationResult(OperationResultType.Error, "接收者信息有误"));
                }

                //发送
                List<MessagerDto> listDto = new List<MessagerDto>()
                {
                    new MessagerDto()
                    {
                        MessageTitle = title,
                        Description = content,
                        SenderId = senderId,
                        ReceiverIds = new List<int>() {receiverId },
                        ReceiverId = receiverId,
                        Status = (int)MessagerStatusFlag.UnRead,
                    }
                };

                var result = _messagerContract.Insert(sendMessageAction, listDto.ToArray());

                return Json(result);

            }
            catch (Exception e)
            {
                return Json(new OperationResult(OperationResultType.Error, "系统错误"));
            }
        }
        [HttpPost]
        public ActionResult GetMsg(int AdminId, int? Status)
        {
            try
            {
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
                        m.Sender.Member.UserPhoto,
                        SendTime = m.CreatedTime,
                        m.Status
                    }).ToList();
                var res = list.Select(m => new
                {
                    m.Id,
                    Title = m.Title,
                    Content = m.Content,
                    Sender = m.Sender,
                    SenderPhoto = m.UserPhoto,
                    SendTime = m.SendTime.ToUnixTime(),
                    Status = m.Status
                }).ToList();
                return Json(new OperationResult(OperationResultType.Success, string.Empty, res));
            }
            catch (Exception e)
            {
                return Json(new OperationResult(OperationResultType.Error, "系统错误"));
            }
        }

        [HttpPost]
        public ActionResult TagRead(int adminId, int msgType, int msgId)
        {
            try
            {
                OperationResult result = new OperationResult(OperationResultType.Error, string.Empty);
                if (msgType == 1)  //系统通知
                {
                    var entity = _msgNotificationContract.View(msgId);
                    if (entity == null || entity.AdministratorId != adminId)
                    {
                        return Json(new OperationResult(OperationResultType.Error, "消息不存在"));
                    }
                    entity.IsRead = true;
                    var dto = Mapper.Map<MsgNotificationReaderDto>(entity);
                    result = _msgNotificationContract.Update(dto);
                }
                else if (msgType == 2)  //用户消息
                {
                    var entity = _messagerContract.View(msgId);
                    if (entity == null || entity.ReceiverId != adminId)
                    {
                        return Json(new OperationResult(OperationResultType.Error, "消息不存在"));
                    }
                    entity.Status = (int)MessagerStatusFlag.Read;
                    var dto = Mapper.Map<MessagerDto>(entity);
                    result = _messagerContract.Update(null, dto);
                }
                if (result.ResultType == OperationResultType.Success)
                {
                    sendMessageAction(new List<int>() { adminId });
                }
                return Json(result);
            }
            catch (Exception e)
            {
                return Json(new OperationResult(OperationResultType.Error, "系统错误"));
            }
        }

        [HttpPost]
        public ActionResult GetStaffList()
        {
            try
            {
                var list = _adminContract.Administrators.Where(a => !a.IsDeleted && a.IsEnabled).Select(a => new
                {
                    Id = a.Id,
                    AdminName = a.Member.RealName
                }
                ).ToList();
                return Json(new OperationResult(OperationResultType.Success, string.Empty, list));
            }
            catch (Exception e)
            {
                return Json(new OperationResult(OperationResultType.Error, "系统错误"));
            }
        }

        [HttpPost]
        public ActionResult GetHistorySend(int adminId)
        {
            try
            {
                var list = _messagerContract.Messagers.Where(m => m.SenderId == adminId).OrderByDescending(m => m.CreatedTime)
                    .Select(m => new
                    {
                        m.Id,
                        Title = m.MessageTitle,
                        Content = m.Description,
                        Sender = m.Operator.Member.RealName,
                        Receiver = m.Receiver.Member.RealName,
                        m.Operator.Member.UserPhoto,
                        SendTime = m.CreatedTime,
                        ReceiverPhoto = m.Receiver.Member.UserPhoto,
                        m.Status
                    })
                    .ToList();

                var res = list.Select(m => new
                {
                    m.Id,
                    Title = m.Title,
                    Content = m.Content,
                    Sender = m.Sender,
                    Receiver = m.Receiver,
                    ReceiverPhoto = m.ReceiverPhoto,
                    SenderPhoto = m.UserPhoto,
                    SendTime = m.SendTime.ToUnixTime(),
                    Status = m.Status
                }).ToList();
                return Json(new OperationResult(OperationResultType.Success, string.Empty, res));
            }
            catch (Exception e)
            {
                return Json(new OperationResult(OperationResultType.Error, "系统错误"));
            }
        }

        [HttpPost]
        public ActionResult GetNoticeTarget()
        {

            var dict = new Dictionary<int, string>();
            dict.Add((int)NoticeTargetFlag.Admin, NoticeTargetFlag.Admin.ToString());
            dict.Add((int)NoticeTargetFlag.Department, NoticeTargetFlag.Department.ToString());
            //dict.Add((int)NoticeTargetFlag.Member, NoticeTargetFlag.Member.ToString()); //暂不支持

            var res = dict.Select(kp => new { value = kp.Key, name = kp.Value }).ToList();

            return Json(new OperationResult(OperationResultType.Success, string.Empty, res));
        }

        [HttpPost]
        public ActionResult GetDepartmentInfo()
        {
            var list = _departmentContract.Departments.Where(w => w.IsDeleted == false && w.IsEnabled == true).Select(m => new
            {
                Name = m.DepartmentName,
                value = m.Id.ToString()
            }).ToList();
            return Json(new OperationResult(OperationResultType.Success, string.Empty, list));
        }

        [HttpPost]
        public ActionResult SendNotice(int adminId, int target, string title, string content, string departmentIds)
        {
            try
            {
                NoticeTargetFlag noticeTarget;
                NoticeFlag noticeType = NoticeFlag.Immediate;
                if (!Enum.TryParse(target.ToString(), out noticeTarget))
                {
                    return Json(new OperationResult(OperationResultType.Error, "通知目标类型错误"));
                }
                if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(content))
                {
                    return Json(new OperationResult(OperationResultType.Error, "通知标题及内容不能为空"));
                }



                var dto = new NotificationDto()
                {
                    Title = title,
                    Description = content,
                    NoticeType = noticeType,
                    NoticeTargetType = (int)noticeTarget,
                };

                switch (noticeTarget)
                {
                    case NoticeTargetFlag.Admin:
                        break;

                    case NoticeTargetFlag.Department:
                        {
                            //按部门发送,传入部门id
                            if (string.IsNullOrEmpty(departmentIds))
                            {
                                return Json(new OperationResult(OperationResultType.Error, "部门id不能为空"));
                            }
                            var deptArr = departmentIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).Distinct().ToArray();
                            for (int i = 0; i < deptArr.Length; i++)
                            {
                                dto.DepartmentIds.Add(deptArr[i]);
                            }
                        }
                        break;
                    default:
                        break;
                }
                var res = _notificationContract.Insert(sendNotificationAction, dto);
                return Json(res);
            }
            catch (Exception e)
            {

                return Json(new OperationResult(OperationResultType.Error, "系统错误"));
            }

        }
        /// <summary>
        /// 发送条码信息到PC
        /// </summary>
        /// <param name="AdminId"></param>
        /// <param name="strbarcode"></param>
        /// <returns></returns>
        public ActionResult SendBarCodeInfo(int AdminId, params string[] barcode)
        {
            var result = NotificationHub.SendBarCodeInfo(AdminId, barcode);
            return Json(result);
        }

        //[CheckCookieAttrbute]
        public ActionResult ViewBlog(int blogId)
        {
            var entity = _trainintBlogContract.View(blogId);
            if (entity == null)
            {
                return HttpNotFound();
            }
            return View(entity);
        }

        [HttpPost]
        [CheckCookieAttrbute]

        public ActionResult GetExamRecord(int pageIndex = 1, int pageSize = 10)
        {
            var adminId = AuthorityHelper.OperatorId.Value;
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
            return Json(res);

        }



        /// <summary>
        /// 是否是再次考试
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [CheckCookieAttrbute]

        public ActionResult IsRestartExam(int examRecordId)
        {
            var res = _examRecordContract.IsRestartExam(examRecordId);
            if (!res.Item1)
            {
                return Json(new OperationResult(OperationResultType.Success, "no", res.Item2) { Other = res.Item3});

            }
            return Json(new OperationResult(OperationResultType.Success, "yes", res.Item2) { Other = res.Item3});

        }

        [HttpPost]
        [CheckCookieAttrbute]

        public ActionResult StartExam(int examRecordId)
        {

            var res = _examRecordContract.StartOrRestartExam(examRecordId);
            return Json(res);

        }



        [HttpPost]
        [CheckCookieAttrbute]

        public ActionResult SubmitExam(SubmitExamDTO dto)
        {
            try
            {
                var answerDetail = Request.Params["answerDetail"] ?? Request.Params["AnswerDetail"];
                if (answerDetail.IsNullOrEmpty())
                {
                    throw new ArgumentNullException("answerDetail", "参数不能为空");
                }
                dto.AnswerDetail = JsonHelper.FromJson<Answerdetail[]>(answerDetail);
                var res = _examRecordContract.SubmitExam(dto);
                return Json(res);
            }
            catch (Exception e)
            {
                return Json(OperationResult.Error(e.Message));
            }

        }


        [HttpPost]
        [CheckCookieAttrbute]

        public ActionResult ViewExamRecordDetail(int examRecordId)
        {
            var res = _examRecordContract.GetExamRecordDetail(examRecordId);
            return Json(res);
        }

        #region 是否需要答题
        /// <summary>
        /// 是否需要答题
        /// </summary>
        /// <param name="notificationReadId"></param>
        /// <param name="adminId"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult IsAnswer(int notificationReadId, int adminId)
        {
            if (!_adminContract.CheckExists(a => a.Id == adminId))
            {
                return Json(new OperationResult(OperationResultType.Error, "用户不存在"), JsonRequestBehavior.AllowGet);
            }
            var notificationReader = _msgNotificationContract.View(notificationReadId);
            if (notificationReader == null)
            {
                return Json(new OperationResult(OperationResultType.Error, "消息不存在"), JsonRequestBehavior.AllowGet);
            }
            int num = _notificationQASystemContract.Entities.Count(q => q.NotificationId == notificationReader.NotificationId && q.IsEnabled && !q.IsDeleted);
            if (num == 0)
            {//看完就标致已读
                if (notificationReadId > 0)
                {
                    var entity = _msgNotificationContract.View(notificationReadId);
                    if (entity == null || entity.AdministratorId != adminId)
                    {
                        return Json(new OperationResult(OperationResultType.Error, "消息不存在"));
                    }
                    entity.IsRead = true;
                    var dto = Mapper.Map<MsgNotificationReaderDto>(entity);
                    _msgNotificationContract.Update(dto);
                }

                return Json(new OperationResult(OperationResultType.QueryNull), JsonRequestBehavior.AllowGet);
            }

            OperationResult opera = _notificationQASystemContract.CheckIsAnswer(notificationReadId, adminId);

            return Json(opera, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 获取要回答的题目
        /// <summary>
        /// 获取要回答的题目
        /// </summary>
        /// <param name="notificationReadId"></param>
        /// <param name="adminId"></param>
        /// <returns></returns>
        public JsonResult GetQuestion(int notificationReadId, int adminId)
        {
            var opera = _notificationQASystemContract.GetQuestion(notificationReadId, adminId <= 0 ? AuthorityHelper.OperatorId ?? 0 : adminId);

            if (opera.Data != null)
            {
                opera.Data = (opera.Data as List<NotificationQuestion>).Select(q => new
                {
                    q.Id,
                    q.GuidId,
                    q.QuestionType,
                    q.Content,
                    q.NotificationId,
                    AnsweringList = q.AnsweringsList == null ? new List<Answering>() : q.AnsweringsList.Select(a => new Answering
                    {
                        Id = a.Id,
                        GuidId = a.GuidId,
                        QGuidId = a.GuidId,
                        Number = a.Number,
                        Content = q.QuestionType == (int)QuestionTypeFlag.Choice ? a.Content : ""
                    })
                });
            }
            return Json(opera, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 答题
        /// <summary>
        /// 答题
        /// </summary>
        /// <param name="list"></param>
        /// <param name="Ids"></param>
        /// <param name="notificationReadId"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult InAnswer(string dicJson, string Ids, int notificationReadId, int adminId)
        {
            IDictionary<Guid, string> dic = JsonHelper.FromJson<Dictionary<Guid, string>>(dicJson);
            if (dic.Count() != Ids.Split(",").Length)
            {
                return Json(new OperationResult(OperationResultType.Error, "题目未答完"));
            }

            OperationResult opera = _notificationQASystemContract.Answer(adminId, notificationReadId, dic);

            return Json(opera);
        }
        #endregion
    }

    public class Answering
    {
        public int Id { get; set; }
        public Guid GuidId { get; set; }
        public Guid QGuidId { get; set; }
        public string Number { get; set; }
        public string Content { get; set; }
    }
}