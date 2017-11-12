using System.Web.Mvc;
using Whiskey.ZeroStore.ERP.Services.Contracts;

namespace Whiskey.ZeroStore.MobileApi.Extensions.Web
{
    public class HubEntityContract
    {
        public static IAdministratorContract _adminContract { get; set; }

        public static INotificationContract _notificationContract { get; set; }

        public static IMsgNotificationContract _msgNotificationContract { get; set; }

        public static IMessagerContract _messageerContract { get; set; }

        public static INotificationQASystemContract _notificationQASystemContract { get; set; }

        public static void InitContract()
        {
            _adminContract = DependencyResolver.Current.GetService<IAdministratorContract>();
            _notificationContract = DependencyResolver.Current.GetService<INotificationContract>();
            _msgNotificationContract = DependencyResolver.Current.GetService<IMsgNotificationContract>();
            _messageerContract = DependencyResolver.Current.GetService<IMessagerContract>();
            _notificationQASystemContract = DependencyResolver.Current.GetService<INotificationQASystemContract>();
        }
    }
}