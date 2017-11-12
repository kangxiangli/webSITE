using System.Collections.Generic;
using Whiskey.Web;

namespace Whiskey.ZeroStore.ERP.Services.Content
{
    public class DepartmentStores
    {
        public DepartmentStores()
        {
            Departments = new List<IdName>();
            Stores = new List<IdName>();
            Storages = new List<IdName>();
        }
        public IEnumerable<IdName> Departments { get; set; }
        public IEnumerable<IdName> Stores { get; set; }
        public IEnumerable<IdName> Storages { get; set; }
    }
}
