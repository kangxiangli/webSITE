using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.Core.Data.Extensions;
using System.Linq.Expressions;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Base;
using System.Web.Script.Serialization;
using Whiskey.ZeroStore.ERP.Transfers.APIEntities.Articles;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Article;
using Whiskey.Web.Helper;
using Whiskey.Utility.Helper;
using System.Text;
using System.Drawing.Imaging;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Transfers.Enum;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Comment;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Product;
using Whiskey.ZeroStore.MobileApi.Extensions.Attribute;
using Whiskey.Utility.Class;
using Whiskey.ZeroStore.MobileApi.Areas.Articles.Models;
using Whiskey.Utility.Extensions;
using System.IO;
using Newtonsoft.Json;
using System.Drawing;

namespace Whiskey.ZeroStore.MobileApi.Areas.Articles.Controllers
{
    [License(CheckMode.Verify)]
    public class AppArticleController : Controller
    {
        #region 初始化操作对象

        //日志记录
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(AppArticleController));
        //声明业务层操作对象
        protected readonly IAppArticleContract _appArticleContract;

        protected readonly IAppArticleItemContract _appArticleItemContract;

        protected readonly ICommentContract _commentContract;

        protected readonly IApprovalContract _approvalContract;
        public AppArticleController(IAppArticleContract appArticleContract,
            ICommentContract commentContract,
            IApprovalContract approvalContract,
            IAppArticleItemContract appArticleItemContract)
        {
            _appArticleContract = appArticleContract;
            _commentContract = commentContract;
            _approvalContract = approvalContract;
            _appArticleItemContract = appArticleItemContract;
        }
        #endregion

        string apiUrl = ConfigurationHelper.GetAppSetting("ApiUrl");

        #region 获取数据
        /// <summary>
        /// 获取数据
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Get(int PageIndex = 1, int PageSize = 10)
        {
            try
            {

                string strMemberId = Request["MemberId"];
                int memberId = int.Parse(strMemberId);
                IQueryable<AppArticle> listAppArticle = _appArticleContract.AppArticles.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.MemberId == memberId);
                listAppArticle = listAppArticle.OrderByDescending(x => x.Id).Skip((PageIndex - 1) * PageSize).Take(PageSize);
                IQueryable<Comment> listComment = _commentContract.Comments.Where(x => x.CommentSource == (int)CommentSourceFlag.AppArticle);
                IQueryable<Approval> listApproval = _approvalContract.Approvals.Where(x => x.ApprovalSource == (int)CommentSourceFlag.AppArticle);
                var entity = listAppArticle.Select(x => new
                {
                    ArticleId = x.Id,
                    x.ArticleTitle,
                    x.IsRecommend,
                    CoverImagePath = apiUrl + x.CoverImagePath,
                    CommentCount = listComment.Where(k => k.SourceId == x.Id).Count(),
                    ApprovalCount = listApproval.Where(k => k.SourceId == x.Id).Count(),
                    IsApprove = listApproval.Where(k => k.SourceId == x.Id && x.MemberId == memberId).Count() > 0 ? (int)IsApproval.Yes : (int)IsApproval.No,
                });
                return Json(new OperationResult(OperationResultType.Success, "获取成功", entity));
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return Json(new OperationResult(OperationResultType.Error, "服务器忙，请稍后重试"));
            }
        }
        #endregion

        #region 添加数据
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Add(ArticlePara para)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error);
            try
            {
                //string strPath = @"C:\Users\Pirate\Desktop\text.txt";
                //para.AppArticleInfos = System.IO.File.ReadAllText(strPath);


                //参数校验
                oper = this.CheckParameter(para);

                if (oper.ResultType == OperationResultType.Success)
                {
                    var dto = oper.Data as AppArticleDto;
                    //JavaScriptSerializer js = new JavaScriptSerializer();

                    // 获取文章信息
                    var listAppArticleInfo = JsonConvert.DeserializeObject<List<AppArticleInfo>>(para.AppArticleInfos);
                    if (listAppArticleInfo.Count == 0)
                    {
                        oper.Message = "暂时无法添加，请稍后重试";
                        return Json(oper);
                    }
                    else
                    {

                        var sbNum = new StringBuilder();
                        var listNum = new List<string>();
                        var articleItemQuery = _appArticleItemContract.AppArticleItems.Where(x => x.IsDeleted == false && x.IsEnabled == true);
                        if (articleItemQuery.Count() > 0)
                        {
                            listNum = articleItemQuery.Select(x => x.AppArticleNum).Distinct().ToList();
                        }

                        var listAppArticleItem = new List<AppArticleItem>();
                        foreach (var appArticleInfo in listAppArticleInfo)
                        {
                            // 生成一个随机的编码
                            sbNum.Append(RandomHelper.GetRandomCode(7));

                            // 去重
                            while (listNum.Count > 0)
                            {
                                if (listNum.Contains(sbNum.ToString()))
                                {
                                    sbNum.Clear();
                                    sbNum.Append(RandomHelper.GetRandomCode(7));
                                }
                                else
                                {
                                    listNum.Add(sbNum.ToString());
                                    break;
                                }
                            }


                            // 文章页编号
                            int sequence = appArticleInfo.Sequence;

                            // 保存文章图片
                            oper = AddImageInfos(appArticleInfo.ImageInfos, sbNum.ToString(), appArticleInfo.TemplateType, null, sequence);

                            if (oper.ResultType == OperationResultType.Success)
                            {
                                List<AppArticleItem> listEntity = oper.Data as List<AppArticleItem>;
                                listAppArticleItem.AddRange(listEntity);
                            }
                            else
                            {
                                return Json(oper);
                            }
                            listAppArticleItem.AddRange(AddDynamicPictures(appArticleInfo.DynamicPictures, sbNum.ToString(), appArticleInfo.TemplateType, sequence));
                            listAppArticleItem.AddRange(AddMaterials(appArticleInfo.Materials, sbNum.ToString(), appArticleInfo.TemplateType, sequence));
                            listAppArticleItem.AddRange(AddContentInfos(appArticleInfo.ContentInfos, sbNum.ToString(), appArticleInfo.TemplateType, sequence));
                            sbNum.Clear();
                        }
                        dto.AppArticleItems = listAppArticleItem;
                        oper = _appArticleContract.Insert(dto);
                        return Json(oper);
                    }
                }
                else
                {
                    return Json(oper);
                }
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                oper.Message = "服务器忙，请稍后重试";
                return Json(oper);
            }

        }
        #endregion

        #region 获取编辑数据
        /// <summary>
        /// 获取编辑数据
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetEdit()
        {
            OperationResult oper = new OperationResult(OperationResultType.Error);
            try
            {
                string strMemberId = Request["MemberId"];
                string strArticleId = Request["ArticleId"];
                int memberId = int.Parse(strMemberId);
                int articleId = int.Parse(strArticleId);
                AppArticle appArticle = _appArticleContract.AppArticles.Where(x => x.MemberId == memberId && x.Id == articleId).FirstOrDefault();
                if (appArticle == null)
                {
                    oper.Message = "要编辑的文章不存在";
                    return Json(oper);
                }
                else
                {
                    List<AppArticleItem> listAppArticleItems = appArticle.AppArticleItems.OrderBy(x => x.CreatedTime).ToList();
                    List<string> listNum = listAppArticleItems.Select(x => x.AppArticleNum).Distinct().ToList();
                    List<AppArticleInfo> AppArticleInfos = new List<AppArticleInfo>();
                    int template = 0;
                    foreach (string strNum in listNum)
                    {
                        #region 初始化数据
                        List<GalleryInfo> DynamicPictures = new List<GalleryInfo>();
                        List<GalleryInfo> Materials = new List<GalleryInfo>();
                        List<ArticleImageInfo> ImageInfos = new List<ArticleImageInfo>();
                        List<ArticleContentInfo> ContentInfos = new List<ArticleContentInfo>();
                        List<AppArticleItem> appArticleItems = listAppArticleItems.Where(x => x.AppArticleNum == strNum).ToList();
                        int Sequence = 0;
                        foreach (AppArticleItem articleItem in appArticleItems)
                        {
                            Sequence = articleItem.Sequence;
                            template = articleItem.TemplateType;
                            if (articleItem.AppArticleItemType == (int)AppArticleItemFlag.DynamicPicture)
                            {
                                GalleryInfo galleryInfo = new GalleryInfo()
                                {
                                    Id = articleItem.Id,
                                    GalleryId = articleItem.GalleryId ?? 0,
                                    Position = articleItem.Position,
                                    Ratio = articleItem.Ratio,
                                    ImagePath = apiUrl + articleItem.Gallery.ThumbnailPath,
                                };
                                DynamicPictures.Add(galleryInfo);
                            }
                            else if (articleItem.AppArticleItemType == (int)AppArticleItemFlag.Material)
                            {
                                GalleryInfo galleryInfo = new GalleryInfo()
                                {
                                    Id = articleItem.Id,
                                    GalleryId = articleItem.GalleryId ?? 0,
                                    Position = articleItem.Position,
                                    Ratio = articleItem.Ratio,
                                    ImagePath = apiUrl + articleItem.Gallery.ThumbnailPath,
                                };
                                Materials.Add(galleryInfo);
                            }
                            else if (articleItem.AppArticleItemType == (int)AppArticleItemFlag.ArticleImage)
                            {
                                ArticleImageInfo imageInfo = new ArticleImageInfo()
                                {
                                    Id = articleItem.Id,
                                    Rotation = articleItem.Rotation,
                                    Position = articleItem.Position,
                                    ImagePath = apiUrl + articleItem.ImagePath,
                                };
                                ImageInfos.Add(imageInfo);
                            }
                            else if (articleItem.AppArticleItemType == (int)AppArticleItemFlag.Text)
                            {
                                ArticleContentInfo contentInfo = new ArticleContentInfo()
                                {
                                    Id = articleItem.Id,
                                    AlignStyle = articleItem.AlignStyle,
                                    Content = articleItem.Content,
                                    FontColor = articleItem.FontColor,
                                    FontSize = articleItem.FontSize,
                                    Position = articleItem.Position,
                                    ContentRow = articleItem.ContentRow,
                                    Rotation = articleItem.Rotation,
                                    Ratio = articleItem.Ratio,
                                };
                                ContentInfos.Add(contentInfo);
                            }
                        }
                        #endregion                        
                        AppArticleInfo articleInfo = new AppArticleInfo()
                        {
                            DynamicPictures = DynamicPictures,
                            Materials = Materials,
                            ImageInfos = ImageInfos,
                            ContentInfos = ContentInfos,
                            TemplateType = template,
                            Num = strNum,
                            Sequence = Sequence,
                        };
                        AppArticleInfos.Add(articleInfo);
                    }
                    Detail detail = new Detail();
                    detail.AppArticleInfos = AppArticleInfos;
                    var data = new
                    {
                        ArticleId = appArticle.Id,
                        appArticle.ArticleTitle,
                        CoverImagePath = apiUrl + appArticle.CoverImagePath,
                        IsEffect = appArticle.IsEffect,
                        Detail = detail,
                        DeviceType = appArticle.DeviceType
                    };
                    return Json(new OperationResult(OperationResultType.Success, "获取成功", data));
                }
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return Json(new OperationResult(OperationResultType.Error, "服务器忙，请稍后重试"));
            }
        }
        #endregion

        #region 删除数据
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <returns></returns>
        public JsonResult Delete()
        {
            OperationResult oper = new OperationResult(OperationResultType.Error);
            try
            {
                string strMemberId = Request["MemberId"];
                string strArticleId = Request["ArticleId"];
                int memberId = int.Parse(strMemberId);
                int articleId = int.Parse(strArticleId);
                var data = _appArticleContract.AppArticles.Where(x => x.MemberId == memberId && x.Id == articleId).FirstOrDefault();
                var res = _appArticleContract.Remove(data.Id);
                return Json(res);
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                oper.Message = "服务器忙，请稍后重试";
                return Json(oper);
            }
        }
        #endregion

        #region 保存编辑数据
        /// <summary>
        /// 保存编辑数据
        /// </summary>
        /// <returns></returns>
        public JsonResult SaveEdit(ArticlePara para)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error);
            try
            {
                oper = this.CheckParameter(para);
                AppArticleDto entityDto = _appArticleContract.Edit(para.ArticleId);
                AppArticleDto dto = new AppArticleDto();
                if (oper.ResultType == OperationResultType.Success)
                {
                    dto = oper.Data as AppArticleDto;
                    entityDto.ArticleTitle = dto.ArticleTitle;
                    if (!string.IsNullOrEmpty(dto.CoverImagePath))
                    {
                        entityDto.CoverImagePath = dto.CoverImagePath;
                    }
                    entityDto.IsRecommend = dto.IsRecommend;
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    List<AppArticleInfo> listAppArticleInfo = js.Deserialize<List<AppArticleInfo>>(para.AppArticleInfos);
                    if (listAppArticleInfo == null)
                    {
                        oper.Message = "暂时无法编辑，请稍后重试";
                        return Json(oper);
                    }
                    else
                    {
                        List<AppArticleItem> listAppArticleItem = new List<AppArticleItem>();
                        StringBuilder sbNum = new StringBuilder();
                        List<string> listNum = new List<string>();
                        IQueryable<AppArticleItem> listItem = _appArticleItemContract.AppArticleItems.Where(x => x.IsDeleted == false && x.IsEnabled == true);
                        if (listItem.Count() > 0)
                        {
                            listNum = listItem.Select(x => x.AppArticleNum).Distinct().ToList();
                        }
                        foreach (AppArticleInfo appArticleInfo in listAppArticleInfo)
                        {
                            if (appArticleInfo.OperationType == (int)OperationFlag.Del)
                            {

                            }
                            else
                            {
                                if (appArticleInfo.OperationType == (int)OperationFlag.Add)
                                {
                                    sbNum.Append(RandomHelper.GetRandomCode(7));
                                    while (listNum.Count > 0)
                                    {
                                        if (listNum.Contains(sbNum.ToString()))
                                        {
                                            sbNum.Clear();
                                            sbNum.Append(RandomHelper.GetRandomCode(7));
                                        }
                                        else
                                        {
                                            listNum.Add(sbNum.ToString());
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    sbNum.Append(appArticleInfo.Num);
                                }
                                int Sequence = appArticleInfo.Sequence;
                                oper = AddImageInfos(appArticleInfo.ImageInfos, sbNum.ToString(), appArticleInfo.TemplateType, dto.Id, Sequence);
                                if (oper.ResultType == OperationResultType.Success)
                                {
                                    List<AppArticleItem> listEntity = oper.Data as List<AppArticleItem>;
                                    listAppArticleItem.AddRange(listEntity);
                                }
                                else
                                {
                                    return Json(oper);
                                }
                                listAppArticleItem.AddRange(AddDynamicPictures(appArticleInfo.DynamicPictures, sbNum.ToString(), appArticleInfo.TemplateType, Sequence));
                                listAppArticleItem.AddRange(AddMaterials(appArticleInfo.Materials, sbNum.ToString(), appArticleInfo.TemplateType, Sequence));
                                listAppArticleItem.AddRange(AddContentInfos(appArticleInfo.ContentInfos, sbNum.ToString(), appArticleInfo.TemplateType, Sequence));
                            }
                            sbNum.Clear();
                        }
                        entityDto.AppArticleItems = listAppArticleItem;
                    }
                    oper = _appArticleContract.Update(entityDto);
                    return Json(oper);
                }
                else
                {
                    return Json(oper);
                }
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                oper.Message = "服务器忙，请稍后重试";
                return Json(oper);
            }
        }
        #endregion

        #region 获取动态图
        /// <summary>
        /// 添加动态图
        /// </summary>
        /// <param name="appArticleInfo"></param>
        /// <returns></returns>
        private List<AppArticleItem> AddDynamicPictures(List<GalleryInfo> dynamicPictures, string strNum, int templateType, int Sequence)
        {
            List<AppArticleItem> listAppArticleItem = new List<AppArticleItem>();
            foreach (GalleryInfo dynamic in dynamicPictures)
            {
                AppArticleItem appArticleItem = new AppArticleItem()
                {
                    Id = dynamic.Id,
                    AppArticleItemType = (int)AppArticleItemFlag.DynamicPicture,
                    GalleryId = dynamic.GalleryId,
                    Position = dynamic.Position,
                    Ratio = dynamic.Ratio,
                    AppArticleNum = strNum,
                    TemplateType = templateType,
                    Sequence = Sequence,
                };
                listAppArticleItem.Add(appArticleItem);
            }
            return listAppArticleItem;
        }

        #endregion

        #region 添加文章图片

        /// <summary>
        /// 添加文章图片
        /// </summary>
        /// <param name="appArticleInfo"></param>
        /// <returns></returns>
        private OperationResult AddImageInfos(List<ArticleImageInfo> imageInfos, string artibleItemNum, int templateType, int? articleId, int sequence)
        {
            var listAppArticleItem = new List<AppArticleItem>();
            var imagePath = ConfigurationHelper.GetAppSetting("ArticleImagePath");

            string suffix = ".png";
            var currentDate = DateTime.Now;
            string strDate = currentDate.Year.ToString() + "/" + currentDate.Month.ToString() + "/" + currentDate.Day.ToString() + "/" + currentDate.Hour.ToString() + "/";
            string imgKey = string.Empty;
            foreach (var itemImg in imageInfos)
            {
                var appArticleItemEntity = new AppArticleItem()
                {
                    AppArticleItemType = (int)AppArticleItemFlag.ArticleImage,
                    Id = itemImg.Id,
                    Position = itemImg.Position,
                    Rotation = itemImg.Rotation,
                    AppArticleNum = artibleItemNum,
                    TemplateType = templateType,
                    Sequence = sequence,
                    AppArticleId = articleId
                };
                imgKey = "Image" + int.Parse(itemImg.Image).ToString();

                // 没有图片
                if (string.IsNullOrEmpty(imgKey) || Request.Files[imgKey] == null
                    || Request.Files[imgKey].InputStream == null
                    || Request.Files[imgKey].InputStream.Length <= 0)
                {
                    appArticleItemEntity.ImagePath = string.Empty;
                }
                else // 批量保存图片
                {
                    //var savePath = string.Format(imagePath + strDate + itemImg.Image.MD5Hash() + suffix);
                    var savePath = string.Format(imagePath + strDate + Guid.NewGuid().ToString("N").MD5Hash() + suffix);

                    bool result = ImageHelper.SaveOriginImg(Request.Files[imgKey].InputStream, savePath);
                    //bool result = ImageHelper.SaveBase64Image(itemImg.Image, sbFileName.ToString());
                    if (result == false)
                    {
                        return new OperationResult(OperationResultType.Error, "上传图片失败");
                    }
                    else
                    {
                        appArticleItemEntity.ImagePath = savePath;
                    }

                }


                listAppArticleItem.Add(appArticleItemEntity);
            }
            return new OperationResult(OperationResultType.Success, "上传成功", listAppArticleItem);
        }


        /// <summary>
        /// 编辑文章图片
        /// </summary>
        /// <param name="appArticleInfo"></param>
        /// <returns></returns>
        private OperationResult EditImageInfos(List<ArticleImageInfo> imageInfos, string strNum)
        {
            List<AppArticleItem> listAppArticleItem = new List<AppArticleItem>();
            string imagePath = ConfigurationHelper.GetAppSetting("ArticleImagePath");
            StringBuilder sbFileName = new StringBuilder();
            string suffix = ".png";
            foreach (ArticleImageInfo imageInfo in imageInfos)
            {
                if (imageInfo.OperationType == (int)OperationFlag.Add)
                {
                    Guid gid = Guid.NewGuid();
                    sbFileName.Append(imagePath + gid.ToString() + suffix);
                    bool result = ImageHelper.SaveBase64Image(imageInfo.Image, sbFileName.ToString());
                    if (result == false)
                    {
                        return new OperationResult(OperationResultType.Error, "上传图片失败");
                    }
                }
                else
                {
                    sbFileName.Append(imageInfo.ImagePath);
                }
                AppArticleItem appArticleItem = new AppArticleItem()
                {
                    Id = imageInfo.Id,
                    AppArticleItemType = (int)AppArticleItemFlag.ArticleImage,
                    Position = imageInfo.Position,
                    Rotation = imageInfo.Rotation,
                    ImagePath = sbFileName.ToString(),
                    AppArticleNum = strNum,
                };
                listAppArticleItem.Add(appArticleItem);
                sbFileName.Clear();
            }
            return new OperationResult(OperationResultType.Success, "上传成功", listAppArticleItem);
        }

        #endregion

        #region 获取素材图
        /// <summary>
        /// 获取素材图
        /// </summary>
        /// <param name="appArticleInfo"></param>
        /// <returns></returns>
        private List<AppArticleItem> AddMaterials(List<GalleryInfo> materials, string strNum, int templateType, int Sequence)
        {
            List<AppArticleItem> listAppArticleItem = new List<AppArticleItem>();
            foreach (GalleryInfo dynamic in materials)
            {
                AppArticleItem appArticleItem = new AppArticleItem()
                {
                    Id = dynamic.Id,
                    AppArticleItemType = (int)AppArticleItemFlag.Material,
                    GalleryId = dynamic.GalleryId,
                    Position = dynamic.Position,
                    Ratio = dynamic.Ratio,
                    AppArticleNum = strNum,
                    TemplateType = templateType,
                    Sequence = Sequence,
                };
                listAppArticleItem.Add(appArticleItem);
            }
            return listAppArticleItem;
        }
        #endregion

        #region 获取文字
        /// <summary>
        /// 获取文字
        /// </summary>
        /// <param name="materials"></param>
        /// <returns></returns>
        private List<AppArticleItem> AddContentInfos(List<ArticleContentInfo> ContentInfos, string strNum, int templateType, int Sequence)
        {
            List<AppArticleItem> listAppArticleItem = new List<AppArticleItem>();
            foreach (ArticleContentInfo contentInfo in ContentInfos)
            {
                AppArticleItem appArticleItem = new AppArticleItem()
                {
                    Id = contentInfo.Id,
                    AppArticleItemType = (int)AppArticleItemFlag.Text,
                    AlignStyle = contentInfo.AlignStyle,
                    Content = contentInfo.Content,
                    FontColor = contentInfo.FontColor,
                    FontSize = contentInfo.FontSize,
                    Position = contentInfo.Position,
                    ContentRow = contentInfo.ContentRow,
                    Rotation = contentInfo.Rotation,
                    Ratio = contentInfo.Ratio,
                    AppArticleNum = strNum,
                    TemplateType = templateType,
                    Sequence = Sequence,
                };
                listAppArticleItem.Add(appArticleItem);
            }
            return listAppArticleItem;
        }
        #endregion

        #region 校验参数
        private OperationResult CheckParameter(ArticlePara para)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error);
            string strArticleTitle = para.ArticleTitle;
            string strCoverImageKey = para.CoverImage;
            string strDetail = para.AppArticleInfos;
            int memberId = para.MemberId;
            AppArticleDto dto = new AppArticleDto();
            dto.MemberId = memberId;
            dto.Id = para.ArticleId;
            dto.IsEffect = para.IsEffect;
            dto.DeviceType = para.DeviceType;
            string strMsg = string.Empty;
            strMsg = this.ArticleTitle(strArticleTitle, dto);
            if (!string.IsNullOrEmpty(strMsg))
            {
                oper.Message = strMsg;
                return oper;
            }
            strMsg = this.CheckConverImage(strCoverImageKey, dto);
            if (!string.IsNullOrEmpty(strMsg))
            {
                oper.Message = strMsg;
                return oper;
            }
            if (string.IsNullOrEmpty(strDetail))
            {
                oper.Message = "添加失败，暂时无法添加，请稍后重试";
                return oper;
            }
            oper.ResultType = OperationResultType.Success;
            oper.Data = dto;
            return oper;
        }

        /// <summary>
        /// 校验文章封面图
        /// </summary>
        /// <param name="strCoverImage"></param>
        /// <param name="dto"></param>
        /// <returns>校验结果</returns>
        private string CheckConverImage(string strCoverImage, AppArticleDto dto)
        {
            string strMsg = string.Empty;
            if (!string.IsNullOrEmpty(strCoverImage))
            {
                DateTime current = DateTime.Now;
                string strSavePath = ConfigurationHelper.GetAppSetting("ArticleCoverImagePath");
                string strDate = current.Year.ToString() + "/" + current.Month.ToString() + "/" + current.Day.ToString() + "/" + current.Hour.ToString() + "/";
                //string strFileName = strCoverImage.MD5Hash() + ".jpg";
                string strFileName = Guid.NewGuid().ToString("N").MD5Hash() + ".jpg";
                strSavePath = strSavePath + strFileName;
                if (Request.Files[strCoverImage] != null && Request.Files[strCoverImage].InputStream.Length <= 0)
                {
                    return "上传封面失败请重试";
                }
                var res = ImageHelper.SaveOriginImg(Request.Files[strCoverImage].InputStream, strSavePath);
                if (!res)
                {
                    strMsg = "上传封面失败请重试";
                }
                else
                {
                    dto.CoverImagePath = strSavePath;
                }
            }
            return strMsg;
        }

        /// <summary>
        /// 校验文章标题
        /// </summary>
        /// <param name="strArticleTitle"></param>
        /// <param name="dto"></param>
        private string ArticleTitle(string strArticleTitle, AppArticleDto dto)
        {
            string strMsg = string.Empty;
            if (string.IsNullOrEmpty(strArticleTitle))
            {
                strMsg = "请填写文章标题";
            }
            else
            {
                strArticleTitle = strArticleTitle.Trim();
                if (strArticleTitle.Length > 25)
                {
                    strMsg = "文章标题长度在1-25个字符";
                }
                else
                {
                    dto.ArticleTitle = strArticleTitle;
                }
            }
            return strMsg;
        }
        #endregion

        #region 获取数据详情
        /// <summary>
        /// 获取编辑数据
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetDetail(int ArticleId)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error);
            try
            {
                AppArticle appArticle = _appArticleContract.AppArticles.Where(x => x.Id == ArticleId).FirstOrDefault();
                if (appArticle == null)
                {
                    oper.Message = "文章不存在";
                    return Json(oper);
                }
                else
                {
                    List<AppArticleItem> listAppArticleItems = appArticle.AppArticleItems.ToList();
                    List<string> listNum = listAppArticleItems.Select(x => x.AppArticleNum).Distinct().ToList();
                    List<AppArticleInfo> AppArticleInfos = new List<AppArticleInfo>();

                    int template = 0;
                    foreach (string strNum in listNum)
                    {
                        #region 初始化数据
                        List<GalleryInfo> DynamicPictures = new List<GalleryInfo>();
                        List<GalleryInfo> Materials = new List<GalleryInfo>();
                        List<ArticleImageInfo> ImageInfos = new List<ArticleImageInfo>();
                        List<ArticleContentInfo> ContentInfos = new List<ArticleContentInfo>();
                        List<AppArticleItem> appArticleItems = listAppArticleItems.Where(x => x.AppArticleNum == strNum).ToList();
                        int Sequence = 0;
                        foreach (AppArticleItem articleItem in appArticleItems)
                        {
                            Sequence = articleItem.Sequence;
                            template = articleItem.TemplateType;
                            if (articleItem.AppArticleItemType == (int)AppArticleItemFlag.DynamicPicture)
                            {
                                GalleryInfo galleryInfo = new GalleryInfo()
                                {
                                    Id = articleItem.Id,
                                    GalleryId = articleItem.GalleryId ?? 0,
                                    Position = articleItem.Position,
                                    Ratio = articleItem.Ratio,
                                    ImagePath = apiUrl + articleItem.Gallery.ThumbnailPath,
                                };
                                DynamicPictures.Add(galleryInfo);
                            }
                            else if (articleItem.AppArticleItemType == (int)AppArticleItemFlag.Material)
                            {
                                GalleryInfo galleryInfo = new GalleryInfo()
                                {
                                    Id = articleItem.Id,
                                    GalleryId = articleItem.GalleryId ?? 0,
                                    Position = articleItem.Position,
                                    Ratio = articleItem.Ratio,
                                    ImagePath = apiUrl + articleItem.Gallery.ThumbnailPath,
                                };
                                Materials.Add(galleryInfo);
                            }
                            else if (articleItem.AppArticleItemType == (int)AppArticleItemFlag.ArticleImage)
                            {
                                ArticleImageInfo imageInfo = new ArticleImageInfo()
                                {
                                    Id = articleItem.Id,
                                    Rotation = articleItem.Rotation,
                                    Position = articleItem.Position,
                                    ImagePath = apiUrl + articleItem.ImagePath,
                                };
                                ImageInfos.Add(imageInfo);
                            }
                            else if (articleItem.AppArticleItemType == (int)AppArticleItemFlag.Text)
                            {
                                ArticleContentInfo contentInfo = new ArticleContentInfo()
                                {
                                    Id = articleItem.Id,
                                    AlignStyle = articleItem.AlignStyle,
                                    Content = articleItem.Content,
                                    FontColor = articleItem.FontColor,
                                    FontSize = articleItem.FontSize,
                                    Position = articleItem.Position,
                                    ContentRow = articleItem.ContentRow,
                                    Rotation = articleItem.Rotation,
                                    Ratio = articleItem.Ratio,
                                };
                                ContentInfos.Add(contentInfo);
                            }
                        }
                        #endregion                         
                        AppArticleInfo articleInfo = new AppArticleInfo()
                        {
                            DynamicPictures = DynamicPictures,
                            Materials = Materials,
                            ImageInfos = ImageInfos,
                            ContentInfos = ContentInfos,
                            TemplateType = template,
                            Num = strNum,
                            Sequence = Sequence,
                        };
                        AppArticleInfos.Add(articleInfo);
                    }
                    Detail detail = new Detail();
                    detail.AppArticleInfos = AppArticleInfos;
                    var data = new
                    {
                        ArticleId = appArticle.Id,
                        appArticle.ArticleTitle,
                        IsEffect = appArticle.IsEffect,
                        CoverImagePath = apiUrl + appArticle.CoverImagePath,
                        Detail = detail,
                        DeviceType = appArticle.DeviceType
                    };
                    return Json(new OperationResult(OperationResultType.Success, "获取成功", data));
                }
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return Json(new OperationResult(OperationResultType.Error, "服务器忙，请稍后重试"));
            }
        }

        #endregion
    }
}