using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.ZeroStore.ERP.Models.Entities;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    public class ArticleImageService : ServiceBase, IArticleImageContract
    {
        /// <summary>
        /// 操作对象
        /// </summary>
        private readonly IRepository<ArticleImage, int> _articleImageRepository;
        /// <summary>
        /// 拿到上下文并赋值给操作对象
        /// </summary>
        /// <param name="articleRepository"></param>
        public ArticleImageService(IRepository<ArticleImage, int> articleImageRepository)
            : base(articleImageRepository.UnitOfWork)
        {
            _articleImageRepository = articleImageRepository;
        }

        public IQueryable<ArticleImage> ArticleImages
        {
            get { return _articleImageRepository.Entities; }
        }

        public TemplateType Insert(ArticleImage articleImage)
        {
            try
            {
                var result = _articleImageRepository.Insert(articleImage);
                if (result>0)
                {
                    return TemplateType.InsertSuccess;
                }
                else
                {
                    return TemplateType.InsertFail;
                }
            }
            catch (Exception)
            {
                return TemplateType.Error;
            }
        }

        public TemplateType CheckJSName(string Name)
        {
            try
            {
                var result = _articleImageRepository.Entities.Where(x => x.ImageName == Name);
                if (result.Count()>0)
                {
                    return TemplateType.Repeat;
                }
                else
                {
                    return TemplateType.NotRepeat;
                }
            }
            catch (Exception)
            {
                return TemplateType.Error;                
            }
        }
    }
}
