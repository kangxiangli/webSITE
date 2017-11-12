using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using AutoMapper;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Web.Helper;
using Whiskey.Web.SignalR;
using Whiskey.Web.Extensions;
using Whiskey.Utility;
using Whiskey.Utility.Data;
using Whiskey.Utility.Web;
using Whiskey.Utility.Class;
using Whiskey.Utility.Extensions;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Models;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    public interface IMsgNotificationContract : IDependency
    {
        IQueryable<MsgNotificationReader> MsgNotificationReaders { get; }
        MsgNotificationReader View(int Id);
        OperationResult Insert(params MsgNotificationReaderDto[] dtos);
        OperationResult Update(params MsgNotificationReaderDto[] dtos);
        OperationResult Update(params MsgNotificationReader[] dtos);

    }
}
