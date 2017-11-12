using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Whiskey.ZeroStore.MobileApi.Areas.Articles.Models
{
    public class ArticlePara
    {
        public int  MemberId {get;set;}

        public int ArticleId { get; set; }

        public string ArticleTitle { get; set; }

        public string CoverImage { get; set; }

        public string AppArticleInfos { get; set; }

        public bool IsEffect { get; set; }

        public string DeviceType { get; set; }

    }
}