using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Transfers.APIEntities.Articles
{
    public class ArticleContentInfo:AppArticleBase
    {

        public string AlignStyle { get; set; }
        
        public string Content { get; set; }
        
        public string FontColor { get; set; }
        
        public string FontSize { get; set; }
         
        public virtual int ContentRow { get; set; }         
    }
}
