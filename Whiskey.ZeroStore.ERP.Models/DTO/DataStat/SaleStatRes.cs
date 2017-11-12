using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Models.DTO
{
    public class SaleStatRes
    {
        /// <summary>
        /// 当前店铺统计出的数量
        /// </summary>
        public int CountFromCurrentStore { get; set; }

        /// <summary>
        /// 所有店铺统计出的数量
        /// </summary>
        public int CountFromAllStore { get; set; }

        /// <summary>
        /// 统计起始时间
        /// </summary>
        public string StartTime { get; set; }

        /// <summary>
        /// 统计起始时间
        /// </summary>
        public string EndTime { get; set; }

    }

    public class BrandStatRes
    {
        public BrandStatRes()
        {
            TopBrands = new List<BrandStatEntry>();
        }
        public List<BrandStatEntry> TopBrands { get; set; }
        /// <summary>
        /// 统计起始时间
        /// </summary>
        public string StartTime { get; set; }

        /// <summary>
        /// 统计起始时间
        /// </summary>
        public string EndTime { get; set; }
    }


    public class CategoryStatRes
    {
        public CategoryStatRes()
        {
            TopCategories = new List<CategoryStatEntry>();
        }
        public List<CategoryStatEntry> TopCategories { get; set; }
        /// <summary>
        /// 统计起始时间
        /// </summary>
        public string StartTime { get; set; }

        /// <summary>
        /// 统计起始时间
        /// </summary>
        public string EndTime { get; set; }
    }



    public class BigProductNumStatRes
    {
        public BigProductNumStatRes()
        {
            Entries = new List<BigProductNumStatEntry>();
        }
        /// <summary>
        /// 统计起始时间
        /// </summary>
        public string StartTime { get; set; }

        /// <summary>
        /// 统计起始时间
        /// </summary>
        public string EndTime { get; set; }


        public List<BigProductNumStatEntry> Entries { get; set; }
    }

    public class BigProductNumStatEntry
    {
        /// <summary>
        /// colorId或sizeId
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// colorName或sizeName
        /// </summary>
        public string Name { get; set; }


        /// <summary>
        /// 当前店铺统计出的数量（销售/退货）
        /// </summary>
        public int CountFromCurrentStore { get; set; }

        /// <summary>
        /// 所有店铺统计出的数量（销售/退货）
        /// </summary>
        public int CountFromAllStore { get; set; }
    }


    public class BrandStatEntry
    {
        public int BrandId { get; set; }
        public string BrandName { get; set; }
        public int Quantity { get; set; }
    }

    public class CategoryStatEntry
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int Quantity { get; set; }
    }


    public class MemberStatEntry
    {
        /// <summary>
        /// colorId或sizeId
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// colorName或sizeName
        /// </summary>
        public string Name { get; set; }


        /// <summary>
        /// 当前店铺统计出的数量（销售/退货）
        /// </summary>
        public int CountFromCurrentStore { get; set; }

        /// <summary>
        /// 所有店铺统计出的数量（销售/退货）
        /// </summary>
        public int CountFromAllStore { get; set; }
    }

    public class MemberSizeStatEntry : MemberStatEntry
    {

    }

    public class MemberColorPreferenceStatEntry : MemberStatEntry { }
    public class MemberFigureCommonStatEntry
    {
        public int MinValue { get; set; }
        public int MaxValue { get; set; }
        public int CountFromAllStore { get; set; }
        public int CountFromCurrentStore { get; set; }
    }

    public class MemberFigureCommonStatRes {
        public MemberFigureCommonStatRes()
        {
            StatEntries = new List<MemberFigureCommonStatEntry>();
        }
        public int StoreId { get; set; }
        public string FigureStatType { get; set; }
        public List<MemberFigureCommonStatEntry> StatEntries { get; set; }
    }   
    public class MemberColorPreferenceStatRes
    {
        public int StoreId { get; set; }
        public MemberColorPreferenceStatRes()
        {
            ColorPreference = new Dictionary<string, MemberColorPreferenceStatEntry>();
        }
        public Dictionary<string, MemberColorPreferenceStatEntry> ColorPreference { get; set; }

    }

    public class MemberSizeStatRes
    {
        public MemberSizeStatRes()
        {
            TopSizeDict = new Dictionary<string, MemberSizeStatEntry>();
            BottomSizeDict = new Dictionary<string, MemberSizeStatEntry>();
        }
        public int StoreId { get; set; }
        public Dictionary<string, MemberSizeStatEntry> TopSizeDict { get; set; }
        public Dictionary<string, MemberSizeStatEntry> BottomSizeDict { get; set; }
    }


}
