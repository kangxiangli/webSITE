using System;
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
    public interface ITemplateNotificationContract : IDependency
    {
        OperationResult Update(params TemplateNotificationDto[] dtos);
        IQueryable<TemplateNotification> templateNotifications { get; }
        OperationResult Insert(params TemplateNotificationDto[] dtos);
        OperationResult Delete(params int[] ids);
        TemplateNotification View(int id);
        OperationResult Disable(params int[] ids);
        OperationResult Enable(params int[] ids);
    }
}
