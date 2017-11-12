using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Warehouses.Models
{
    public class StoreWithStorage<T>
    {
        public StoreWithStorage()
        {
            Children = new List<StoreWithStorage<T>>();
        }
        public T Id { get; set; }
        public string Name { get; set; }
        public List<StoreWithStorage<T>> Children { get; set; }

        public string Other { get; set; }
    }
}