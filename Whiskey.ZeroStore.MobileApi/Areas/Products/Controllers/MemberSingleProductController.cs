using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Data;
using Whiskey.Utility.Helper;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Transfers.APIEntities.MemberSingleProduct;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Product;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Transfers.ProductInfo;
using System.Drawing;
using Whiskey.Utility.Class;
using Whiskey.ZeroStore.MobileApi.Extensions.Attribute;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Comment;
using Whiskey.Utility.Extensions;
using Whiskey.ZeroStore.MobileApi.Areas.Products.Models;
using AutoMapper;
using Whiskey.ZeroStore.ERP.Models.Enums;

namespace Whiskey.ZeroStore.MobileApi.Areas.Products.Controllers
{
    [License(CheckMode.Verify)]
    public class MemberSingleProductController : Controller
    {

        #region 声明业务层操作对象
        //日志记录
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(MemberSingleProductController));
        //声明业务层操作对象
        protected readonly IMemberSingleProductContract _memberSingleProductContract;

        protected readonly ISensitiveWordContract _sensitiveWordContract;

        protected readonly ICommentContract _singleProductCommentContract;

        protected readonly IApprovalContract _singleApprovalContract;

        protected readonly IMemberContract _memberContract;

        protected readonly ICategoryContract _categoryContract;

        protected readonly IColorContract _colorContract;

        protected readonly ISeasonContract _seasonContract;

        protected readonly ISizeContract _sizeContract;

        protected readonly IProductContract _productContract;

        protected readonly IProductAttributeContract _productAttrContract;
        protected readonly IRecommendMemberSingleProductContract _recommendMemberSingleProductContract;
        //构造函数-初始化业务层操作对象
        public MemberSingleProductController(
            IMemberSingleProductContract memberSingleProductContract,
            ISensitiveWordContract sensitiveWordContract,
            ICommentContract singleProductCommentContract,
            IApprovalContract singleApprovalContract,
            IMemberContract memberContract,
            ICategoryContract categoryContract,
            IProductContract productContract,
            IColorContract colorContract,
            ISeasonContract seasonContract,
            ISizeContract sizeContract,
            IProductAttributeContract productAttrContract,
            IRecommendMemberSingleProductContract recommendMemberSingleProductContract)
        {
            _memberSingleProductContract = memberSingleProductContract;
            _sensitiveWordContract = sensitiveWordContract;
            _singleProductCommentContract = singleProductCommentContract;
            _singleApprovalContract = singleApprovalContract;
            _memberContract = memberContract;
            _categoryContract = categoryContract;
            _productContract = productContract;
            _colorContract = colorContract;
            _seasonContract = seasonContract;
            _sizeContract = sizeContract;
            _productAttrContract = productAttrContract;
            _recommendMemberSingleProductContract = recommendMemberSingleProductContract;
        }
        #endregion

        string apiUrl = ConfigurationHelper.GetAppSetting("ApiUrl");
        string strWebUrl = ConfigurationHelper.GetAppSetting("WebUrl");

        #region 创建数据
        /// <summary>
        /// 创建数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>

        [HttpPost]
        [Log]
        public ActionResult Add(SingleInfo singleInfo)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error);
            try
            {
                oper = CheckParameter(singleInfo);
                if (oper.ResultType == OperationResultType.Success)
                {
                    MemberSingleProductDto dto = oper.Data as MemberSingleProductDto;
                    //获取封面字节流
                    HttpPostedFileBase coverImage = Request.Files["CoverImage"];
                    //获取图片字节流
                    HttpPostedFileBase image = Request.Files["Image"];
                    //long maxSize = 3500480;
                    if (coverImage != null)
                    {
                        oper = SaveImage(coverImage.InputStream);
                        if (oper.ResultType == OperationResultType.Success)
                        {
                            dto.CoverImage = oper.Data.ToString();
                        }
                        else
                        {
                            return Json(oper);
                        }
                    }
                    else
                    {
                        //return Json(new OperationResult(OperationResultType.Error, "请选择封面图！"));
                    }
                    if (image != null)
                    {
                        oper = SaveImage(image.InputStream);
                        if (oper.ResultType == OperationResultType.Success)
                        {
                            dto.Image = oper.Data.ToString();
                        }
                        else
                        {
                            return Json(oper);
                        }
                    }
                    else
                    {
                        //return Json(new OperationResult(OperationResultType.Error, "请选择搭配图！"));
                    }
                    var result = _memberSingleProductContract.Insert(dto);
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(oper);
                }
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return Json(new OperationResult(OperationResultType.Error, "添加失败！"));
            }

        }
        #endregion

        #region 获取会员分类信息
        /// <summary>
        /// 获取分类信息
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        public JsonResult GetList()
        {
            try
            {
                string strMemmberId = Request["MemberId"];
                if (string.IsNullOrEmpty(strMemmberId))
                {
                    return Json(new OperationResult(OperationResultType.Error, "登录异常，请重新登录"), JsonRequestBehavior.AllowGet);
                }
                int memberId = int.Parse(strMemmberId);
                IQueryable<Category> listCategory = _categoryContract.Categorys.Where(x => x.IsDeleted == false && x.IsEnabled == true);
                IQueryable<MemberSingleProduct> listMemberSingle = _memberSingleProductContract.MemberSingleProducts.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.MemberId == memberId).OrderByDescending(x => x.Id);
                //List<int> listCategoryId = listMemberSingle.Select(x => x.CategoryId).ToList();
                List<SingleProduct> listPro = new List<SingleProduct>();
                IQueryable<Category> listCategoryParent = listCategory.Where(x => x.ParentId == null);
                foreach (var item in listCategoryParent)
                {
                    //List<int> listCategoryId = new List<int>();
                    var listCategoryId = listCategory.Where(x => x.ParentId == item.Id).Select(x => x.Id).ToList();
                    string strCoverImage = string.Empty;
                    var listSingle = listMemberSingle.Where(x => listCategoryId.Contains(x.CategoryId ?? 0));
                    int count = listSingle.Count();
                    if (count > 0)
                    {
                        strCoverImage = apiUrl + listSingle.FirstOrDefault().CoverImage;
                    }

                    SingleProduct sPro = new SingleProduct();
                    sPro.ParentCategoryName = item.CategoryName;
                    sPro.CoverImagePath = strCoverImage;
                    sPro.Count = count;
                    listPro.Add(sPro);
                }
                var result = listPro.Select(x => new
                {
                    ParentCategoryName = x.ParentCategoryName,
                    Count = x.Count,
                    CoverImagePath = x.CoverImagePath

                });
                return Json(new { ResultType = ((int)OperationResultType.Success), List = result }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(new { ResultType = ((int)OperationResultType.Error), List = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region 获取数据列表
        /// <summary>
        /// 获取数据列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        public JsonResult GetAllList(int? ColorId, int? ProductAttrId, int? CategoryId, int operationType, int PageIndex = 1, int PageSize = 10, SingleProductFlag flag = SingleProductFlag.All)
        {
            try
            {

                string strMemberId = Request["MemberId"];

                if (string.IsNullOrEmpty(strMemberId))
                {
                    return Json(new OperationResult(OperationResultType.Error, "登录异常，请重新登录！"));
                }
                else
                {
                    int memberId = int.Parse(strMemberId);
                    var listProInfo = _memberSingleProductContract.GetAllList(flag,memberId, ColorId, ProductAttrId, CategoryId, PageIndex, PageSize);
                    var singleComment = _singleProductCommentContract.Comments.Where(x => x.CommentSource == (int)CommentSourceFlag.MemberSingleProduct && x.IsDeleted == false && x.IsEnabled == true);
                    var singleApp = _singleApprovalContract.Approvals.Where(x => x.ApprovalSource == (int)CommentSourceFlag.MemberSingleProduct);
                    var res = listProInfo.Select(x => new
                    {
                        x.ColorId,
                        x.BigProdNumber,
                        x.ProductId,
                        x.ProductType,
                        x.CategoryName,
                        x.ColorName,
                        x.SeasonName,
                        x.SizeName,
                        x.Price,
                        ImagePath = GetPath(operationType, x.ProductType, x.CoverImagePath, x.ImagePath),
                        ColorIconPath = strWebUrl + x.ColorIconPath,
                        CommentCount = singleComment.Where(k => k.SourceId == x.ProductId).Count(),
                        IsApproval = singleApp.Any(k => k.SourceId == x.ProductId && k.MemberId == memberId) ? (int)IsApproval.Yes : (int)IsApproval.No,
                    }).ToList();


                    return Json(new  { ResultType = ((int)OperationResultType.Success), List = res }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return Json(new OperationResult(OperationResultType.Error, "服务器忙，请稍后重试！"));
            }

        }

        public enum ViewType {

        }

        public ActionResult RemoveRecommend(int memberId, int colorId, string bigProdNumber)
        {
            var entity = _recommendMemberSingleProductContract.Entities.Where(r => !r.IsDeleted && r.IsEnabled)
                .Where(r => r.MemberId == memberId && r.ColorId == colorId && r.BigProdNumber == bigProdNumber)
                .FirstOrDefault();
            if (entity != null)
            {
                var res = _recommendMemberSingleProductContract.Delete(entity);
                if (res.ResultType != OperationResultType.Success)
                {
                    return Json(OperationResult.Error("操作失败"));
                }
            }
            return Json(OperationResult.OK());
        }

        private string GetPath(int operationType, int productType, string coverImg, string colloImg)
        {
            if (operationType == 0) //封面图
            {
                if (productType == 0) //单品
                {
                    return apiUrl + coverImg;
                }
                else  // 已购买
                {
                    return strWebUrl + coverImg;
                }
            }
            else //搭配图
            {
                if (productType == 0) //单品
                {
                    return apiUrl + colloImg;
                }
                else  // 已购买
                {
                    return strWebUrl + colloImg;
                }
            }
        }
        #endregion

        #region 获取商品详细信息
        public JsonResult GetDetail()
        {
            try
            {
                string strProductId = Request["ProductId"];
                string strProductType = Request["ProductType"];
                string strMemberId = Request["MemberId"];
                if (string.IsNullOrEmpty(strMemberId) && string.IsNullOrEmpty(strProductType) && string.IsNullOrEmpty(strProductId))
                {
                    return Json(new OperationResult(OperationResultType.Error, "登录异常，请重新登录！"));
                }
                else
                {
                    int productType = int.Parse(strProductType);
                    int productId = int.Parse(strProductId);
                    int memberId = int.Parse(strMemberId);
                    if (productType == (int)SingleProductFlag.Upload)
                    {
                        var entity = _memberSingleProductContract.MemberSingleProducts.Where(x => x.Id == productId && x.MemberId == memberId).FirstOrDefault();
                        if (entity == null)
                        {
                            return Json(new OperationResult(OperationResultType.Error, "您查看的数据不存在！"));
                        }
                        else
                        {
                            var res = new
                            {
                                entity.Price,
                                CoverImagePath = apiUrl + entity.CoverImage,
                                ImagePath = apiUrl + entity.Image,
                            };
                            return Json(new OperationResult(OperationResultType.Success, "获取成功！", res));
                        }
                    }
                    else if (productType == (int)SingleProductFlag.OrderItem)
                    {
                        var entity = _productContract.Products.Where(x => x.Id == productId).FirstOrDefault();
                        var res = new
                        {
                            Price = entity.ProductOriginNumber.TagPrice,
                            CoverImagePath = strWebUrl + entity.OriginalPath,
                            entity.ProductImages,
                        };
                        return Json(new OperationResult(OperationResultType.Success, "获取成功！", res));
                    }
                    else
                    {
                        return Json(new OperationResult(OperationResultType.Error, "服务器忙，请稍后重试！"));
                    }
                }
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return Json(new OperationResult(OperationResultType.Error, "服务器忙，请稍后重试！"));
            }
        }
        #endregion

        #region 获取评论列表

        /// <summary>
        /// 获取评论列表
        /// </summary>
        /// <param name="PageIndex">页码</param>
        /// <param name="PageSize">每页显示数量</param>
        /// <returns></returns>

        [HttpPost]
        [ValidateInput(false)]
        public JsonResult GetComment(int PageIndex = 1, int PageSize = 10)
        {
            string strSingleProId = Request["SingleProductId"];
            try
            {
                int singleProId = int.Parse(strSingleProId);
                List<MemberComment> comments = _singleProductCommentContract.GetComment(singleProId, CommentSourceFlag.MemberSingleProduct, PageIndex, PageSize);
                if (comments == null)
                {
                    return Json(new { ResultType = ((int)OperationResultType.Success), ListComment = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var listComent = comments.Select(x => new
                    {
                        x.ProductId,
                        x.ReplyId,
                        x.MemberId,
                        x.CommentId,
                        x.MemberName,
                        x.ReplyMemberName,
                        x.Content,
                        CommentTime = x.CommentTime.ToString("yyyy-MM-dd HH:mm")

                    });
                    return Json(new { ResultType = ((int)OperationResultType.Success), ListComment = listComent }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return Json(new OperationResult(OperationResultType.Error, "服务器忙， 请稍后重试！"), JsonRequestBehavior.AllowGet);
            }

        }
        #endregion

        #region 获取赞列表

        /// <summary>
        /// 获取赞列表
        /// </summary>
        /// <returns></returns>

        [HttpPost]
        [ValidateInput(false)]
        public JsonResult GetApproval(int PageIndex = 1, int PageSize = 10)
        {
            string strSingleProId = Request["SingleProductId"];
            try
            {
                int singleProId = int.Parse(strSingleProId);
                IQueryable<Approval> listApproval = _singleApprovalContract.GetList(singleProId, CommentSourceFlag.MemberSingleProduct);
                IQueryable<Member> listMember = _memberContract.Members.Where(x => x.IsEnabled == true && x.IsDeleted == false);
                if (listApproval == null && listMember == null)
                {
                    return Json(new { ResultType = ((int)OperationResultType.Success), ListApproval = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var entityList = from a in listApproval
                                     join
                                     m in listMember
                                     on
                                     a.MemberId equals m.Id
                                     select new
                                     {
                                         MemberId = m.Id,
                                         MemberName = m.MemberName
                                     };
                    return Json(new { ResultType = ((int)OperationResultType.Success), ListApproval = entityList }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return Json(new OperationResult(OperationResultType.Error, "服务器忙，请稍后访问！"));
            }
        }

        #endregion

        #region 删除数据
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <returns></returns>

        [HttpPost]
        [ValidateInput(false)]
        public JsonResult Delete()
        {
            try
            {
                string strMemberId = Request["MemberId"];
                string strId = Request["ProductId"];
                string strProductType = Request["ProductType"];
                if (string.IsNullOrEmpty(strMemberId)) return Json(new OperationResult(OperationResultType.Error, "登录异常，请重新登录"), JsonRequestBehavior.AllowGet);
                if (string.IsNullOrEmpty(strId)) return Json(new OperationResult(OperationResultType.Error, "单品不存在！"), JsonRequestBehavior.AllowGet);
                if (string.IsNullOrEmpty(strProductType)) return Json(new OperationResult(OperationResultType.Error, "单品无法删除！"), JsonRequestBehavior.AllowGet);
                int productType = int.Parse(strProductType);
                if (productType == (int)SingleProductFlag.Upload)
                {
                    int memberId = int.Parse(strMemberId);
                    int id = int.Parse(strId);
                    var res = _memberSingleProductContract.Delete(memberId, id);
                    return Json(res, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new OperationResult(OperationResultType.Error, "此类型不能被删除！"), JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return Json(new OperationResult(OperationResultType.Error, "服务器忙，请稍后重试！"));
            }
        }
        #endregion

        #region 编辑数据
        /// <summary>
        /// 获取编辑信息
        /// </summary>
        /// <returns></returns>

        [HttpPost]
        [ValidateInput(false)]
        public JsonResult GetEdit()
        {
            try
            {
                string strMemberId = Request["MemberId"];
                string strId = Request["ProductId"];
                string strProductType = Request["ProductType"];
                if (string.IsNullOrEmpty(strMemberId)) return Json(new OperationResult(OperationResultType.Error, "登录异常，请重新登录"));
                if (string.IsNullOrEmpty(strId)) return Json(new OperationResult(OperationResultType.Error, "该商品不存在！"));
                if (string.IsNullOrEmpty(strProductType)) return Json(new OperationResult(OperationResultType.Error, "单品无法编辑！"), JsonRequestBehavior.AllowGet);
                int memberId = int.Parse(strMemberId);
                int id = int.Parse(strId);
                int productType = int.Parse(strProductType);
                if (productType == (int)SingleProductFlag.Upload)
                {
                    MemberProductInfo proInfo = _memberSingleProductContract.GetEdit(id);
                    //MemberSingleProductDto proDto = _memberSingleProductContract.Edit(id);
                    var entity = new
                    {
                        ProductId = proInfo.ProductId,
                        CoverImagePath = apiUrl + proInfo.CoverImagePath,
                        ImagePath = apiUrl + proInfo.ImagePath,
                        proInfo.CategoryId,
                        proInfo.CategoryName,
                        proInfo.ColorId,
                        proInfo.ColorName,
                        proInfo.ProductAttrId,
                        proInfo.ProductAttrName,
                        proInfo.SizeId,
                        proInfo.SizeName,
                        proInfo.SeasonId,
                        proInfo.SeasonName,
                        proInfo.ProductName,
                        proInfo.Price,
                        proInfo.Brand,
                        proInfo.Notes
                    };
                    return Json(new OperationResult(OperationResultType.Success, "获取成功！", entity), JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new OperationResult(OperationResultType.Error, "此商品不能被编辑！"));
                }
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return Json(new OperationResult(OperationResultType.Error, "服务器忙，请稍候重试！"));
            }

        }
        /// <summary>
        /// 保存编辑
        /// </summary>
        /// <returns></returns>

        [HttpPost]
        [ValidateInput(false)]
        public JsonResult SaveEdit(SingleInfo singleInfo)
        {
            try
            {
                OperationResult oper = new OperationResult(OperationResultType.Error);
                oper = this.CheckParameter(singleInfo);
                if (oper.ResultType == OperationResultType.Success)
                {
                    MemberSingleProductDto proDto = oper.Data as MemberSingleProductDto;
                    var entity = _memberSingleProductContract.Edit(proDto.Id);
                    proDto.CoverImage = entity.CoverImage;
                    proDto.Image = entity.Image;
                    var coverImage = Request.Files["CoverImage"];
                    var image = Request.Files["Image"];
                    if (coverImage != null)
                    {
                        oper = this.SaveImage(coverImage.InputStream);
                        if (oper.ResultType == OperationResultType.Success)
                        {
                            proDto.CoverImage = oper.Data.ToString();
                        }
                        else
                        {
                            return Json(oper);
                        }
                    }
                    if (image != null)
                    {
                        oper = this.SaveImage(image.InputStream);
                        if (oper.ResultType == OperationResultType.Success)
                        {
                            proDto.Image = oper.Data.ToString();
                        }
                        else
                        {
                            return Json(oper);
                        }
                    }
                    var updateRes = _memberSingleProductContract.Update(proDto);
                    return Json(updateRes, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(oper);
                }

            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return Json(new OperationResult(OperationResultType.Error, "服务器忙，请稍后重试！"), JsonRequestBehavior.AllowGet);
            }

        }

        #endregion

        #region 校验参数
        /// <summary>
        /// 校验参数
        /// </summary>
        /// <param name="singleInfo"></param>
        /// <returns></returns>
        private OperationResult CheckParameter(SingleInfo singleInfo)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error);
            try
            {
                #region 数据校验


                if (singleInfo.CategoryId == null)
                {
                    oper.Message = "请选择分类";
                    return oper;
                }

                if (!string.IsNullOrEmpty(singleInfo.Brand) && singleInfo.Brand.Length > 12)
                {
                    oper.Message = "品牌在1~12个字符之间";
                    return oper;
                }
                if (!string.IsNullOrEmpty(singleInfo.Notes) && singleInfo.Notes.Length > 12)
                {
                    oper.Message = "备注不能超过12个字符之间";
                    return oper;
                }
                string Notes = string.IsNullOrEmpty(singleInfo.Notes) ? string.Empty : singleInfo.Notes.Trim();

                MemberSingleProductDto dto = new MemberSingleProductDto()
                {
                    Brand = singleInfo.Brand,
                    CategoryId = singleInfo.CategoryId ?? 0,
                    ColorId = singleInfo.ColorId ?? 0,
                    MemberId = singleInfo.MemberId,
                    Notes = singleInfo.Notes,
                    Price = singleInfo.Price,
                    ProductAttrId = singleInfo.ProductAttrId,
                    SeasonId = string.IsNullOrEmpty(singleInfo.SeasonId) ? null : (int?)(int.Parse(singleInfo.SeasonId)),
                    SizeId = singleInfo.SizeId,
                    ProductName = string.IsNullOrEmpty(singleInfo.ProductName) ? string.Empty : singleInfo.ProductName.Trim(),
                    Id = singleInfo.ProductId,
                };
                oper.ResultType = OperationResultType.Success;
                oper.Data = dto;
                return oper;
                #endregion
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                oper.Message = "验证数据出错";
                return oper;
            }

        }
        #endregion

        #region 保存图片
        private OperationResult SaveImage(Stream stream)
        {
            string conPath = ConfigurationHelper.GetAppSetting("SaveMemberSingleProduct");
            DateTime currentDate = DateTime.Now;
            string strDate = currentDate.Year.ToString() + "/" + currentDate.Month.ToString() + "/" + currentDate.Day.ToString() + "/" + currentDate.ToString("HH") + "/";
            byte[] bytes = new byte[stream.Length];
            string strFileName = Encoding.Default.GetString(bytes);
            string savePath = conPath + strDate + strFileName.MD5Hash();
            string strRes = ImageHelper.MakeThumbnail(stream, savePath, 200, 200, "W", "Png");
            if (string.IsNullOrEmpty(strRes))
            {
                return new OperationResult(OperationResultType.Error, "上传图片失败");
            }
            else
            {
                return new OperationResult(OperationResultType.Success, string.Empty, strRes);
            }

        }
        #endregion

        #region 批量上传
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Multiple()
        {
            OperationResult oper = new OperationResult(OperationResultType.Error, "服务忙，请稍后访问");
            try
            {
                HttpFileCollectionBase files = Request.Files;
                int memberId = int.Parse(Request["MemberId"]);
                var color = _colorContract.Colors.FirstOrDefault(x => x.IsDeleted == false && x.IsEnabled == true);
                var season = _seasonContract.Seasons.FirstOrDefault(x => x.IsDeleted == false && x.IsEnabled == true);
                int categoryId = 0;
                List<Category> listCategory = _categoryContract.Categorys.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.ParentId != null).ToList();
                Category category = listCategory.FirstOrDefault(x => x.Id == 9);
                if (category == null)
                {
                    categoryId = listCategory.FirstOrDefault().Id;
                }
                else
                {
                    categoryId = category.Id;
                }
                //var size = _sizeContract.Sizes.FirstOrDefault(x => x.IsDeleted == false && x.IsEnabled == true && x.CategoryId == categoryId);
                var size = _sizeContract.Sizes.FirstOrDefault(x => x.IsDeleted == false && x.IsEnabled == true);
                ProductAttribute proAttr = _productAttrContract.ProductAttributes.FirstOrDefault(x => x.IsDeleted == false && x.IsEnabled == true && x.ParentId == 1);
                MemberSingleProductDto dto = new MemberSingleProductDto()
                {
                    MemberId = memberId,
                    Price = 0,
                    ColorId = color.Id,
                    SeasonId = season.Id,
                    SizeId = size.Id,
                    CategoryId = categoryId,
                    ProductAttrId = proAttr.Id.ToString(),
                };
                int count = files.Count;
                List<MemberSingleProduct> listPro = _memberSingleProductContract.MemberSingleProducts.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.MemberId == memberId).ToList();
                List<string> listName = new List<string>();
                StringBuilder sbName = new StringBuilder();
                while (true)
                {
                    if (listName.Count() == count)
                    {
                        break;
                    }
                    sbName.Append(RandomHelper.GetRandomCode(6));
                    int index = listPro.Where(x => x.ProductName == sbName.ToString()).Count();
                    if (index == 0)
                    {
                        index = listName.Where(x => x == sbName.ToString()).Count();
                        if (index == 0)
                        {
                            listName.Add(sbName.ToString());
                        }
                    }
                    sbName.Clear();
                }
                List<MemberSingleProductDto> listDto = new List<MemberSingleProductDto>();
                for (int i = 0; i < files.Count; i++)
                {
                    oper = this.SaveImage(files[i].InputStream);
                    if (oper.ResultType == OperationResultType.Success)
                    {
                        MemberSingleProductDto entity = dto.DeepClone();
                        entity.CoverImage = oper.Data.ToString();
                        entity.Image = oper.Data.ToString();
                        entity.ProductName = listName[i];
                        listDto.Add(entity);
                    }
                    else
                    {
                        return Json(oper);
                    }
                }
                oper = _memberSingleProductContract.Insert(listDto.ToArray());
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
            List<MemberSingleProduct> listSingPro = _memberSingleProductContract.MemberSingleProducts.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.MemberId == MemberId).ToList();
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
            List<int> listSeven = GetListByDay(listSingPro);
            List<int> listAllYear = GetListByYear(listSingPro);
            var data = new
            {
                AllCont = listSingPro.Count(),
                CategoryInfos = categoryInfos,
                SeventDays = listSeven,
                AllYear = listAllYear,
            };
            OperationResult oper = new OperationResult(OperationResultType.Success, "获取成功", data);
            return Json(oper);
        }
        #endregion

        #region 获取过去7天的搭配数据
        private List<int> GetListByDay(List<MemberSingleProduct> listSingPro)
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
                    int count = listSingPro.Where(x => x.CreatedTime.Year == year && x.CreatedTime.Month == month && x.CreatedTime.Day == day).Count();
                    list.Add(count);
                }
                index = index - 1;
                currentDate.AddDays(index);
            }
            return list;
        }
        #endregion

        #region 获取整年搭配数据
        private List<int> GetListByYear(List<MemberSingleProduct> listSingPro)
        {
            DateTime currentDate = DateTime.Now;
            List<int> list = new List<int>();
            int year = currentDate.Year;
            int month = currentDate.Month;
            for (int i = 12; i <= 0; i--)
            {
                int count = listSingPro.Where(x => x.CreatedTime.Year == year && x.CreatedTime.Month == month).Count();
                list.Add(count);
            }
            return list;
        }
        #endregion

        #region 根据颜色或者分类获取单品数据
        public JsonResult GetDataByColorOrCategory(int MemberId, int CategoryId, int DateFlag)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error, "获取数据失败");
            Category category = _categoryContract.View(CategoryId);
            List<int> listId = new List<int>();
            if (category == null)
            {
                return Json(oper);
            }
            else
            {
                if (category.Children == null)
                {
                    oper.ResultType = OperationResultType.Success;
                    oper.Message = "暂无数据";
                    return Json(oper);
                }
                else
                {
                    listId = category.Children.Select(x => x.Id).ToList();
                }
            }
            List<MemberSingleProduct> listPro = _memberSingleProductContract.MemberSingleProducts.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.MemberId == MemberId && listId.Contains(x.CategoryId ?? 0)).ToList();
            if (DateFlag == 0)
            {
                List<Whiskey.ZeroStore.ERP.Models.Color> listColor = _colorContract.Colors.Where(x => x.IsDeleted == false && x.IsEnabled == true).ToList();
                var data = listColor.Select(x => new
                {
                    x.Id,
                    Name = x.ColorName,
                    IconPath = strWebUrl + x.IconPath,
                    Count = listPro.Where(k => k.ColorId == x.Id).Count(),
                });
                oper.ResultType = OperationResultType.Success;
                oper.Message = "获取成功";
                oper.Data = data;
                return Json(oper);
            }
            else if (DateFlag == 1)
            {
                List<ProductAttribute> listProAttr = _productAttrContract.ProductAttributes.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.ParentId == 1).ToList();
                var data = listProAttr.Select(x => new
                {
                    x.Id,
                    Name = x.AttributeName,
                    IconPath = strWebUrl + x.IconPath,
                    Count = listPro.Where(k => k.ProductAttrId.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Contains(x.Id.ToString())).Count(),
                });
                oper.ResultType = OperationResultType.Success;
                oper.Message = "获取成功";
                oper.Data = data;
                return Json(oper);
            }
            else
            {
                return Json(oper);
            }

        }
        #endregion
    }
}
