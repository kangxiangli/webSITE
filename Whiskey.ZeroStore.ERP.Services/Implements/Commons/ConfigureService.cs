
using AutoMapper;
using System;
using System.Linq;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Utility;
using Whiskey.Utility.Data;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    public class ConfigureService : ServiceBase, IConfigureContract
    {
        private readonly IRepository<Configure, int> _ConfigureRepository;
        public ConfigureService(
            IRepository<Configure, int> _ConfigureRepository
            ) : base(_ConfigureRepository.UnitOfWork)
        {
            this._ConfigureRepository = _ConfigureRepository;
        }

        public IQueryable<Configure> Entities
        {
            get
            {
                return _ConfigureRepository.Entities;
            }
        }


        public OperationResult EnableOrDisable(bool enable, params int[] ids)
        {
            return OperationHelper.Try((opera) =>
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _ConfigureRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = enable;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                }
                return OperationHelper.ReturnOperationResult(UnitOfWork.SaveChanges() > 0, opera);
            }, enable ? Operation.Enable : Operation.Disable);
        }

        public OperationResult Insert(params Configure[] entities)
        {
            return OperationHelper.Try((opera) =>
            {
                entities.CheckNotNull("entities");
                OperationResult result = _ConfigureRepository.Insert(entities,
                entity =>
                {
                    entity.CreatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                });
                return result;
            }, Operation.Add);
        }

        public OperationResult DeleteOrRecovery(bool delete, params int[] ids)
        {
            return OperationHelper.Try((opera) =>
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _ConfigureRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = delete;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                }
                return OperationHelper.ReturnOperationResult(UnitOfWork.SaveChanges() > 0, opera);
            }, delete ? Operation.Delete : Operation.Recovery);
        }

        public OperationResult Update(params Configure[] entities)
        {
            return OperationHelper.Try((opera) =>
            {
                entities.CheckNotNull("entities");
                UnitOfWork.TransactionEnabled = true;
                OperationResult result = _ConfigureRepository.Update(entities,
                entity =>
                {
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                });
                int count = UnitOfWork.SaveChanges();
                return OperationHelper.ReturnOperationResult(count > 0, opera);
            }, Operation.Update);
        }

        public Configure View(int Id)
        {
            return _ConfigureRepository.GetByKey(Id);
        }

        public OperationResult Insert(params ConfigureDto[] dtos)
        {
            return OperationHelper.Try((opera) =>
            {
                dtos.CheckNotNull("dtos");
                UnitOfWork.TransactionEnabled = true;
                OperationResult result = _ConfigureRepository.Insert(dtos, a => { },
                    (dto, entity) =>
                    {
                        entity.CreatedTime = DateTime.Now;
                        entity.OperatorId = AuthorityHelper.OperatorId;
                        return entity;
                    });
                int count = UnitOfWork.SaveChanges();
                return OperationHelper.ReturnOperationResult(count > 0, opera);
            }, Operation.Add);
        }

        public OperationResult Update(params ConfigureDto[] dtos)
        {
            return OperationHelper.Try((opera) =>
            {
                dtos.CheckNotNull("dtos");
                UnitOfWork.TransactionEnabled = true;
                OperationResult result = _ConfigureRepository.Update(dtos, a => { },
                    (dto, entity) =>
                    {
                        entity.UpdatedTime = DateTime.Now;
                        entity.OperatorId = AuthorityHelper.OperatorId;
                        return entity;
                    });
                int count = UnitOfWork.SaveChanges();
                return OperationHelper.ReturnOperationResult(count > 0, opera);
            }, Operation.Update);
        }

        public ConfigureDto Edit(int Id)
        {
            var entity = _ConfigureRepository.GetByKey(Id);
            Mapper.CreateMap<Configure, ConfigureDto>();
            var dto = Mapper.Map<Configure, ConfigureDto>(entity);
            return dto;
        }

        #region 设置节点
        ///<summary>
        /// 设置节点
        ///</summary>
        ///<param name="DirName">配置文件名</param>
        /// <param name="xmlFileName">配置类名</param>
        /// <param name="QueryElementID">配置节点名称</param>
        /// <param name="innerText">配置的值</param>
        /// <returns></returns>
        public bool SetConfigure(string DirName, string xmlFileName, string QueryElementID, string innerText)
        {
            OperationResult opera = new OperationResult(OperationResultType.Error, "");
            try
            {
                Configure model = _ConfigureRepository.Entities.FirstOrDefault(c => c.DirName == DirName && c.ClassName == xmlFileName && c.NodeName == QueryElementID && !c.IsDeleted && c.IsEnabled);
                if (model == null)
                {
                    model = new Configure()
                    {
                        DirName = DirName,
                        ClassName = xmlFileName,
                        NodeName = QueryElementID,
                        Value = innerText
                    };
                    opera = Insert(model);
                }
                else
                {
                    model.Value = innerText;
                    opera = Update(model);
                }
            }
            catch (Exception ex)
            {
                opera.ResultType = OperationResultType.Error;
            }
            return opera.ResultType == OperationResultType.Success;
        }
        #endregion

        #region 获取配置值
        ///<summary>
        /// 获取配置值
        ///</summary>
        ///<param name="DirName">配置文件名</param>
        /// <param name="xmlFileName">配置类名</param>
        /// <param name="QueryElementID">配置节点名称</param>
        /// <param name="defValue">默认的值</param>
        /// <returns></returns>
        public string GetConfigureValue(string DirName, string xmlFileName, string QueryElementID, string defValue = "0")
        {
            OperationResult opera = new OperationResult(OperationResultType.Error, "");
            try
            {
                Configure model = _ConfigureRepository.Entities.FirstOrDefault(c => c.DirName == DirName && c.ClassName == xmlFileName && c.NodeName == QueryElementID && !c.IsDeleted && c.IsEnabled);
                if (model == null)
                {
                    return defValue;
                }
                return model.Value;
            }
            catch (Exception ex)
            {
                return defValue;
            }
        }
        #endregion
    }
}