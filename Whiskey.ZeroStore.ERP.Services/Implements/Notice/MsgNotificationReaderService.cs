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
using Whiskey.Web.Http;
using Whiskey.Web.Extensions;
using Whiskey.Utility;
using Whiskey.Utility.Helper;
using Whiskey.Utility.Data;
using Whiskey.Utility.Web;
using Whiskey.Utility.Class;
using Whiskey.Utility.Extensions;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Notices;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    public class MsgNotificationReaderService : ServiceBase, IMsgNotificationContract
    {
        private readonly IRepository<MsgNotificationReader, int> _msgnotificationRepository;
        public MsgNotificationReaderService(
            IRepository<MsgNotificationReader, int> msgnotificationRepository
        )
            : base(msgnotificationRepository.UnitOfWork)
        {
            _msgnotificationRepository = msgnotificationRepository;
        }
        /// <summary>
        /// 获取单个数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public MsgNotificationReader View(int Id)
        {
            var entity = _msgnotificationRepository.GetByKey(Id);
            return entity;
        }
        public OperationResult Insert(params MsgNotificationReaderDto[] dtos)
        {
            dtos.CheckNotNull("dtos");
            OperationResult result = _msgnotificationRepository.Insert(dtos,
                dto =>
                {
                    
                },
                (dto, entity) =>
                {
                    entity.CreatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    return entity;
                });
            return result;
        }
        public IQueryable<MsgNotificationReader> MsgNotificationReaders { get { return _msgnotificationRepository.Entities; } }


        public OperationResult Update(params MsgNotificationReaderDto[] dtos)
        {
            dtos.CheckNotNull("dtos");
            OperationResult result = _msgnotificationRepository.Update(dtos,
                dto =>
                {

                },
                (dto, entity) =>
                {
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    return entity;
                });
            return result;
        }


        public OperationResult Update(params MsgNotificationReader[] dtos)
        {
            dtos.CheckNotNull("dtos");
            return _msgnotificationRepository.Update(dtos);
        }

        
    }
}
