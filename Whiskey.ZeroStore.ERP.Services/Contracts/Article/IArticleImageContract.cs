using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models.Entities;
using Whiskey.ZeroStore.ERP.Transfers;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    public interface IArticleImageContract : IDependency
    {

        IQueryable<ArticleImage> ArticleImages { get; }

        //OperationResult Insert(params ArticleImage[] articleImage);
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="template">数据载体对象</param>
        /// <returns></returns>
        TemplateType Insert(ArticleImage articleImage);
        /// <summary>
        /// 校验JS名称
        /// </summary>
        /// <param name="Name">js名称</param>
        /// <returns></returns>
        TemplateType CheckJSName(string Name);
    }
}
