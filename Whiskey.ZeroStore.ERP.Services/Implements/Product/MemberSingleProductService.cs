using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using AutoMapper;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Web.Helper;
using Whiskey.Web.SignalR;
using Whiskey.Web.Http;
using Whiskey.Web.Extensions;
using Whiskey.Utility;
using Whiskey.Utility.Helper;
using Whiskey.Utility.Data;
using Whiskey.Utility.Web;
using Whiskey.Utility.Class;
using Whiskey.Utility.Extensions;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using System.Security.Cryptography;
using System.Text;
using Whiskey.Utility.Secutiry;
using System.Web.Mvc;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Product;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Transfers.Enum;
using Whiskey.ZeroStore.ERP.Transfers.ProductInfo;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Order;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Base;
using Whiskey.ZeroStore.ERP.Models.Entities;
using Whiskey.ZeroStore.ERP.Models.Enums;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    /// <summary>
    /// 单品
    /// </summary>
    public class MemberSingleProductService : ServiceBase, IMemberSingleProductContract
    {
        #region 声明业务层操作对象

        //日志记录
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(MemberSingleProductService));

        private readonly IRepository<MemberSingleProduct, int> _memberSingleProductRepository;

        private readonly IRepository<Comment, int> _productCommentRepository;

        private readonly IRepository<Approval, int> _productApprovalRepository;

        private readonly IRepository<Product, int> _productRepository;

        private readonly IRepository<Category, int> _categoryRepository;

        private readonly IRepository<Size, int> _sizeRepository;

        private readonly IRepository<Color, int> _colorRepository;

        private readonly IRepository<Season, int> _seasonRepository;

        private readonly IRepository<ProductAttribute, int> _productAttrRepository;

        private readonly IRepository<MemberColloEle, int> _memberColloEleRepository;
        private readonly IRepository<Retail, int> _retailRepo;
        private readonly IRecommendMemberSingleProductContract _recommendMemberSingleProductContract;

        public MemberSingleProductService(
            IRepository<MemberSingleProduct, int> memberSingleProductRepository,
            IRepository<Comment, int> productCommentRepository,
            IRepository<Approval, int> productApprovalRepository,
            IRepository<Product, int> productRepository,
            IRepository<Category, int> categoryRepository,
            IRepository<Size, int> sizeRepository,
            IRepository<Color, int> colorRepository,
            IRepository<Season, int> seasonRepository,
            IRepository<ProductAttribute, int> productAttrRepository,
            IRepository<MemberColloEle, int> memberColloEleRepository,
            IRepository<Retail, int> retailRepo,
            IRecommendMemberSingleProductContract recommendMemberSingleProductContract)
            : base(memberSingleProductRepository.UnitOfWork)
        {
            _memberSingleProductRepository = memberSingleProductRepository;
            _productCommentRepository = productCommentRepository;
            _productApprovalRepository = productApprovalRepository;
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _sizeRepository = sizeRepository;
            _colorRepository = colorRepository;
            _seasonRepository = seasonRepository;
            _productAttrRepository = productAttrRepository;
            _memberColloEleRepository = memberColloEleRepository;
            _retailRepo = retailRepo;
            _recommendMemberSingleProductContract = recommendMemberSingleProductContract;
        }
        #endregion

        #region 根据Id获取数据
        /// <summary>
        /// 获取单个数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns>数据实体</returns>
        public MemberSingleProduct View(int Id)
        {
            var entity = _memberSingleProductRepository.GetByKey(Id);
            return entity;
        }
        #endregion

        #region 根据Id获取数据
        /// <summary>
        /// 获取单个DTO数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns>数据实体模型</returns>
        public MemberSingleProductDto Edit(int Id)
        {
            var entity = _memberSingleProductRepository.GetByKey(Id);
            Mapper.CreateMap<MemberSingleProduct, MemberSingleProductDto>();
            var dto = Mapper.Map<MemberSingleProduct, MemberSingleProductDto>(entity);
            return dto;
        }
        #endregion

        #region 获取数据集
        /// <summary>
        /// 获取数据集
        /// </summary>
        public IQueryable<MemberSingleProduct> MemberSingleProducts { get { return _memberSingleProductRepository.Entities; } }

        /// <summary>
        /// 获取单品集合
        /// </summary>
        /// <param name="memberId">会员Id</param>
        /// <param name="PageIndex">页码</param>
        /// <param name="PageSize">每页显示数据量</param>
        public void GetList(int memberId, int PageIndex, int PageSize)
        {
            try
            {

            }
            catch (Exception)
            {

                throw;
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
        public bool CheckExists(Expression<Func<MemberSingleProduct, bool>> predicate, int id = 0)
        {
            return _memberSingleProductRepository.ExistsCheck(predicate, id);
        }
        #endregion

        #region 添加数据
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dtos">要添加的DTO数据</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Insert(params MemberSingleProductDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                foreach (var dto in dtos)
                {
                    bool isHave = _memberSingleProductRepository.Entities.Any(x => x.MemberId == dto.MemberId
                                                                                && !string.IsNullOrEmpty(x.ProductName)
                                                                                && x.ProductName == dto.ProductName
                                                                                && x.IsDeleted == false
                                                                                && x.IsEnabled == true);
                    if (isHave == true)
                    {
                        return new OperationResult(OperationResultType.Error, "名称重复");
                    }
                }
                OperationResult result = _memberSingleProductRepository.Insert(dtos,
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

        #region 修改数据
        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="dtos">包含更新数据的DTO数据</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Update(params MemberSingleProductDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                IQueryable<MemberSingleProduct> listMemeber = MemberSingleProducts;
                foreach (var dto in dtos)
                {
                    MemberSingleProduct memberCollocation = listMemeber.Where(x => x.ProductName == dto.ProductName && dto.MemberId == x.MemberId).FirstOrDefault();
                    if (memberCollocation != null && memberCollocation.Id != dto.Id)
                    {
                        return new OperationResult(OperationResultType.Error, "名称已经存在！");
                    }
                }
                OperationResult result = _memberSingleProductRepository.Update(dtos,
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
            catch (Exception ex)
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
                var entities = _memberSingleProductRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _memberSingleProductRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "移除成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "移除失败！错误如下：" + ex.Message);
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
                var entities = _memberSingleProductRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _memberSingleProductRepository.Update(entity);
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
                OperationResult result = _memberSingleProductRepository.Delete(ids);
                return result;
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "删除失败！错误如下：" + ex.Message);
            }

        }

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="memberId">会员Id</param>
        /// <param name="id">标识Id</param>
        /// <returns></returns>
        public OperationResult Delete(int memberId, int id)
        {
            try
            {
                //开启事务
                base.UnitOfWork.TransactionEnabled = true;
                IQueryable<MemberSingleProduct> listPro = _memberSingleProductRepository.Entities.Where(x => x.Id == id && x.MemberId == memberId);
                if (listPro.Count() > 0)
                {
                    MemberSingleProduct pro = listPro.FirstOrDefault();
                    pro.IsDeleted = true;
                    pro.UpdatedTime = DateTime.Now;
                    _memberSingleProductRepository.Update(pro);
                    int count = base.UnitOfWork.SaveChanges();
                    if (count > 0)
                    {
                        return new OperationResult(OperationResultType.Success, "删除成功！");
                    }
                    else
                    {
                        return new OperationResult(OperationResultType.Error, "删除失败！");
                    }
                }
                else
                {
                    return new OperationResult(OperationResultType.Error, "删除失败！");
                }
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return new OperationResult(OperationResultType.Error, "服务器忙，请稍后重试！");
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
                var entities = _memberSingleProductRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _memberSingleProductRepository.Update(entity);
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
                var entities = _memberSingleProductRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _memberSingleProductRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "禁用成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "禁用失败！错误如下：" + ex.Message);
            }
        }
        #endregion

        #region 获取数据键值对
        /// <summary>
        /// 获取数据键值对
        /// </summary>
        /// <param name="title">默认显示标题</param>
        /// <returns></returns>
        public IEnumerable<SelectListItem> SelectList(string title)
        {

            List<SelectListItem> list = new List<SelectListItem>();
            IQueryable<MemberSingleProduct> listMember = MemberSingleProducts.Where(x => x.IsEnabled == true && x.IsDeleted == false);
            if (listMember.Count() > 0)
            {

                foreach (var memeber in listMember)
                {
                    //list.Add(new SelectListItem() { Text = memeber.ColorName, Value = memeber.Id.ToString() });
                }
            }
            list.Insert(0, new SelectListItem() { Text = title, Value = "" });
            return list;
        }
        #endregion

        #region 获取单品和已购买商品集合

        private List<MemberProductInfo> GetSingleProductByFlag(SingleProductFlag flag, int memberId, int? strColorId, int? strProductAttrId, int? strCategoryId, int PageIndex, int PageSize)
        {

            switch (flag)
            {
                case SingleProductFlag.Upload:
                    {
                        var query = _memberSingleProductRepository.Entities.Where(x => !x.IsDeleted && x.IsEnabled && x.MemberId == memberId);
                        if (strColorId.HasValue)
                        {
                            query = query.Where(q => q.ColorId == strColorId);
                        }
                        if (strProductAttrId.HasValue)
                        {
                            query = query.Where(q => q.ProductAttrId.Contains(strProductAttrId.ToString()));
                        }
                        if (strCategoryId.HasValue)
                        {
                            query = query.Where(q => q.Category.ParentId.Value == strCategoryId);
                        }

                        var orderInfo = query.OrderByDescending(i => i.UpdatedTime)
                            .Select(i => new MemberProductInfo
                            {
                                CategoryName = i.Category.CategoryName,
                                SeasonName = i.Season.SeasonName,
                                SizeName = i.Size.SizeName,
                                Price = i.Price,
                                ProductId = i.Id,   //单品id
                                ProductType = (int)SingleProductFlag.Upload,
                                CreateTime = i.CreatedTime,
                                CoverImagePath = i.CoverImage,
                                ImagePath = i.Image,
                                ColorId = i.ColorId,
                                SeasonId = i.SeasonId,
                                SizeId = i.SizeId,
                                ColorName = i.Color.ColorName,
                                ColorIconPath = i.Color.IconPath,
                                CategoryId = i.CategoryId
                            }).Skip((PageIndex - 1) * PageSize).Take(PageSize).ToList();

                        return orderInfo;
                    }
                    break;

                case SingleProductFlag.OrderItem:
                    {
                        // 查询会员已购买的商品记录
                        var listOrder = _retailRepo.Entities.Where(e => !e.IsDeleted && e.IsEnabled)
                                        .Where(e => e.RetailState == RetailStatus.正常 && e.ConsumerId.Value == memberId);
                        if (!listOrder.Any())
                        {
                            return new List<MemberProductInfo>();
                        }

                        var query = listOrder.SelectMany(o => o.RetailItems);
                        if (strColorId.HasValue)
                        {
                            query = query.Where(q => q.Product.ColorId == strColorId);
                        }
                        if (strProductAttrId.HasValue)
                        {
                            query = query.Where(q => q.Product.ProductOriginNumber.ProductAttributes.Any(p => p.Id == strProductAttrId));
                        }
                        if (strCategoryId.HasValue)
                        {
                            query = query.Where(q => q.Product.ProductOriginNumber.Category.ParentId.Value == strCategoryId);
                        }

                        var orderInfo = query.OrderByDescending(i => i.UpdatedTime)
                            .Select(i => new MemberProductInfo
                            {
                                BigProdNumber = i.Product.ProductOriginNumber.BigProdNum,
                                CategoryName = i.Product.ProductOriginNumber.Category.CategoryName,
                                SeasonName = i.Product.ProductOriginNumber.Season.SeasonName,
                                SizeName = i.Product.Size.SizeName,
                                Price = i.ProductTagPrice,
                                ProductId = i.ProductId,
                                ProductType = (int)SingleProductFlag.OrderItem,
                                CreateTime = i.CreatedTime,
                                CoverImagePath = i.Product.ProductOriginNumber.ProductCollocationImg,
                                ImagePath = i.Product.ProductOriginNumber.ProductCollocationImg,
                                ColorId = i.Product.ColorId,
                                SeasonId = i.Product.ProductOriginNumber.SeasonId,
                                SizeId = i.Product.SizeId,
                                ColorName = i.Product.Color.ColorName,
                                ColorIconPath = i.Product.Color.IconPath,
                                CategoryId = i.Product.ProductOriginNumber.CategoryId
                            }).Skip((PageIndex - 1) * PageSize).Take(PageSize).ToList();


                        return orderInfo;


                    }
                    break;
                case SingleProductFlag.Recommend:
                    {
                        // 查询推荐的单品
                        var listRecommend = _recommendMemberSingleProductContract.Entities.Where(e => !e.IsDeleted && e.IsEnabled && e.MemberId == memberId);
                        if (!listRecommend.Any())
                        {
                            return new List<MemberProductInfo>();
                        }


                        var query = listRecommend.SelectMany(e => e.ProductOriginNumber.Products.Where(p => p.ColorId == e.ColorId));
                        if (strColorId.HasValue)
                        {
                            query = query.Where(q => q.ColorId == strColorId);
                        }
                        if (strProductAttrId.HasValue)
                        {
                            query = query.Where(q => q.ProductOriginNumber.ProductAttributes.Any(p => p.Id == strProductAttrId));
                        }
                        if (strCategoryId.HasValue)
                        {
                            query = query.Where(q => q.ProductOriginNumber.Category.ParentId.Value == strCategoryId);
                        }
                        var recommendInfo = query.GroupBy(e => e.BigProdNum)
                                                .Select(e => new
                                                {
                                                    BigProdNumber = e.FirstOrDefault().ProductOriginNumber.BigProdNum,
                                                    CategoryName = e.FirstOrDefault().ProductOriginNumber.Category.CategoryName,
                                                    SeasonName = e.FirstOrDefault().ProductOriginNumber.Season.SeasonName,
                                                    SizeName = e.FirstOrDefault().Size.SizeName,
                                                    Price = e.FirstOrDefault().ProductOriginNumber.TagPrice,
                                                    ProductId = e.FirstOrDefault().Id,
                                                    ProductType = (int)SingleProductFlag.Recommend,
                                                    CreateTime = e.FirstOrDefault().CreatedTime,
                                                    CoverImagePath = e.FirstOrDefault().ProductOriginNumber.ProductCollocationImg,
                                                    ImagePath = e.FirstOrDefault().ProductOriginNumber.ProductCollocationImg,
                                                    ColorId = e.FirstOrDefault().ColorId,
                                                    SeasonId = e.FirstOrDefault().ProductOriginNumber.SeasonId,
                                                    SizeId = e.FirstOrDefault().SizeId,
                                                    ColorName = e.FirstOrDefault().Color.ColorName,
                                                    ColorIconPath = e.FirstOrDefault().Color.IconPath,
                                                    CategoryId = e.FirstOrDefault().ProductOriginNumber.CategoryId
                                                }).OrderByDescending(e => e.CreateTime).Skip((PageIndex - 1) * PageSize).Take(PageSize).ToList()
                                                .Select(e => new MemberProductInfo
                                                {
                                                    BigProdNumber = e.BigProdNumber,
                                                    CategoryName = e.CategoryName,
                                                    SeasonName = e.SeasonName,
                                                    SizeName = e.SizeName,
                                                    Price = (decimal)e.Price,
                                                    ProductId = e.ProductId,
                                                    ProductType = e.ProductType,
                                                    CreateTime = e.CreateTime,
                                                    CoverImagePath = e.CoverImagePath,
                                                    ImagePath = e.ImagePath,
                                                    ColorId = e.ColorId,
                                                    SeasonId = e.SeasonId,
                                                    SizeId = e.SizeId,
                                                    ColorName = e.ColorName,
                                                    ColorIconPath = e.ColorIconPath,
                                                    CategoryId = e.CategoryId
                                                }).ToList();
                        return recommendInfo;

                    }
                    break;
                default:

                    break;
            }
            return null;
        }
        public List<MemberProductInfo> GetAllList(SingleProductFlag? flag, int memberId, int? strColorId, int? strProductAttrId, int? strCategoryId, int PageIndex, int PageSize)
        {
            try
            {
                var listProInfo = new List<MemberProductInfo>();
                if (flag.HasValue && flag.Value != SingleProductFlag.All)
                {
                    var res = GetSingleProductByFlag(flag.Value, memberId, strColorId, strProductAttrId, strCategoryId, PageIndex, PageSize);
                    return res;
                }
                // 查询推荐的单品
                var listRecommend = _recommendMemberSingleProductContract.Entities.Where(e => !e.IsDeleted && e.IsEnabled && e.MemberId == memberId);
                if (listRecommend.Any())
                {
                    var query = listRecommend.SelectMany(e => e.ProductOriginNumber.Products.Where(p => p.ColorId == e.ColorId));
                    if (strColorId.HasValue)
                    {
                        query = query.Where(q => q.ColorId == strColorId);
                    }
                    if (strProductAttrId.HasValue)
                    {
                        query = query.Where(q => q.ProductOriginNumber.ProductAttributes.Any(p => p.Id == strProductAttrId));
                    }
                    if (strCategoryId.HasValue)
                    {
                        query = query.Where(q => q.ProductOriginNumber.Category.ParentId.Value == strCategoryId);
                    }
                    var recommendInfo = query.GroupBy(e => e.BigProdNum)
                                            .Select(e => new
                                            {
                                                BigProdNumber = e.FirstOrDefault().ProductOriginNumber.BigProdNum,
                                                CategoryName = e.FirstOrDefault().ProductOriginNumber.Category.CategoryName,
                                                SeasonName = e.FirstOrDefault().ProductOriginNumber.Season.SeasonName,
                                                SizeName = e.FirstOrDefault().Size.SizeName,
                                                Price = e.FirstOrDefault().ProductOriginNumber.TagPrice,
                                                ProductId = e.FirstOrDefault().Id,
                                                ProductType = (int)SingleProductFlag.Recommend,
                                                CreateTime = e.FirstOrDefault().CreatedTime,
                                                CoverImagePath = e.FirstOrDefault().ProductOriginNumber.ProductCollocationImg,
                                                ImagePath = e.FirstOrDefault().ProductOriginNumber.ProductCollocationImg,
                                                ColorId = e.FirstOrDefault().ColorId,
                                                SeasonId = e.FirstOrDefault().ProductOriginNumber.SeasonId,
                                                SizeId = e.FirstOrDefault().SizeId,
                                                ColorName = e.FirstOrDefault().Color.ColorName,
                                                ColorIconPath = e.FirstOrDefault().Color.IconPath,
                                                CategoryId = e.FirstOrDefault().ProductOriginNumber.CategoryId
                                            }).OrderByDescending(e => e.CreateTime).Skip((PageIndex - 1) * PageSize).Take(PageSize).ToList()
                                            .Select(e => new MemberProductInfo
                                            {
                                                BigProdNumber = e.BigProdNumber,
                                                CategoryName = e.CategoryName,
                                                SeasonName = e.SeasonName,
                                                SizeName = e.SizeName,
                                                Price = (decimal)e.Price,
                                                ProductId = e.ProductId,
                                                ProductType = e.ProductType,
                                                CreateTime = e.CreateTime,
                                                CoverImagePath = e.CoverImagePath,
                                                ImagePath = e.ImagePath,
                                                ColorId = e.ColorId,
                                                SeasonId = e.SeasonId,
                                                SizeId = e.SizeId,
                                                ColorName = e.ColorName,
                                                ColorIconPath = e.ColorIconPath,
                                                CategoryId = e.CategoryId
                                            }).ToList();
                    listProInfo.AddRange(recommendInfo);
                }



                // 查询会员已购买的商品记录
                var listOrder = _retailRepo.Entities.Where(e => !e.IsDeleted && e.IsEnabled)
                                .Where(e => e.RetailState == RetailStatus.正常 && e.ConsumerId.Value == memberId);
                if (listOrder.Any())
                {
                    var query = listOrder.SelectMany(o => o.RetailItems);
                    if (strColorId.HasValue)
                    {
                        query = query.Where(q => q.Product.ColorId == strColorId);
                    }
                    if (strProductAttrId.HasValue)
                    {
                        query = query.Where(q => q.Product.ProductOriginNumber.ProductAttributes.Any(p => p.Id == strProductAttrId));
                    }
                    if (strCategoryId.HasValue)
                    {
                        query = query.Where(q => q.Product.ProductOriginNumber.Category.ParentId.Value == strCategoryId);
                    }

                    var orderInfo = query.OrderByDescending(i => i.UpdatedTime)
                        .Select(i => new MemberProductInfo
                        {
                            BigProdNumber = i.Product.ProductOriginNumber.BigProdNum,
                            CategoryName = i.Product.ProductOriginNumber.Category.CategoryName,
                            SeasonName = i.Product.ProductOriginNumber.Season.SeasonName,
                            SizeName = i.Product.Size.SizeName,
                            Price = i.ProductTagPrice,
                            ProductId = i.ProductId,
                            ProductType = (int)SingleProductFlag.OrderItem,
                            CreateTime = i.CreatedTime,
                            CoverImagePath = i.Product.ProductOriginNumber.ProductCollocationImg,
                            ImagePath = i.Product.ProductOriginNumber.ProductCollocationImg,
                            ColorId = i.Product.ColorId,
                            SeasonId = i.Product.ProductOriginNumber.SeasonId,
                            SizeId = i.Product.SizeId,
                            ColorName = i.Product.Color.ColorName,
                            ColorIconPath = i.Product.Color.IconPath,
                            CategoryId = i.Product.ProductOriginNumber.CategoryId
                        }).Skip((PageIndex - 1) * PageSize).Take(PageSize - listProInfo.Count).ToList();


                    listProInfo.AddRange(orderInfo);
                }

                // 判断数据是否满足分页条数
                if (listProInfo.Count >= PageSize)
                {
                    return listProInfo;
                }
                else
                {
                    var query = _memberSingleProductRepository.Entities.Where(x => !x.IsDeleted && x.IsEnabled && x.MemberId == memberId);
                    if (strColorId.HasValue)
                    {
                        query = query.Where(q => q.ColorId == strColorId);
                    }
                    if (strProductAttrId.HasValue)
                    {
                        query = query.Where(q => q.ProductAttrId.Contains(strProductAttrId.ToString()));
                    }
                    if (strCategoryId.HasValue)
                    {
                        query = query.Where(q => q.Category.ParentId.Value == strCategoryId);
                    }

                    var orderInfo = query.OrderByDescending(i => i.UpdatedTime)
                        .Select(i => new MemberProductInfo
                        {
                            CategoryName = i.Category.CategoryName,
                            SeasonName = i.Season.SeasonName,
                            SizeName = i.Size.SizeName,
                            Price = i.Price,
                            ProductId = i.Id,   //单品id
                            ProductType = (int)SingleProductFlag.Upload,
                            CreateTime = i.CreatedTime,
                            CoverImagePath = i.CoverImage,
                            ImagePath = i.Image,
                            ColorId = i.ColorId,
                            SeasonId = i.SeasonId,
                            SizeId = i.SizeId,
                            ColorName = i.Color.ColorName,
                            ColorIconPath = i.Color.IconPath,
                            CategoryId = i.CategoryId
                        }).Skip((PageIndex - 1) * PageSize).Take(PageSize - listProInfo.Count).ToList();

                    listProInfo.AddRange(orderInfo);
                }
                return listProInfo;
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return null;
            }
        }

        #region 获取编辑对象
        /// <summary>
        /// 获取编辑对象
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public MemberProductInfo GetEdit(int id)
        {
            try
            {
                var entity = _memberSingleProductRepository.GetByKey(id);
                MemberProductInfo proInfo = new MemberProductInfo();
                proInfo.ProductId = entity.Id;
                proInfo.ProductName = entity.ProductName;
                proInfo.CategoryId = entity.CategoryId;
                proInfo.CategoryName = _categoryRepository.GetByKey(entity.CategoryId ?? 0).CategoryName;
                proInfo.ColorId = entity.ColorId;
                proInfo.ProductAttrId = entity.ProductAttrId ?? "0";
                proInfo.ProductAttrName = GetAttrNames(entity.ProductAttrId);
                proInfo.SeasonId = entity.SeasonId;
                proInfo.SeasonName = _seasonRepository.GetByKey(entity.SeasonId ?? 0).SeasonName;
                proInfo.SizeId = entity.SizeId;
                proInfo.SizeName = entity.SizeId == null ? "" : _sizeRepository.GetByKey(entity.SizeId ?? 0).SizeName;
                proInfo.Price = entity.Price;
                proInfo.CoverImagePath = entity.CoverImage;
                proInfo.ImagePath = entity.Image;
                proInfo.Notes = entity.Notes;
                proInfo.Brand = entity.Brand;
                return proInfo;
            }
            catch (Exception e)
            {

                throw;
            }
        }
        #endregion

        #endregion

        public string GetAttrNames(string attrIds)
        {
            if (string.IsNullOrEmpty(attrIds))
            {
                return string.Empty;
            }
            var attrArr = attrIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => int.Parse(s)).ToList();
            if (attrArr.Count <= 0)
            {
                return string.Empty;
            }
            var attrNames = _productAttrRepository.Entities.Where(a => !a.IsDeleted && a.IsEnabled && attrArr.Contains(a.Id)).Select(a => a.AttributeName).ToList();
            return string.Join(",", attrNames);
        }

        public void Update(List<MemberSingleProduct> list)
        {
            _memberSingleProductRepository.Update(list);
        }
    }
}
