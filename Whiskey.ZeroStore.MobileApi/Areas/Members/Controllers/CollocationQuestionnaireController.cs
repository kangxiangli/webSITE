using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Data;
using Whiskey.Utility.Extensions;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.MobileApi.Controllers;

namespace Whiskey.ZeroStore.MobileApi.Areas.Members.Controllers
{
    public class CollocationQuestionnaireController : BaseController
    {

        #region 初始化操作对象
        //日志记录
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(CollocationQuestionnaireController));

        protected readonly IMemberContract _memberContract;
        protected readonly ICollocationQuestionnaireContract _collocationQuestionnaireContract;
        protected readonly IAdministratorContract _administratorContract;

        public CollocationQuestionnaireController(
            IMemberContract memberContract,
            ICollocationQuestionnaireContract collocationQuestionnaireContract,
            IAdministratorContract administratorContract
           )
        {
            _memberContract = memberContract;
            _collocationQuestionnaireContract = collocationQuestionnaireContract;
            _administratorContract = administratorContract;
        }
        #endregion

        // GET: Members/CollocationQuestionnaire
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 会员详细测试添加，准备弃用，使用下方接口
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="dicJson"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult InsertCQA(int memberId, string dicJson)
        {
            try
            {
                if (!_memberContract.CheckExists(m => m.Id == memberId))
                {
                    return Json(new OperationResult(OperationResultType.Error, "该会员不存在"), JsonRequestBehavior.AllowGet);
                }

                IDictionary<string, string> dic = JsonHelper.FromJson<Dictionary<string, string>>(dicJson);
                if (dic == null || dic.Count() == 0)
                {
                    return Json(new OperationResult(OperationResultType.Error, "问卷不可为空"), JsonRequestBehavior.AllowGet);
                }

                List<CollocationQuestionnaire> list = new List<CollocationQuestionnaire>();
                foreach (var item in dic)
                {
                    string[] values = item.Value.Split(',');

                    foreach (var val in values)
                    {
                        if (val.Trim() == "")
                        {
                            continue;
                        }
                        CollocationQuestionnaire model = new CollocationQuestionnaire()
                        {
                            GuidId = Guid.NewGuid(),
                            QuestionName = item.Key,
                            Content = val,
                            MemberId = memberId
                        };
                        list.Add(model);
                    }
                }

                OperationResult oper = _collocationQuestionnaireContract.Insert(list.ToArray());
                return Json(oper, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new OperationResult(OperationResultType.Error, "系统错误"), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 获取会员线上咨询师问卷调查信息
        /// </summary>
        /// <param name="memberId">用户ID</param>
        /// <param name="adminOrMember">传的是员工ID还是会员ID（0：员工ID；1是会员ID；默认为1）</param>
        /// <returns></returns>
        public JsonResult GetList(int memberId, int adminOrMember = 1)
        {
            try
            {
                if (adminOrMember == 0)
                {
                    var admin = _administratorContract.View(memberId);
                    if (admin == null)
                    {
                        return Json(new OperationResult(OperationResultType.Error, "该会员不存在"), JsonRequestBehavior.AllowGet);
                    }

                    memberId = admin.MemberId ?? 0;
                }

                if (!_memberContract.CheckExists(m => m.Id == memberId))
                {
                    return Json(new OperationResult(OperationResultType.Error, "该会员不存在"), JsonRequestBehavior.AllowGet);
                }

                #region 线上咨询师问卷调查回答信息
                IDictionary<string, string[]> dic = new Dictionary<string, string[]>();

                var names = _collocationQuestionnaireContract.Entities.Where(c => c.MemberId == memberId).GroupBy(c => new { c.QuestionName }).Select(c => c.Key).ToArray();

                foreach (var name in names)
                {
                    string[] values = _collocationQuestionnaireContract.Entities.Where(c => c.MemberId == memberId && c.QuestionName == name.QuestionName && !c.IsDeleted && c.IsEnabled).Select(c => c.Content).ToArray();

                    KeyValuePair<string, string[]> item = new KeyValuePair<string, string[]>(name.QuestionName, values);

                    dic.Add(item);
                }

                #endregion
                return Json(new OperationResult(OperationResultType.Success, "", dic), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new OperationResult(OperationResultType.Error, "系统错误"), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult AddCQA(int memberId, string CQAs)
        {
            try
            {
                if (!_memberContract.CheckExists(m => m.Id == memberId))
                {
                    return Json(new OperationResult(OperationResultType.Error, "该会员不存在"), JsonRequestBehavior.AllowGet);
                }

                Dictionary<string, Dictionary<string, int>> dic = JsonHelper.FromJson<Dictionary<string, Dictionary<string, int>>>(CQAs);
                if (dic == null || dic.Count() == 0)
                {
                    return Json(new OperationResult(OperationResultType.Error, "问卷不可为空"), JsonRequestBehavior.AllowGet);
                }

                List<CollocationQuestionnaire> list = new List<CollocationQuestionnaire>();
                foreach (var item in dic)
                {
                    var qName = item.Key;
                    var values = item.Value;
                    foreach (var cq in values)
                    {
                        CollocationQuestionnaire model = new CollocationQuestionnaire()
                        {
                            GuidId = Guid.NewGuid(),
                            QuestionName = qName,
                            Content = cq.Key,
                            Score = cq.Value,
                            MemberId = memberId
                        };
                        list.Add(model);
                    }
                }

                OperationResult oper = _collocationQuestionnaireContract.Insert(list.ToArray());
                return Json(oper, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new OperationResult(OperationResultType.Error, "系统错误"), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 获取详情风格测试
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="isDetail">获取详细列表</param>
        /// <returns></returns>
        public JsonResult GetCQA(int memberId, bool isDetail = false)
        {
            var rdata = OperationHelper.Try(() =>
            {
                var query = _collocationQuestionnaireContract.Entities.Where(w => w.IsEnabled && !w.IsDeleted && w.MemberId == memberId);

                var allscore = query.Sum(s => s.Score);

                var imgpath = allscore >= 80 ? ""
                            : allscore >= 50 ? ""
                            : "";

                var typeimg = $"{WebUrl}{imgpath}";

                var data = new object();
                if (isDetail)
                {
                    var list = (from s in query
                                group s by s.QuestionName into g
                                select new
                                {
                                    QuestionName = g.Key,
                                    QuestionAnswers = g.Select(s => new
                                    {
                                        s.Content,
                                        s.Score,
                                    })
                                }).ToList();
                    data = new
                    {
                        CQAs = list,
                        TypeImg = typeimg,
                    };
                }
                else
                {
                    data = new
                    {
                        TypeImg = typeimg
                    };
                }

                return OperationHelper.ReturnOperationResultDIY(OperationResultType.Success, "", data);

            }, ex =>
            {
                return OperationHelper.ReturnOperationResultDIY(OperationResultType.Error, "系统错误");
            });

            return Json(rdata, JsonRequestBehavior.AllowGet);
        }
    }
}