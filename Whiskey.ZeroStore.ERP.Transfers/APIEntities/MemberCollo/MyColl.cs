using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.ZeroStore.ERP.Models;

namespace Whiskey.ZeroStore.ERP.Transfers.APIEntities.MemberCollo
{
    public class MyColl : MemberCollocationDto
    {                          
        /// <summary>
        /// 图片流字符串
        /// </summary>
        public string Image { get; set; }
            
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
        /// 图片路径
        /// </summary>
        public string ImagePath { get; set; }

        /// <summary>
        /// 是否为搭配日历
        /// </summary>
        public int IsCallendar { get; set; }
        /// <summary>
        /// 零件图
        /// </summary>
        public List<ImageList> ImageList { get; set; }

        public List<TextList> TextList { get; set; }
        
    }
}
