using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Transfers;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    public interface IMobileInfoContract: IDependency
    {
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dtos"></param>
        /// <returns></returns>
        OperationResult Insert(params MobileInfoDto[] dtos);
    }
}
