using Microsoft.Owin;
using Owin;
using Whiskey.ZeroStore.ERP.WebMember.Extensions.Web;

[assembly: OwinStartup(typeof(Whiskey.ZeroStore.ERP.WebMember.SingalRConfig))]
namespace Whiskey.ZeroStore.ERP.WebMember
{
    public class SingalRConfig
    {
        public void Configuration(IAppBuilder app)
        {
            MvcApplication.AutofacMvcRegister();
            HubEntityContract.InitContract();
            app.MapSignalR();
            //GlobalHost.HubPipeline.RequireAuthentication();
        }
    }
}