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
using Whiskey.ZeroStore.ERP.Transfers.APIEntities.MemberCollo;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Product;
using System.Text;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    public class ColloCalendarService : ServiceBase, IColloCalendarContract
    {
        #region 初始化业务层操作对象
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(ColloCalendarService));
        private readonly IRepository<ColloCalendar, int> _colloCalendarRepository;
        public ColloCalendarService(IRepository<ColloCalendar, int> colloCalendarRepository)
            : base(colloCalendarRepository.UnitOfWork)
        {
            _colloCalendarRepository = colloCalendarRepository;
        }
        #endregion

        #region 获取数据集
                
        /// <summary>
        /// 获取数据集
        /// </summary>
        public IQueryable<ColloCalendar> ColloCalendars
        {
            get { return _colloCalendarRepository.Entities; }
        }
        #endregion

        #region 添加数据
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dtos"></param>
        /// <returns></returns>
        public OperationResult Insert(params ColloCalendarDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                OperationResult result = _colloCalendarRepository.Insert(dtos,
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
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return new OperationResult(OperationResultType.Error, "服务器忙，请稍后");
            }
        }
        #endregion

        #region 更新数据
        
        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="dtos"></param>
        /// <returns></returns>
        public  OperationResult Update(params ColloCalendarDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                OperationResult result = _colloCalendarRepository.Update(dtos,
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
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return new OperationResult(OperationResultType.Error, "服务器忙，请稍后");
            }
        }
        #endregion
    }
}
