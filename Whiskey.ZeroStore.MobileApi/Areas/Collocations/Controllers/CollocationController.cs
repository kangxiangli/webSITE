using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Class;
using Whiskey.Utility.Data;
using Whiskey.Utility.Helper;
using Whiskey.Utility.Logging;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Collocation;
using Whiskey.ZeroStore.MobileApi.Areas.Collocations.Models;
using Whiskey.ZeroStore.MobileApi.Extensions.Attribute;

namespace Whiskey.ZeroStore.MobileApi.Areas.Collocations.Controllers
{
    [License(CheckMode.Verify)]
    public class CollocationController : Controller
    {
        #region 业务层操作对象
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(CollocationController));

        protected readonly IMissionContract _missionContract;

        protected readonly ICategoryContract _categoryContract;

        protected readonly IMemberContract _memberContract;

        protected readonly IMemberCollocationContract _memberCollContract;

        protected readonly ICollocationContract _collocationContract;
        public CollocationController(IMissionContract missionContract,
            IMemberContract memberContract,
            ICategoryContract categoryContract,
            ICollocationContract collocationContract,
            IMemberCollocationContract memberCollContract)
        {
            _missionContract = missionContract;             
            _memberContract = memberContract;
            _categoryContract = categoryContract;
            _collocationContract = collocationContract;
            _memberCollContract = memberCollContract;
        }
        #endregion
        string strWebUrl = ConfigurationHelper.GetAppSetting("WebUrl");


        #region 获取搭配师
        /// <summary>
        /// 获取搭配师
        /// </summary>
        /// <param name="PageIndex"></param>
        /// <param name="PageSize"></param>
        /// <returns></returns>
        public JsonResult GetList(int PageIndex=1,int PageSize=10)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error);
            try
            {
                int memberId = int.Parse(Request["MemberId"]);                
                Member member = _memberContract.View(memberId);
                if (member.CollocationId!=null)
                {
                    oper.Message = "已经拥有搭配师，无法再次绑定";
                }
                else
                {
                    IQueryable<Collocation> listCo=  _collocationContract.Collocations.Where(x => x.IsDeleted == false && x.IsEnabled == true);
                    IQueryable<Member> listMember= _memberContract.Members.Where(x=>x.IsDeleted==false && x.IsEnabled==true);
                    int count = listCo.Count();
                    int size = count / PageSize;
                    if (size==0)
                    {
                        size = 1;
                    }
                    Random random = new Random();
                    PageIndex = random.Next(1, size);
                    listCo = listCo.OrderByDescending(x => x.Id).Skip((PageIndex - 1) * PageSize).Take(PageSize);
                    var entity = (from co in listCo
                                  join
                                  me in listMember
                                  on
                                  co.Numb equals me.UniquelyIdentifies
                                  select new { 
                                   CollocationId= co.Id,
                                   me.MemberName,
                                   UserPhoto=strWebUrl+me.UserPhoto,
                                  }).ToList();
                    oper.ResultType = OperationResultType.Success;
                    oper.Data = entity;                   
                }
                return Json(oper);
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                oper.Message = "服务器忙，请稍后重试";
                return Json(oper);
            }
        }
        #endregion

        #region 是否拥有搭配师
        private JsonResult IsHave()
        {
            try
            {
                int memberId = int.Parse(Request["MemberId"]);
                Member member = _memberContract.View(memberId);
                int isHave = 0; //0表示没有 1表示有;
                if (member.CollocationId==null)
                {
                    isHave = 0;
                }
                else
                {
                    isHave = 1;
                }
                return Json(new OperationResult(OperationResultType.Success, "获取成功", isHave));
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return Json(new OperationResult(OperationResultType.Error, "服务器忙，请稍后重试"));
            }
        }
        #endregion

        #region 获取粉丝列表
        public JsonResult GetFansList(int PageIndex = 1, int PageSize = 10)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error);
            try
            {
                string strMemberId = Request["MemberId"];
                int memberId = int.Parse(strMemberId);
                IQueryable<Member> listMember = _memberContract.Members.Where(x => x.Id == memberId);
                IQueryable<Collocation> listColl = _collocationContract.Collocations.Where(x => x.IsDeleted == false && x.IsEnabled == true);
                var entity = (from me in listMember
                              join
                              co in listColl
                              on
                              me.UniquelyIdentifies equals co.Numb
                              select new
                              {
                                  MemberId = me.Id,
                                  ColloId = co.Id,
                              }).FirstOrDefault();
                if (entity == null)
                {
                    oper.Message = "访问失败，请稍后重试";
                }
                else
                {
                    oper = _collocationContract.GetFansList(entity.ColloId, PageIndex, PageSize);
                }
                return Json(oper);
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #region 查看粉丝详情
        public JsonResult GetFansDetail()
        {
            OperationResult oper = new OperationResult(OperationResultType.Error);
            try
            {
                string strMemberId = Request["MemberId"];
                string strFansId = Request["FansId"];
                if (string.IsNullOrEmpty(strFansId))
                {
                    oper.Message = "请选择粉丝";
                }
                else
                {                    
                    int fansId=int.Parse(strFansId);
                    Member member=  _memberContract.View(fansId);
                    var entity = new {                      
                     member.MemberName,
                     Birth = member.DateofBirth == null ? string.Empty : ((DateTime)member.DateofBirth).ToString("MMdd"),
                    };
                    oper.ResultType = OperationResultType.Success;
                    oper.Data=entity;                    
                }
                return Json(oper);
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                oper.Message = "服务器忙，请稍后重试";
                return Json(oper);
            }
        }
        #endregion

        #region 给任务选择搭配
        /// <summary>
        /// 给任务选择搭配
        /// </summary>
        public JsonResult AddMissionColl()
        {
            OperationResult oper = new OperationResult(OperationResultType.Error, "服务器忙，请稍后重试");
            try
            {
                string strCollId=Request["CollIds"];
                string strMissionId = Request["MessionId"];
                if (string.IsNullOrEmpty(strCollId))
                {
                    oper.Message = "请选择搭配";
                    return Json(oper);
                }
                if (string.IsNullOrEmpty(strMissionId))
                {
                    oper.Message = "请选择任务";
                    return Json(oper);
                }
                int missionId = int.Parse(strMissionId);
                string[] arrId = strCollId.Split(',');
                List<int> list = new List<int>();
                foreach (string id in arrId)
                {
                    list.Add(int.Parse(id));
                }
                List<MemberCollocation> listColl= _memberCollContract.MemberCollocations.Where(x => list.Contains(x.Id)).ToList();
                return null;
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        #endregion
    }
}