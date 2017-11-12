
using System.Collections.Generic;
using System.Web.Mvc;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    public interface IMemberModuleContract : IBaseContract<MemberModule, MemberModuleDto>
    {
        OperationResult SetSeq(int Id, int SequenceType);
        List<SelectListItem> SelectList(bool hasHit = false);
    }
}

