using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Whiskey.ZeroStore.MobileApi.Areas.Collocations.Models
{
    public class MissionEntity
    {
        /// <summary>
        /// 任务类型
        /// </summary>
        public string MissionType { get; set; }

        /// <summary>
        /// 任务指派类型
        /// </summary>
        public string MissionAttrType { get; set; }

        /// <summary>
        /// 搭配师
        /// </summary>
        public string CollocationId { get; set; }

        /// <summary>
        /// 会员
        /// </summary>
        public string MemberId {get;set;}

        /// <summary>
        /// 品类
        /// </summary>
        public string CategoryId {get;set;}
        /// <summary>
        /// 颜色
        /// </summary>
        public string ColorId {get;set;}
        /// <summary>
        /// 场合
        /// </summary>
        public string SituationId {get;set;}

        /// <summary>
        /// 风格
        /// </summary>
        public string StyleId {get;set;}
        /// <summary>
        /// 季节
        /// </summary>
        public string SeasonId {get;set;}
        /// <summary>
        /// 价格
        /// </summary>
        public string Price {get;set;}
        /// <summary>
        /// 备注
        /// </summary>
        public string Notes { get; set; }
    }
}