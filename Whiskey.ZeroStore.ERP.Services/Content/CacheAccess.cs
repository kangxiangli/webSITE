using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using System.Xml.Serialization;
using AutoMapper;
using Whiskey.Utility.Data;
using Whiskey.Utility.Extensions;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Models.Entities;
using Whiskey.ZeroStore.ERP.Models.Entities.Notices;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Services.Extensions.License;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.Utility.Graphic;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Template;
using Whiskey.ZeroStore.ERP.Transfers.Entities;
using XKMath36;
using Whiskey.ZeroStore.ERP.Services.Contracts.Warehouse;
using Whiskey.ZeroStore.ERP.Models.Entities.Warehouses;
using System.Diagnostics;
using Whiskey.Utility.Logging;
using System.Data.Entity;
using System.Collections;
using System.Collections.Concurrent;
using Autofac;
using System.Collections.ObjectModel;

namespace Whiskey.ZeroStore.ERP.Services.Content
{
    //提供缓存操作类
    ///yxk 2015-10-
    public class CacheAccess
    {
        protected static readonly ILogger _log = LogManager.GetLogger(typeof(CacheAccess));
        static object lockflg = new object();
        const string CACHE_KEY_ALL_STORE = "CACHE_ALL_STORES";
        const string CACHE_KEY_ALL_STORAGE = "CACHE_ALL_STORAGES";
        const string TABLE_NAME_STORE = "S_Store";
        const string TABLE_NAME_STORAGE = "W_Storage";
        const string TABLE_NAME_PRODUCT = "P_Product";
        const string CACHE_KEY_PREFIX_ENABLED_STORE = "_enable_9202_";
        const string CACHE_KEY_PREFIX_ENABLED_STORAGE = "_enable_storage_";
        const string CACHE_KEY_PREFIX_ENABLED_DEPAREMENTSTORES = "20170307_managedstores_";
        const string CACHE_KEY_PRODUCT = "Products_sel_1190";

        #region 店铺
        


        /// <summary>
        /// 获取所有的仓库
        /// </summary>
        public static List<SelectListItem> GetAllStorageListItem(IStorageContract _storageContract, bool hasTitle = true)
        {
            var storageList = CacheHelper.GetCache(TABLE_NAME_STORAGE) as List<SelectListItem>;

            if (storageList == null)
            {
                storageList = _storageContract.Storages.Where(c => !c.IsDeleted && c.IsEnabled)
                                            .Select(m => new SelectListItem { Text = m.StorageName, Value = m.Id.ToString() })
                                            .ToList();
                if (storageList.Count() > 0)
                {
                    CacheHelper.SetCache(CACHE_KEY_ALL_STORAGE, storageList, new SqlCacheDependency(DBName, TABLE_NAME_STORAGE));
                }
            }
            var resList = Clone(storageList);

            if (hasTitle)
            {
                if (resList[0].Value != "")
                {
                    resList.Insert(0, new SelectListItem()
                    {
                        Text = "-请选择仓库-",
                        Value = "",
                    });
                }
            }
            return resList.ToList();
        }


        

        /// <summary>
        /// 获取掌管的仓库
        /// </summary>
        public static List<SelectListItem> GetManagedStorageListItem(IStorageContract _storageContract, IAdministratorContract _adminContract, bool hasTitle = true)
        {
            var adminId = AuthorityHelper.OperatorId;
            var storageList = GetManagedStorage(_storageContract, _adminContract)
                                 .Select(m => new SelectListItem { Text = m.StorageName, Value = m.Id.ToString() })
                                 .ToList();
            if (storageList == null || !storageList.Any())
            {
                storageList = new List<SelectListItem>();
            }

            if (hasTitle)
            {
                storageList.Insert(0, new SelectListItem()
                {
                    Text = "-请选择仓库-",
                    Value = "",
                });
            }
            return storageList.ToList();
        }


        /// <summary>
        /// 获取掌管的仓库
        /// </summary>
        public static IQueryable<Storage> GetManagedStorage(IStorageContract _storageContract, IAdministratorContract _adminContract)
        {
            var adId = AuthorityHelper.OperatorId;
            return PermissionHelper.ManagedStorages(adId.Value, _adminContract, s => s).AsQueryable();
        }


        /// <summary>
        /// 根据店铺id获取店铺下的仓库
        /// </summary>
        public static List<SelectListItem> GetManagedStorageByStoreId(IStorageContract _storageContract, IAdministratorContract _adminContract, int StoreId, bool hasTitle = false)
        {
            var itemList = new List<SelectListItem>();
            var da = _adminContract.GetDesignerStoreStorage(AuthorityHelper.OperatorId.Value);
            if (da.Item1)
            {
                itemList = da.Item2.Where(s => s.StoreId == StoreId).SelectMany(s => s.Storages).Select(s => new SelectListItem() { Text = s.StorageName, Value = s.StorageId + "" }).ToList();
            }
            else
            {
                itemList = GetManagedStorage(_storageContract, _adminContract).Where(c => c.StoreId == StoreId).Select(c => new SelectListItem()
                {
                    Text = c.StorageName,
                    Value = c.Id + ""
                }).ToList();
            }

            if (itemList == null || itemList.Count <= 0)
            {
                itemList = new List<SelectListItem>();
            }
            if (hasTitle)
            {
                itemList.Insert(0, new SelectListItem()
                {
                    Text = "选择仓库",
                    Value = ""
                });
            }
            return itemList;
        }


        /// <summary>
        /// 获取所有的采购库
        /// </summary>
        public static List<SelectListItem> GetOrderStorages(IStorageContract _storageContract, bool hasTitle)
        {
            var li = CacheHelper.GetCache("_orderStorage_1102_all_") as List<SelectListItem>;

            if (li == null)
            {
                var t =
                    _storageContract.Storages.Where(c => c.IsEnabled && !c.IsDeleted && c.IsOrderStorage)
                        .Select(x => new SelectListItem()
                        {
                            Value = x.Id.ToString(),
                            Text = x.StorageName
                        }).ToList();
                if (t != null && t.Count > 0)
                {
                    li = Clone(t);
                    CacheHelper.SetCache("_orderStorage_1102_all_", t, new SqlCacheDependency(DBName, "W_Storage"));
                }
            }
            List<SelectListItem> resul = new List<SelectListItem>();
            if (li != null)
                resul = Clone(li);
            if (hasTitle)
                resul.Insert(0, new SelectListItem()
                {
                    Text = "选择店铺",
                    Value = ""
                });
            return resul;
        }

        /// <summary>
        /// 获取店铺类型
        /// </summary>
        /// <param name="_storeTypeContract"></param>
        /// <param name="hasTitle"></param>
        /// <returns></returns>
        public static List<SelectListItem> GetStoreType(IStoreTypeContract _storeTypeContract, bool hasTitle)
        {
            var _key = "_storetype_20170329_";
            var li = CacheHelper.GetCache(_key) as List<SelectListItem>;

            if (li == null)
            {
                var t =
                    _storeTypeContract.Entities.Where(c => c.IsEnabled && !c.IsDeleted)
                        .Select(x => new SelectListItem()
                        {
                            Value = x.Id.ToString(),
                            Text = x.TypeName
                        }).ToList();
                if (t != null && t.Count > 0)
                {
                    li = Clone(t);
                    CacheHelper.SetCache(_key, t, new SqlCacheDependency(DBName, "S_StoreType"));
                }
            }
            List<SelectListItem> resul = new List<SelectListItem>();
            if (li != null)
                resul = Clone(li);
            if (hasTitle)
                resul.Insert(0, new SelectListItem()
                {
                    Text = "选择店铺类型",
                    Value = ""
                });
            return resul;
        }


        #endregion

        #region 商品
        /// <summary>
        /// 获取商品列表
        /// </summary>
        public static List<SelectListItem> GetProductListItem(IProductContract _productContract, bool hasTitle)
        {
            var productList = CacheHelper.GetCache(CACHE_KEY_PRODUCT) as List<SelectListItem>;
            if (productList == null)
            {
                productList = (_productContract.Selectlist("", c => c.IsDeleted == false && c.IsEnabled == true).Select(m => new SelectListItem { Text = m.Value, Value = m.Key })).ToList();
                if (productList.Count() > 0)
                {
                    CacheHelper.SetCache(CACHE_KEY_PRODUCT, productList, new SqlCacheDependency(DBName, TABLE_NAME_PRODUCT));
                }
            }
            var _productList = CacheAccess.Clone<List<SelectListItem>>(productList);

            if (hasTitle)
            {
                if (_productList.IsNotNullOrEmptyThis()) // _productList[0].Value != "")
                {
                    _productList.Insert(0, new SelectListItem()
                    {
                        Value = "",
                        Text = "-请选择商品-"

                    });
                }
            }
            return _productList;

        }

        /// <summary>
        /// 获取商品面向人群
        /// </summary>
        /// <param name="_productCrowdContract"></param>
        /// <param name="hasTitle"></param>
        /// <returns></returns>
        public static List<SelectListItem> GetProductCrowd(IProductCrowdContract _productCrowdContract, bool hasTitle = false)
        {
            List<SelectListItem> lis = CacheHelper.GetCache("productCrowd_li_20170425") as List<SelectListItem>;
            if (lis == null)
            {
                lis = new List<SelectListItem>();
                var list = _productCrowdContract.Entities.Where(c => c.IsEnabled && !c.IsDeleted).Select(s => new SelectListItem { Value = s.Id + "", Text = s.CrowdName }).ToList();
                if (list != null)
                {
                    lis = list;
                    CacheHelper.SetCache("productCrowd_li_20170425", list, new SqlCacheDependency(DBName, "P_ProductCrowd"));
                }
            }
            List<SelectListItem> _lis = CacheAccess.Clone<List<SelectListItem>>(lis);
            if (hasTitle)
            {
                _lis.Insert(0, new SelectListItem()
                {
                    Value = "",
                    Text = "请选择"
                });
            }
            return _lis;
        }

        #endregion

        #region 品牌
        public static List<BrandDto> GetBrands(IBrandContract _brandContract)
        {

            List<Brand> li = CacheHelper.GetCache("brand_li_9211") as List<Brand>;
            if (li == null)
            {
                li = _brandContract.Brands.ToList();
                CacheHelper.SetCache("brand_li_9211", li, new SqlCacheDependency(DBName, "P_Brand"));
            }

            return AutoMapper.Mapper.Map<List<BrandDto>>(li);
        }
        /// <summary>
        /// 获取品牌列表
        /// </summary>
        /// <param name="_brandContract"></param>
        /// <param name="hastitle">是否显示标题</param>
        /// <param name="showParId">是否显示父级分类的id</param>
        /// <returns></returns>
        public static List<SelectListItem> GetBrand(IBrandContract _brandContract, bool hastitle = false, bool showParId = false, int Now = 0)
        {

            //var li = CacheHelper.GetCache("brand_9211") as List<SelectListItem>;
            var li = new List<SelectListItem>();
            if (li == null || li.Count() == 0)
            {
                //li = (_brandContract.SelectList("").Select(m => new SelectListItem { Text = m.Key.ToString(), Value = m.Value })).ToList();
                li = new List<SelectListItem>();
                var lis = _brandContract.Brands.Where(c => c.IsDeleted == false && c.IsEnabled == true).ToList();
                var parsId = lis.Where(c => c.ParentId == null).Select(c => c.Id);
                foreach (int _id in parsId)
                {
                    //父类节点的id都设置为-1，这样在view中可以通过id查找到父类节点
                    var childs = lis.Where(c => c.ParentId == _id).Select(c => new SelectListItem()
                    {
                        Text = StringHelper.GetPrefix(1) + c.BrandName,
                        Value = c.Id.ToString()
                    }).ToList();
                    if (showParId)
                    {
                        li.Add(new SelectListItem()
                        {
                            Text = lis.Where(c => c.Id == _id).FirstOrDefault().BrandName,
                            Value = lis.Where(c => c.Id == _id).FirstOrDefault().Id.ToString()
                        });
                    }
                    else
                    {
                        li.Add(new SelectListItem()
                        {
                            Text = lis.Where(c => c.Id == _id).FirstOrDefault().BrandName,
                            Value = "-1"
                        });
                    }


                    li.AddRange(childs);
                }
                CacheHelper.SetCache("brand_9211", li, new SqlCacheDependency(DBName, "P_Brand"));

            }
            List<SelectListItem> _brandList = CacheAccess.Clone<List<SelectListItem>>(li);
            if (hastitle)
            {
                _brandList.Insert(0, new SelectListItem()
                {
                    Text = "请选择品牌",
                    Value = "-1",
                    Selected = true
                });
            }
            return _brandList;
        }
        public static SelectList GetBrand(IBrandContract _brandContract)
        {

            //var li = CacheHelper.GetCache("brand_9211") as List<SelectListItem>;
            SelectList li = null;

            var lis = _brandContract.Brands.Where(c => !c.IsDeleted && c.IsEnabled && c.ParentId != null).Select(c => new
            {
                Text = c.BrandName,
                Value = c.Id,
                GroupField = c.Parent.BrandName
            });

            li = new SelectList(lis, "Value", "Text", "GroupField", (object)"");
            return li;

        }
        #endregion

        #region 品类
        /// <summary>
        /// 获取list<CategoryDto>
        /// </summary>
        /// <param name="_categoryContract"></param>
        /// <returns></returns>
        public static List<CategoryDto> GetCategorys(ICategoryContract _categoryContract)
        {

            var li = CacheHelper.GetCache("category_li_9210") as List<Category>;
            if (li == null)
            {
                li = _categoryContract.Categorys.Where(c => c.IsEnabled && !c.IsDeleted).ToList();
                CacheHelper.SetCache("category_li_9210", li, new SqlCacheDependency(DBName, "P_Category"));
            }

            return AutoMapper.Mapper.Map<List<CategoryDto>>(li);
        }
        /// <summary>
        /// 获取分类
        /// </summary>
        /// <param name="_cagegoryContract"></param>
        /// <param name="hasTitle"></param>
        /// <returns></returns>
        public static List<SelectListItem> GetCategory(ICategoryContract _categoryContract, bool hasTitle)
        {

            var li = CacheHelper.GetCache("category_1102") as List<SelectListItem>;
            if (li == null)
            {
                li = _categoryContract.SelectList("").Select(m => new SelectListItem { Text = m.Key.ToString(), Value = m.Value }).ToList();

                CacheHelper.SetCache("category_1102", li, new SqlCacheDependency(DBName, "P_Category"));
            }
            List<SelectListItem> _categoryList = CacheAccess.Clone<List<SelectListItem>>(li);
            if (hasTitle)
            {
                _categoryList.Insert(0, new SelectListItem()
                {
                    Text = "选择分类",
                    Value = "",
                    Selected = true
                });
            }
            return _categoryList;
        }
        public static SelectList GetCategory(ICategoryContract _categoryContract)
        {
            var li = _categoryContract.Categorys.Where(c => c.IsEnabled && !c.IsDeleted && c.ParentId != null).Select(c => new
            {
                Text = c.CategoryName,
                Value = c.Id,
                GroupField = c.Parent.CategoryName
            });
            return new SelectList(li, "Value", "Text", "GroupField", (object)"");
        }
        #endregion

        #region 尺码
        /// <summary>
        /// 获取List<SizeDto>
        /// </summary>
        /// <param name="_sizeContract"></param>
        /// <returns></returns>
        public static List<SizeDto> GetSizes(ISizeContract _sizeContract)
        {

            List<Size> li = CacheHelper.GetCache("size_li_1102") as List<Size>;
            if (li == null)
            {
                li = _sizeContract.Sizes.ToList();
                CacheHelper.SetCache("size_li_1102", li, new SqlCacheDependency(DBName, "P_Size"));
            }
            var dtos = AutoMapper.Mapper.Map<List<SizeDto>>(li);
            return CacheAccess.Clone<List<SizeDto>>(dtos);
        }

        /// <summary>
        /// 获取指定款式尺码
        /// </summary>
        /// <param name="_sizeContract"></param>
        /// <param name="hasTitle"></param>
        /// <returns></returns>
        public static List<SelectListItem> GetSize(ISizeContract _sizeContract, string strCategoryId)
        {
            var li = CacheAccess.GetSizes(_sizeContract);
            List<SelectListItem> lis = new List<SelectListItem>();
            int categoryId = int.Parse(strCategoryId);
            List<SizeDto> listSizeDto = li.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.CategoryId == categoryId).ToList();
            foreach (var sizeDto in listSizeDto)
            {
                lis.Add(new SelectListItem() { Text = StringHelper.GetPrefix(1) + sizeDto.SizeName, Value = sizeDto.Id.ToString() });
            }
            #region
            //List<SizeDto> parentIds = li.Where(c => c.IsDeleted == false && c.IsEnabled == true && string.IsNullOrEmpty(c.ParentId.ToString())).ToList();
            //for (int i = 0; i < parentIds.Count; i++)
            //{
            //    lis.Add(new SelectListItem()
            //    {
            //        Text = parentIds[i].SizeName,
            //        Value = "-1"
            //    });
            //    lis.AddRange(li.Where(c => c.IsEnabled == true && c.IsDeleted == false && c.ParentId == parentIds[i].Id).Select(c => new SelectListItem()
            //    {
            //        Value = c.Id.ToString(),
            //        Text = StringHelper.GetPrefix(1) + c.SizeName
            //    }).ToList());
            //}
            #endregion
            return lis;
        }
        /// <summary>
        /// 获取尺码
        /// </summary>
        /// <param name="_sizeContract"></param>
        /// <param name="hasTitle"></param>
        /// <returns></returns>
        public static List<SelectListItem> GetSize(ISizeContract _sizeContract, ICategoryContract _categoryContract, bool hasTitle = false)
        {
            List<SelectListItem> lis = CacheHelper.GetCache("size_li_9202") as List<SelectListItem>;
            if (lis == null)
            {
                lis = new List<SelectListItem>();
                var sizeli = CacheAccess.GetSizes(_sizeContract);
                if (sizeli != null)
                {
                    var groupsize = sizeli.GroupBy(c => c.CategoryId).ToList();
                    for (int i = 0; i < groupsize.Count(); i++)
                    {
                        string name = GetCategorys(_categoryContract).Where(c => c.Id == groupsize[i].Key).FirstOrDefault().CategoryName;
                        lis.Add(new SelectListItem()
                        {
                            Text = name,
                            Value = "",
                            Disabled = true,
                        });
                        var chils = groupsize[i].Select(c => new SelectListItem()
                        {
                            Text = StringHelper.GetPrefix(1) + c.SizeName,
                            Value = c.Id.ToString()
                        }).ToList();
                        lis.AddRange(chils);
                    }

                    if (lis != null)
                    {
                        CacheHelper.SetCache("size_li_9202", lis, new SqlCacheDependency(DBName, "P_Size"));
                    }
                }
            }
            List<SelectListItem> _lis = CacheAccess.Clone<List<SelectListItem>>(lis);
            if (hasTitle)
            {
                _lis.Insert(0, new SelectListItem()
                {
                    Value = "-1",
                    Text = "选择尺码"
                });
            }
            return _lis;
        }

        public static SelectList GetSize(ISizeContract _sizeContract, ICategoryContract _categoryContract, Expression<Func<Size, bool>> exp)
        {
            var t = _sizeContract.Sizes.Where(c => c.IsEnabled && !c.IsDeleted).Where(exp).Select(c => new
            {
                c.SizeName,
                c.Id,
            });
            List<object> objli = new List<object>();
            foreach (var ite in t)
            {
                //var categ = GetCategorys(_categoryContract).FirstOrDefault(c => c.Id == ite.CategoryId);
                //objli.Add(new {
                //    Value = ite.Id,
                //    Text = ite.SizeName,
                //    GroupField = categ != null ? categ.CategoryName : ""
                //});
            }
            return new SelectList(objli, "Value", "Text", "GroupField", (object)"");
        }

        #endregion

        #region 季节
        /// <summary>
        /// 返回List<SeasonDto>
        /// </summary>
        /// <param name="_seasonContract"></param>
        /// <returns></returns>
        public static List<SeasonDto> GetSeasons(ISeasonContract _seasonContract)
        {
            var li = CacheHelper.GetCache("Season_li_1102") as List<Season>;
            if (li == null)
            {
                li = _seasonContract.Seasons.ToList();
                CacheHelper.SetCache("Season_li_1102", li, new SqlCacheDependency(DBName, "P_Season"));
            }
            return AutoMapper.Mapper.Map<List<SeasonDto>>(li);
        }
        /// <summary>
        /// 获取季节
        /// </summary>
        /// <param name="_seasonContract"></param>
        /// <param name="hasTitl"></param>
        /// <returns></returns>
        public static List<SelectListItem> GetSeason(ISeasonContract _seasonContract, bool hasTitl)
        {
            var li = CacheHelper.GetCache("season_9211") as List<SelectListItem>;
            if (li == null)
            {
                li = _seasonContract.SelectList("").OrderBy(c => c.Key).Select(c => new SelectListItem()
                {
                    Text = StringHelper.GetPrefix(1) + c.Value,
                    Value = c.Key
                }).ToList();
                CacheHelper.SetCache("season_9211", li, new SqlCacheDependency(DBName, "P_Season"));
            }

            var _seasonli = CacheAccess.Clone<List<SelectListItem>>(li);
            if (hasTitl)
            {
                _seasonli.Insert(0, new SelectListItem()
                {
                    Text = "选择季节"
                    ,
                    Value = ""
                });

            }
            return _seasonli;
        }
        #endregion

        #region 颜色
        /// <summary>
        /// 获取色系列表,返回图片形式
        /// </summary>
        /// <param name="_colorContract"></param>
        /// <param name="hasTitl">是否显示标题</param>
        /// <param name="showParId">是否显示父类的Id</param>
        /// <returns></returns>

        public static List<SelectListItem> GetColors(IColorContract _colorContract, bool hasTitl = false)
        {
            var li = CacheHelper.GetCache("colors_li_9211") as List<SelectListItem>;

            if (li == null || li.Count == 0)
            {
                li = new List<SelectListItem>();
                var colis = GetColorList(_colorContract).Where(c => c.IsDeleted == false && c.IsEnabled == true).Select(c => new { c.Id, c.ColorName, c.IconPath }).Distinct().ToList(); //获取所有的父类id
                string path = "/Content/Images/redrawColorImg";
                foreach (var item in colis)
                {
                    string[] pas = GraphicHelper.ReDraw(path, false, item.IconPath);
                    if (pas != null && pas.Count() > 0)
                        li.Add(new SelectListItem()
                        {
                            Text = pas[0] + "|" + item.ColorName,
                            Value = item.Id.ToString(),
                        });
                    //+" <div style='background-color:"+c.RGB+"';width:25px;height:25px;margin:0 auto;display:inline ></div>" 
                }
                CacheHelper.SetCache("colors_li_9211", li, new SqlCacheDependency(DBName, "P_Color"));

                //else
                //{
                //    foreach (var item in parentIds)
                //    {
                //        li.Add(new SelectListItem()
                //        {
                //            Text =item.IconPath+"|"+ item.ColorName,
                //            Value = "-1"
                //        });

                //        li.AddRange(CacheAccess.GetColorList(_colorContract).Where(c => c.IsDeleted == false && c.IsEnabled == true && c.ParentId == item.Id).Select(c => new SelectListItem()
                //        {
                //            Value = c.Id.ToString(),
                //            Text = item.IconPath + "|" + StringHelper.GetPrefix(1) + c.ColorName
                //        }).ToList());
                //        //+" <div style='background-color:"+c.RGB+"';width:25px;height:25px;margin:0 auto;display:inline ></div>" 
                //    }
                //    CacheHelper.SetCache("colors_9211", li, new SqlCacheDependency(DBName, "P_Color"));
                //}


            }
            var _collis = CacheAccess.Clone<List<SelectListItem>>(li);
            if (hasTitl)
                _collis.Insert(0, new SelectListItem()
                {
                    Text = "请选择颜色",
                    Value = ""
                });
            return _collis;

        }

        /// <summary>
        /// 获取色系列表，文字形式
        /// </summary>
        /// <param name="_colorContract"></param>
        /// <param name="hasTitl"></param>
        /// <returns></returns>
        public static List<SelectListItem> GetColorsName(IColorContract _colorContract, bool hasTitl = false)
        {
            var li = CacheHelper.GetCache("_color_wordli_1102") as List<SelectListItem>;

            if (li == null || li.Count == 0)
            {
                li = new List<SelectListItem>();
                var t = GetColorList(_colorContract).Where(c => c.IsEnabled && !c.IsDeleted).Select(x => new SelectListItem()
                {
                    Text = x.ColorName,
                    Value = x.Id.ToString()
                }).ToList();
                CacheHelper.SetCache("_color_wordli_1102", t, new SqlCacheDependency(DBName, "P_Color"));
                li = Clone(t);
            }
            var col = new List<SelectListItem>();

            if (li != null)
            {
                col = Clone(li);
                if (hasTitl)
                    col.Insert(0, new SelectListItem()
                    {
                        Value = "",
                        Text = "选择颜色"
                    });
            }
            return col;
        }
        public static List<Color> GetColorList(IColorContract _colorContract)
        {
            // List<Color> li = CacheHelper.GetCache("colors_li_1102") as List<Color>;

            var li = _colorContract.Colors.ToList();
            return li;
        }
        #endregion

        #region 部门 角色
        
        public static List<SelectListItem> GetDepartmentListItem(IDepartmentContract _departmentContract, bool hasTitle, params DepartmentTypeFlag[] typeflag)
        {
            var _key = "Departments_sel_1102_";
            if (typeflag.Any())
            {
                var suffix = string.Join("_", typeflag.Select(s => s + ""));
                _key += suffix;
            }

            List<SelectListItem> li = CacheHelper.GetCache(_key) as List<SelectListItem>;
            if (li == null)
            {
                var query = _departmentContract.Departments.Where(c => c.IsDeleted == false && c.IsEnabled == true);
                if (typeflag.Any())
                {
                    query = query.Where(w => typeflag.Contains(w.DepartmentType));
                }
                li = query.Select(m => new SelectListItem
                {
                    Text = m.DepartmentName,
                    Value = m.Id + ""
                }).ToList();
                CacheHelper.SetCache(_key, li, new SqlCacheDependency(DBName, "A_Department"));
            }
            var resli = CacheAccess.Clone<List<SelectListItem>>(li);
            if (hasTitle)
            {
                resli.Insert(0, new SelectListItem()
                {
                    Text = "选择部门",
                    Value = " ",
                });
            }
            return resli;
        }


        public static List<SelectListItem> GetFactorys(IFactorysContract _factoryContract, bool hastit)
        {

            List<SelectListItem> li = CacheHelper.GetCache("Factory_sel_2202") as List<SelectListItem>;
            if (li == null)
            {
                li = _factoryContract.SelectFactorys.Where(c => c.IsDeleted == false && c.IsEnabled == true).Select(m => new SelectListItem
                {
                    Text = m.FactoryName,
                    Value = m.Id + "",
                }).ToList();

                CacheHelper.SetCache("Factory_sel_2202", li, new SqlCacheDependency(DBName, "F_Factorys"));
            }
            var resli = CacheAccess.Clone<List<SelectListItem>>(li);

            if (hastit)
            {
                resli.Insert(0, new SelectListItem()
                {
                    Text = "选择工厂",
                    Value = "",
                });
            }
            return resli;
        }

        

        /// <summary>
        /// 获取部门
        /// </summary>
        /// <param name="_departmentContract"></param>
        /// <returns></returns>
        public static IQueryable<Department> GetDepartments(IDepartmentContract _departmentContract)
        {
            //List<Department> li = CacheHelper.GetCache("department_li_1102") as List<Department>;
            //if (li == null)
            //{
            //    li = _departmentContract.Departments.ToList();
            //    CacheHelper.SetCache("department_li_1102", li, new SqlCacheDependency(DBName, "A_Department"));
            //}
            //return li;
            return _departmentContract.Departments;
        }

        public static IQueryable<Administrator> GetAdministrator(IAdministratorContract _administratorContract)
        {
            //List<Administrator> li = CacheHelper.GetCache("administ_li_1102") as List<Administrator>;
            //if (li == null)
            //{
            //    li = _administratorContract.Administrators.ToList();
            //    CacheHelper.SetCache("administ_li_1102", li, new SqlCacheDependency(DBName, "A_Administrator"));
            //}
            //return li;
            return _administratorContract.Administrators;

        }

        public static List<Models.Module> GetModules(IModuleContract _moduleContract)
        {
            List<Models.Module> li = CacheHelper.GetCache("module_li_9211") as List<Models.Module>;
            if (li == null)
            {
                li = _moduleContract.Modules.ToList();
                CacheHelper.SetCache("module_li_9211", li, new SqlCacheDependency(DBName, "A_Module"));
            }
            return li;
        }

        //public static List<MemberModule> GetMemberModule(IMemberModuleContract _memberModuleContract)
        //{
        //    List<MemberModule> li = CacheHelper.GetCache("memberModule_li_201705171106") as List<MemberModule>;
        //    if (li == null)
        //    {
        //        li = _memberModuleContract.Entities.ToList();
        //        CacheHelper.SetCache("memberModule_li_201705171106", li, new SqlCacheDependency(DBName, "A_MemberModule"));
        //    }
        //    return li;
        //}

        public static List<SelectListItem> GetModules(IModuleContract _moduleContract, int? parentId, bool hasTitle = false)
        {

            List<SelectListItem> modules =
                GetModules(_moduleContract).Where(c => !c.IsDeleted && c.IsEnabled && c.ParentId == parentId).Select(c => new SelectListItem()
                {
                    Text = c.ModuleName,
                    Value = c.Id + ""
                }).ToList();


            if (hasTitle)
                modules.Insert(0, new SelectListItem()
                {
                    Text = "下拉选择",
                    Value = "-1"
                });
            return modules;
        }
        /// <summary>
        /// 获取所有的权限列表
        /// </summary>
        /// <param name="_permissionContract"></param>
        /// <returns></returns>
        public static List<Permission> GetPermissions(IPermissionContract _permissionContract)
        {
            List<Permission> li = CacheHelper.GetCache("permission_li_9211") as List<Permission>;
            if (li == null)
            {
                li = _permissionContract.Permissions.ToList();
                CacheHelper.SetCache("permission_li_9211", li, new SqlCacheDependency(DBName, "A_Permission"));
            }
            return li;
        }
        //public static IQueryable<Permission> GetPermissionsQueryable(IPermissionContract _permissionContract)
        //{
        //    return _permissionContract.Permissions;
        //}

        /// <summary>
        /// 获取一级模块
        /// </summary>
        /// <param name="_moduleContract"></param>
        /// <param name="hasTitle"></param>
        /// <returns></returns>
        public static List<SelectListItem> GetParentModules(IModuleContract _moduleContract, bool hasTitle)
        {
            List<SelectListItem> li = CacheHelper.GetCache("module_sel_1102") as List<SelectListItem>;
            if (li == null)
            {
                li = GetModules(_moduleContract).Where(c => c.IsDeleted == false && c.IsEnabled == true && c.ParentId == null).Select(c => new SelectListItem()
                {
                    Text = c.ModuleName,
                    Value = c.Id.ToString()
                }).ToList();
            }
            var resu = CacheAccess.Clone(li);
            if (hasTitle)
                resu.Insert(0, new SelectListItem()
                {
                    Text = "下拉选择",
                    Value = ""
                });
            return resu;
        }
        /// <summary>
        /// 获取所有角色
        /// </summary>
        /// <param name="_roleContract"></param>
        public static List<Role> GetRoles(IRoleContract _roleContract)
        {
            List<Role> li = CacheHelper.GetCache("role_li_1102") as List<Role>;
            if (li == null)
            {
                li = _roleContract.Roles.ToList();
                CacheHelper.SetCache("role_li_1102", li, new SqlCacheDependency(DBName, "A_Role"));
            }
            return li;
        }
        #endregion


        #region 获取当前用户的权限

        /// <summary>
        /// 获取当前用户的权限列表
        /// </summary>
        /// <param name="_administratorContract"></param>
        /// <returns></returns>
        public static List<Permission> GetCurrentUserPermission(IAdministratorContract _administratorContract, IPermissionContract _permissionContract)
        {
            var adminId = AuthorityHelper.OperatorId;
            if (!adminId.HasValue) return new List<Permission>();
            string key = string.Format("_permission_user_{0}", adminId);
            List<Permission> lis = CacheHelper.GetCache(key) as List<Permission>;

            #region 获取权限

            if (lis.IsNullThis())
            {
                lis = PermissionHelper.GetOneUserPermission(adminId.Value, _administratorContract, _permissionContract);

                AggregateCacheDependency deps = new AggregateCacheDependency();
                deps.Add(new SqlCacheDependency(DBName, "A_Permission"));
                deps.Add(new SqlCacheDependency(DBName, "A_Role"));
                //deps.Add(new SqlCacheDependency(DBName, "A_Group"));
                deps.Add(new SqlCacheDependency(DBName, "A_Department"));

                CacheHelper.SetCache(key, lis, deps, new TimeSpan(24, 0, 0));
            }

            #endregion

            return lis;
        }

        /// <summary>
        /// 清除缓存的权限
        /// </summary>
        public static void ClearPermissionCache()
        {
            CacheHelper.RemoveAllCache("_permission_user_", true);
        }
        /// <summary>
        /// 清除指定Id用户的缓存权限
        /// </summary>
        /// <param name="adminIds">用户Id</param>
        public static void ClearPermissionCache(params int[] adminIds)
        {
            if (adminIds.IsNotNullOrEmptyThis())
            {
                foreach (var adminId in adminIds)
                {
                    string key = string.Format("_permission_user_{0}", adminId);
                    string keymenu = key + "_menu_";//menu
                    CacheHelper.RemoveAllCache(key);
                    CacheHelper.RemoveAllCache(keymenu);
                }
            }
        }

        #endregion

        #region 获取当前用户可以显示的菜单

        /// <summary>
        /// 获取当前用户可以显示的菜单
        /// </summary>
        /// <param name="_administratorContract"></param>
        /// <param name="_permissionContract"></param>
        /// <returns></returns>
        public static List<Models.Module> GetCurrentUserMenuModule(IAdministratorContract _administratorContract, IPermissionContract _permissionContract, IModuleContract _moduleContract)
        {
            string key = string.Format("_permission_user_{0}", AuthorityHelper.GetId());
            string keymenu = key + "_menu_";//menu
            List<Models.Module> model = CacheHelper.GetCache(keymenu) as List<Models.Module>;

            if (model.IsNullThis())
            {
                #region 获取可显示的权限菜单,用时1秒左右

                model = new List<Models.Module>();

                var lisMenu_ModuleId = new List<int>();
                var listIdsIsShow = new List<int>();

                Administrator admin = _administratorContract.Administrators.Where(c => c.Id == AuthorityHelper.OperatorId).FirstOrDefault();
                if (admin.IsNotNull())
                {
                    #region 用户自身可显示权限菜单

                    //var aadminper = admin.AdministratorPermissionRelations.Where(w => w.IsShow != false && !w.IsDeleted && w.IsEnabled).Select(s => s.PermissionId.Value).ToList();
                    //listIdsIsShow.AddRanges(aadminper);

                    #endregion

                    #region 用户所属角色可显示权限菜单
                    var aroleper = admin.Roles
                        .Where(w => !w.IsDeleted && w.IsEnabled)
                        .SelectMany(s => s.ARolePermissionRelations).Where(w => w.IsShow != false && !w.IsDeleted && w.IsEnabled)
                        .Select(s => s.PermissionsId.Value).ToList();
                    listIdsIsShow.AddRanges(aroleper);
                    #endregion

                    #region 用户所属部门可显示权限菜单
                    //if (!admin.Department.IsDeleted && admin.Department.IsEnabled)
                    //{
                    //    var adepper = admin.Department
                    //    .ADepartmentPermissionRelations.Where(w => w.IsShow != false && !w.IsDeleted && w.IsEnabled).Select(s => s.PermissionsId.Value).ToList();
                    //    listIdsIsShow.AddRanges(adepper);
                    //}
                    #endregion

                    #region 用户所属组可显示权限菜单
                    //var agroupper = admin.Groups
                    //    .Where(w => !w.IsDeleted && w.IsEnabled)
                    //    .SelectMany(s => s.AGroupPermissionRelations).Where(w => w.IsShow != false && !w.IsDeleted && w.IsEnabled).Select(s => s.PermissionsId.Value).ToList();
                    //listIdsIsShow.AddRanges(agroupper);
                    #endregion

                    listIdsIsShow = listIdsIsShow.Distinct().ToList();
                }

                var listmoduleids = CacheAccess.GetPermissions(_permissionContract).Where(w => listIdsIsShow.Contains(w.Id) && !w.IsDeleted && w.IsEnabled).Select(s => s.ModuleId).Distinct().ToList();

                lisMenu_ModuleId.AddRanges(listmoduleids);

                #region 组建层级关系

                if (lisMenu_ModuleId.IsNotNullOrEmptyThis())
                {
                    var modelcontains = _moduleContract.Modules.Where(w => lisMenu_ModuleId.Contains(w.Id) && !w.IsDeleted && w.IsEnabled).ToList();
                    if (modelcontains.IsNotNullOrEmptyThis())
                    {
                        foreach (var item in modelcontains)
                        {
                            item.Children.Clear();
                            treepar(item, model);
                        }
                    }
                }
                model = model.OrderBy(m => m.Sequence).ToList();

                #endregion

                AggregateCacheDependency deps = new AggregateCacheDependency();
                deps.Add(new SqlCacheDependency(DBName, "A_Permission"));
                deps.Add(new SqlCacheDependency(DBName, "A_Role"));
                //deps.Add(new SqlCacheDependency(DBName, "A_Group"));
                //deps.Add(new SqlCacheDependency(DBName, "A_Department"));
                deps.Add(new SqlCacheDependency(DBName, "A_Module"));
                CacheHelper.SetCache(keymenu, model, deps, new TimeSpan(24, 0, 0));

                #endregion
            }

            return model;
        }
        static void treepar(Models.Module par, List<Models.Module> list)
        {
            if (par.Parent == null)//父类不存在，达到顶层
            {
                if (!list.Contains(par))
                {
                    list.Add(par);
                }
            }
            else
            {
                var mod = treechild(par.Parent, list);//查看父类是否已经存在，存在则将当前类归属到存在的父类之下
                if (mod == null)
                {
                    par.Parent.Children.Clear();
                    par.Parent.Children.Add(par);
                    par.Parent.Children = par.Parent.Children.OrderBy(o => o.Sequence).ToList();
                    treepar(par.Parent, list);
                }
                else
                {
                    mod.Children.Add(par);
                    mod.Children = mod.Children.OrderBy(o => o.Sequence).ToList();
                }
            }
        }
        static Models.Module treechild(Models.Module par, List<Models.Module> list)
        {
            if (!list.Exists(c => c.Id == par.Id))
            {
                var childs = list.SelectMany(s => s.Children).ToList();
                if (childs.Count > 0)
                {
                    return treechild(par, childs);
                }
            }
            else
            {
                return list.First(f => f.Id == par.Id);
            }
            return null;
        }

        #endregion

        #region 获取当前会员可以显示的菜单

        //public static List<MemberModule> GetCurrentMemberMenuModule(IMemberContract _memberContract,IMemberModuleContract _memberModuleContract)
        //{
        //    string key = string.Format("_permission_member_{0}", AuthorityMemberHelper.GetId());
        //    string keymenu = key + "_menu_";//menu
        //    List<MemberModule> model = CacheHelper.GetCache(keymenu) as List<MemberModule>;

        //    if (model.IsNullThis())
        //    {
        //        model = new List<MemberModule>();

        //        Member member = _memberContract.Members.Where(w => w.Id == AuthorityMemberHelper.OperatorId).AsNoTracking().FirstOrDefault();
        //        if (member.IsNotNull())
        //        {
        //            var list = member.MemberRoles.Where(s => s.IsEnabled && !s.IsDeleted).SelectMany(s => s.MemberModules.Where(w => w.IsEnabled && !w.IsDeleted)).Distinct().ToList();
        //            #region 组建层级关系

        //            var listpar = list.Where(w => w.ParentId == null).OrderBy(o => o.Sequence).ToList();
        //            foreach (var item in listpar)
        //            {
        //                item.Children = list.Where(w => w.ParentId == item.Id).OrderBy(o => o.Sequence).ToList();
        //                model.Add(item);
        //            }

        //            #endregion
        //        }

        //        AggregateCacheDependency deps = new AggregateCacheDependency();
        //        deps.Add(new SqlCacheDependency(DBName, "A_MemberRole_MemberModule_Relation"));
        //        deps.Add(new SqlCacheDependency(DBName, "A_Member_MemberRole_Relation"));
        //        deps.Add(new SqlCacheDependency(DBName, "M_Member"));
        //        deps.Add(new SqlCacheDependency(DBName, "A_MemberModule"));
        //        CacheHelper.SetCache(keymenu, model, deps, new TimeSpan(24, 0, 0));
        //    }
        //    return model;
        //}

        #endregion

        #region 模板
        public static List<SelectListItem> GetTemplates(ITemplateContract _templateContract, bool showtit, TemplateTypeFlag typeflag = TemplateTypeFlag.PC)
        {
            List<SelectListItem> resli = CacheHelper.GetCache("template_sel_1102") as List<SelectListItem>;
            if (resli == null)
            {
                var lis = GetTemplates(_templateContract);
                if (lis != null)
                {
                    var li = lis.Select(c => new SelectListItem()
                    {
                        Value = c.Id.ToString(),
                        Text = c.TemplateName,
                        Selected = typeflag == TemplateTypeFlag.手机 ? c.IsDefaultPhone : c.IsDefault
                    }).ToList();
                    resli = CacheAccess.Clone<List<SelectListItem>>(li);
                }

                if (resli == null)
                {
                    resli.Insert(0, new SelectListItem()
                    {
                        Value = "-1",
                        Text = "请先添加模板"
                    });
                }
                if (resli.Count == 0 && showtit)
                {
                    resli.Insert(0, new SelectListItem()
                    {
                        Value = "-1",
                        Text = "请先选择模板"
                    });
                }
            }

            return resli;
        }
        public static List<Template> GetTemplates(ITemplateContract _templateContract)
        {
            List<Template> li = CacheHelper.GetCache("template_li_1102") as List<Template>;
            if (li == null)
            {
                li = _templateContract.Templates.Where(x => x.TemplateType == (int)TemplateFlag.Product && x.IsEnabled && !x.IsDeleted).OrderByDescending(x => x.IsDefault == true).ToList();
                CacheHelper.SetCache("template_li_1102", li, new SqlCacheDependency(DBName, "T_Template"));
            }
            return li;
        }
        #endregion

        private static object GetAssistants(IProductOrigNumberContract _productOrigNumContr)
        {
            string _key = "1102_assistant_92";
            var cha = CacheHelper.GetCache(_key);
            if (cha == null)
            {
                var li = _productOrigNumContr.OrigNumbs.Select(c => new ProductOriginNumberDto
                {
                    AssistantNum = c.AssistantNum,
                    AssistantNumberOfInt = c.AssistantNumberOfInt,
                    CategoryId = c.CategoryId,
                    BrandId = c.BrandId,
                    OriginNumber = c.OriginNumber
                }).Distinct().ToList();

                Dictionary<string, List<ProductOriginNumberDto>> dic = li.GroupBy(g => g.BrandId + "_" + g.CategoryId).ToDictionary(t => t.Key, item => item.ToList());

                AggregateCacheDependency chchs = new AggregateCacheDependency();
                chchs.Add(new SqlCacheDependency(DBName, "P_Brand"));
                chchs.Add(new SqlCacheDependency(DBName, "P_Category"));
                chchs.Add(new SqlCacheDependency(DBName, "P_ProductOrigNumber"));

                if (dic.IsNotNull())
                    CacheHelper.SetCache(_key, dic, chchs, new TimeSpan(0, 3, 0, 0));

                cha = dic;
            }
            return cha;
        }
        /// <summary>
        ///获取辅助号(生成商品款号用)
        /// </summary>
        /// <param name="orignum"></param>
        /// <param name="_productOrigNumContr"></param>
        /// <param name="_productContract"></param>
        /// <returns></returns>
        public static string GetAssistantNum(string origNumber, int brandId, int categoryId, IProductOrigNumberContract _productOrigNumContr, IProductContract _productContract)
        {
            string _key = brandId + "_" + categoryId;
            string _allkey = "1123_assistantnum_0830";
            bool isChange = false;
            string assNum = string.Empty;

            #region 默认值
            var ponNew = new ProductOriginNumberDto()
            {
                AssistantNum = "001",
                AssistantNumberOfInt = 1,
                CategoryId = categoryId,
                BrandId = brandId,
                OriginNumber = origNumber
            };
            #endregion

            lock (lockflg)
            {
                var ponDtos = CacheHelper.GetCache(_allkey) as Dictionary<string, List<ProductOriginNumberDto>>;
                if (ponDtos == null)
                {
                    ponDtos = GetAssistants(_productOrigNumContr) as Dictionary<string, List<ProductOriginNumberDto>>;
                }

                if (ponDtos.IsNotNullOrEmptyThis())
                {
                    var curFd = ponDtos.FirstOrDefault(w => w.Key == _key);
                    if (curFd.Key.IsNotNull())
                    {
                        var curBraCate = curFd.Value;
                        if (curBraCate.IsNotNull())
                        {
                            var cur = curBraCate.Where(f => f.OriginNumber == origNumber).OrderByDescending(o => o.AssistantNumberOfInt).FirstOrDefault();
                            if (cur.IsNotNull())
                            {
                                assNum = cur.AssistantNum;
                            }
                            else
                            {
                                ponNew.AssistantNumberOfInt = curBraCate.Max(m => m.AssistantNumberOfInt) + 1;
                                Math36 math = new Math36();
                                ponNew.AssistantNum = math.To36(ponNew.AssistantNumberOfInt);
                                curBraCate.Add(ponNew);
                                isChange = true;
                            }
                        }
                        else
                        {
                            curBraCate = new List<ProductOriginNumberDto>() { ponNew };
                            isChange = true;
                        }
                    }
                    else
                    {
                        ponDtos.Add(_key, new List<ProductOriginNumberDto>() { ponNew });
                        isChange = true;
                    }
                }
                else
                {
                    ponDtos = new Dictionary<string, List<ProductOriginNumberDto>>();
                    ponDtos.Add(_key, new List<ProductOriginNumberDto>() { ponNew });
                    isChange = true;
                }

                if (isChange)
                {
                    assNum = ponNew.AssistantNum;
                    CacheHelper.SetCache(_allkey, ponDtos, new TimeSpan(0, 1, 0, 0));
                }

                return assNum.PadLeft(3, '0');
            }
        }

        //获取可以参与的商品活动
        public static List<SalesCampaignDto> GetSalesCampaign(ISalesCampaignContract _salesCampaignContract)
        {
            List<SalesCampaignDto> camplist = CacheHelper.GetCache("campli_li_9211") as List<SalesCampaignDto>;
            if (camplist == null)
            {
                var camps = _salesCampaignContract.SalesCampaigns.Where(c => c.IsDeleted == false && c.IsEnabled == true).ToList();
                Mapper.CreateMap<SalesCampaign, SalesCampaignDto>().ForMember(c => c.BigProdNums,
                    t => t.MapFrom(x => x.ProductOriginNumbers.Select(f => f.BigProdNum).ToArray()))
                          .ForMember(c => c.ISPass, x => x.MapFrom(t => t.CampaignEndTime.CompareTo(DateTime.Now) < 0));

                camplist = Mapper.Map<List<SalesCampaign>, List<SalesCampaignDto>>(camps);
            }
            List<SalesCampaignDto> sales = CacheAccess.Clone<List<SalesCampaignDto>>(camplist);
            return sales;
        }


        #region 库存操作

        /// <summary>
        /// 获取可访问的库存
        /// </summary>
        public static IQueryable<Inventory> GetAccessibleInventorys(IInventoryContract _inventoryContract, IStoreContract _storeContract)
        {
            var storageids = _storeContract.QueryManageStorageId(AuthorityHelper.OperatorId.Value);
            return _inventoryContract.Inventorys.Where(i => !i.IsDeleted && i.IsEnabled && storageids.Contains(i.StorageId));
        }


        public static IQueryable<InventoryRecord> GetInventoryRecords(IInventoryRecordContract _inventoryContract, IStoreContract _storeContract)
        {
            var storageids = _storeContract.QueryManageStorageId(AuthorityHelper.OperatorId.Value);
            return _inventoryContract.InventoryRecords.Where(i => storageids.Contains(i.StorageId));
        }

        #endregion



        #region other
        public static string DBName
        {
            get { return "ZeroStore"; }

        }
        public static T Clone<T>(T RealObject)
        {
            T t = default(T);
            using (Stream stream = new MemoryStream())
            {

                try
                {
                    XmlSerializer xmlser = new XmlSerializer(typeof(T));
                    xmlser.Serialize(stream, RealObject);
                    stream.Seek(0, SeekOrigin.Begin);
                    t = (T)xmlser.Deserialize(stream);
                }
                catch (Exception)
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    BinaryFormatter binary = new BinaryFormatter();
                    binary.Serialize(stream, RealObject);
                    stream.Seek(0, SeekOrigin.Begin);
                    t = (T)binary.Deserialize(stream);
                }
                return t;
            }
        }

        public static void Set(string key, object val)
        {
            string caKey = "_cah" + key + "_";
            CacheHelper.SetCache(caKey, val);
        }

        public static void Set(string key, object val, int minutes)
        {
            string caKey = "_cah" + key + "_";
            CacheHelper.SetCache(caKey, val, TimeSpan.FromMinutes(minutes));
        }

        public static object Get(string key)
        {
            string caKey = "_cah" + key + "_";
            return CacheHelper.GetCache(caKey);
        }

        public static void SaveHttpContextState(HttpContext hc = null)
        {
            HttpContext httpcontext = hc ?? HttpContext.Current;

            Set("_httpcontext_", httpcontext);
        }

        public static HttpContext GetHttpContext()
        {
            return Get("_httpcontext_") as HttpContext;
        }

        #endregion

        /// <summary>
        /// 获取可用的商品搭配属性
        /// </summary>
        /// <param name="_productAttributeContract"></param>
        /// <returns></returns>
        public static List<ProductAttributeDto> GetProductAttributeDtos(IProductAttributeContract _productAttributeContract)
        {
            var li = CacheHelper.GetCache("_productAttri_9211_") as List<ProductAttributeDto>;
            if (li == null || li.Count == 0)
            {
                li = new List<ProductAttributeDto>();
                var t = _productAttributeContract.ProductAttributes.Where(c => c.IsEnabled && !c.IsDeleted).ToList();
                List<ProductAttributeDto> tem =
                    Mapper.Map<List<ProductAttributeDto>>(t);
                CacheHelper.SetCache("_productAttri_9211_", tem, new SqlCacheDependency(DBName, "P_Product_Attribute"));
                li = Clone(tem);
            }
            return li;
        }

        /// <summary>
        /// 获取搭配风格
        /// </summary>
        /// <param name="_collocationContract"></param>
        /// <param name="hasTitle"></param>
        /// <returns></returns>
        public static List<SelectListItem> GetCollocationStyle(IProductAttributeContract _productAttributeContract, bool hasTitle = true)
        {
            var li = CacheHelper.GetCache("_collocat_style_1102") as List<SelectListItem>;
            if (li == null || li.Count == 0)
            {
                li = new List<SelectListItem>();
                var te =
                    GetProductAttributeDtos(_productAttributeContract)
                        .FirstOrDefault(c => c.AttributeName.Trim() == "风格");
                if (te != null)
                {
                    int id = te.Id;
                    var temli = GetProductAttributeDtos(_productAttributeContract)
                        .Where(c => c.ParentId == id)
                        .Select(c => new SelectListItem()
                        {
                            Text = StringHelper.GetPrefix(1) + c.AttributeName,
                            Value = c.Id.ToString()
                        }).ToList();
                    CacheHelper.SetCache("_collocat_style_1102", temli,
                        new SqlCacheDependency(DBName, "P_Product_Attribute"));
                    li = temli;
                }
            }
            var resuli = Clone(li);
            if (hasTitle)
            {
                resuli.Insert(0, new SelectListItem() { Text = "下拉选择", Value = "" });
            }
            return resuli;
        }

        /// <summary>
        /// 获取搭配场合
        /// </summary>
        /// <param name="_productAttributeContract"></param>
        /// <param name="hasTitle"></param>
        /// <returns></returns>
        public static List<SelectListItem> GetCollocationSituation(IProductAttributeContract _productAttributeContract, bool hasTitle = true)
        {

            var li = CacheHelper.GetCache("_collocat_Situation_9211") as List<SelectListItem>;
            if (li == null || li.Count == 0)
            {
                li = new List<SelectListItem>();
                var te =
                    GetProductAttributeDtos(_productAttributeContract)
                        .FirstOrDefault(c => c.AttributeName.Trim() == "场合");
                if (te != null)
                {
                    int id = te.Id;
                    var temli = GetProductAttributeDtos(_productAttributeContract)
                        .Where(c => c.ParentId == id)
                        .Select(c => new SelectListItem()
                        {
                            Text = StringHelper.GetPrefix(1) + c.AttributeName,
                            Value = c.Id.ToString()
                        }).ToList();
                    CacheHelper.SetCache("_collocat_Situation_9211", temli,
                        new SqlCacheDependency(DBName, "P_Product_Attribute"));
                    li = temli;
                }
            }
            var resuli = Clone(li);
            if (hasTitle)
            {
                resuli.Insert(0, new SelectListItem() { Text = "拉下选择", Value = "" });
            }
            return resuli;
        }
        /// <summary>
        /// 商品管理页面，获取一级搭配属性
        /// </summary>
        /// <param name="_collocationContract"></param>
        /// <param name="hasTitle"></param>
        /// <returns></returns>
        public static List<SelectListItem> GetOneCollo(IProductAttributeContract _productAttributeContract, bool hasTitle)
        {
            var li = CacheHelper.GetCache("_OneCollo_9211_") as List<SelectListItem>;
            if (li == null)
            {

                li = _productAttributeContract.ProductAttributes.Where(c => c.IsEnabled && !c.IsDeleted && c.ParentId == null).Select(c => new { c.AttributeName, c.Id, c.ParentId }).ToList()
                     .Select(c => new SelectListItem()
                     {
                         Text = StringHelper.GetPrefix(1) + c.AttributeName,
                         Value = c.Id.ToString()
                     }).ToList();
                CacheHelper.SetCache("_OneCollo_9211_", li, new SqlCacheDependency(DBName, "P_Product_Attribute"));

            }
            var t = Clone(li);
            if (hasTitle)
                t.Insert(0, new SelectListItem() { Text = "选择一级搭配属性", Value = "" });
            return t;
        }


        public static int GetOrderblankMaxId(IOrderblankContract _orderblankContract)
        {
            lock (lockflg)
            {

                var key = "orderblank_numb_1192";
                var val = CacheHelper.GetCache(key);
                var retval = 1;
                if (val == null)
                {
                    val =
                        _orderblankContract.Orderblanks.OrderByDescending(c => c.Id).Select(c => c.Id).FirstOrDefault();

                }
                retval = Convert.ToInt32(val) + 1;

                CacheHelper.SetCache(key, retval, new SqlCacheDependency(DBName, "W_OrderBlank"),
                    new TimeSpan(0, 0, 5, 0));
                return (int)val;
            }
        }


        /// <summary>
        /// 获取当前最大的采购单序号
        /// </summary>
        /// <param name="_purchaseContract"></param>
        /// <returns></returns>
        public static int GetBuyNumber(IPurchaseContract _purchaseContract)
        {
            var key = "purchase_num_1192";
            var max = 0;
            var curnum = CacheAccess.Get(key) as string;
            if (string.IsNullOrEmpty(curnum))
            {
                if (_purchaseContract != null && _purchaseContract.Purchases.Any())
                    curnum = _purchaseContract.Purchases.Max(c => c.Id).ToString();

            }
            if (!string.IsNullOrEmpty(curnum))
            {
                max = int.Parse(curnum) + 1;
                CacheAccess.Set(key, max, 10);
            }
            return max;
        }
        /// <summary>
        /// 获取运营统计的消费支出类型
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> GetSpendType()
        {
            /*本来是应该将消费类型存入数据库，在Property下的StoreSpendType*/
            List<SelectListItem> li = new List<SelectListItem>();
            foreach (int myCode in Enum.GetValues(typeof(SpendType)))
            {
                string strName = Enum.GetName(typeof(SpendType), myCode);//获取名称
                string name = GetDescriptionText<SpendType>((SpendType)myCode);
                li.Add(new SelectListItem()
                {
                    Value = myCode.ToString(),
                    Text = name
                });
            }
            return li;
        }
        public static string GetDescriptionText<T>(T enumName)
        {
            string str = enumName.ToString();
            System.Reflection.FieldInfo field = enumName.GetType().GetField(str);
            object[] objs = field.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (objs.Length == 0) return str;
            DescriptionAttribute da = (DescriptionAttribute)objs[0];
            return da.Description;
        }
        /// <summary>
        /// 获取当前启用的主题
        /// </summary>
        /// <param name="_templateThemeContract"></param>
        /// <returns></returns>
        public static TemplateTheme GetCurTheme(ITemplateThemeContract _templateThemeContract, TemplateThemeFlag themeFlag)
        {
            string key = string.Format("_theme_default_flag_{0}", themeFlag);
            TemplateTheme theme = CacheHelper.GetCache(key) as TemplateTheme;

            #region 获取当前启用的主题

            if (theme == null)
            {
                theme = _templateThemeContract.templateThemes.FirstOrDefault(f => f.IsEnabled && !f.IsDeleted && f.IsDefault && themeFlag == f.ThemeFlag);
                AggregateCacheDependency deps = new AggregateCacheDependency();
                deps.Add(new SqlCacheDependency(DBName, "T_TemplateTheme"));
                if (theme.IsNotNull())
                    CacheHelper.SetCache(key, theme, deps, new TimeSpan(24, 0, 0));
            }

            #endregion

            return theme;
        }

        #region 员工类型
        //public static List<SelectListItem> GetAdministratorTypeListItem(IAdministratorTypeContract _AdministratorTypeContract, bool hasTitle)
        //{
        //    List<SelectListItem> li = CacheHelper.GetCache("AdministratorType_sel_20170418") as List<SelectListItem>;
        //    if (li == null)
        //    {
        //        li = _AdministratorTypeContract.Entities.Where(c => c.IsDeleted == false && c.IsEnabled == true).Select(m => new SelectListItem
        //        {
        //            Text = m.TypeName,
        //            Value = m.Id.ToString()
        //        }).ToList();
        //        CacheHelper.SetCache("AdministratorType_sel_20170418", li, new SqlCacheDependency(DBName, "A_Administrator_Type"));
        //    }
        //    var resli = CacheAccess.Clone<List<SelectListItem>>(li);
        //    if (hasTitle)
        //    {
        //        resli.Insert(0, new SelectListItem()
        //        {
        //            Text = "选择类型",
        //            Value = "",
        //        });
        //    }
        //    return resli;
        //}

        #endregion

        #region 会员角色

        //public static List<SelectListItem> GetMemberRole(IMemberRoleContract _memberRoleContract, bool hasHit = false)
        //{
        //    List<SelectListItem> li = CacheHelper.GetCache("MemberRole_sel_201705171155") as List<SelectListItem>;
        //    if (li == null)
        //    {
        //        li = _memberRoleContract.Entities.Where(c => c.IsDeleted == false && c.IsEnabled == true).Select(m => new SelectListItem
        //        {
        //            Text = m.Name,
        //            Value = m.Id + ""
        //        }).ToList();
        //        CacheHelper.SetCache("MemberRole_sel_201705171155", li, new SqlCacheDependency(DBName, "M_MemberRole"));
        //    }
        //    var resli = CacheAccess.Clone<List<SelectListItem>>(li);
        //    if (hasHit)
        //    {
        //        resli.Insert(0, new SelectListItem()
        //        {
        //            Text = "请选择",
        //            Value = "",
        //        });
        //    }
        //    return resli;
        //}

        #endregion
    }
}
