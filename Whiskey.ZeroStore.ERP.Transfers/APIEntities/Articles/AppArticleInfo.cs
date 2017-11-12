using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Transfers.APIEntities.Articles
{

    /// <summary>
    /// App文章
    /// </summary>
    public class AppArticleInfo
    {
        /// <summary>
        /// 动态图片集合
        /// </summary>
        public List<GalleryInfo> DynamicPictures { get; set; }

        /// <summary>
        /// 素材集合
        /// </summary>
        public List<GalleryInfo> Materials { get; set; }
        
        /// <summary>
        /// 图片集合
        /// </summary>
        public List<ArticleImageInfo> ImageInfos { get; set;}

        /// <summary>
        /// 文字集合
        /// </summary>
        public List<ArticleContentInfo> ContentInfos { get; set; }

        /// <summary>
        /// 模板类型
        /// </summary>
        public int TemplateType { get; set; }

        /// <summary>
        /// 唯一标识
        /// </summary>
        public string Num { get; set; }
        /// <summary>
        /// 操作类型
        /// </summary>
        public int OperationType { get; set; }

        /// <summary>
        /// 序号
        /// </summary>
        public int Sequence { get; set; }
    }

    public class Detail
    {
        public List<AppArticleInfo> AppArticleInfos { get; set; }
    }
}
