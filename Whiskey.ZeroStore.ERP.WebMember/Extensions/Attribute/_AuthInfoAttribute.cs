using System;
using System.Linq;
using System.Web.Mvc;
using Whiskey.Utility.Extensions;
using Whiskey.Utility.Helper;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Services.Contracts;

namespace Whiskey.ZeroStore.ERP.WebMember.Extensions.Attribute
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class _AuthInfoAttribute : ActionFilterAttribute
    {
        public IMemberContract _memberContract { get; set; }
        public readonly string weburl = ConfigurationHelper.WebUrl;

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);

            var isAjaxView = filterContext.HttpContext.Request.IsAjaxRequest();

            var viewResult = filterContext.Result as ViewResult;

            if (viewResult.IsNotNull())
            {
                var ViewBag = viewResult.ViewBag;

                ViewBag.isAjaxView = isAjaxView;
                ViewBag.isLogin = AuthorityMemberHelper.IsVerified;

                if (AuthorityMemberHelper.IsVerified)
                {
                    var modMember = _memberContract.Members.FirstOrDefault(f => f.Id == AuthorityMemberHelper.OperatorId);
                    if (modMember.IsNotNull())
                    {
                        ViewBag.MemberId = modMember.Id;
                        ViewBag.MemberName = modMember.MemberName;
                        ViewBag.MemberPhoto = modMember.UserPhoto.IsNotNullAndEmpty() ? weburl + modMember.UserPhoto : string.Empty;
                    }
                }
            }
        }
    }
}