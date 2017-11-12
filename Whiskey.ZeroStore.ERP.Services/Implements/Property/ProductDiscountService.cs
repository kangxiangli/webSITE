using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
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
using Whiskey.ZeroStore.ERP.Models.Entities;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Product;
using XKMath36;


namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    public class ProductDiscountService : ServiceBase, IProductDiscountContract
    {
        #region 声明数据层操作对象

        private readonly IRepository<ProductDiscount, int> _productDiscountRepository;
        private readonly IRepository<Season, int> _seasonRepository;
        private readonly IRepository<Color, int> _colorRepository;
        private readonly IRepository<Size, int> _sizeRepository;
        private readonly IRepository<Product, int> _productRepository;
        private readonly IRepository<Brand, int> _brandRepository;
        private readonly IRepository<Category, int> _categoryRepository;
        private readonly IRepository<ProductOriginNumber, int> _productOrigNumbRepository;
        private static object lockObj=new object(); 

        public ProductDiscountService(
            IRepository<ProductDiscount, int> productDiscountRepository,
            IRepository<Season, int> seasonRepository,
            IRepository<Color, int> colorRepository,
            IRepository<Size, int> sizeRepository,
            IRepository<Product, int> productRepository,
            IRepository<Brand, int> brandRepository,
            IRepository<Category, int> categoryRepository,
        IRepository<ProductOriginNumber, int> productOrigNumbRepository)
            : base(productDiscountRepository.UnitOfWork)
        {
            _productDiscountRepository = productDiscountRepository;
            _seasonRepository = seasonRepository;
            _colorRepository = colorRepository;
            _sizeRepository = sizeRepository;
            _productRepository = productRepository;
            _brandRepository = brandRepository;
            _categoryRepository = categoryRepository;
            _productOrigNumbRepository = productOrigNumbRepository;
        }
        #endregion

        #region 添加数据
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public OperationResult Insert(params ProductDiscountDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                CreateMap();
                List<ProductDiscount> productDiscounts = new List<ProductDiscount>();

                foreach (var dto in dtos)
                {
                    var ent = CreateMap(dto);
                    ent.IsDeleted = false;
                    ent.IsEnabled = false;//添加的折扣方案默认不启用；
                    ent.DiscountCode= CreateDiscountCode();
                    productDiscounts.Add(ent);
                }
                int resul = _productDiscountRepository.Insert((IEnumerable<ProductDiscount>)productDiscounts);
                return resul > 0 ? new OperationResult(OperationResultType.Success, resul + "条数据受影响") : new OperationResult(OperationResultType.Error);
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "添加失败！错误如下：" + ex.Message);
            }
        }
        /// <summary>
        /// 生成一个唯一的折扣编码4位数
        /// </summary>
        /// <returns></returns>
        private string CreateDiscountCode()
        {
            lock (lockObj)
            {
                var maxid=
                _productDiscountRepository.Entities.OrderByDescending(c => c.Id).Select(c=>c.Id).FirstOrDefault();
                XKMath36.Math36 math=new Math36();
                var basecode= math.To36(maxid+1);
                Random random=new Random();
                int rand= random.Next(65, 91);
                basecode = (char)rand + basecode;
                if (basecode.Length < 4)
                {
                   basecode+=rand;
                }
                return basecode.Substring(0, 4);
            }
        }

        private void CreateMap()
        {
            AutoMapper.Mapper.CreateMap<ProductDiscountDto, ProductDiscount>()
               .ForMember(c => c.Brands, g => g.Ignore())
               .ForMember(c => c.ProductOrigNumbers, g => g.Ignore());
            AutoMapper.Mapper.CreateMap<ProductDiscount, ProductDiscountDto>()
                .ForMember(c => c.Brands, g => g.MapFrom(t => string.Join(",", t.Brands.Select(f => f.Id).ToArray())));

        }

      
        private ProductDiscount CreateMap(ProductDiscountDto dto)
        {
            CreateMap();
            ProductDiscount enty = AutoMapper.Mapper.Map<ProductDiscount>(dto);
            enty.CreatedTime = DateTime.Now;
            enty.UpdatedTime = DateTime.Now;
            enty.OperatorId = AuthorityHelper.OperatorId;
         

            #region 数据映射
            if (!string.IsNullOrEmpty(dto.Brands))
            {
                int[] brandIds = dto.Brands.Split(",", true).Select(c => Convert.ToInt32(c)).ToArray();
                enty.Brands = _brandRepository.Entities.Where(c => brandIds.Contains(c.Id)).ToList();
                if (enty.Brands.Any())
                    enty.BrandCount = enty.Brands.Count;
            }
            if (!string.IsNullOrEmpty(dto.BigNumbers))
            {//bignumbers对应原始ID
                int[] orignumbeIds = dto.BigNumbers.Split(",", true).Select(c=>Convert.ToInt32(c)).ToArray();
           
                enty.ProductOrigNumbers =
                    _productOrigNumbRepository.Entities.Where(c => orignumbeIds.Contains(c.Id)).ToList();
                if (enty.ProductOrigNumbers.Any())
                    enty.ProductOrigNumberCount = enty.ProductOrigNumbers.Count;
            }
            #endregion
            return enty;
        }



        private List<T> LoadEntity<T>(string arr, string flg) where T : class
        {
            if (string.IsNullOrEmpty(arr) || string.IsNullOrEmpty(flg))
            {
                throw new NullReferenceException("参数不为空");
            }
            switch (flg)
            {
                case "brands":
                    {
                        int[] brandIds = arr.Split(",", true).Select(c => Convert.ToInt32(c)).ToArray();
                        return _brandRepository.Entities.Where(c => brandIds.Contains(c.Id)).ToList() as List<T>;
                    }
                case "category":
                    {
                        int[] categoryIds = arr.Split(",", true).Select(c => Convert.ToInt32(c)).ToArray();
                        return _categoryRepository.Entities.Where(c => categoryIds.Contains(c.Id)).ToList() as List<T>;
                    }
                case "season":
                    {
                        int[] seasonIds = arr.Split(",", true).Select(c => Convert.ToInt32(c)).ToArray();
                        return _seasonRepository.Entities.Where(c => seasonIds.Contains(c.Id)).ToList() as List<T>;
                    }
                case "color":
                    {
                        int[] colorIds = arr.Split(",", true).Select(c => Convert.ToInt32(c)).ToArray();
                        return _colorRepository.Entities.Where(c => colorIds.Contains(c.Id)).ToList() as List<T>;
                    }
                case "size":
                    {
                        int[] sizeIds = arr.Split(",", true).Select(c => Convert.ToInt32(c)).ToArray();
                        return _sizeRepository.Entities.Where(c => sizeIds.Contains(c.Id)).ToList() as List<T>;
                    }
                case "product":
                    {
                        int[] productids = arr.Split(",", true).Select(c => Convert.ToInt32(c)).ToArray();
                        var t1 = _productRepository.Entities.Where(c => productids.Contains(c.Id));
                        var t = _productRepository.Entities.Where(c => productids.Contains(c.Id)).ToList() as List<T>;
                        return t;
                    }
            }
            throw new Exception("参数异常");
        }
        #endregion

        #region 获取编辑对象

        /// <summary>
        /// 获取编辑对象
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ProductDiscountDto Edit(int Id)
        {
            var entity = _productDiscountRepository.GetByKey(Id);
            CreateMap();
            return AutoMapper.Mapper.Map<ProductDiscountDto>(entity);
           
        }
        #endregion
    
        /// <summary>
        /// 获取数据集
        /// </summary>        
        public IQueryable<ProductDiscount> ProductDiscounts
        {
            get { return _productDiscountRepository.Entities; }
        }

        public List<ProductDiscountDto> ProductDiscountDtos(Expression<Func<ProductDiscount,bool>> exp)
        {
           IQueryable<ProductDiscount> coll= ProductDiscounts.Where(exp);
            CreateMap();
           return AutoMapper.Mapper.Map<List<ProductDiscountDto>>(coll.ToList());
        } 


        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public OperationResult Update(params ProductDiscountDto[] dtos)
        {
            throw new Exception();
        }


        public OperationResult Remove(params int[] ids)
        {
           var ents= _productDiscountRepository.Entities.Where(c => ids.Contains(c.Id));
            ents.Each(c =>
            {
                c.IsDeleted = true;
                c.OperatorId = AuthorityHelper.OperatorId;
                c.UpdatedTime = DateTime.Now;
            });
          return  _productDiscountRepository.Update(ents.ToList());
        }

        public OperationResult Delete(params int[] ids)
        {
            throw new NotImplementedException();
        }

        public ProductDiscount View(int Id)
        {
            return _productDiscountRepository.Entities.FirstOrDefault(c => c.Id == Id);
        }

        public OperationResult Recovery(params int[] ids)
        {
            var ents = _productDiscountRepository.Entities.Where(c => ids.Contains(c.Id));
            ents.Each(c => {
                c.IsDeleted = false;
                c.OperatorId = AuthorityHelper.OperatorId;
                c.UpdatedTime = DateTime.Now;
            });
            return _productDiscountRepository.Update(ents.ToList());
        }


        public OperationResult Enable(params int[] ids)
        {
           var ents= _productDiscountRepository.Entities.Where(c => ids.Contains(c.Id));
            ents.Each(c =>
            {
                c.UpdatedTime = DateTime.Now;
                c.IsEnabled = true;
                c.OperatorId = AuthorityHelper.OperatorId;
                c.Description = "启用折扣方案";

            });
           return _productDiscountRepository.Update(ents.ToArray());
        }


        public OperationResult Disable(params int[] ids)
        {
            var ents = _productDiscountRepository.Entities.Where(c => ids.Contains(c.Id));
            ents.Each(c => {
                c.UpdatedTime = DateTime.Now;
                c.IsEnabled = false;
                c.OperatorId = AuthorityHelper.OperatorId;
                c.Description = "禁用折扣方案";

            });
            return _productDiscountRepository.Update(ents.ToArray());
        }
    }
}
