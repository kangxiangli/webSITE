using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.ZeroStore.ERP.Models;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    public interface ITemplateArticleAttrRelationshipContract : IDependency
    {
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="template">数据载体对象</param>
        /// <returns></returns>
        bool Insert(TemplateArticleAttrRelationship templateArticleAttr);
        /// <summary>
        /// 修改数据
        /// </summary>
        /// <param name="templateArticleAttr"></param>
        /// <returns></returns>
        bool Update(TemplateArticleAttrRelationship templateArticleAttr);
        /// <summary>
        /// 获取数据集合
        /// </summary>
        IQueryable<TemplateArticleAttrRelationship> templateArticleAttrs { get; }
        /// <summary>
        /// 批量添加数据
        /// </summary>
        /// <param name="listTemplateArticleAttr"></param>
        /// <returns></returns>
        bool Insert(List<TemplateArticleAttrRelationship> listTemplateArticleAttr);
        /// <summary>
        /// 批量删除数据
        /// </summary>
        /// <param name="articleAttrId">栏目Id</param>
        /// <returns></returns>
        bool Delete(int articleAttrId);
    }
}
