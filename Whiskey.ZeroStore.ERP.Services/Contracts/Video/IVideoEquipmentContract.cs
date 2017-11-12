﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
   public interface IVideoEquipmentContract: IDependency
    {
        IQueryable<VideoEquipment> VideoEquipments { get; }
        OperationResult Insert(params VideoEquipment[] equipments);
        OperationResult Update(params VideoEquipmentDto[] equipments);
        OperationResult Remove(bool state, params int[] ids);
        OperationResult Disable(bool state, params int[] ids);
    }
}