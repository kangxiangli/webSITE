using System.Configuration;
using System.Web.Caching;

namespace Whiskey.ZeroStore.ERP.MobileApi.App_Start
{
    //cache dependency config
    public static class CacheDependencyConfig
    {
        public static void SetDependency()
        {

            string conn = ConfigurationManager.AppSettings["Whiskey-Database-String"];
            string connString = ConfigurationManager.ConnectionStrings[conn].ConnectionString;
            string[] tables = new string[] { "W_Storage", "A_Department", "S_Store", "P_Product",
                "P_Brand", "P_Category", "P_Size", "P_Color", "P_Season", "W_Storage", "W_OrderBlank_Item", "W_OrderBlank", "A_Administrator",
                "M_Member",
                //"A_MemberRole_MemberModule_Relation","A_Member_MemberRole_Relation","A_MemberModule","M_MemberRole","A_Administrator_Type",
                "A_Module", "A_Permission", "A_Role", "T_Template", "P_ProductOrigNumber", "P_Product_Attribute", "T_TemplateTheme",
                "A_QrLogin", "O_JobPosition","S_StoreType","P_ProductCrowd" };
            SqlCacheDependencyAdmin.EnableNotifications(connString);
            SqlCacheDependencyAdmin.EnableTableForNotifications(connString, tables);
        }
    }
}