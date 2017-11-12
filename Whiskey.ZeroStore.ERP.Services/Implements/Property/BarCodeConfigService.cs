using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Utility;
using Whiskey.Utility.Data;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;

namespace Whiskey.ZeroStore.ERP.Services
{
    public class BarCodeConfigService : ServiceBase, IBarCodeConfigContract
    {
        private readonly IRepository<BarCodeConfig, int> _BarCodeConfigRepository;
        public BarCodeConfigService(
            IRepository<BarCodeConfig, int> _BarCodeConfigRepository
            ) : base(_BarCodeConfigRepository.UnitOfWork)
        {
            this._BarCodeConfigRepository = _BarCodeConfigRepository;
        }

        public IQueryable<BarCodeConfig> BarCodeConfigs
        {
            get
            {
                return _BarCodeConfigRepository.Entities;
            }
        }

        public BarCodeConfigDto Edit(int Id)
        {
            var entity = _BarCodeConfigRepository.GetByKey(Id);
            var dto = Mapper.Map<BarCodeConfig, BarCodeConfigDto>(entity);
            return dto;
        }

        public OperationResult Insert(params BarCodeConfigDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                OperationResult result = _BarCodeConfigRepository.Insert(dtos,
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

        public OperationResult Update(params BarCodeConfigDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                OperationResult result = _BarCodeConfigRepository.Update(dtos,
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
                return new OperationResult(OperationResultType.Error, "更新失败！错误如下：" + ex.Message);
            }
        }

        public BarCodeConfig View(int Id)
        {
            return _BarCodeConfigRepository.GetByKey(Id);
        }
    }
}
