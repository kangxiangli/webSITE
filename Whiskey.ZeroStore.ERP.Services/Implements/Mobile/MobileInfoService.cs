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
using System.Security.Cryptography;
using System.Text;
using Whiskey.Utility.Secutiry;
using System.Web.Mvc;

namespace Whiskey.ZeroStore.ERP.Services.Implements.Mobile
{
    public class MobileInfoService : ServiceBase, IMobileInfoContract
    {
        #region MobileInfoService

        #region 声明数据层操作对象
        private readonly IRepository<MobileInfo, int> _memberInfoRepository;

        public MobileInfoService(
            IRepository<MobileInfo, int> memberInfoRepository)
            : base(memberInfoRepository.UnitOfWork)
		{
            _memberInfoRepository = memberInfoRepository;
        }
        #endregion

        #region 添加数据
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dtos">要添加的DTO数据</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Insert(params MobileInfoDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");                 
                OperationResult result = _memberInfoRepository.Insert(dtos,
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
                return new OperationResult(OperationResultType.Error, "添加失败！错误如下：" + ex.Message);
            }
        }
        #endregion

        #endregion
    }
}
