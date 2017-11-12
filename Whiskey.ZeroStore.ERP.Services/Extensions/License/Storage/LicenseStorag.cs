using System.Collections.Generic;
using System.Linq;
using System.Web;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.ZeroStore.ERP.Services.Contracts;


namespace Whiskey.ZeroStore.ERP.Services.Extensions.License
{
    public class LicenseBase
    {
        protected HttpContext httpContext;
        public LicenseBase()
        {
            httpContext = HttpContext.Current ?? CacheAccess.GetHttpContext();
            HttpContext.Current = httpContext;

        }

    }

    //yxk 2016-1 判断用户操作仓库的权限
    public class LicenseStorag : LicenseBase
    {

        #region MyRegion

        protected IStorageContract _storageContract { get; set; }
        protected IAdministratorContract _administratorContract { get; set; }

        public int? operatId { get; set; }

        public LicenseStorag(IStorageContract storageContract, int? operat = null)
            : base()
        {
            if (operat == null)
            {
                operatId = AuthorityHelper.OperatorId;
            }
            _storageContract = storageContract;
            // _storageContract = storageContract;
        }

        private void Valid()
        {
            List<int> storageIds = CacheAccess.GetManagedStorage(_storageContract, _administratorContract).Select(s => s.Id).ToList();

            string key = "_storageids_auth_";

            httpContext.Items.Remove(key);
            httpContext.Items.Add("_storageids_auth_", storageIds);
            CacheAccess.SaveHttpContextState(httpContext);


        }
        /// <summary>
        /// 获取当前用户可以访问的仓库id
        /// </summary>
        /// <returns></returns>
        public List<int> GetStorageIdsForCurrentUser()
        {
            Valid();
            List<int> ids = httpContext.Items["_storageids_auth_"] as List<int>;
            
            return ids??new List<int>();
        }
        #endregion

    }
}