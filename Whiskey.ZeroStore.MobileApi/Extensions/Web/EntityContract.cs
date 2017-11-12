using System.Web.Mvc;
using Whiskey.ZeroStore.ERP.Services.Contracts;

namespace Whiskey.ZeroStore.MobileApi.Extensions.Web
{
    public static class EntityContract
    {

        public static IAdministratorContract _adminContract { get; set; }
        public static IAttendanceStatisticsContract _attenStatisticsContract { get; set; }
        public static IAttendanceContract _attenContract { get; set; }
        public static IHolidayContract _holidayContract { get; set; }
        public static IModuleContract _moduleContract { get; set; }
        public static IPermissionContract _permissionContract { get; set; }
        public static INotificationContract _notificationContract { get; set; }
        public static IMemberContract _memberContract { get; set; }
        public static ITemplateNotificationContract _templateNotificationContract { get; set; }
        public static IStorageContract _storageContract { get; set; }
        public static IWorkTimeDetaileContract _workTimeDetaileContract { get; set; }
        public static IProductOrigNumberContract _productOriginNumberContract { get; set; }
        public static void InitContract()
        {
            _adminContract = DependencyResolver.Current.GetService<IAdministratorContract>();
            _attenContract = DependencyResolver.Current.GetService<IAttendanceContract>();
            _attenStatisticsContract = DependencyResolver.Current.GetService<IAttendanceStatisticsContract>();
            _holidayContract = DependencyResolver.Current.GetService<IHolidayContract>();

            _moduleContract = DependencyResolver.Current.GetService<IModuleContract>();
            _permissionContract = DependencyResolver.Current.GetService<IPermissionContract>();
            _notificationContract = DependencyResolver.Current.GetService<INotificationContract>();
            _memberContract = DependencyResolver.Current.GetService<IMemberContract>();
            _templateNotificationContract = DependencyResolver.Current.GetService<ITemplateNotificationContract>();
            _storageContract = DependencyResolver.Current.GetService<IStorageContract>();
            _workTimeDetaileContract = DependencyResolver.Current.GetService<IWorkTimeDetaileContract>();
            _productOriginNumberContract = DependencyResolver.Current.GetService<IProductOrigNumberContract>();
        }
    }
}