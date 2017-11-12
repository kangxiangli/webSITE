using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    public interface IMemberIMProfileContract :IDependency
    {
        IQueryable<MemberIMProfile> Entities { get; }
        string GetToken(string memberId, string name, string avatar);

    }


  
}
