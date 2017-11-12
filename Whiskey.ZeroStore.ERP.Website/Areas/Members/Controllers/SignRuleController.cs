using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Globalization;
using AutoMapper;
using Antlr3;
using Antlr3.ST;
using Antlr3.ST.Language;
using Antlr3.ST.Extensions;
using Newtonsoft.Json;
using Whiskey.Utility.Class;
using Whiskey.Utility.Data;
using Whiskey.Utility.Filter;
using Whiskey.Utility.Logging;
using Whiskey.Utility.Extensions;
using Whiskey.Web.Helper;
using Whiskey.Web.Mvc.Binders;
using Whiskey.Core.Data;
using Whiskey.Core.Data.Extensions;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.Utility.Helper;
using System.Data.SqlClient;
using System.Data.Mapping;
using System.Data.Linq;
using System.Web.Script.Serialization;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Member;
using Whiskey.ZeroStore.ERP.Website.Areas.Members.Models;
namespace Whiskey.ZeroStore.ERP.Website.Areas.Members.Controllers
{
    [License(CheckMode.Verify)]
    public class SignRuleController : BaseController
    {
        #region 声明业务层操作对象
        //日志记录
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(MemberController));
        //声明业务层操作对象
        protected readonly IPrizeContract _prizeContract;

        protected readonly ISignRuleContract _signRuleContract;

        protected readonly ICouponContract _couponContract;
        //构造函数-初始化业务层操作对象
        public SignRuleController(IPrizeContract prizeContract,
            ISignRuleContract signRuleContract,
            ICouponContract couponContract)
        {
            _prizeContract = prizeContract;
            _signRuleContract = signRuleContract;
            _couponContract = couponContract;
		}
        #endregion

        #region 初始化界面
        
        /// <summary>
        /// 初始化界面
        /// </summary>
        /// <returns></returns>
        [Layout]
        public ActionResult Index()
        {
            return View();
        }
        #endregion

        #region 获取数据列表
        public async Task<ActionResult> List()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<SignRule, bool>> predicate = FilterHelper.GetExpression<SignRule>(request.FilterGroup);

            var data = await Task.Run(() =>
            {
                int count = 0;
                var list = _signRuleContract.SignRules.Where<SignRule, int>(predicate, request.PageCondition, out count).Select(entity => new
                {
                    SignRuleName = entity.SignRuleName,
                    Id = entity.Id,
                    Week = entity.Week,
                    PrizeType = entity.PrizeType,
                    CouponName = entity.CouponId == null ? string.Empty : entity.Coupon.CouponName,
                    PrizeName = entity.PrizeId == null ? string.Empty : entity.Prize.PrizeName,
                    IsDeleted = entity.IsDeleted,
                    IsEnabled = entity.IsEnabled
                }).ToList();
                return new GridData<object>(list, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 添加数据

        #region 初始化添加界面
        /// <summary>
        /// 初始化添加界面
        /// </summary>
        /// <returns></returns>
        public ActionResult Create()
        {
            return PartialView();
        }
        #endregion

        #region 添加数据
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Create(SignRuleDto dto)
        {
            if (dto.PrizeType == (int)PrizeFlag.Score || dto.PrizeType == (int)PrizeFlag.Res)
            {
                dto.CouponId = null;
            }
            else
            {
                dto.PrizeId = null;
            }
            var result = _signRuleContract.Insert(dto);
            return Json(result);
        }
        #endregion
        #endregion

        #region 修改数据
        #region 初始化修改界面
        /// <summary>
        /// 初始化修改界面
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>        signPrize
        public ActionResult Update(int Id)
        {
            var dto = _signRuleContract.Edit(Id);
            if (dto.PrizeType==(int)PrizeFlag.Coupon)
            {
                Coupon coupon = _couponContract.Coupons.Where(x => x.Id == dto.CouponId).FirstOrDefault();
                dto.CouponName = coupon.CouponName;
            }
            else if(dto.PrizeType==(int)PrizeFlag.Res)
            {
                Prize prize= _prizeContract.Prizes.Where(x => x.Id == dto.PrizeId).FirstOrDefault();
                dto.PrizeName = prize.PrizeName;
            }
            return PartialView(dto);
        }
        /// <summary>
        /// 修改数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Update(SignRuleDto dto)
        {
            var result = _signRuleContract.Update(dto);
            return Json(result);
        }
        #endregion
        #endregion

        #region 查看详情
        /// <summary>
        /// 查看详情
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult View(int Id)
        {
            var entity =  _signRuleContract.View(Id);
            return PartialView(entity);
        }
        #endregion

        #region 上传图片
        /// <summary>
        /// 上传图片
        /// </summary>
        /// <returns></returns>
        public JsonResult UploadImage()
        {
            string conPath = ConfigurationHelper.GetAppSetting("RewardImagePath");
            string savePath = conPath;
            Guid gid = Guid.NewGuid();
            string fileName = gid.ToString();
            fileName = fileName.Substring(0, 15);
            var file = Request.Files;
            bool result = false;
            for (int i = 0; i < file.Count; i++)
            {
                string name = file[i].FileName;
                int lastIndex = name.LastIndexOf('.');
                fileName += name.Substring(lastIndex);
                savePath = savePath + fileName;
                result = FileHelper.SaveUpload(file[i].InputStream, FileHelper.UrlToPath(savePath));
            }
            if (result)
            {
                return Json(new { ResultType = OperationResultType.Success, Path = savePath }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { ResultType = OperationResultType.Error, path = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region 启用数据
        /// <summary>
        /// 启用数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public ActionResult Enable(int[] Id)
        {
            var result = _signRuleContract.Enable(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 禁用数据
        /// <summary>
        /// 禁用数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public ActionResult Disable(int[] Id)
        {
            var result = _signRuleContract.Disable(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 移除数据
        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public JsonResult Remove(int Id)
        {
            var res = _signRuleContract.Remove(Id);
            return Json(res);
        }
        #endregion

        #region 恢复数据
        /// <summary>
        /// 恢复数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public JsonResult Recovery(int Id)
        {
            var res = _signRuleContract.Recovery(Id);
            return Json(res);
        }
        #endregion

        #region 优惠卷
        
        #region 初始化优惠卷界面
                
        /// <summary>
        /// 初始化优惠卷界面
        /// </summary>
        /// <returns></returns>
        public ActionResult Coupon()
        {
            return PartialView();
        }
        #endregion

        #region 获取数据列表
        /// <summary>
        /// 获取数据列表
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> CouponList()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<Coupon, bool>> predicate = FilterHelper.GetExpression<Coupon>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                var count = 0;
                var list = _couponContract.Coupons.Where<Coupon, int>(predicate, request.PageCondition, out count).Select(m => new
                {
                    m.Id,
                    m.CouponName,
                    m.CouponPrice,
                    m.Quantity,
                    SendQuantity = m.CouponItems.Where(x => x.MemberId != null).Count(),                    
                    m.IsDeleted,
                    m.IsEnabled,
                    m.Sequence,
                    m.UpdatedTime,
                    m.CreatedTime,
                    m.StartDate,
                    m.EndDate,
                    
                }).ToList();
                return new GridData<object>(list, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #endregion

        #region 奖品
        #region 初始化奖品界面
        /// <summary>
        /// 初始化奖品界面
        /// </summary>
        /// <returns></returns>
        public ActionResult Prize()
        {
            string strPrizeType=Request["PrizeType"];
            ViewBag.PrizeType = strPrizeType;
            return PartialView();
        }
        #endregion

        #region 获取数据列表
        public async Task<ActionResult> PrizeList()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<Prize, bool>> predicate = FilterHelper.GetExpression<Prize>(request.FilterGroup);
            //总页码
            int count = 0;
            var data = await Task.Run(() =>
            {
                var list = _prizeContract.Prizes.Where<Prize, int>(predicate, request.PageCondition, out count).Select(m => new
                {
                    m.Id,
                    m.PrizeName,
                    m.Quantity,
                    m.GetQuantity,
                    m.ReceiveQuantity,
                    m.UpdatedTime,
                    m.IsDeleted,
                    m.IsEnabled,
                    m.RewardImagePath,
                    m.Operator.Member.MemberName,
                    m.PrizeType 
                }).ToList();
                return new GridData<object>(list, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #endregion
    }
}