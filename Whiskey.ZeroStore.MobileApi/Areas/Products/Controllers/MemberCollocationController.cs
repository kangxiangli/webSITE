using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Whiskey.Utility.Class;
using Whiskey.Utility.Data;
using Whiskey.Utility.Helper;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Transfers.APIEntities.MemberCollo;
using Whiskey.ZeroStore.ERP.Transfers.APIEntities.MemberSingleProduct;
using Whiskey.ZeroStore.ERP.Transfers.APIEntities.Factory;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Comment;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Member;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Product;
using Whiskey.ZeroStore.MobileApi.Areas.Members.Models;
using Whiskey.ZeroStore.MobileApi.Areas.Products.Models;
using Whiskey.ZeroStore.MobileApi.Extensions.Attribute;
using Whiskey.ZeroStore.ERP.Models.Enums;

namespace Whiskey.ZeroStore.MobileApi.Areas.Products.Controllers
{
    [License(CheckMode.Verify)]
    public class MemberCollocationController : Controller
    {

        #region 初始化业务层操作对象

        //日志记录
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(MemberCollocationController));
        //声明业务层操作对象
        protected readonly IMemberCollocationContract _memberCollocationContract;

        protected readonly IMemberColloEleContract _memberColloEleContract;

        protected readonly IMemberSingleProductContract _memberSingleProContract;

        protected readonly ICommentContract _commentContract;

        protected readonly IApprovalContract _approvalContract;

        protected readonly IColorContract _colorContract;

        protected readonly IMemberContract _memberContract;

        protected readonly IColloCalendarContract _colloCalendarContract;

        protected readonly IAppArticleContract _appArticleContract;

        protected readonly IGalleryContract _galleryContract;

        protected readonly IProductContract _productContract;

        protected readonly ICategoryContract _categoryContract;
        protected readonly IStoreContract _storeContract;
        protected readonly IRecommendMemberCollocationContract _recommendMemberCollocationContract;
        //构造函数-初始化业务层操作对象
        public MemberCollocationController(IMemberCollocationContract memberCollocationContract,
            IMemberColloEleContract memberColloEleContract,
            IMemberSingleProductContract memberSingleProContract,
            ICommentContract productCommentContract,
            IApprovalContract productApprovalContract,
            IColorContract colorContract,
            IColloCalendarContract colloCalendarContract,
            IMemberContract memberContract,
            IAppArticleContract appArticleContract,
            IGalleryContract galleryContract,
            IProductContract productContract,
            ICategoryContract categoryContract,
            IStoreContract storeContract,
            IRecommendMemberCollocationContract recommendMemberCollocationContract
            )
        {
            _memberCollocationContract = memberCollocationContract;
            _memberColloEleContract = memberColloEleContract;
            _memberSingleProContract = memberSingleProContract;
            _commentContract = productCommentContract;
            _approvalContract = productApprovalContract;
            _colorContract = colorContract;
            _colloCalendarContract = colloCalendarContract;
            _memberContract = memberContract;
            _appArticleContract = appArticleContract;
            _galleryContract = galleryContract;
            _productContract = productContract;
            _categoryContract = categoryContract;
            _storeContract = storeContract;
            _recommendMemberCollocationContract = recommendMemberCollocationContract;
        }
        #endregion

        string strApiUrl = ConfigurationHelper.GetAppSetting("ApiUrl");
        string strWebUrl = ConfigurationHelper.GetAppSetting("WebUrl");

        #region 添加搭配数据
        [HttpPost]
        [ValidateInput(false)]
        public JsonResult Add()
        {
            try
            {
                string strMyColl = Request["MyCollo"];
                if (strMyColl != null)
                {
                    MyColl myColl = new MyColl();
                    myColl =JsonHelper.FromJson<MyColl>(strMyColl);
                    OperationResult result = _memberCollocationContract.Add(myColl);
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new OperationResult(OperationResultType.Error, "请填写参数"), JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _Logger.Debug<string>(ex.ToString());
                return Json(new OperationResult(OperationResultType.Error, "服务器忙，请重新填写参数！"));
            }

        }
        #endregion




        #region 获取数据列表

        /// <summary>
        /// 获取我的搭配
        /// </summary>
        /// <returns></returns>

        [HttpPost]
        [ValidateInput(false)]
        public JsonResult GetList(int PageIndex = 1, int PageSize = 10)
        {

            try
            {
                string strMemberId = Request["MemberId"];
                string strColloName = Request["ColloName"];
                string strColorId = Request["ColorId"];
                string strSeasonId = Request["SeasonId"];
                string strProductAttrId = Request["ProductAttrId"];
                string strSituationId = Request["SituationId"];
                if (string.IsNullOrEmpty(strMemberId))
                {
                    return Json(new OperationResult(OperationResultType.Error, "请先登录！"));
                }
                else
                {
                    int memberId = int.Parse(strMemberId);
                    PagedOperationResult operRes;
                    var list = _memberCollocationContract.GetList(memberId, strColloName, strColorId, strSeasonId, strProductAttrId, strSituationId, PageIndex, PageSize, out operRes);
                    if (operRes.ResultType == OperationResultType.Success)
                    {
                        var collos = list.Select(x => new
                        {
                            Id = x.ColloId,
                            ProductId = x.ColloId,
                            MemberId = x.MemberId,
                            ColloName = x.ColloName,
                            Notes = x.Notes,
                            MemberImage = strWebUrl + x.MemberImage,
                            ColloImagePath = strApiUrl + x.ColloImagePath,
                            CommentCount = x.CommentCount,
                            ApproveCount = x.ApproveCount,
                            IsApprove = x.IsApprove,
                            x.CollocationType
                        });
                        return Json(new { ResultType = OperationResultType.Success, ListCollo = collos, operRes.AllCount, operRes.PageSize, operRes.PageCount }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(operRes, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return Json(new OperationResult(OperationResultType.Error, "服务器忙，请稍后重试！"));
            }
        }

        [HttpPost]
        public ActionResult RemoveRecommend(int memberId, int colloId)
        {
            var entity = _recommendMemberCollocationContract.Entities.Where(r => !r.IsDeleted && r.IsEnabled && r.MemberId == memberId && r.MemberCollocationId == colloId).FirstOrDefault();
            if (entity != null)
            {
                var res = _recommendMemberCollocationContract.Delete(entity);
                return Json(res);
            }
            return Json(OperationResult.Error("信息不存在"));
        }


        #endregion

        #region 根据月份获取我的搭配
        /// <summary>
        /// 获取当前月我的搭配
        /// </summary>
        /// <param name="PageIndex">页码</param>
        /// <param name="PageSize">每页显示数据量</param>
        /// <returns></returns>

        [HttpPost]
        [ValidateInput(false)]
        public JsonResult GetCollocation(int PageIndex = 1, int PageSize = 10, int Count = 4)
        {
            string strMemberId = Request["MemberId"];
            string strDate = Request["Date"];
            string strType = Request["Type"];//0表示获取列表数据，//1表示获取日历标识
            try
            {
                int memberId = int.Parse(strMemberId);
                IQueryable<ColloCalendar> listColloCalendar = _colloCalendarContract.ColloCalendars.Where(x => x.MemberId == memberId && x.IsDeleted == false && x.IsEnabled == true);
                if (strType == "0")
                {
                    DateTime currentDate = DateTime.Now.AddDays(2);
                    string date = currentDate.ToString("yyyy/MM/dd 00:00:00");
                    currentDate = DateTime.Parse(date);
                    listColloCalendar = listColloCalendar.Where(x => x.CollocationTime.CompareTo(currentDate) < 0);
                }
                IQueryable<MemberCollocation> listCollo = _memberCollocationContract.MemberCollocations.Where(x => x.MemberId == memberId).OrderBy(x => x.Id);
                IQueryable<MemberColloEle> listColloEle = _memberColloEleContract.MemberColloEles.Where(x => x.EleType == (int)MemberColloEleFlag.ImageEle);
                IQueryable<Comment> listComment = _commentContract.Comments.Where(x => x.CommentSource == (int)CommentSourceFlag.MemberCollocation);
                IQueryable<Approval> listApproval = _approvalContract.Approvals.Where(x => x.ApprovalSource == (int)CommentSourceFlag.MemberCollocation);
                var listColor = _colorContract.Colors.Where(x => x.IsDeleted == false && x.IsEnabled == true);
                List<MemberCollo> list = new List<MemberCollo>();
                var listEntity = from cc in listColloCalendar
                                 join
                                     c in listCollo
                                 on
                                 cc.ColloId equals c.Id
                                 select new
                                 {
                                     c.Id,
                                     cc.CollocationTime,
                                     c.ColorId,
                                     cc.Temperature,
                                     cc.Weather,
                                     cc.CityName,
                                     c.MemberColloEles,
                                     c.IsDeleted
                                 };
                //List<string> listImage = new List<string>();
                foreach (var item in listEntity)
                {
                    MemberCollo mCollo = new MemberCollo();
                    var eleImage = item.MemberColloEles.FirstOrDefault(x => x.Parent == null);
                    //List<MemberColloEle> childImage = item.MemberColloEles.Where(x => x.Parent != null && x.ProductSource == ProductSourceFlag.MemberProduct && x.ProductType == (int)SingleProductFlag.Upload).Take(Count).ToList();
                    List<MemberColloEle> childImage = eleImage.Children.Take(Count).ToList();
                    
                    mCollo.ColloImagePath = eleImage == null ? string.Empty : strApiUrl + eleImage.ImagePath;
                    mCollo.MemberId = memberId;
                    mCollo.ListImagePath = this.GetElePath(childImage);//listColloEle.Where(x => x.MemberColloId == item.Id && x.Parent != null).Select(x => x.ImagePath).ToList();
                    mCollo.CollocationTime = item.CollocationTime;
                    mCollo.ColorId = item.ColorId;
                    mCollo.ColloId = item.Id;
                    mCollo.CommentCount = listComment.Where(x => x.Id == item.Id).Count();
                    mCollo.IsApprove = listApproval.Where(x => x.Id == item.Id && x.MemberId == memberId).Count() > 0 ? (int)IsApproval.Yes : (int)IsApproval.No;
                    mCollo.Weather = item.Weather;
                    mCollo.Temperature = item.Temperature;
                    mCollo.CityName = item.CityName;
                    mCollo.IsDeleted = item.IsDeleted;
                    list.Add(mCollo);
                }

                //获取数据类型
                int type = int.Parse(strType);
                if (type == 0)
                {
                    var allCount = list.Count;
                    var result = list.OrderByDescending(x => x.CollocationTime).Skip((PageIndex - 1) * PageSize).Take(PageSize).Select(x => new
                    {
                        ProductId = x.ColloId,
                        ColloImagePath = x.ColloImagePath,
                        ListImagePath = x.ListImagePath,
                        CollocationTime = x.CollocationTime.ToString("yyyy-MM-dd"),
                        CommentCount = x.CommentCount,
                        x.IsApprove,
                        x.Weather,
                        x.Temperature,
                        x.CityName,
                        x.IsDeleted
                    });
                    return Json(new PagedOperationResult(OperationResultType.Success, "获取成功！", result) { AllCount = allCount, PageSize = PageSize }, JsonRequestBehavior.AllowGet);
                }
                else if (type == 1)
                {
                    if (string.IsNullOrEmpty(strDate))
                    {
                        return Json(new OperationResult(OperationResultType.Error, "获取日期异常，请稍后重试！"));
                    }
                    DateTime currentTime = DateTime.Parse(strDate);
                    list = list.Where(x => x.CollocationTime.Year == currentTime.Year && x.CollocationTime.Month == currentTime.Month).ToList();
                    var result = list.Select(x => new
                    {
                        ProductId = x.ColloId,
                        CollocationTime = x.CollocationTime.ToString("yyyy-MM-dd"),
                        ColorIconPath = listColor.Where(k => k.Id == x.ColorId).FirstOrDefault() != null ? strWebUrl + listColor.Where(k => k.Id == x.ColorId).FirstOrDefault().IconPath : string.Empty,
                        x.IsDeleted
                    });
                    return Json(new PagedOperationResult(OperationResultType.Success, "获取成功！", result) { AllCount = list.Count, PageSize = PageSize });
                }
                else
                {
                    return Json(new OperationResult(OperationResultType.Error, "获取数据异常，请稍后重试！"));
                }
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return Json(new OperationResult(OperationResultType.Error, "服务器忙，请稍后重试！"));
            }
        }
        #endregion

        #region 根据日期获取我的搭配

        /// <summary>
        /// 根据日期获取数据
        /// </summary>
        /// <param name="Count">获取素材的数量</param>
        /// <returns></returns>

        [HttpPost]
        [ValidateInput(false)]
        public JsonResult GetCollocationByDate(int Count = 4)
        {

            string strMemberId = Request["MemberId"];
            string strProductId = Request["ProductId"];
            try
            {
                if (string.IsNullOrEmpty(strMemberId)) return Json(new OperationResult(OperationResultType.LoginError, "登录异常，请重试！"));
                if (string.IsNullOrEmpty(strProductId)) return Json(new OperationResult(OperationResultType.LoginError, "搭配不存在，请重试！"));
                int productId = int.Parse(strProductId);
                int memberId = int.Parse(strMemberId);
                MemberCollocation collo = _memberCollocationContract.MemberCollocations.Where(x => x.MemberId == memberId && x.Id == productId).OrderBy(x => x.Id).FirstOrDefault();
                //IQueryable<MemberColloEle> listColloEle = _memberColloEleContract.MemberColloEles.Where(x => x.EleType == (int)MemberColloEleFlag.ImageEle && x.MemberColloId == collo.Id);
                if (collo == null)
                {
                    return Json(new { ResultType = OperationResultType.Success, ListCollo = "" });
                }
                else
                {
                    List<MemberColloEle> listColloEle = collo.MemberColloEles.Where(x => x.IsDeleted == false && x.IsEnabled == true).ToList();
                    MemberColloEle parent = listColloEle.Where(x => x.Parent == null).FirstOrDefault();
                    List<MemberColloEle> children = listColloEle.Where(x => x.Parent != null
                    && (x.ProductSource == ProductSourceFlag.MemberProduct || x.ProductSource == ProductSourceFlag.StoreProduct)
                    && x.ProductType == (int)SingleProductFlag.Upload).Take(Count).ToList();
                    List<string> listPath = new List<string>();
                    listPath = this.GetElePath(children);
                    var result = new
                    {
                        ColloImagePath = parent == null ? string.Empty : strApiUrl + parent.ImagePath,
                        ImagePathList = listPath
                    };
                    return Json(new { ResultType = OperationResultType.Success, ListCollo = result });
                }
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return Json(new OperationResult(OperationResultType.Error, "服务器忙，请稍后重试！"));
            }
        }
        #endregion

        #region 获取元素的图片路径
        private List<string> GetElePath(List<MemberColloEle> children)
        {
            List<string> listPath = new List<string>();
            foreach (MemberColloEle ele in children)
            {
                if (ele.ProductSource == ProductSourceFlag.MaterialProduct)
                {

                }
                else if ((ele.ProductSource == ProductSourceFlag.MemberProduct && ele.ProductType == SingleProductFlag.OrderItem) || ele.ProductSource == ProductSourceFlag.StoreProduct)
                {
                    Product product = _productContract.Products.FirstOrDefault(x => x.Id == ele.ProductId);
                    if (product == null)
                    {
                        listPath.Add(string.Empty);
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(product.ProductCollocationImg))
                        {
                            listPath.Add(strWebUrl + product.ProductOriginNumber.ProductCollocationImg);

                        }
                        else
                        {
                            listPath.Add(strWebUrl + product.ProductCollocationImg);
                        }
                    }
                }
                else if (ele.ProductSource == ProductSourceFlag.MemberProduct && ele.ProductType == (int)SingleProductFlag.Upload)
                {
                    MemberSingleProduct single = _memberSingleProContract.MemberSingleProducts.FirstOrDefault(x => x.Id == ele.ProductId);
                    if (single == null)
                    {
                        listPath.Add(string.Empty);
                    }
                    else
                    {
                        listPath.Add(strApiUrl + single.Image);
                    }
                }
                else
                {
                    listPath.Add(strApiUrl + ele.ImagePath);
                }
            }
            return listPath;
        }
        #endregion

        #region 添加穿衣日历
        /// <summary>
        /// 添加穿衣日历
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult AddCalendar(int Count = 1)
        {
            try
            {
                string strMemberId = Request["MemberId"];
                string strTemperature = Request["Temperature"];
                string strWeather = Request["Weather"];
                string strProductId = Request["ProductId"];
                string strCityName = Request["CityName"];
                if (string.IsNullOrEmpty(strMemberId)) return Json(new OperationResult(OperationResultType.LoginError, "登录异常，请重新登录！"), JsonRequestBehavior.AllowGet);
                if (string.IsNullOrEmpty(strTemperature)) return Json(new OperationResult(OperationResultType.Error, "获取温度失败,请重试！"), JsonRequestBehavior.AllowGet);
                if (string.IsNullOrEmpty(strWeather)) return Json(new OperationResult(OperationResultType.Error, "获取天气失败,请重试！"), JsonRequestBehavior.AllowGet);
                if (string.IsNullOrEmpty(strProductId)) return Json(new OperationResult(OperationResultType.Error, "获取商品失败，请重试！"), JsonRequestBehavior.AllowGet);
                int colloId = int.Parse(strProductId);
                int memberId = int.Parse(strMemberId);
                ColloCalendarDto dto = new ColloCalendarDto();
                dto.MemberId = memberId;
                dto.ColloId = colloId;
                dto.Temperature = strTemperature;
                dto.Weather = strWeather;
                dto.CollocationTime = DateTime.Now.AddDays(Count);
                dto.CityName = strCityName;
                ColloCalendar colloCalender = _colloCalendarContract.ColloCalendars.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.MemberId == memberId && x.CollocationTime.Year == dto.CollocationTime.Year && x.CollocationTime.Month == dto.CollocationTime.Month && x.CollocationTime.Day == dto.CollocationTime.Day).FirstOrDefault();
                if (colloCalender != null)
                {
                    dto.Id = colloCalender.Id;
                    var result = _colloCalendarContract.Update(dto);
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var result = _colloCalendarContract.Insert(dto);
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return Json(new OperationResult(OperationResultType.Error, "服务器忙，请稍后重试"), JsonRequestBehavior.AllowGet);
            }
        }
        #endregion


        #region 获取日期

        public JsonResult GetDate(int Count = 5)
        {
            DateTime currentDate = DateTime.Now;
            List<M_Date> list = new List<M_Date>();
            for (int i = 1; i <= Count; i++)
            {
                list.Add(new M_Date() { Count = i, Date = currentDate.AddDays(i).ToString("yyyy年MM月dd日") });
            }

            return Json(new OperationResult(OperationResultType.Success, string.Empty, list));
        }
        #endregion

        #region 获取详情
        /// <summary>
        /// 获取搭配详情
        /// </summary>
        /// <returns></returns>

        [HttpPost]
        [ValidateInput(false)]
        public JsonResult GetDetail(int ColloId, int MemberId)
        {
            try
            {
                var result = _memberCollocationContract.GetDeail(MemberId, ColloId);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return Json(new OperationResult(OperationResultType.Error, "服务器忙，请稍后重试！"), JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region 获取编辑数据
        /// <summary>
        /// 获取编辑对象
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetEdit()
        {
            try
            {
                string strMemberId = Request["MemberId"];
                string strColloId = Request["ColloId"];
                if (string.IsNullOrEmpty(strMemberId)) return Json(new OperationResult(OperationResultType.Error, "登录异常，请重新登录！"));
                if (string.IsNullOrEmpty(strColloId)) return Json(new OperationResult(OperationResultType.Error, "该搭配，暂时无法编辑，请稍后重试！"));
                int memberId = int.Parse(strMemberId);
                int colloId = int.Parse(strColloId);
                OperationResult operResult = _memberCollocationContract.GetEdit(memberId, colloId);
                return Json(operResult, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                _Logger.Error<string>(ex.StackTrace.ToString());
                _Logger.Error<string>(ex.Message.ToString());
                return Json(new OperationResult(OperationResultType.Error, "服务器忙，请售后重试！" + ex.StackTrace + ex.Message), JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region 保存编辑数据
        /// <summary>
        /// 保存编辑数据
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult SaveEdit()
        {
            try
            {
                string strColloInfo = Request["ColloInfo"];
                //strColloInfo = System.IO.File.ReadAllText(@"C:\Users\Pirate-ky\Desktop\mycollo.txt");
                if (!string.IsNullOrEmpty(strColloInfo))
                {
                    // _Logger.Error<string>("____"+strColloInfo);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    ColloInfo colloInfo = js.Deserialize<ColloInfo>(strColloInfo);
                    OperationResult result = _memberCollocationContract.SaveEdit(colloInfo);
                    return Json(result);
                }
                else
                {
                    return Json(new OperationResult(OperationResultType.Error, "暂时无法编辑该对象，请稍后重试！"));
                }
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return Json(new OperationResult(OperationResultType.Error, "服务器忙，请稍后重试！"));
            }
        }
        #endregion

        #region 获取搭配素材
        /// <summary>
        /// 获取搭配素材
        /// </summary>
        /// <param name="PageIndex">页码</param>
        /// <param name="PageSize">每页数据显示量</param>
        /// <returns></returns>

        [HttpPost]
        [ValidateInput(false)]
        public JsonResult GetCollocationElement(int MemberId, ProductSourceFlag ProductSource, int PageIndex = 1, int PageSize = 10)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error);
            Func<int, int, List<ColloProductInfo>> func = null;
            try
            {
                switch (ProductSource)
                {
                    case ProductSourceFlag.MemberProduct:
                        break;
                    case ProductSourceFlag.StoreProduct:
                        func = GetRecommendProuct;
                        break;
                    case ProductSourceFlag.FansProduct:
                        break;
                    default:
                        return null;
                }
                if (func == null)
                {
                    oper.Message = "服务器忙，请稍候访问";
                }
                else
                {
                    oper.ResultType = OperationResultType.Success;
                    oper.Data = func(PageIndex, PageSize);
                }
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                oper.Message = "服务器忙，请稍后重试！";
            }
            return Json(oper);
        }

        /// <summary>
        /// 获取推荐的商品
        /// </summary>
        /// <returns></returns>
        private List<ColloProductInfo> GetRecommendProuct(int PageIndex, int PageSize)
        {
            var listProduct = _productContract.GetStoreCollocationMaterials(PageIndex, PageSize);
            return listProduct;
        }
        #endregion

        #region 注释代码

        public JsonResult Read()
        {

            HttpFileCollectionBase file = Request.Files;
            string str1 = string.Empty;
            for (int i = 0; i < file.Count; i++)
            {
                byte[] buffer = new byte[file[i].InputStream.Length];
                file[i].InputStream.Read(buffer, 0, buffer.Length);
                file[i].InputStream.Seek(0, SeekOrigin.Begin);
                str1 = Convert.ToBase64String(buffer);
            }
            System.IO.File.WriteAllText(@"C:\Users\Pirate-ky\Desktop\123.txt", str1);
            string json = System.IO.File.ReadAllText(@"C:\Users\Pirate-ky\Desktop\json.txt", Encoding.Default);
            JavaScriptSerializer js = new JavaScriptSerializer();


            //Stream stream
            return null;
            //string json =   ("D:\\json\\jsonmy4.txt");


        }
        #endregion

        #region 获取推荐搭配和文章
        [HttpPost]
        public JsonResult GetRecommedListByStore(int? StoreId, int PageIndex = 1, int PageSize = 10)
        {
            try
            {
                if (!StoreId.HasValue)
                {
                    return Json(new OperationResult(OperationResultType.Error, "参数错误：storeId"));
                }
                if (PageSize % 2 != 0)
                {
                    return Json(new OperationResult(OperationResultType.Error, "分页大小为偶数"));
                }
                //获取memid
                var memberId = int.Parse(Request["MemberId"]);

                // 商城信息校验
                var storeEntity = _storeContract.Stores.FirstOrDefault(s => s.IsEnabled == true && s.IsDeleted == false && s.Id == StoreId.Value);
                if (storeEntity == null)
                {
                    return Json(new OperationResult(OperationResultType.Error, "商城信息不存在"));
                }



                var page = PageSize / 2; //实际每个集合中要取得页数

                var source = _memberCollocationContract.MemberCollocations
                    .Where(mc => mc.IsDeleted == false && mc.IsEnabled == true && mc.IsRecommend == true);
                var articleQuery = _appArticleContract.AppArticles.Where(a => a.IsDeleted == false && a.IsEnabled == true && a.IsRecommend == true);

                //如果是归属店铺,需要查找商城下的所有会员id
                var mainStoreId = int.Parse(ConfigurationHelper.GetAppSetting("OnlineStorage"));
                if (storeEntity.Id != mainStoreId)
                {
                    var memberIdList = _memberContract.Members.Where(m => m.IsDeleted == false && m.IsEnabled == true && m.StoreId == StoreId.Value).Select(m => m.Id).ToList();
                    if (memberIdList == null || memberIdList.Count == 0)
                    {
                        return Json(new OperationResult(OperationResultType.Success, string.Empty, null));
                    }
                    source = source.Where(mc => memberIdList.Contains(mc.MemberId));
                    articleQuery = articleQuery.Where(a => memberIdList.Contains(a.MemberId));
                }
                var collocationAllCount = source.Count();
                var articleAllCount = articleQuery.Count();
                var displayAllCount = Math.Min(collocationAllCount, articleAllCount);
                var collocationList = source.OrderByDescending(mc => mc.UpdatedTime)
                   .Skip((PageIndex - 1) * page)
                   .Take(page)
                   .ToList();

                var articleList = articleQuery.OrderByDescending(a => a.UpdatedTime)
                    .Skip((PageIndex - 1) * page)
                    .Take(page)
                    .ToList();
                if (collocationList.Count == 0 || articleList.Count == 0)
                {
                    return Json(new OperationResult(OperationResultType.Success, string.Empty, null));
                }

                // 保证两个集合元素个数一致
                if (collocationList.Count != articleList.Count)
                {
                    // 将长的集合截短
                    if (collocationList.Count > articleList.Count)
                    {
                        collocationList = collocationList.Take(articleList.Count).ToList();
                    }
                    else
                    {
                        articleList = articleList.Take(collocationList.Count).ToList();
                    }
                }

                List<int> listColloId = collocationList.Select(x => x.Id).ToList();
                List<int> listArtId = articleList.Select(x => x.Id).ToList();
                //获取评论列表
                List<Comment> listColloComment = GetComments(CommentSourceFlag.MemberCollocation, listColloId);
                List<Comment> listArtComment = GetComments(CommentSourceFlag.AppArticle, listArtId);
                //获取点赞列表
                List<Approval> listColloApproval = GetApprovals(CommentSourceFlag.MemberCollocation, listColloId);
                List<Approval> listArtApproval = GetApprovals(CommentSourceFlag.AppArticle, listArtId);
                List<M_ColloArticle> listColloArticle = new List<M_ColloArticle>();

                //获取分页数据
                var iteratorCount = collocationList.Count;
                for (int i = 0; i < iteratorCount; i++)
                {
                    var collocation = collocationList[i];
                    var article = articleList[i];

                    //完善collcation数据


                    //图片封面
                    var strCoverImagePath = strApiUrl + collocation.MemberColloEles.FirstOrDefault(x => x.ParentId == null).ImagePath;

                    //优先从获得2个上传的图片
                    var eles = collocation.MemberColloEles.Where(mc => mc.ParentId != null && mc.EleType == (int)MemberColloEleFlag.ImageEle).ToList();

                    var listPath = eles.Where(mc => !string.IsNullOrEmpty(mc.ImagePath)).Select(x => x.ImagePath).Take(2).ToList();
                    if (listPath.Count < 2)
                    {
                        //不够2张的话，从MemberColloEles中补上
                        foreach (MemberColloEle ele in eles)
                        {
                            if (listPath.Count >= 2)
                            {
                                break;
                            }
                            else
                            {
                                if (ele.ProductSource == ProductSourceFlag.MaterialProduct)
                                {
                                    Gallery gallery = _galleryContract.Gallerys.FirstOrDefault(x => x.GalleryType == GalleryFlag.Product && x.Id == ele.ProductId);
                                    if (gallery != null)
                                    {
                                        listPath.Add(gallery.ThumbnailPath);
                                    }
                                }
                                else if (ele.ProductSource == ProductSourceFlag.MemberProduct)
                                {
                                    if (ele.ProductType == (int)SingleProductFlag.Upload)
                                    {
                                        MemberSingleProduct single = _memberSingleProContract.MemberSingleProducts.FirstOrDefault(x => x.Id == ele.ProductId);
                                        if (single != null)
                                        {
                                            listPath.Add(single.Image);
                                        }
                                    }
                                    else
                                    {
                                        Product pro = _productContract.Products.FirstOrDefault(x => x.Id == ele.ProductId);
                                        if (pro != null)
                                        {
                                            listPath.Add(pro.OriginalPath);
                                        }
                                    }
                                }
                                else if (ele.ProductSource == ProductSourceFlag.StoreProduct)
                                {
                                    Product pro = _productContract.Products.FirstOrDefault(x => x.Id == ele.ProductId);
                                    if (pro != null)
                                    {
                                        listPath.Add(pro.OriginalPath);
                                    }
                                }
                            }
                        }
                    }
                    M_ColloArticle mCollo = new M_ColloArticle()
                    {
                        Id = collocation.Id,
                        MemberName = collocation.Member.MemberName,
                        UserPhoto = strWebUrl + collocation.Member.UserPhoto,
                        Title = collocation.CollocationName,
                        CoverImagePath = strCoverImagePath,
                        ColloArticleType = ColloArticleFlag.Collo,
                        ListImage = listPath
                    };
                    mCollo = GetColloArticle(memberId, collocation.Id, mCollo, listColloComment, listColloApproval);
                    listColloArticle.Add(mCollo);

                    var mArt = new M_ColloArticle()
                    {
                        Id = article.Id,
                        MemberName = article.Member.MemberName,
                        UserPhoto = strWebUrl + article.Member.UserPhoto,
                        Title = article.ArticleTitle,
                        CoverImagePath = strApiUrl + article.CoverImagePath,
                        ColloArticleType = ColloArticleFlag.Article,
                    };
                    mArt = GetColloArticle(memberId, article.Id, mArt, listArtComment, listArtApproval);
                    listColloArticle.Add(mArt);

                }
                return Json(new PagedOperationResult(OperationResultType.Success, string.Empty, listColloArticle) { AllCount = displayAllCount, PageSize = PageSize });

            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return Json(new OperationResult(OperationResultType.Error, "服务器忙，请稍后重试！"));
            }
        }
        #endregion
        #region 初始化ColloArticle对象
        private M_ColloArticle GetColloArticle(int memberId, int sourceId, M_ColloArticle m, List<Comment> listComment, List<Approval> listApproval)
        {
            List<Comment> comments = listComment.OrderByDescending(x => x.CreatedTime).Where(x => x.SourceId == sourceId).ToList();
            List<Approval> approvals = listApproval.Where(x => x.SourceId == sourceId).ToList();

            m.CommentCount = comments.Count();
            if (comments != null && comments.Count() > 0)
            {
                //m.UserPhotos = comments.Take(6).Select(x => x.Member.UserPhoto).ToList();
                m.CommentInfos = comments.Take(3).Select(x => new M_CommentInfo
                {
                    MemberId = x.MemberId,
                    MemberName = x.Member.MemberName,
                    Content = x.Content,
                }).ToList();
            }
            if (approvals != null && approvals.Count() > 0)
            {
                m.UserPhotos = approvals.Take(6).Select(x => x.Member.UserPhoto).ToList();

            }

            m.ApprovalCount = approvals.Count();
            int count = approvals.Where(x => x.MemberId == memberId).Count();
            m.IsApproval = count == 0 ? (int)IsApproval.No : (int)IsApproval.Yes;
            return m;
        }
        #endregion

        #region 获取评论
        /// <summary>
        /// 获取评论
        /// </summary>
        /// <param name="flag">枚举标识</param>
        /// <param name="listId">被评论Id</param>
        /// <returns></returns>
        private List<Comment> GetComments(CommentSourceFlag flag, List<int> listId)
        {
            IQueryable<Comment> listComments = _commentContract.Comments.Where(x => x.IsDeleted == false && x.IsEnabled == true);
            List<Comment> coments = listComments.Where(x => x.CommentSource == (int)flag && listId.Contains(x.SourceId)).ToList();
            return coments;
        }
        #endregion

        #region 获取赞
        /// <summary>
        /// 获取赞
        /// </summary>
        /// <param name="flag">枚举标识</param>
        /// <param name="listId">被评论Id</param>
        /// <returns></returns>
        private List<Approval> GetApprovals(CommentSourceFlag flag, List<int> listId)
        {
            List<Approval> listApproval = _approvalContract.Approvals.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.ApprovalSource == (int)flag && listId.Contains(x.SourceId)).ToList();
            return listApproval;
        }
        #endregion

        #region 获取点赞会员列表
        /// <summary>
        /// 获取点赞会员列表
        /// </summary>
        /// <param name="listApproval"></param>
        /// <param name="listMember"></param>
        /// <param name="productId"></param>
        /// <param name="appCount"></param>
        /// <returns></returns>
        private List<MemberApproval> GetMemberApproval(IQueryable<Approval> listApproval, IQueryable<Member> listMember, int productId, CommentSourceFlag commentType, out int appCount)
        {
            IQueryable<Approval> listApp = listApproval.Where(x => x.SourceId == productId && x.ApprovalSource == (int)commentType);
            appCount = listApp.Count();
            List<MemberApproval> listMemberApp = new List<MemberApproval>();
            if (appCount > 0)
            {
                listMemberApp = (from ap in listApp
                                 join
                                 me in listMember
                                 on
                                 ap.MemberId equals me.Id
                                 select new MemberApproval
                                 {
                                     MemberImagePath = me.MobilePhone,
                                 }).ToList();
            }
            return listMemberApp;
        }
        #endregion

        #region 获取评论会员列表
        /// <summary>
        /// 获取评论会员列表
        /// </summary>
        /// <param name="listComm"></param>
        /// <param name="listMember"></param>
        /// <param name="productId"></param>
        /// <param name="commCount"></param>
        private List<MemberComment> GetMemberComment(IQueryable<Comment> listComment, IQueryable<Member> listMember, int productId, CommentSourceFlag commentType, out int commCount)
        {
            IQueryable<Comment> listComm = listComment.Where(x => x.SourceId == productId && x.CommentSource == (int)commentType).OrderByDescending(x => x.Id).Take(3);
            commCount = listComm.Count();
            List<MemberComment> listMemberComment = new List<MemberComment>();
            if (commCount > 0)
            {
                listMemberComment = (from co in listComm
                                     join
                                     me in listMember
                                     on
                                     co.MemberId equals me.Id
                                     select new MemberComment
                                     {
                                         MemberName = me.MemberName,
                                         Content = co.Content,
                                     }).ToList();
            }
            return listMemberComment;
        }
        #endregion

        #region 获取会员是否点赞
        private int GetIsApproval(IQueryable<Approval> listApproval, int memberId, int productId, CommentSourceFlag commentType)
        {
            IQueryable<Approval> listApp = listApproval.Where(x => x.SourceId == productId && x.ApprovalSource == (int)commentType && x.MemberId == memberId);
            int count = listApp.Count() > 0 ? (int)IsApproval.Yes : (int)IsApproval.No;
            return count;
        }
        #endregion

        #region 获取零件图两张
        private List<string> GetListImagePath(IEnumerable<MemberColloEle> listMemberColloEle)
        {
            List<string> list = new List<string>();
            if (listMemberColloEle.Count() > 0)
            {
                foreach (var item in listMemberColloEle)
                {
                    if (list.Count >= 2)
                    {
                        break;
                    }
                    if (item.ProductSource == ProductSourceFlag.MaterialProduct)
                    {
                        IQueryable<Gallery> listGallery = _galleryContract.Gallerys.Where(x => x.Id == item.ProductId && x.GalleryType == GalleryFlag.Product);
                        if (listGallery.Count() > 0)
                        {
                            list.Add(listGallery.FirstOrDefault().ThumbnailPath);
                        }
                        else
                        {
                            list.Add(string.Empty);
                        }
                    }
                    else if (item.ProductSource == ProductSourceFlag.MemberProduct)
                    {
                        if (item.ProductType == SingleProductFlag.OrderItem)
                        {
                            IQueryable<Product> listProduct = _productContract.Products.Where(x => x.Id == item.ProductId);
                            if (listProduct.Count() > 0)
                            {
                                list.Add(listProduct.FirstOrDefault().ThumbnailPath);
                            }
                            else
                            {
                                list.Add(string.Empty);
                            }
                        }
                        else if (item.ProductType == (int)SingleProductFlag.Upload)
                        {
                            IQueryable<MemberSingleProduct> listSingle = _memberSingleProContract.MemberSingleProducts.Where(x => x.Id == item.ProductId);
                            if (listSingle.Count() > 0)
                            {
                                list.Add(listSingle.FirstOrDefault().Image);
                            }
                            else
                            {
                                list.Add(string.Empty);
                            }
                        }
                    }
                    else if (item.ProductSource == ProductSourceFlag.StoreProduct)
                    {
                        IQueryable<Product> listProduct = _productContract.Products.Where(x => x.Id == item.ProductId);
                        if (listProduct.Count() > 0)
                        {
                            list.Add(listProduct.FirstOrDefault().ThumbnailPath);
                        }
                        else
                        {
                            list.Add(string.Empty);
                        }
                    }
                    else if (item.ProductSource == ProductSourceFlag.UploadProduct)
                    {
                        list.Add(item.ImagePath);
                    }
                    else
                    {

                    }
                }
            }
            return list;
        }
        #endregion



        #region 获取推荐搭配
        /// <summary>
        /// 获取推荐搭配
        /// </summary>
        /// <param name="PageIndex"></param>
        /// <param name="PageSize"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetRecommendCollo(int PageIndex = 1, int PageSize = 10)
        {
            try
            {
                string strMemberId = Request["MemberId"];
                int memberId = int.Parse(strMemberId);
                IQueryable<MemberCollocation> listMemberCollo = _memberCollocationContract.MemberCollocations.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.IsRecommend == true);
                listMemberCollo = listMemberCollo.OrderByDescending(x => x.Id).Skip((PageIndex - 1) * PageSize).Take(PageSize);
                IQueryable<MemberColloEle> listMemberColloEle = _memberColloEleContract.MemberColloEles;
                IQueryable<Comment> listComment = _commentContract.Comments.Where(x => x.IsDeleted == false && x.IsEnabled == true).OrderByDescending(x => x.Id);
                IQueryable<Approval> listApproval = _approvalContract.Approvals.Where(x => x.IsDeleted == false && x.IsEnabled == true);
                IQueryable<Member> listMember = _memberContract.Members.Where(x => x.IsDeleted == false && x.IsEnabled == true);
                var dataCollo = (from co in listMemberCollo
                                 join
                                 me in listMember
                                 on
                                 co.MemberId equals me.Id
                                 select new ColloArticle
                                 {
                                     DataType = (int)ColloArticleFlag.Collo,
                                     ProductId = co.Id,
                                     Title = co.CollocationName,
                                     MemberName = me.MemberName,
                                     MemberImagePath = me.UserPhoto,
                                     CoverImagePath = co.MemberColloEles.Where(x => x.Parent == null).FirstOrDefault().ImagePath,
                                 }).ToList();
                foreach (var item in dataCollo)
                {
                    //获取点赞会员列表
                    int appCount = 0;
                    item.ListMemberApproval = this.GetMemberApproval(listApproval, listMember, item.ProductId, CommentSourceFlag.MemberCollocation, out appCount);
                    item.ApprovalCount = appCount;
                    //获取评论会员列表
                    int commCount = 0;
                    item.ListMemberComment = this.GetMemberComment(listComment, listMember, item.ProductId, CommentSourceFlag.MemberCollocation, out commCount);
                    item.CommentCount = commCount;
                    //当前会员是否点赞
                    item.IsApproval = this.GetIsApproval(listApproval, memberId, item.ProductId, CommentSourceFlag.MemberCollocation);
                    IQueryable<MemberColloEle> listEle = _memberColloEleContract.MemberColloEles.Where(x => x.MemberColloId == item.ProductId && x.Parent != null);
                    item.ListImagePath = this.GetListImagePath(listEle);
                }
                List<ColloArticle> listColloArticle = new List<ColloArticle>();
                listColloArticle.AddRange(dataCollo);
                return Json(new OperationResult(OperationResultType.Success, "获取成功！", listColloArticle));
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return Json(new OperationResult(OperationResultType.Error, "服务器忙，请稍后重试！"));
            }

        }
        #endregion

        #region 获取推荐文章
        /// <summary>
        /// 获取推荐文章
        /// </summary>
        /// <param name="PageIndexs"></param>
        /// <param name="PageIndex"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetRecommendArticle(int PageIndex = 1, int PageSize = 10)
        {
            try
            {
                string strMemberId = Request["MemberId"];
                int memberId = int.Parse(strMemberId);
                IQueryable<AppArticle> listArticle = _appArticleContract.AppArticles.Where(x => x.IsDeleted == false && x.IsEnabled == true);
                listArticle = listArticle.OrderByDescending(x => x.IsRecommend).Skip((PageIndex - 1) * PageSize).Take(PageSize);
                IQueryable<MemberColloEle> listMemberColloEle = _memberColloEleContract.MemberColloEles;
                IQueryable<Comment> listComment = _commentContract.Comments.Where(x => x.IsDeleted == false && x.IsEnabled == true).OrderByDescending(x => x.Id);
                IQueryable<Approval> listApproval = _approvalContract.Approvals.Where(x => x.IsDeleted == false && x.IsEnabled == true);
                IQueryable<Member> listMember = _memberContract.Members.Where(x => x.IsDeleted == false && x.IsEnabled == true);
                var dataArticle = (from ar in listArticle
                                   join
                                   me in listMember
                                   on
                                   ar.OperatorId equals me.Id
                                   select new ColloArticle
                                   {
                                       DataType = (int)ColloArticleFlag.Collo,
                                       ProductId = ar.Id,
                                       Title = ar.ArticleTitle,
                                       MemberName = me.MemberName,
                                       MemberImagePath = me.UserPhoto,
                                       CoverImagePath = ar.CoverImagePath,
                                   }).ToList();
                foreach (var item in dataArticle)
                {
                    //获取点赞会员列表
                    int appCount = 0;
                    item.ListMemberApproval = this.GetMemberApproval(listApproval, listMember, item.ProductId, CommentSourceFlag.Article, out appCount);
                    item.ApprovalCount = appCount;
                    //获取评论会员列表
                    int commCount = 0;
                    item.ListMemberComment = this.GetMemberComment(listComment, listMember, item.ProductId, CommentSourceFlag.Article, out commCount);
                    item.CommentCount = commCount;
                    item.IsApproval = this.GetIsApproval(listApproval, memberId, item.ProductId, CommentSourceFlag.Article);
                }
                List<ColloArticle> listColloArticle = new List<ColloArticle>();
                listColloArticle.AddRange(dataArticle);
                var data = listColloArticle.Select(x => new
                {
                    x.DataType,
                    x.ProductId,
                    x.Title,
                    x.MemberName,
                    x.MemberImagePath,
                    x.ApprovalCount,
                    x.CommentCount,
                    x.IsApproval,
                    x.CoverImagePath,
                    x.ListMemberApproval,
                    x.ListMemberComment
                });
                return Json(new OperationResult(OperationResultType.Success, "获取成功！", listColloArticle), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return Json(new OperationResult(OperationResultType.Error, "服务器忙，请稍后重试！"), JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region 移除数据
        public JsonResult Remove()
        {
            OperationResult oper = new OperationResult(OperationResultType.Error, "服务器忙，请稍后访问");
            try
            {
                string strColloId = Request["ColloId"];
                string[] arrId = strColloId.Split(',');
                List<int> list = new List<int>();
                foreach (string id in arrId)
                {
                    if (!string.IsNullOrEmpty(id))
                    {
                        list.Add(int.Parse(id));
                    }
                }
                int memberId = int.Parse(Request["MemberId"]);
                oper = _memberCollocationContract.Remove(memberId, list.ToArray());
                return Json(oper);
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return Json(oper);
            }

        }
        #endregion

        #region 获取穿衣数据
        /// <summary>
        /// 获取穿衣数据
        /// </summary>
        /// <returns></returns>
        public JsonResult GetHistoryList(int MemberId)
        {

            List<MemberSingleProduct> listSingPro = _memberSingleProContract.MemberSingleProducts.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.MemberId == MemberId).ToList();
            List<MemberCollocation> listCollo = _memberCollocationContract.MemberCollocations.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.MemberId == MemberId).ToList();
            List<Category> listCategory = _categoryContract.Categorys.Where(x => x.IsDeleted == false && x.IsEnabled == true).ToList();
            List<Category> listParent = listCategory.Where(x => x.ParentId == null).ToList();
            List<SingleProduct> listPro = new List<SingleProduct>();
            foreach (Category parent in listParent)
            {
                SingleProduct singlePro = new SingleProduct()
                {
                    Id = parent.Id,
                    CoverImagePath = parent.IconPath,
                    Count = 0,
                };
                if (parent.Children.Count > 0)
                {
                    List<int> listId = parent.Children.Select(x => x.Id).ToList();
                    int count = listSingPro.Where(x => listId.Contains(x.CategoryId ?? 0)).Count();
                    singlePro.Count = count;
                }
                listPro.Add(singlePro);
            }
            var categoryInfos = listPro.Select(x => new
            {
                x.Id,
                x.CoverImagePath,
                x.Count,
            });
            List<int> listSeven = GetListByDay(listCollo);
            List<int> listAllYear = GetListByYear(listCollo);
            var data = new
            {
                AllCont = listSingPro.Count(),
                SingPros = categoryInfos,
                SeventDaysCollo = listSeven,
                AllYearCollo = listAllYear,
            };
            OperationResult oper = new OperationResult(OperationResultType.Success, "获取成功", data);
            return Json(oper);
        }
        #endregion

        #region 获取过去7天的搭配数据
        private List<int> GetListByDay(List<MemberCollocation> listCollo)
        {

            DateTime currentDate = DateTime.Now;
            List<int> list = new List<int>();
            int year = currentDate.Year;
            int month = currentDate.Month;
            int index = 0;
            while (true)
            {
                if (index <= -7)
                {
                    break;
                }
                else
                {
                    int day = currentDate.Day;
                    int count = listCollo.Where(x => x.CreatedTime.Year == year && x.CreatedTime.Month == month && x.CreatedTime.Day == day).Count();
                    list.Add(count);
                }
                index = index - 1;
                currentDate = currentDate.AddDays(index);
            }
            return list;
        }
        #endregion

        #region 获取整年搭配数据
        private List<int> GetListByYear(List<MemberCollocation> listCollo)
        {
            DateTime currentDate = DateTime.Now;
            List<int> list = new List<int>();
            int year = currentDate.Year;
            int month = currentDate.Month;
            for (int i = 12; i <= 0; i--)
            {
                int count = listCollo.Where(x => x.CreatedTime.Year == year && x.CreatedTime.Month == i).Count();
                list.Add(count);
            }
            return list;
        }
        #endregion
    }
}
