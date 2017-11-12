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
    public class MissionController : Controller
    {

        #region 声业务层操作对象
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(MissionController));

        protected readonly IMissionContract _missionContract;

        protected readonly ICategoryContract _categoryContract;

        protected readonly IMemberContract _memberContract;

        protected readonly ICollocationContract _collocationContract;
        public MissionController(IMissionContract missionContract,
            IMemberContract memberContract,
            ICategoryContract categoryContract,
            ICollocationContract collocationContract)
        {
            _missionContract = missionContract;             
            _memberContract = memberContract;
            _categoryContract = categoryContract;
            _collocationContract = collocationContract;
        }
        #endregion

        #region 发布任务
        /// <summary>
        /// 发布任务
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public JsonResult Create(MissionEntity entity)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error);
            try
            {               
                oper = this.CheckParameters(entity);
                if (oper.ResultType==OperationResultType.Success)
                {
                    MissionDto dto = oper.Data as MissionDto;
                    oper= _missionContract.Insert(dto);                    
                }
                return Json(oper);
            }
            catch (Exception ex)
            {
                oper.Message = "服务器忙，请稍后重试";
                _Logger.Error<string>(ex.ToString());
                return Json(oper);                
            }
        }
        #endregion

        #region 校验参数
        /// <summary>
        /// 校验参数
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private OperationResult CheckParameters(MissionEntity entity)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error);  
            try
            {
                MissionDto mission = new MissionDto();
                
                List<int> listCategoryId = new List<int>();
                #region 参数校验
                int memberId = int.Parse(entity.MemberId);
                string num= _memberContract.Members.FirstOrDefault(x => x.Id == memberId).UniquelyIdentifies;
                int count= _collocationContract.Collocations.Where(x => x.Numb == num).Count();
                if (count>0)
                {
                    oper.Message = "搭配师不能发布任务";
                    return oper;
                }
                else
                {
                    mission.MemberId = memberId;
                }
                DateTime curretnDate= DateTime.Now;
                IQueryable<Mission> listMission = _missionContract.Missions.Where(x=>x.IsDeleted==false && x.IsEnabled==true);
                count =listMission.Where(x => x.MemberId == memberId && x.ScheduleType == (int)ScheduleFlag.Completed).Count();
                if (count>0)
                {
                    return new OperationResult(OperationResultType.Error, "还有任务未完成，不能发布");
                }
                count = listMission.Where(x => x.CreatedTime.Year == curretnDate.Year && x.CreatedTime.Month == curretnDate.Month && x.CreatedTime.Day == curretnDate.Day).Count();
                if (count > 0)
                {
                    return new OperationResult(OperationResultType.Error, "一天只能发布一个任务");
                }
                if (string.IsNullOrEmpty(entity.MissionAttrType))
                {
                    oper.Message = "请选择任务类型";
                    return oper;
                }
                else
                {
                    int missionAttrType = int.Parse(entity.MissionAttrType);
                    if (missionAttrType==(int)MissionAttrFlag.Public)
                    {
                        mission.MissionAttrType = missionAttrType;
                    }
                    else
                    {
                        oper.Message = "请选择任务类型";
                        return oper;
                    }
                    if (missionAttrType==(int)MissionAttrFlag.Appoint)
                    {
                        mission.MissionAttrType = missionAttrType;
                        if (string.IsNullOrEmpty(entity.CollocationId))
                        {
                            oper.Message = "请指定搭配师";
                            return oper;
                        }
                        else
                        {
                            int collocationId = int.Parse(entity.CollocationId);
                            mission.MissionItems.Add(new MissionItem()
                            {
                                ScheduleType = (int)ScheduleFlag.Completing,
                                MissionAttrType = mission.MissionAttrType,
                                StartTime = DateTime.Now,
                                MemberId = collocationId,
                            });
                        }
                    }
                }

                
                if (string.IsNullOrEmpty(entity.MissionType))
                {
                    oper.Message = "暂时无法发布任务，请稍后重试";
                    return oper;
                }
                else
                {
                    int missionType=int.Parse(entity.MissionType);
                    if (missionType == (int)MissionFlag.Buy || missionType == (int)MissionFlag.Collocation || missionType == (int)MissionFlag.CombinedOrders)
                    {
                        mission.MissionType = missionType;
                    }
                    else
                    {
                        oper.Message = "暂时无法发布任务，请稍后重试";
                        return oper;
                    }
                }
                if (string.IsNullOrEmpty(entity.CategoryId))
                {
                    oper.Message = "请选择品类";
                    return oper;
                }
                else
                {
                    string[] arrId = entity.CategoryId.Split(',');
                    foreach (string strId in arrId)
                    {
                        if (string.IsNullOrEmpty(strId))
                        {
                            listCategoryId.Add(int.Parse(strId));
                        }
                    }
                    if (listCategoryId.Count == 0)
                    {
                        oper.Message = "请选择品类";
                        return oper;
                    }
                    else
                    {
                        List<Category> listCategory= _categoryContract.Categorys.Where(x => listCategoryId.Contains(x.Id)).ToList();
                        if (listCategory.Count==0)
                        {
                            oper.Message = "选择品类不存在，请重新选择";
                            return oper;
                        }
                        else
                        {
                            mission.Categorys = listCategory;
                        }                        
                    }
                }
                if (string.IsNullOrEmpty(entity.ColorId))
                {
                    oper.Message = "请选择颜色";
                    return oper;
                }
                else
                {
                    mission.ColorId = int.Parse(entity.ColorId);
                }
                if (string.IsNullOrEmpty(entity.SituationId))
                {
                    oper.Message = "请选择场合";
                    return oper;
                }
                else
                {
                    mission.SituationId = int.Parse(entity.SituationId);
                }
                if (string.IsNullOrEmpty(entity.SeasonId))
                {
                    oper.Message = "请选择季节";
                    return oper;
                }
                else
                {
                    mission.ColorId = int.Parse(entity.ColorId);
                }
                if (string.IsNullOrEmpty(entity.StyleId))
                {
                    oper.Message = "请选择风格";
                    return oper;
                }
                else
                {
                    mission.StyleId = int.Parse(entity.StyleId);
                }
                if (string.IsNullOrEmpty(entity.Price))
                {
                    oper.Message = "请选择价格";
                    return oper;
                }
                else
                {
                    mission.PriceRange = entity.Price;
                }
                if (!string.IsNullOrEmpty(entity.Notes))
                {
                    if (entity.Notes.Trim().Length>120)
                    {
                        oper.Message = "备注不能超过120个字符";
                        return oper;    
                    }
                    else
                    {
                        mission.Notes = entity.Notes.Trim();
                    }
                }
                oper.ResultType = OperationResultType.Success;
                oper.Data = mission;
                return oper;
                #endregion
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                oper.Message = "校验数据出错，请稍后重试";
                return oper;
            }
            


        }
        #endregion

        #region 获取搭配师任务列表
        public JsonResult GetList(int PageIndex=1,int PageSize=10)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error);
            try
            {
                string strMemberId= Request["MemberId"];
                string strCollocationId = Request["CollocationId"];
                int memberId = int.Parse(strMemberId);
                int collocationId = int.Parse(strCollocationId);
                oper = _missionContract.GetList(memberId, collocationId, PageIndex, PageSize);
            }
            catch (Exception)
            {
                
                throw;
            }
            return null;
        }
        #endregion

        #region 搭配师任务
        public JsonResult GetCollMission()
        {
            try
            {
                string strCollcation = Request["CollcationId"];
            }
            catch (Exception)
            {
                
                throw;
            }
            return null;
        }
        #endregion
    }
}