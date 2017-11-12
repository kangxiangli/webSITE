using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    public class TempalteArticleAttrRelationshipService : ServiceBase, ITemplateArticleAttrRelationshipContract
    {
        #region 声明数据层操作对象
        /// <summary>
        /// 声明操作对象
        /// </summary>
        private readonly IRepository<TemplateArticleAttrRelationship, int> _TemplateArticleAttr;
        /// <summary>
        /// 拿到上下文并赋值给操作对象
        /// </summary>
        /// <param name="articleRepository"></param>
        public TempalteArticleAttrRelationshipService(IRepository<TemplateArticleAttrRelationship, int> templateArticleAttrRepository)
            : base(templateArticleAttrRepository.UnitOfWork)
        {
            _TemplateArticleAttr = templateArticleAttrRepository;
        }
        #endregion

        #region 添加数据
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="templateArticleAttr"></param>
        /// <returns></returns>
        public bool Insert(TemplateArticleAttrRelationship templateArticleAttr)
        {
            try
            {
                int count = _TemplateArticleAttr.Insert(templateArticleAttr);
                if (count >0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
        /// <summary>
        /// 批量添加数据
        /// </summary>
        /// <param name="listTemplateArticleAttr">数据集</param>
        /// <returns></returns>
        public bool Insert(List<TemplateArticleAttrRelationship> listTemplateArticleAttr)
        {
            try
            {
                int count = _TemplateArticleAttr.Insert(listTemplateArticleAttr.AsEnumerable());
                if (count >0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;                
            }
        }
        #endregion
        public bool Update(TemplateArticleAttrRelationship templateArticleAttr)
        {
            try
            {                                          
                int count = _TemplateArticleAttr.Update(templateArticleAttr);
                if (count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }


        public IQueryable<TemplateArticleAttrRelationship> templateArticleAttrs
        {
            get { return _TemplateArticleAttr.Entities; }
        }
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="articleAttrId"></param>
        /// <returns></returns>
        public bool Delete(int articleAttrId)
        {
            try
            {
                 var templateArticleAttr= _TemplateArticleAttr.Entities.Where(x=>x.ArticleAttrId==articleAttrId);
                 int count =_TemplateArticleAttr.Delete(templateArticleAttr);
                 if (count > 0)
                 {
                     return true;
                 }
                 else 
                 {
                     return false;
                 }
            }
            catch (Exception)
            {
                return false;                
            }
        }
    }
}
