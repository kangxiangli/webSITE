
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
using Whiskey.ZeroStore.ERP.Models.Enums;
using Whiskey.Utility.Extensions;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    public class AppVerManageService : ServiceBase, IAppVerManageContract
    {
        private readonly IRepository<AppVerManage, int> _AppVerManageRepository;
        public AppVerManageService(
            IRepository<AppVerManage, int> _AppVerManageRepository
            ) : base(_AppVerManageRepository.UnitOfWork)
        {
            this._AppVerManageRepository = _AppVerManageRepository;
        }

        public IQueryable<AppVerManage> Entities
        {
            get
            {
                return _AppVerManageRepository.Entities;
            }
        }


        public OperationResult EnableOrDisable(bool enable, params int[] ids)
        {
            return OperationHelper.Try((opera) =>
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _AppVerManageRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = enable;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                }
                return OperationHelper.ReturnOperationResult(UnitOfWork.SaveChanges() > 0, opera);
            }, enable ? Operation.Enable : Operation.Disable);
        }

        public OperationResult Insert(params AppVerManage[] entities)
        {
            return OperationHelper.Try((opera) =>
            {
                entities.CheckNotNull("entities");
                OperationResult result = _AppVerManageRepository.Insert(entities,
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
                var entities = _AppVerManageRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = delete;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                }
                return OperationHelper.ReturnOperationResult(UnitOfWork.SaveChanges() > 0, opera);
            }, delete ? Operation.Delete : Operation.Recovery);
        }

        public OperationResult Update(params AppVerManage[] entities)
        {
            return OperationHelper.Try((opera) =>
            {
                entities.CheckNotNull("entities");
                UnitOfWork.TransactionEnabled = true;
                OperationResult result = _AppVerManageRepository.Update(entities,
                entity =>
                {
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                });
                int count = UnitOfWork.SaveChanges();
                return OperationHelper.ReturnOperationResult(count > 0, opera);
            }, Operation.Update);
        }

        public AppVerManage View(int Id)
        {
            return _AppVerManageRepository.GetByKey(Id);
        }

        public OperationResult Insert(params AppVerManageDto[] dtos)
        {
            return OperationHelper.Try((opera) =>
            {
                dtos.CheckNotNull("dtos");
                UnitOfWork.TransactionEnabled = true;
                OperationResult result = _AppVerManageRepository.Insert(dtos, a => { },
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

        public OperationResult Update(params AppVerManageDto[] dtos)
        {
            return OperationHelper.Try((opera) =>
            {
                dtos.CheckNotNull("dtos");
                UnitOfWork.TransactionEnabled = true;
                OperationResult result = _AppVerManageRepository.Update(dtos, a => { },
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

        public AppVerManageDto Edit(int Id)
        {
            var entity = _AppVerManageRepository.GetByKey(Id);
            Mapper.CreateMap<AppVerManage, AppVerManageDto>();
            var dto = Mapper.Map<AppVerManage, AppVerManageDto>(entity);
            return dto;
        }

        public OperationResult CheckUpdate(string version, AppTypeFlag? AppFlag)
        {
            OperationResult result = new OperationResult(OperationResultType.Error, "参数无效");

            if (version.IsNotNullAndEmpty() && AppFlag.HasValue)
            {
                var listv = version.Split(".", true);
                if (listv.Length >= 3)
                {
                    try
                    {
                        var v1 = listv[0].CastTo<int>();
                        var v2 = listv[1].CastTo<int>();
                        var v3 = listv[2].CastTo<int>();
                        var lastVer = Entities.Where(w => w.IsEnabled && !w.IsDeleted && w.AppType == AppFlag).OrderByDescending(o => o.V1).ThenByDescending(t => t.V2).ThenByDescending(t => t.V3).FirstOrDefault();
                        if (lastVer.IsNotNull())
                        {
                            var hasV = lastVer.V1 > v1 ? true : lastVer.V2 > v2 ? true : lastVer.V3 > v3;
                            if (hasV)
                            {
                                var downloadpath = lastVer.DownloadPath;
                                downloadpath = downloadpath.IsNotNullAndEmpty() ? downloadpath.StartsWith("http") ? downloadpath : WebUrl + downloadpath : string.Empty;
                                result.Message = null;
                                result.ResultType = OperationResultType.Success;
                                result.Data = new
                                {
                                    DownloadPath = lastVer.DownloadPath,
                                    AccessPath = lastVer.AccessPath,
                                    Version = lastVer.Version,
                                    AppType = lastVer.AppType + ""
                                };
                            }
                            else
                            {
                                result.Message = "未检测到更新";
                            }
                        }
                        else
                        {
                            result.Message = "未检测到更新";
                        }
                    }
                    catch
                    {
                        result.Message = "版本号无效,例: 1.0.1";
                    }
                }
                else
                {
                    result.Message = "版本号无效,例: 1.0.1";
                }
            }
            return result;
        }
    }
}

