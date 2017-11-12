using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Owin;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using Whiskey.ZeroStore.ERP.Website.Hubs;

[assembly: OwinStartup(typeof(Whiskey.ZeroStore.ERP.Website.SingalRConfig))]
namespace Whiskey.ZeroStore.ERP.Website
{
    public class SingalRConfig
    {
        public void Configuration(IAppBuilder app)
        {
            MvcApplication.AutofacMvcRegister();
            HubEntityContract.InitContract();
            app.UseCors(CorsOptions.AllowAll);
            app.MapSignalR();
            //app.MapSignalR<RetailMemberAPPLoginConnection>("/applogin");
            
            app.Map("/realtime/appConfirmLogin", map =>
            {
                map.RunSignalR<APIConfirmLoginConnection>();
            });
            //GlobalHost.HubPipeline.RequireAuthentication();
        }

    }
}