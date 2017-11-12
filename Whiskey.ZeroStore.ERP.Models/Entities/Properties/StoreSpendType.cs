using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models.Entities.Properties
{
    public class StoreSpendType : EntityBase<int>
    {
        public string Name { get; set; }
        public string Note { get; set; }
    }
}
