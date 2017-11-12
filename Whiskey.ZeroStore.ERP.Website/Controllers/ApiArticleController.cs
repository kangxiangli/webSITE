using System; 
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Data;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;

namespace Whiskey.ZeroStore.ERP.Website.Controllers
{
    public class ApiArticleController : Controller
    {
        #region 声明操作对象
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(ApiArticleController));

        /// <summary>
        /// 初始文章分类化对象
        /// </summary>
        protected readonly IArticleItemContract _articleItemContract;
        /// <summary>
        /// 初始化文章操作对象
        /// </summary>
        protected readonly IArticleContract _articleContract;

        protected readonly ITemplateContract _TemplateContract;

        protected readonly IArticleImageContract _ArticleImageContract;

        protected readonly IAdministratorContract _adminContract;

        protected readonly IMemberContract _memberContract;
        /// <summary>
        /// 初始化业务层操作对象
        /// </summary>
        public ApiArticleController(IArticleContract articleContract,
            IArticleItemContract articleItemContract,
            ITemplateContract templateContract,
            IArticleImageContract articleImageContract,
            IAdministratorContract adminContract,
            IMemberContract memberContract)
        {
            _articleContract = articleContract;
            _articleItemContract = articleItemContract;
            _TemplateContract = templateContract;
            _ArticleImageContract = articleImageContract;
            _adminContract = adminContract;
            _memberContract = memberContract;
        }

        #endregion

        [HttpPost]
        public JsonResult GetList(int PageIndex=1,int PageSize=10)
        {
            OperationResult oper= new OperationResult(OperationResultType.Error,"服务器忙，请稍后");
            try
            {                
                string strId = Request["Id"];
                int id=int.Parse(strId);
                ArticleItem articleItem = _articleItemContract.View(id);
                List<int> listId = articleItem.Children.Select(x => x.Id).ToList();
                IQueryable<Article> listArticle = _articleContract.Articles.Where(x => x.IsDeleted == false && x.IsEnabled == true && listId.Contains(x.ArticleItemId));
                listArticle= listArticle.OrderBy(x => x.IsHot || x.IsRecommend || x.IsTop).ThenByDescending(x => x.Hits).OrderByDescending(x=> x.UpdatedTime);                
                IQueryable<Article> listArt= listArticle.Skip((PageIndex - 1) * PageSize).Take(PageSize);
                IQueryable<Administrator> listAdmin= _adminContract.Administrators;
                IQueryable<Member> listMember=_memberContract.Members;
                var entity = (from ar in listArt
                              join
                              ad in listAdmin
                              on
                              ar.AdminId equals ad.Id
                              join
                              me in listMember
                              on
                              ad.Member.MemberName equals me.UniquelyIdentifies
                              select new {
                                  ar.Id,
                                  ar.ArticlePath,
                                  ar.CoverImagePath,
                                  CreatedTime = ar.CreatedTime,
                                  ar.Hits,
                                  ar.Title,
                                  AdminName = ad.Member.MemberName,
                                  me.UserPhoto,
                                  ar.Summary,                              
                              }).ToList();
                var data = entity.Select(x => new
                {
                    x.Id,
                    x.ArticlePath,
                    x.CoverImagePath,
                    CreatedTime = x.CreatedTime.ToString("yyyy-MM-dd"),
                    x.Hits,
                    x.Title,
                    x.AdminName,
                    x.UserPhoto,
                    x.Summary,
                });
                oper.ResultType=OperationResultType.Success;
                oper.Data = data;
                oper.Message = "获取成功";
                return Json(oper);
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return Json(oper);                
            }
            
        }

        /// <summary>
        /// 获取 手机零时尚首页内容
        /// </summary>
        /// <returns></returns>
        public JsonResult GetIndexPageInfo()
        {
            var listArticle = _articleContract.Articles;
            var listArticleItem = _articleItemContract.ArticleItems.Where(x => !x.IsDeleted && x.IsEnabled);
            //var parentItem = listArticleItem.Where(x => x.ParentId == null)
            var childItem = listArticleItem.Where(x => x.ParentId != null);

            var listparentIds = new Dictionary<int, int>();
            listparentIds.Add(26, 1);//小蝶寄语
            listparentIds.Add(18, 2);//时尚开讲
            listparentIds.Add(30, 3);//样衣平台
            listparentIds.Add(38, 4);//搭配师工作室
            listparentIds.Add(28, 5);//搭配师后花园
            listparentIds.Add(32, 6);//合作平台

            var ids = listparentIds.Select(s => s.Key);

            var list = childItem.Where(w => ids.Contains(w.ParentId.Value))
                        .GroupBy(g => g.ParentId.Value).Select(s => new
                        {
                            Key = s.Key,
                            Items = s.SelectMany(ss => ss.Articles).Where(w => !w.IsDeleted && w.IsEnabled)
                            //.Where(w => w.TemplateId == 10)/*手机文章模板*/
                        })
                        .Select(s => new
                        {
                            SID = s.Key,
                            list = s.Items.Where(w => w.CoverImagePath != null && w.CoverImagePath != "").Select(ss => new
                            {
                                ss.Id,
                                ss.Title,
                                ss.CoverImagePath,
                                ss.Summary,
                                ss.JumpLink
                                //ss.Content
                            })
                        }).ToList().Select(s => new
                        {
                            SID = listparentIds[s.SID],
                            list = s.list
                        }).OrderBy(o => o.SID).ToList();

            return Json(list, JsonRequestBehavior.AllowGet);
        }
	}
}