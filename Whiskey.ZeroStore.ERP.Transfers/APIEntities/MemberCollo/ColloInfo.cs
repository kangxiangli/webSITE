using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Transfers.APIEntities.MemberCollo
{
    public class ColloInfo : MemberCollocationDto
    {
        /// <summary>
        /// 图片流字符串
        /// </summary>
        public string Image { get; set; }


        public int ImageId { get; set; }
        /// <summary>
        /// 图片路径
        /// </summary>
        public string ImagePath { get; set; }

        /// <summary>
        /// 搭配Id
        /// </summary>
        public int ColloId { get; set; }

        public List<ColloImage> ListColloImage { get; set; }

        public List<ColloText> ListColloText { get; set; }
    }
}
