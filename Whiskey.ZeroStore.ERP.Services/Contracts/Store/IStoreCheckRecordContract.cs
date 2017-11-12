using System.Collections.Generic;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Models.DTO;
using Whiskey.ZeroStore.ERP.Transfers;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    public interface IStoreCheckRecordContract : IBaseContract<StoreCheckRecord, StoreCheckRecordDTO>
    {
        OperationResult Insert(StoreCheckRecordDTO dto, List<CheckInfoModel> checkInfos);
    }
}
