using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models.Entities.Properties;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    public interface IMaintainContract : IDependency
    {
        IQueryable<MaintainExtend> Maintains { get; }
        OperationResult Insert(params MaintainExtend[] maintain);
        OperationResult Update(params MaintainExtend[] maintain);


        OperationResult  Remove(params int[] ids);

        OperationResult Recovery(params int[] ids);
    }
}
