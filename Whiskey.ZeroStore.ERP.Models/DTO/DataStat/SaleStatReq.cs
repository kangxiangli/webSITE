using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.ZeroStore.ERP.Models.Enums;

namespace Whiskey.ZeroStore.ERP.Models.DTO
{
    public class SaleStatReq
    {
        public int? AdminId { get; set; }
        /// <summary>
        /// 店铺
        /// </summary>
        public int? StoreId { get; set; }

        /// <summary>
        /// 统计类型
        /// </summary>
        public StatTypeEnum? StatType { get; set; }


        /// <summary>
        /// 统计日期天数
        /// </summary>
        public int? Days { get; set; }

        /// <summary>
        /// 统计开始日期
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// 统计结束日期
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// 季节
        /// </summary>
        public int? SeasonId { get; set; }

        /// <summary>
        /// 品类(基础品类)
        /// </summary>
        public int? CategoryId { get; set; }

        /// <summary>
        /// 品牌
        /// </summary>
        public int? BrandId { get; set; }

        /// <summary>
        /// 颜色
        /// </summary>
        public int? ColorId { get; set; }

        /// <summary>
        /// 尺码
        /// </summary>
        public int? SizeId { get; set; }


    }

    public class BrandStatReq : SaleStatReq
    {
        public BrandStatReq()
        {
            PageSize = 10;
        }
        /// <summary>
        /// 取前几条数据
        /// </summary>
        public int PageSize { get; set; }
    }

    public class BigProductNumStatReq : SaleStatReq
    {

        /// <summary>
        /// 款号
        /// </summary>
        public string BigProductNum { get; set; }

    }

    public class MemberStatReq
    {
        public int? StoreId { get; set; }
        public int? AdminId { get; set; }

        /// <summary>
        /// 偏好颜色统计范围
        /// </summary>
        public string PreferenceColorRanges { get; set; }

        /// <summary>
        /// 身高统计范围
        /// </summary>
        public string HeightRanges { get; set; }
        /// <summary>
        /// 体重统计范围
        /// </summary>

        public string WeightRanges { get; set; }

        /// <summary>
        /// 肩宽统计范围
        /// </summary>
        public string ShoudlerRanges { get; set; }

        /// <summary>
        /// 胸围统计范围
        /// </summary>
        public string BustRanges { get; set; }

        /// <summary>
        /// 腰围统计范围
        /// </summary>
        public string WaistLineRanges { get; set; }


        /// <summary>
        /// 臀围统计范围
        /// </summary>
        public string HipRanges { get; set; }


        public MemberStatTypeEnum? MemberStatType{ get; set; }

    }

    public class RangeEntry
    {
        public int MinValue { get; set; }
        public int MaxValue { get; set; }
    }



}
