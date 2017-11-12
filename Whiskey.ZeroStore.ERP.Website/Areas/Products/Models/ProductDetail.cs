using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Products.Models
{
    public class ProductDetail
    {
        public int Id { get; set; }
        /// <summary>
        /// 模板ID
        /// </summary>
        public int TemplateId { get; set; }
        /// <summary>
        /// 手机模板Id
        /// </summary>
        public int TemplatePhoneId { get; set; }
        /// <summary>
        /// 模板内容
        /// </summary>
        public string TemplateContent { get; set; }
        /// <summary>
        /// 手机模板内容
        /// </summary>
        public string TemplatePhoneContent { get; set; }
    }
}