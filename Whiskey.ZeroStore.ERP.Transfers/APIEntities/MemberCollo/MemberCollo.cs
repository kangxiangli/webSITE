using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Transfers.APIEntities.MemberCollo
{
    public class MemberCollo
    {       

        /// <summary>
        /// 会员Id
        /// </summary>
        public int MemberId { get; set; }

        /// <summary>
        /// 会员头像
        /// </summary>
        public string MemberImage { get; set; }

        /// <summary>
        /// 搭配Id
        /// </summary>
        public int ColloId { get; set; }

        /// <summary>
        /// 搭配名称
        /// </summary>
        public string ColloName { get; set; }

        /// <summary>
        /// 搭配颜色
        /// </summary>
        public int ColorId { get; set; }
        
        /// <summary>
        /// 搭配图       
        /// </summary>
        public string ColloImagePath { get; set; }

        /// <summary>
        /// 搭配备注
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// 评论数量
        /// </summary>
        public int CommentCount { get; set; }

        /// <summary>
        /// 点赞数量
        /// </summary>
        public int ApproveCount { get; set; }

        /// <summary>
        /// 是否赞
        /// </summary>
        public int IsApprove { get; set; }

        /// <summary>
        /// 温度
        /// </summary>
        public string Temperature { get; set; }

        /// <summary>
        /// 天气
        /// </summary>
        public string Weather { get; set; }

        /// <summary>
        /// 城市
        /// </summary>
        public string CityName { get; set; }
        /// <summary>
        /// 搭配素材图
        /// </summary>
        public List<string> ListImagePath { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 搭配时间
        /// </summary>
        public DateTime CollocationTime { get; set; }

        /// <summary>
        /// 是否删除
        /// </summary>
        public bool IsDeleted { get; set; }

        public CollocationTypeEnum CollocationType { get; set; }
    }

    public enum CollocationTypeEnum {
        Normal = 0,
        Recommend =1
    }

}
