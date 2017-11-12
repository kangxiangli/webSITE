using System.Web.Mvc;
using Whiskey.ZeroStore.ERP.Services.Contracts;

namespace Whiskey.ZeroStore.ERP.WebMember.Extensions.Web
{
    public class HubEntityContract
    {
        public static IMemberContract _memberContract { get; set; }

        public static void InitContract()
        {
            _memberContract = DependencyResolver.Current.GetService<IMemberContract>();
        }
    }
}