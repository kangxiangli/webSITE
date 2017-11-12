using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Web.Helper;
using Whiskey.Utility;
using Whiskey.Utility.Helper;
using Whiskey.Utility.Data;
using Whiskey.Utility.Extensions;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers.APIEntities.MemberCollo;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Product;
using System.Text;
using Whiskey.ZeroStore.ERP.Transfers.Enum;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Comment;
using System.Data.Entity;
using Whiskey.ZeroStore.ERP.Models.Enums;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    /// <summary>
    /// 会员搭配业务层
    /// </summary>
    public class MemberCollocationService : ServiceBase, IMemberCollocationContract
    {
        #region 初始化数据层操作对象

        private readonly IRepository<MemberCollocation, int> _memberCollocationRepository;

        private readonly IRepository<Member, int> _memberRepository;

        private readonly IRepository<MemberColloEle, int> _memberColloEleRepository;

        private readonly IRepository<Comment, int> _productCommentRepository;

        private readonly IRepository<Approval, int> _productApprovalRepository;

        private readonly IRepository<Color, int> _colorRepository;

        private readonly IRepository<Season, int> _seasonRepository;

        private readonly IRepository<ProductAttribute, int> _productAttrRepository;

        private readonly IRepository<Product, int> _productRepository;

        private readonly IRepository<ColloCalendar, int> _colloCalendarRepository;

        private readonly IRepository<Store, int> _storeRepository;

        private readonly IRepository<MemberSingleProduct, int> _singleProductRepository;

        private readonly IRepository<Gallery, int> _galleryRepository;
        private readonly IRecommendMemberCollocationContract _recommendMemberCollocationContract;
        private readonly IAdministratorContract _administratorContract;

        //日志记录
        protected readonly ILogger _Logger = LogManager.GetLogger(typeof(MemberCollocationService));

        public MemberCollocationService(IRepository<MemberCollocation, int> memberCollocationRepository,
            IRepository<Member, int> memberRepository,
            IRepository<MemberColloEle, int> memberColloEleRepository,
            IRepository<Comment, int> productCommentRepository,
            IRepository<Approval, int> productApprovalRepository,
            IRepository<Color, int> colorRepository,
            IRepository<Season, int> seasonRepository,
            IRepository<ProductAttribute, int> productAttrRepository,
            IRepository<Product, int> productRepository,
            IRepository<ColloCalendar, int> colloCalendarRepository,
            IRepository<Gallery, int> galleryRepository,
            IRepository<Store, int> storeRepository,
            IRepository<MemberSingleProduct, int> singleProductRepository,
            IRecommendMemberCollocationContract recommendMemberCollocationContract,
            IAdministratorContract administratorContract)
            : base(memberCollocationRepository.UnitOfWork)
        {
            _memberCollocationRepository = memberCollocationRepository;
            _memberRepository = memberRepository;
            _memberColloEleRepository = memberColloEleRepository;
            _productCommentRepository = productCommentRepository;
            _productApprovalRepository = productApprovalRepository;
            _colorRepository = colorRepository;
            _seasonRepository = seasonRepository;
            _productAttrRepository = productAttrRepository;
            _productRepository = productRepository;
            _colloCalendarRepository = colloCalendarRepository;
            _galleryRepository = galleryRepository;
            _storeRepository = storeRepository;
            _singleProductRepository = singleProductRepository;
            _recommendMemberCollocationContract = recommendMemberCollocationContract;
            _administratorContract = administratorContract;
        }
        #endregion

        private string apiUrl { get { return ConfigurationHelper.GetAppSetting("ApiUrl"); } }

        private string strWebUrl { get { return ConfigurationHelper.GetAppSetting("WebUrl"); } }

        #region 获取单个数据
        /// <summary>
        /// 获取单个数据
        /// </summary>
        /// <param name="Id">主键ID</param>
        /// <returns></returns>
        public MemberCollocation View(int Id)
        {
            MemberCollocation memberCollocation = _memberCollocationRepository.GetByKey(Id);
            return memberCollocation;
        }

        /// <summary>
        /// 获取单个DTO数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public MemberCollocationDto Edit(int Id)
        {
            var entity = _memberCollocationRepository.GetByKey(Id);
            Mapper.CreateMap<MemberCollocation, MemberCollocationDto>();
            var dto = Mapper.Map<MemberCollocation, MemberCollocationDto>(entity);
            return dto;
        }
        #endregion

        #region 获取数据集

        /// <summary>
        /// 获取数据集
        /// </summary>
        public IQueryable<MemberCollocation> MemberCollocations { get { return _memberCollocationRepository.Entities; } }

        /// <summary>
        /// 获取数据集
        /// </summary>
        /// <param name="memberId">会员Id</param>
        /// <param name="dto">领域模型对象</param>
        /// <param name="operRes">操作信息</param>
        public List<MemberCollo> GetList(int memberId, string strColloName, string strColorId, string strSeasonId, string strProductAttrId, string strSituationId, int pageIndex, int pageSize, out PagedOperationResult operRes)
        {
            List<MemberCollo> list = new List<MemberCollo>();
            try
            {
                Member member = _memberRepository.Entities.Where(x => !x.IsDeleted && x.IsEnabled && x.Id == memberId).FirstOrDefault();
                if (member == null)
                {
                    operRes = new PagedOperationResult(OperationResultType.Error, "登录异常，请重新登录", null);
                }
                else
                {
                    var recommendCount = 0;

                    //分页
                    operRes = new PagedOperationResult(OperationResultType.Success, "获取成功", null);

                    operRes.PageSize = pageSize;

                    List<MemberCollocation> dataList = new List<MemberCollocation>();
                    var recommendQuery = _recommendMemberCollocationContract.Entities.Where(r => !r.IsDeleted && r.IsEnabled && r.MemberId == memberId)
                        .Select(r => r.MemberCollocationId)
                        .ToList();
                    if (recommendQuery.Any())
                    {
                        var query = _memberCollocationRepository.Entities
                            .Where(x => !x.IsDeleted && x.IsEnabled)
                            .Where(m => recommendQuery.Contains(m.Id));
                        //多条件搜素时
                        if (!string.IsNullOrEmpty(strColloName))
                        {
                            query = query.Where(x => x.CollocationName.Contains(strColloName));
                        }
                        if (!string.IsNullOrEmpty(strColorId))
                        {
                            int colorId = int.Parse(strColorId);
                            query = query.Where(x => x.ColorId == colorId);
                        }
                        if (!string.IsNullOrEmpty(strSeasonId))
                        {
                            int seasonId = int.Parse(strSeasonId);
                            query = query.Where(x => x.SeasonId == seasonId);
                        }
                        if (!string.IsNullOrEmpty(strProductAttrId))
                        {

                            var attrs = strProductAttrId.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                            query = query.Where(x => attrs.Contains(x.ProductAttrId));
                        }
                        if (!string.IsNullOrEmpty(strSituationId))
                        {
                            int situationId = int.Parse(strSituationId);
                            query = query.Where(x => x.SituationId == situationId);
                        }
                        recommendCount = query.Count();
                        operRes.AllCount += recommendCount;
                        dataList.AddRange(query.OrderByDescending(m => m.UpdatedTime).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList());
                    }
                    if (dataList.Count < pageSize)
                    {
                        IQueryable<MemberCollocation> listCollo = _memberCollocationRepository.Entities.Where(x => x.MemberId == memberId && !x.IsDeleted && x.IsEnabled);
                        //多条件搜素时
                        if (!string.IsNullOrEmpty(strColloName))
                        {
                            listCollo = listCollo.Where(x => x.CollocationName.Contains(strColloName));
                        }
                        if (!string.IsNullOrEmpty(strColorId))
                        {
                            int colorId = int.Parse(strColorId);
                            listCollo = listCollo.Where(x => x.ColorId == colorId);
                        }
                        if (!string.IsNullOrEmpty(strSeasonId))
                        {
                            int seasonId = int.Parse(strSeasonId);
                            listCollo = listCollo.Where(x => x.SeasonId == seasonId);
                        }
                        if (!string.IsNullOrEmpty(strProductAttrId))
                        {

                            var attrs = strProductAttrId.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                            listCollo = listCollo.Where(x => attrs.Contains(x.ProductAttrId));
                        }
                        if (!string.IsNullOrEmpty(strSituationId))
                        {
                            int situationId = int.Parse(strSituationId);
                            listCollo = listCollo.Where(x => x.SituationId == situationId);
                        }
                        operRes.AllCount += listCollo.Count();
                        dataList.AddRange(listCollo.OrderByDescending(x => x.Id).Skip((pageIndex - 1) * pageSize).Take(pageSize - dataList.Count).ToList());
                    }



                    //获取合成的搭配图
                    IQueryable<MemberColloEle> listColloEle = _memberColloEleRepository.Entities.Where(x => x.EleType == (int)MemberColloEleFlag.ImageEle && x.ParentId == null);
                    IQueryable<Comment> listComment = _productCommentRepository.Entities.Where(x => x.CommentSource == (int)CommentSourceFlag.MemberCollocation && x.MemberId == member.Id);
                    IQueryable<Approval> listApproval = _productApprovalRepository.Entities.Where(x => x.ApprovalSource == (int)CommentSourceFlag.MemberCollocation);
                    for (int i = 0; i < dataList.Count; i++)
                    {
                        var item = dataList[i];
                        MemberCollo collo = new MemberCollo();
                        if (i < recommendCount)
                        {
                            collo.CollocationType = CollocationTypeEnum.Recommend;
                        }
                        else
                        {
                            collo.CollocationType = CollocationTypeEnum.Normal;

                        }
                        collo.ColloId = item.Id;
                        collo.MemberId = member.Id;
                        collo.ColloName = item.CollocationName;
                        collo.Notes = item.Notes;
                        collo.MemberImage = string.IsNullOrEmpty(member.UserPhoto) ? string.Empty : member.UserPhoto;
                        collo.ColloImagePath = listColloEle.Where(x => x.MemberColloId == item.Id).Count() > 0 ? listColloEle.Where(x => x.MemberColloId == item.Id).FirstOrDefault().ImagePath : string.Empty;
                        collo.CommentCount = listComment.Where(x => x.SourceId == item.Id) == null ? 0 : listComment.Where(x => x.SourceId == item.Id).Count();
                        collo.ApproveCount = listApproval.Where(x => x.SourceId == item.Id).Count();
                        collo.IsApprove = listApproval.Where(x => x.SourceId == item.Id && x.MemberId == member.Id).Count() > 0 ? (int)IsApproval.Yes : (int)IsApproval.No;
                        list.Add(collo);
                    }

                }
                return list;
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                operRes = new PagedOperationResult(OperationResultType.Error, "服务器忙，请稍后访问！", null);
                return list;
            }
        }



        /// <summary>
        /// 批量获取会员搭配信息
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public List<MemberCollo> GetList(int memberId)
        {
            List<MemberCollo> list = new List<MemberCollo>();
            try
            {
                var listCollo = _memberCollocationRepository.Entities.Where(x => x.MemberId == memberId && !x.IsDeleted && x.IsEnabled).ToList();


                //获取合成的搭配图
                IQueryable<MemberColloEle> listColloEle = _memberColloEleRepository.Entities.Where(x => x.EleType == (int)MemberColloEleFlag.ImageEle && x.ParentId == null);
                foreach (var item in listCollo)
                {
                    MemberCollo collo = new MemberCollo();
                    collo.ColloId = item.Id;
                    collo.MemberId = item.MemberId;
                    collo.ColloImagePath = listColloEle.Where(x => x.MemberColloId == item.Id).Count() > 0 ? listColloEle.Where(x => x.MemberColloId == item.Id).FirstOrDefault().ImagePath : string.Empty;
                    list.Add(collo);
                }
                return list;
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return new List<MemberCollo>();
            }
        }

        #endregion

        #region 按条件检查数据是否存在
        /// <summary>
        /// 按条件检查数据是否存在
        /// </summary>
        /// <param name="predicate">检查谓语表达式</param>
        /// <param name="id">更新的编号</param>
        /// <returns>是否存在</returns>
        public bool CheckExists(Expression<Func<MemberCollocation, bool>> predicate, int id = 0)
        {
            return _memberCollocationRepository.ExistsCheck(predicate, id);
        }
        #endregion

        #region 添加数据

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dtos">要添加的DTO数据</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Insert(params MemberCollocationDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                OperationResult result = _memberCollocationRepository.Insert(dtos,
                dto =>
                {

                },
                (dto, entity) =>
                {
                    entity.CreatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    return entity;
                });
                return result;
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "添加失败！错误如下：" + ex.Message);
            }
        }

        #endregion

        #region 更新数据
        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="dtos">包含更新数据的DTO数据</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Update(params MemberCollocationDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                OperationResult result = _memberCollocationRepository.Update(dtos,
                    dto =>
                    {

                    },
                    (dto, entity) =>
                    {
                        entity.UpdatedTime = DateTime.Now;
                        entity.OperatorId = AuthorityHelper.OperatorId;
                        return entity;
                    });
                return result;
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                return new OperationResult(OperationResultType.Error, "更新失败！错误如下：" + ex.Message);
            }
        }
        #endregion

        #region 移除数据
        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="ids">要移除的编号</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Remove(params int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _memberCollocationRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _memberCollocationRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "移除成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "移除失败！错误如下：" + ex.Message);
            }
        }


        public OperationResult Remove(int memberId, params int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _memberCollocationRepository.Entities.Where(m => ids.Contains(m.Id) && m.MemberId == memberId);
                if (entities.Count() == 0)
                {
                    return new OperationResult(OperationResultType.Error, "移除的数据不存在");
                }
                foreach (var entity in entities)
                {
                    entity.IsDeleted = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _memberCollocationRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "移除成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return new OperationResult(OperationResultType.Error, "移除失败");
            }
        }
        #endregion

        #region 恢复数据
        /// <summary>
        /// 恢复数据
        /// </summary>
        /// <param name="ids">要恢复的编号</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Recovery(params int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _memberCollocationRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _memberCollocationRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "恢复成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "恢复失败！错误如下：" + ex.Message);
            }
        }
        #endregion

        #region 删除数据
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="ids">要删除的编号</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Delete(params int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                OperationResult result = _memberCollocationRepository.Delete(ids);
                return result;
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "删除失败！错误如下：" + ex.Message);
            }

        }
        #endregion

        #region 启用数据
        /// <summary>
        /// 启用数据
        /// </summary>
        /// <param name="ids">要启用的编号</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Enable(params int[] ids)
        {

            try
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _memberCollocationRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _memberCollocationRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "启用成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "启用失败！错误如下：" + ex.Message);
            }
        }
        #endregion

        #region 禁用数据
        /// <summary>
        /// 禁用数据
        /// </summary>
        /// <param name="ids">要禁用的编号</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Disable(params int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _memberCollocationRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _memberCollocationRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "禁用成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "禁用失败！错误如下：" + ex.Message);
            }
        }
        #endregion

        #region 获取编辑对象
        /// <summary>
        /// 获取编辑对象
        /// </summary>
        /// <param name="memberId">会员Id</param>
        /// <param name="colloId">搭配Id</param>
        /// <returns></returns>
        public OperationResult GetEdit(int memberId, int colloId)
        {
            try
            {
                // 数据校验
                var memberCollocationQuery = MemberCollocations.Where(x => !x.IsDeleted && x.IsEnabled && x.MemberId == memberId && x.Id == colloId);
                if (!memberCollocationQuery.Any())
                {
                    return new OperationResult(OperationResultType.Error, "该搭配不存在！");
                }


                // 获取搭配下所有元素
                var allElements = _memberColloEleRepository.Entities.Where(x => x.MemberColloId == colloId).ToList();

                // 获取搭配下的父级图片素材;
                var parentElementEntity = allElements.Where(x => x.ParentId == null && x.EleType == MemberColloEleFlag.ImageEle).FirstOrDefault();


                // 获取搭配下子级图片素材
                var childElements = allElements.Where(x => x.EleType == MemberColloEleFlag.ImageEle && x.ParentId == parentElementEntity.Id).ToList();


                // 获取搭配下的所有单品素材相关联的productId
                var materialProductIds = allElements.Where(x => x.EleType == MemberColloEleFlag.ImageEle
                                                                         && x.ProductSource == ProductSourceFlag.MaterialProduct
                                                                         && x.ProductId.HasValue && x.ProductId > 0)
                                                        .Select(x => x.ProductId.Value);


                // 图片素材
                var galaryQuery = _galleryRepository.Entities.Where(x => !x.IsDeleted && x.IsEnabled);

                // 获取搭配下的背景图
                var backgroundImgEntity = galaryQuery.Where(x => materialProductIds.Contains(x.Id) && x.GalleryType == GalleryFlag.Background).FirstOrDefault();

              
                int? materialId = backgroundImgEntity == null ? null : allElements.FirstOrDefault(x => x.EleType == MemberColloEleFlag.ImageEle
                                                                        && x.ProductSource == ProductSourceFlag.MaterialProduct
                                                                        && x.ProductId == backgroundImgEntity.Id)?.Id;



                var productQuery = _productRepository.Entities.Where(x => !x.IsDeleted && x.IsEnabled);// 商品
                var colloImgs = new List<ColloImage>();// 图片

                foreach (var imageEle in childElements)
                {

                    var colloImg = new ColloImage()
                    {
                        Id = imageEle.Id,
                        ImagePath = apiUrl + imageEle.ImagePath,
                        Frame = imageEle.EleInfo,
                        Level = imageEle.Level,
                        ProductId = imageEle.ProductId,
                        ProductSource = imageEle.ProductSource,
                        ProductType = imageEle.ProductType,
                        Spin = imageEle.SpinInfo,
                    };
                    // 根据素材类型获取图片的路径
                    if (imageEle.ProductSource == ProductSourceFlag.MaterialProduct && imageEle.ProductId != materialId)
                    {
                        var entity = galaryQuery.Where(x => x.Id == imageEle.ProductId).FirstOrDefault();
                        colloImg.ImagePath = strWebUrl + entity.OriginalPath;
                        colloImgs.Add(colloImg);
                    }
                    else if (imageEle.ProductSource == ProductSourceFlag.MemberProduct)
                    {
                        if (imageEle.ProductType == SingleProductFlag.OrderItem)
                        {
                            var entity = productQuery.Where(x => x.Id == imageEle.ProductId).FirstOrDefault();
                            colloImg.ImagePath = strWebUrl + entity.ThumbnailPath;
                            colloImgs.Add(colloImg);
                        }
                        else if (imageEle.ProductType == SingleProductFlag.Upload)
                        {
                            var entity = _singleProductRepository.Entities.Where(x => x.Id == imageEle.ProductId).FirstOrDefault();
                            if (entity == null)
                            {
                                return OperationResult.Error($"单品信息未找到productId:{imageEle.ProductId}");
                            }
                            colloImg.ImagePath = apiUrl + entity.Image;
                            colloImgs.Add(colloImg);
                        }
                    }
                    else if (imageEle.ProductSource == ProductSourceFlag.UploadProduct)
                    {
                        colloImgs.Add(colloImg);
                    }
                    else if (imageEle.ProductSource == ProductSourceFlag.StoreProduct)
                    {
                        var entity = productQuery.Where(x => x.Id == imageEle.ProductId).FirstOrDefault();
                        colloImg.ImagePath = strWebUrl + entity.ProductCollocationImg ?? entity.ProductOriginNumber.ProductCollocationImg;
                        colloImgs.Add(colloImg);

                    }
                }
                var eleImages = colloImgs.Select(x => new
                {
                    x.Id,
                    ProductSource = x.ProductSource,
                    ProductType = x.ProductType,
                    ProductId = x.ProductId,
                    Level = x.Level,
                    ImagePath = x.ImagePath,
                    Frame = x.Frame,
                    Spin = x.Spin,
                });

                // 文字元素
                var textList = allElements.Where(x => x.EleType == MemberColloEleFlag.TextEle && x.ParentId == parentElementEntity.Id)
                    .Select(x => new ColloText
                    {
                        Id = x.Id,
                        Text = x.TextInfo,
                        Frame = x.EleInfo,
                        Spin = x.SpinInfo,
                        Color = x.TextColor,
                        FontSize = x.TextFont
                    }).ToList();

                // 封面图
                var strImagePath = parentElementEntity == null ? apiUrl + parentElementEntity.ImagePath : string.Empty;
                var backgroundPath = backgroundImgEntity == null ? string.Empty : strWebUrl + backgroundImgEntity.OriginalPath;

                // 获取场合
                var situationQuery = _productAttrRepository.Entities.Where(x => !x.IsDeleted && x.IsEnabled && x.ParentId == 2);
                var colorQuery = _colorRepository.Entities.Where(x => !x.IsDeleted && x.IsEnabled);
                var seasonQuery = _seasonRepository.Entities.Where(x => !x.IsDeleted && x.IsEnabled);
                var data = (from mc in memberCollocationQuery
                            join c in colorQuery on mc.ColorId equals c.Id
                            join s in seasonQuery on mc.SeasonId equals s.Id
                            join si in situationQuery on mc.SituationId equals si.Id
                            select new GetEditRes
                            {
                                Id = mc.Id,
                                ColloId = mc.Id,
                                CollocationName = mc.CollocationName,
                                ColorId = c.Id,
                                ColorName = c.ColorName,
                                ProductAttrId = mc.ProductAttrId,
                                ProductAttrName = string.Empty,
                                SeasonId = mc.SeasonId,
                                SeasonName = s.SeasonName,
                                SituationId = mc.SituationId,
                                SituationName = si.AttributeName,
                                Notes = mc.Notes,
                                MaterialId = materialId,
                                BackGroundId = materialId,
                                BackGroundPath = backgroundPath,
                                ImagePath = strImagePath,
                                ImageId = parentElementEntity.Id,
                                FigureIds = mc.FigureIds
                            }).FirstOrDefault();
                if (!string.IsNullOrEmpty(data.ProductAttrId) && data.ProductAttrId != "(null)")
                {

                    var attrs = data.ProductAttrId.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(i => int.Parse(i));
                    var attrNames = _productAttrRepository.Entities.Where(a => !a.IsDeleted && a.IsEnabled && attrs.Contains(a.Id)).Select(a => a.AttributeName).ToList();
                    if (attrNames != null && attrNames.Count > 0)
                    {
                        data.ProductAttrName = string.Join(",", attrNames);
                    }
                }

                //处理figure信息
                var listFigure = _productAttrRepository.Entities.Where(x => !x.IsDeleted && x.IsEnabled && x.Parent.AttributeName == "体型")
                    .Select(x => new { x.Id, x.AttributeName }).ToList().ToDictionary(x => x.Id);

                var figureDict = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(data.FigureIds) && data.FigureIds != "(null)")
                {
                    data.FigureIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => int.Parse(x))
                        .ToList()
                        .Select(x => new { FigureId = x, FigureName = listFigure[x].AttributeName })
                        .Each(x => figureDict[x.FigureId.ToString()] = listFigure[x.FigureId].AttributeName);

                }
                var entityData = new
                {
                    data.ColloId,
                    data.CollocationName,
                    data.ColorId,
                    data.ColorName,
                    data.ProductAttrId,
                    data.ProductAttrName,
                    data.SeasonId,
                    data.SeasonName,
                    data.SituationId,
                    data.SituationName,
                    data.Notes,
                    data.MaterialId,
                    data.BackGroundId,
                    data.BackGroundPath,
                    data.ImagePath,
                    data.ImageId,
                    Figure = figureDict,
                    ImageList = colloImgs,
                    TextList = textList
                };
                return new OperationResult(OperationResultType.Success, "获取成功！", entityData);
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                _Logger.Error<string>(ex.Message);
                _Logger.Error<string>(ex.StackTrace);
                return new OperationResult(OperationResultType.Error, "服务器忙，请稍后重试！");
            }

        }
        #endregion

        #region 获取数据详情
        /// <summary>
        /// 获取搭配详情
        /// </summary>
        /// <param name="memberId">会员Id</param>
        /// <param name="colloId">搭配Id</param>
        public OperationResult GetDeail(int memberId, int colloId)
        {
            try
            {
                Member member = _memberRepository.Entities.Where(x => !x.IsDeleted && x.IsEnabled && x.Id == memberId).FirstOrDefault();
                if (member == null) return new OperationResult(OperationResultType.LoginError, "登录异常，请冲向登录！");
                MemberCollocation memberCollo = this.MemberCollocations.Where(x => !x.IsDeleted && x.IsEnabled && x.Id == colloId).FirstOrDefault();
                if (memberCollo == null) return new OperationResult(OperationResultType.Error, "搭配不存在！");
                IQueryable<MemberColloEle> listColloEle = _memberColloEleRepository.Entities.Where(x => !x.IsDeleted && x.IsEnabled && x.MemberColloId == colloId && x.EleType == (int)MemberColloEleFlag.ImageEle);
                IQueryable<Comment> listComment = _productCommentRepository.Entities.Where(x => x.CommentSource == (int)CommentSourceFlag.MemberCollocation && x.SourceId == memberCollo.Id);
                IQueryable<Approval> listApproval = _productApprovalRepository.Entities.Where(x => x.ApprovalSource == (int)CommentSourceFlag.MemberCollocation && x.SourceId == memberCollo.Id);
                string strColloImagePath = listColloEle.Where(x => x.ParentId == null).Count() > 0 ? listColloEle.Where(x => x.ParentId == null).FirstOrDefault().ImagePath : string.Empty;
                IQueryable<MemberColloEle> listEle = listColloEle.Where(x => x.ProductSource != ProductSourceFlag.MaterialProduct && x.ProductSource != ProductSourceFlag.UploadProduct);
                List<int> listProductId = new List<int>();
                foreach (var ele in listEle)
                {
                    if (ele.ProductSource == ProductSourceFlag.FansProduct && ele.ProductType == SingleProductFlag.OrderItem)
                    {
                        listProductId.Add(ele.Id);
                    }
                    else if (ele.ProductSource == ProductSourceFlag.MemberProduct && ele.ProductType == SingleProductFlag.OrderItem)
                    {
                        listProductId.Add(ele.Id);
                    }
                    else if (ele.ProductSource == ProductSourceFlag.StoreProduct)
                    {
                        listProductId.Add(ele.ProductId.Value);
                    }
                    else
                    {
                        continue;
                    }
                }
                var listProduct = _productRepository.Entities.Where(x => listProductId.Contains(x.Id)).Select(x => new
                {
                    ProductId = x.Id,
                    Image = strWebUrl + x.ProductCollocationImg ?? x.ProductOriginNumber.ProductCollocationImg,
                    x.ProductOriginNumber.TagPrice
                });
                List<MemberColloEle> listSingleEle = listColloEle.Where(x => x.ProductSource == (int)ProductSourceFlag.MemberProduct && x.ProductType == (int)SingleProductFlag.Upload).ToList();
                IQueryable<MemberSingleProduct> listSingle = _singleProductRepository.Entities.Where(x => !x.IsDeleted && x.IsEnabled);
                var single = (from se in listSingleEle
                              join
                              si in listSingle
                              on
                              se.ProductId equals si.Id
                              select new
                              {
                                  se.ProductId,
                                  Image = apiUrl + si.Image,
                                  si.Price,
                              }).ToList();
                var entity = new
                {
                    ProductId = memberCollo.Id,
                    MemberId = memberCollo.MemberId,
                    MemberName = memberCollo.Member.MemberName,
                    MemberImage = strWebUrl + member.UserPhoto,
                    ColloName = memberCollo.CollocationName,
                    ColloNotes = memberCollo.Notes,
                    ColloImagePath = apiUrl + strColloImagePath,
                    CommentCount = listComment.Count(),
                    ApproveCount = listApproval.Count(),
                    IsApproval = listApproval.Where(x => x.MemberId == member.Id).Count() > 0 ? (int)IsApproval.Yes : (int)IsApproval.No,
                    ListProduct = listProduct,
                    ListSingle = single,
                };
                return new OperationResult(OperationResultType.Success, "获取成功！", entity);
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return new OperationResult(OperationResultType.Error, "获取失败，请重试！");
            }
        }
        #endregion

        #region 添加我的搭配
        public OperationResult Add(MyColl myColl)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error);
            try
            {
                MemberCollocationDto memberCollo = new MemberCollocationDto();
                memberCollo = myColl as MemberCollocationDto;
                oper = this.AddEle(myColl.Image, myColl.ImageList, myColl.TextList, 0, 0);
                if (oper.ResultType == OperationResultType.Success)
                {
                    MemberColloEle ele = oper.Data as MemberColloEle;
                    memberCollo.MemberColloEles.Add(ele);
                    oper = this.Insert(memberCollo);
                    if (myColl.IsCallendar == (int)IsCallendar.Yes)
                    {
                        int[] arrId = oper.Data as int[];
                        int id = arrId[0];
                        ColloCalendar colloCalendar = new ColloCalendar()
                        {
                            ColloId = id,
                            MemberId = myColl.MemberId,
                            CityName = myColl.CityName,
                            Temperature = myColl.Temperature,
                            Weather = myColl.Weather,
                            CollocationTime = DateTime.Now.AddDays(1),
                        };
                        oper = this.AddColloCalendar(colloCalendar);
                    }
                    return oper;
                }
                else
                {
                    return oper;
                }
                #region 注释代码


                //if (insertResult.ResultType==OperationResultType.Success)
                //{
                //    var arrId = (int[])insertResult.Data;
                //    int colloId = arrId[0];
                //    if (myColl.IsCallendar == (int)IsCallendar.Yes)
                //    {
                //        insertResult = this.AddColloCalendar(myColl.MemberId, colloId, myColl.Temperature, myColl.Weather);
                //        if (insertResult.ResultType != OperationResultType.Success) 
                //        {
                //            this.Delete(colloId);
                //            return insertResult;
                //        }
                //    }
                //    int calendarId = ((int[])insertResult.Data)[0];

                //    if (insertResult.ResultType==OperationResultType.Success)
                //    {
                //        return insertResult;
                //    }
                //    else
                //    {
                //        this.Delete(colloId);
                //        _colloCalendarRepository.Delete(calendarId);
                //        return new OperationResult(OperationResultType.Error, "添加搭配信息失败！");
                //    }                          
                //}
                //else
                //{
                //    return insertResult;
                //}
                #endregion
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return new OperationResult(OperationResultType.Error, "服务器忙，请稍后重试！");
            }

        }

        /// <summary>
        /// 添加搭配日历
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="ColloId"></param>
        /// <param name="temperature"></param>
        /// <param name="weather"></param>
        /// <returns></returns>
        private OperationResult AddColloCalendar(ColloCalendar colloCalendar)
        {
            try
            {
                if (string.IsNullOrEmpty(colloCalendar.Temperature) || string.IsNullOrEmpty(colloCalendar.Weather))
                {
                    return new OperationResult(OperationResultType.Error, "天气信息获取失败！");
                }
                else
                {
                    int count = _colloCalendarRepository.Insert(colloCalendar);
                    return count > 0 ? new OperationResult(OperationResultType.Success) : new OperationResult(OperationResultType.Error, "添加失败！");
                    #region 注释代码

                    //int count = _colloCalendarRepository.Insert(colloCalendar);
                    //if (count == 0)
                    //{
                    //    return new OperationResult(OperationResultType.Error, "添加天气信息失败！");
                    //}
                    //else
                    //{
                    //    return new OperationResult(OperationResultType.Success, "添加成功！", colloCalendar.Id);
                    //}
                    #endregion
                }
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return new OperationResult(OperationResultType.Error, "服务器忙，请稍后重试！");
            }
        }

        /// <summary>
        /// 添加搭配元素
        /// </summary>
        /// <param name="MemberColloId"></param>
        /// <param name="Image"></param>
        /// <param name="listColloImage"></param>
        /// <param name="listText"></param>
        /// <returns></returns>
        private OperationResult AddEle(string Image, List<ImageList> listColloImage, List<TextList> listText, int parentId, int colloId)
        {
            try
            {
                OperationResult oper = new OperationResult(OperationResultType.Error);
                //保存路径
                string configPath = ConfigurationHelper.GetAppSetting("SaveMemberCollocation");
                DateTime currnt = DateTime.Now;
                string strDate = currnt.Year.ToString() + "/" + currnt.Month.ToString() + "/" + currnt.Day.ToString() + "/" + DateTime.Now.ToString("HH") + "/";
                StringBuilder sbPath = new StringBuilder();
                StringBuilder sbSaveFileName = new StringBuilder();
                MemberColloEle memberColloEle = new MemberColloEle();
                //文件唯一名称                
                //Guid gid = Guid.NewGuid();
                string savePath = configPath + strDate;
                oper = SaveImage(Image, savePath);
                if (oper.ResultType == OperationResultType.Success)
                {
                    memberColloEle.Id = parentId;
                    memberColloEle.MemberColloId = colloId;
                    memberColloEle.ImagePath = oper.Data.ToString();
                    memberColloEle.EleType = MemberColloEleFlag.ImageEle;
                    if (listColloImage != null && listColloImage.Count > 0)
                    {
                        List<MemberColloEle> listMemberColloEle = new List<MemberColloEle>();
                        foreach (var item in listColloImage)
                        {
                            MemberColloEle mDto = new MemberColloEle()
                            {
                                Id = item.Id,
                                EleInfo = item.Frame,
                                SpinInfo = item.Spin,
                                Level = item.Level,
                                EleType = MemberColloEleFlag.ImageEle,
                                ProductId = item.ProductId,
                                ProductSource = item.ProductSource,
                                ProductType = item.ProductType,
                                ParentId = parentId,
                                ImagePath = item.ImagePath,
                                MemberColloId = colloId,
                                Parent = memberColloEle,
                            };
                            mDto.ImagePath = string.IsNullOrEmpty(item.ImagePath) ? item.ImagePath : item.ImagePath.Replace(apiUrl, string.Empty);
                            if (item.ProductSource == ProductSourceFlag.UploadProduct && !string.IsNullOrEmpty(item.Image))
                            {
                                //文件唯一名称
                                //Guid guid = Guid.NewGuid();
                                //string imageName = item.Image.MD5Hash() + ".png";                                
                                string path = configPath + strDate;
                                oper = SaveImage(item.Image, path);
                                if (oper.ResultType != OperationResultType.Success)
                                {
                                    return new OperationResult(OperationResultType.Error, "上传图片失败");
                                }
                                else
                                {
                                    mDto.ImagePath = oper.Data.ToString();
                                }
                            }
                            listMemberColloEle.Add(mDto);
                        }
                        if (listText != null && listText.Count() > 0)
                        {
                            foreach (var item in listText)
                            {
                                MemberColloEle eleDto = new MemberColloEle();
                                eleDto.Id = item.Id;
                                eleDto.TextColor = item.Color;
                                eleDto.TextFont = item.FontSize;
                                eleDto.TextInfo = item.Text;
                                eleDto.EleInfo = item.Frame;
                                eleDto.SpinInfo = item.Spin;
                                eleDto.MemberColloId = colloId;
                                eleDto.EleType = MemberColloEleFlag.TextEle;
                                listMemberColloEle.Add(eleDto);
                                eleDto.ParentId = parentId;
                                eleDto.Parent = memberColloEle;
                            }
                        }
                        memberColloEle.Children = listMemberColloEle;
                        oper.ResultType = OperationResultType.Success;
                        oper.Data = memberColloEle;
                        return oper;
                    }
                    else
                    {
                        oper.Message = "请选择要上传的图片";
                        return oper;
                    }
                }
                else
                {
                    return oper;
                }
            }
            catch (Exception ex)
            {
                _Logger.Error(ex.ToString());
                return new OperationResult(OperationResultType.Error, "程序异常");
            }

        }

        /// <summary>
        /// 保存上传图片
        /// </summary>
        /// <param name="image"></param>
        /// <param name="path">相对路径</param>
        /// <returns></returns>
        private OperationResult SaveImage(string image, string path)
        {
            try
            {

                if (string.IsNullOrEmpty(image))
                {
                    return new OperationResult(OperationResultType.Error, "请添加需要上传的图片");
                }
                else
                {
                    string fileName = image.MD5Hash();
                    string saveResult = ImageHelper.UploadImage(image, path, fileName, ".png");
                    if (string.IsNullOrEmpty(saveResult))
                    {
                        return new OperationResult(OperationResultType.Error, "上传图片失败！");
                    }
                    else
                    {
                        return new OperationResult(OperationResultType.Success, "上传成功！", saveResult);
                    }
                }

            }
            catch (Exception ex)
            {
                _Logger.Error(ex.ToString());
                return new OperationResult(OperationResultType.Error, "服务器忙，请稍后重试！");
            }

        }
        #endregion

        #region 保存编辑数据
        public OperationResult SaveEdit(ColloInfo colloInfo)
        {
            try
            {
                OperationResult oper = new OperationResult(OperationResultType.Error, "编辑信息不存在");
                MemberCollocationDto dto = (MemberCollocationDto)colloInfo;
                dto.Id = colloInfo.ColloId;
                MemberColloEle parentEle = _memberColloEleRepository.Entities.FirstOrDefault(x => !x.IsDeleted && x.IsEnabled && x.MemberColloId == dto.Id && x.ParentId == null);
                if (parentEle == null) return oper;
                List<ImageList> imageList = new List<ImageList>();
                foreach (var item in colloInfo.ListColloImage)
                {
                    if (item.OperationType != (int)OperationFlag.Del)
                    {
                        ImageList imageInfo = (ImageList)item;
                        imageList.Add(imageInfo);
                    }
                    //ImageList imageInfo = (ImageList)item;
                    //imageList.Add(imageInfo);

                }
                List<TextList> textList = new List<TextList>();
                foreach (var text in colloInfo.ListColloText)
                {
                    if (text.OperationType != (int)OperationFlag.Del)
                    {
                        TextList textInfo = (TextList)text;
                        textList.Add(textInfo);
                    }
                }
                oper = this.AddEle(colloInfo.Image, imageList, textList, parentEle.Id, parentEle.MemberColloId ?? 0);
                if (oper.ResultType == OperationResultType.Success)
                {
                    MemberColloEle ele = oper.Data as MemberColloEle;
                    List<MemberColloEle> listEle = new List<MemberColloEle>();
                    if (ele != null)
                    {
                        listEle.Add(ele);
                        listEle.AddRange(ele.Children);
                    }


                    //dto.MemberColloEles = listEle;
                    //dto.MemberColloEles.Add(ele);
                    //_memberColloEleRepository.Update(ele);
                    //this.Update()
                    var r = this.Edit(dto.Id);
                    r.ColorId = dto.ColorId;
                    r.Notes = dto.Notes;
                    r.SeasonId = dto.SeasonId;
                    r.CollocationName = dto.CollocationName;
                    r.IsCommond = dto.IsCommond;
                    r.ProductAttrId = dto.ProductAttrId;
                    r.SeasonId = dto.SeasonId;
                    r.SituationId = dto.SituationId;
                    //_memberColloEleRepository
                    r.MemberColloEles = listEle;
                    oper = this.Update(r);
                    return oper;
                }
                else
                {
                    return oper;
                }
                #region 注释代码
                //UnitOfWork.TransactionEnabled=true;              
                //MemberCollocationDto dto = colloInfo as MemberCollocationDto;
                //dto.Id = colloInfo.ColloId;
                //OperationResult operationUpdate= this.Update(dto);
                //List<MemberColloEleDto> listAddEle = new List<MemberColloEleDto>();
                //List<MemberColloEleDto> listEditEle = new List<MemberColloEleDto>();                
                //List<int> listDelEle = new List<int>();
                //IQueryable<MemberColloEle> listEle = _memberColloEleRepository.Entities.Where(x => ! x.IsDeleted && x.IsEnabled  && x.MemberColloId==dto.Id && x.ParentId==null);
                //if (listEle.Count() == 0) return new OperationResult(OperationResultType.Error, "编辑信息不存在！");
                //var parentEntity=listEle.FirstOrDefault();
                //operationUpdate= this.SaveImage(colloInfo.Image, parentEntity.ImagePath);
                //if (operationUpdate.ResultType!=OperationResultType.Success)
                //{
                //    return operationUpdate;
                //}
                //int parentId = parentEntity.Id;
                ////保存路径
                //string configPath = ConfigurationHelper.GetAppSetting("SaveMemberCollocation");
                //string strDate=DateTime.Now.ToString("yyyyMMdd")+"/";
                //StringBuilder sbSavePath = new StringBuilder();
                //if (colloInfo.ListColloImage == null) return new OperationResult(OperationResultType.Error, "添加失败！");
                //foreach (var colloImage in colloInfo.ListColloImage)
                //{                     
                //    MemberColloEle memberColloEle = new MemberColloEle();
                //    //文件唯一名称
                //    Guid gid = Guid.NewGuid();
                //    sbSavePath.Append(configPath + strDate + gid.ToString() + ".png");
                //    MemberColloEleDto colloEle = new MemberColloEleDto();
                //    colloEle.Id = colloImage.Id;
                //    colloEle.ImagePath = colloImage.ImagePath;
                //    colloEle.EleType = (int)MemberColloEleFlag.ImageEle;
                //    colloEle.ParentId = parentId;
                //    colloEle.MemberColloId = dto.Id;
                //    colloEle.ProductId = colloImage.ProductId;
                //    colloEle.ProductSource = colloImage.ProductSource;
                //    colloEle.ProductType = colloImage.ProductType;
                //    colloEle.EleInfo = colloImage.Frame;
                //    colloEle.Level = colloImage.Level;
                //    colloEle.SpinInfo = colloImage.Spin;
                //    colloEle.ImagePath = colloImage.ImagePath;                     
                //    if (colloImage.OperationType == (int)OperationFlag.Add)
                //    {
                //        if (colloImage.ProductSource==(int)ProductSourceFlag.UploadProduct)
                //        {
                //            OperationResult uploadResult = this.SaveImage(colloImage.Image, sbSavePath.ToString());
                //            if (uploadResult.ResultType == OperationResultType.Success)
                //            {
                //                colloEle.ImagePath = sbSavePath.ToString();                                
                //            }
                //            else
                //            {
                //                return uploadResult;
                //            }
                //        }
                //        listAddEle.Add(colloEle);

                //    }
                //    else if (colloImage.OperationType == (int)OperationFlag.Edit)
                //    {                        
                //        listEditEle.Add(colloEle);
                //    }
                //    else if (colloImage.OperationType == (int)OperationFlag.Del)
                //    {
                //        listDelEle.Add(colloImage.Id);
                //    }
                //    sbSavePath.Clear();
                //}
                //if (colloInfo.ListColloText!=null)
                //{
                //    foreach (var colloText in colloInfo.ListColloText)
                //    {
                //        MemberColloEleDto colloEle = new MemberColloEleDto()
                //        {
                //            Id = colloText.Id,
                //            EleType = (int)MemberColloEleFlag.TextEle,
                //            ParentId = parentId,
                //            MemberColloId = dto.Id,
                //            SpinInfo = colloText.Spin,
                //            EleInfo = colloText.Frame,
                //            TextColor = colloText.Color,
                //            TextFont = colloText.FontSize,
                //            TextInfo = colloText.Text,

                //        };
                //        if (colloText.OperationType == (int)OperationFlag.Add)
                //        {
                //            listAddEle.Add(colloEle);
                //        }
                //        else if (colloText.OperationType == (int)OperationFlag.Edit)
                //        {
                //            listEditEle.Add(colloEle);
                //        }
                //        else if (colloText.OperationType == (int)OperationFlag.Del)
                //        {
                //            listDelEle.Add(colloText.Id);
                //        }                       
                //        else
                //        {
                //            return new OperationResult(OperationResultType.Error, "操作类型错误！");
                //        }
                //    }
                //}
                //if (listAddEle!=null && listAddEle.Count>0)
                //{
                //    AddEle(listAddEle.ToArray());
                //}
                //if (listEditEle!=null && listEditEle.Count>0)
                //{
                //    UpdateEle(listEditEle.ToArray());
                //}
                //if (listDelEle!=null && listDelEle.Count>0)
                //{
                //    DelEle(listDelEle.ToArray());
                //}              
                //int count = UnitOfWork.SaveChanges();
                //return count > 0 ? new OperationResult(OperationResultType.Success, "编辑成功！") : new OperationResult(OperationResultType.Error, "编辑失败！");
                #endregion
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                _Logger.Error<string>(ex.ToString());
                return new OperationResult(OperationResultType.Error, "服务器忙，请稍后重试！");
            }
        }

        #region 注释代码

        //private OperationResult AddEle(params MemberColloEleDto[] dtos)
        //{
        //    try
        //    {
        //        dtos.CheckNotNull("dtos");
        //        OperationResult result = _memberColloEleRepository.Insert(dtos,
        //        dto =>
        //        {

        //        },
        //        (dto, entity) =>
        //        {
        //            entity.CreatedTime = DateTime.Now;
        //            return entity;
        //        });
        //        return result;
        //    }
        //    catch (Exception ex)
        //    {
        //        _Logger.Error<string>(ex.ToString());
        //        return new OperationResult(OperationResultType.Error, "编辑失败！");
        //    }
        //}

        //private OperationResult UpdateEle(params MemberColloEleDto[] dtos)
        //{
        //    try
        //    {
        //        dtos.CheckNotNull("dtos");
        //        OperationResult result = _memberColloEleRepository.Update(dtos,
        //            dto =>
        //            {

        //            },
        //            (dto, entity) =>
        //            {
        //                entity.UpdatedTime = DateTime.Now;
        //                //dto.ImagePath = entity.ImagePath;
        //                return entity;
        //            });
        //        return result;
        //    }
        //    catch (Exception ex)
        //    {
        //        _Logger.Error<string>(ex.ToString());
        //        return new OperationResult(OperationResultType.Error, "更新失败！" );
        //    }
        //}

        //private OperationResult DelEle(params int[] ids)
        //{
        //    try
        //    {
        //        ids.CheckNotNull("ids");
        //        OperationResult result = _memberColloEleRepository.Delete(ids);
        //        return result;
        //    }
        //    catch (Exception ex)
        //    {
        //        return new OperationResult(OperationResultType.Error, "删除失败！错误如下：" + ex.Message);
        //    }
        //}
        #endregion
        #endregion

        #region 设为推荐
        public OperationResult Recommend(int[] Ids)
        {
            try
            {
                Ids.CheckNotNull("Ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _memberCollocationRepository.Entities.Where(m => Ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsRecommend = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _memberCollocationRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "推荐成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "推荐失败！错误如下：" + ex.Message);
            }
        }

        #endregion

        #region 取消推荐
        public OperationResult CancleRecommend(int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _memberCollocationRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsRecommend = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _memberCollocationRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "取消推荐成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "取消推荐失败！错误如下：" + ex.Message);
            }
        }
        #endregion


        /// <summary>
        /// 将一个搭配推荐给多个会员
        /// </summary>
        /// <param name="memberCollocationId"></param>
        /// <param name="recommendMemberIds"></param>
        /// <returns></returns>
        public OperationResult SaveRecommendMemberId(int memberCollocationId, params int[] recommendMemberIds)
        {
            var entity = _memberCollocationRepository.Entities.Where(e => !e.IsDeleted && e.IsEnabled && e.Id == memberCollocationId).Include(e => e.RecommendMembers)
                .FirstOrDefault();
            if (entity == null)
            {
                return OperationResult.Error("搭配数据不存在");
            }

            // 校验搭配是否是当前用户的
            var memberId = _administratorContract.GetMemberId(AuthorityHelper.OperatorId.Value);

            var recommendMembers = entity.RecommendMembers.ToList();
            if (recommendMemberIds == null || recommendMemberIds.Length <= 0)
            {
                //清空
                var res = _recommendMemberCollocationContract.Delete(recommendMembers.ToArray());
                return res;
            }
            else
            {
                // 禁止推荐给自己
                if (recommendMemberIds.Contains(memberId))
                {
                    return OperationResult.Error("无法将会员搭配推荐给会员自己");
                }
                var ids = entity.RecommendMembers.Select(m => m.MemberId).ToList();
                // 移除操作
                _recommendMemberCollocationContract.Delete(recommendMembers.ToArray());
                // add
                var list = recommendMemberIds.Select(mid => new RecommendMemberCollocation()
                {

                    MemberCollocationId = memberCollocationId,
                    MemberId = mid
                }).ToArray();

                var res = _recommendMemberCollocationContract.Insert(list);

                return res;
            }
        }


        /// <summary>
        /// 给一个会员推荐多个搭配
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="recommendCollocationIds"></param>
        /// <returns></returns>
        public OperationResult SaveRecommendCollocationId(int memberId, params int[] recommendCollocationIds)
        {
            var adminMemberId = _administratorContract.GetMemberId(AuthorityHelper.OperatorId.Value);
            if (adminMemberId == memberId)
            {
                return OperationResult.Error("无法向自己推荐搭配");
            }

            var historyData = _recommendMemberCollocationContract.Entities.Where(m => m.MemberId == memberId).ToArray();
            if (recommendCollocationIds == null || recommendCollocationIds.Length <= 0)
            {
                //清空
                var res = _recommendMemberCollocationContract.Delete(historyData);
                return res;
            }
            else
            {
                var memberIds = _memberCollocationRepository.Entities.Where(r => !r.IsDeleted && r.IsEnabled && recommendCollocationIds.Contains(r.Id)).Select(m => m.MemberId).ToList();
                if (memberIds.Count <= 0 || memberIds.Count != recommendCollocationIds.Length)
                {
                    return OperationResult.Error("搭配信息不存在");
                }

                // 搭配归属会员校验
                if (memberIds.Any(m => m != adminMemberId))
                {
                    return OperationResult.Error("检测到不属于当前用户会员创建的搭配信息");
                }


                // 移除
                _recommendMemberCollocationContract.Delete(historyData);
                // add
                var list = recommendCollocationIds.Select(cid => new RecommendMemberCollocation()
                {

                    MemberCollocationId = cid,
                    MemberId = memberId
                }).ToArray();

                var res = _recommendMemberCollocationContract.Insert(list);

                return res;
            }
        }

        private class GetEditRes
        {
            public int Id { get; set; }
            public int ColorId { get; set; }
            public string ColorName { get; set; }
            public int ColloId { get; set; }
            public string CollocationName { get; set; }
            public string ProductAttrId { get; set; }
            public string ProductAttrName { get; set; }
            public int SeasonId { get; set; }
            public string SeasonName { get; set; }
            public int SituationId { get; set; }
            public string SituationName { get; set; }
            public string Notes { get; set; }
            public int? MaterialId { get; set; }
            public int? BackGroundId { get; set; }
            public string BackGroundPath { get; set; }
            public string ImagePath { get; set; }
            public int ImageId { get; set; }
            public string FigureIds { get; set; }
        }
    }
}
