
using AutoMapper;
using System;
using System.Linq;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Utility;
using Whiskey.Utility.Data;
using Whiskey.Utility.Extensions;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    public class PosLocationService : ServiceBase, IPosLocationContract
    {
        private readonly IRepository<PosLocation, int> _PosLocationRepository;
        public PosLocationService(
            IRepository<PosLocation, int> _PosLocationRepository
            ) : base(_PosLocationRepository.UnitOfWork)
        {
            this._PosLocationRepository = _PosLocationRepository;
        }

        public IQueryable<PosLocation> Entities
        {
            get
            {
                return _PosLocationRepository.Entities;
            }
        }


        public OperationResult EnableOrDisable(bool enable, params int[] ids)
        {
            return OperationHelper.Try((opera) =>
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _PosLocationRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = enable;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                }
                return OperationHelper.ReturnOperationResult(UnitOfWork.SaveChanges() > 0, opera);
            }, enable ? Operation.Enable : Operation.Disable);
        }

        public OperationResult Insert(params PosLocation[] entities)
        {
            return OperationHelper.Try((opera) =>
            {
                entities.CheckNotNull("entities");
                OperationResult result = _PosLocationRepository.Insert(entities,
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
                var entities = _PosLocationRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = delete;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                }
                return OperationHelper.ReturnOperationResult(UnitOfWork.SaveChanges() > 0, opera);
            }, delete ? Operation.Delete : Operation.Recovery);
        }

        public OperationResult Update(params PosLocation[] entities)
        {
            return OperationHelper.Try((opera) =>
            {
                entities.CheckNotNull("entities");
                UnitOfWork.TransactionEnabled = true;
                OperationResult result = _PosLocationRepository.Update(entities,
                entity =>
                {
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                });
                int count = UnitOfWork.SaveChanges();
                return OperationHelper.ReturnOperationResult(count > 0, opera);
            }, Operation.Update);
        }

        public PosLocation View(int Id)
        {
            return _PosLocationRepository.GetByKey(Id);
        }

        public OperationResult Insert(params PosLocationDto[] dtos)
        {
            return OperationHelper.Try((opera) =>
            {
                dtos.CheckNotNull("dtos");
                UnitOfWork.TransactionEnabled = true;
                OperationResult result = _PosLocationRepository.Insert(dtos, a => { },
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

        public OperationResult Update(params PosLocationDto[] dtos)
        {
            return OperationHelper.Try((opera) =>
            {
                dtos.CheckNotNull("dtos");
                UnitOfWork.TransactionEnabled = true;
                OperationResult result = _PosLocationRepository.Update(dtos, a => { },
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

        public PosLocationDto Edit(int Id)
        {
            var entity = _PosLocationRepository.GetByKey(Id);
            Mapper.CreateMap<PosLocation, PosLocationDto>();
            var dto = Mapper.Map<PosLocation, PosLocationDto>(entity);
            return dto;
        }

        public OperationResult UpdateLocation(string IMEI, double Longitude, double Latitude)
        {
            return OperationHelper.Try((opera) =>
            {
                IMEI.CheckNotNull("IMEI");
                string strAddress = null;
                try
                {
                    var addinfo = GaoDeCoordinateHelper.GaoDeAnalysis(Longitude, Latitude);
                    strAddress = addinfo.regeocode.formatted_address;
                }
                catch (Exception ex) { strAddress = $"定位失败,{ex.Message}"; }
                UnitOfWork.TransactionEnabled = true;
                var mod = Entities.OrderByDescending(o => o.Id).Where(w => w.IMEI == IMEI).FirstOrDefault(f => f.IsEnabled && !f.IsDeleted);
                if (mod.IsNotNull())
                {
                    mod.PrevLongitude = mod.Longitude;
                    mod.PrevLatitude = mod.Latitude;
                    mod.Longitude = Longitude;
                    mod.Latitude = Latitude;
                    mod.PrevUpdatedTime = mod.UpdatedTime;
                    mod.PrevAddress = mod.Address;
                    mod.Address = strAddress;
                    mod.UpdatedTime = DateTime.Now;
                    _PosLocationRepository.Update(mod);
                }
                else
                {
                    mod = new PosLocation()
                    {
                        IMEI = IMEI,
                        PrevLongitude = Longitude,
                        Longitude = Longitude,
                        PrevLatitude = Latitude,
                        Latitude = Latitude,
                        PrevAddress = strAddress,
                        Address= strAddress,
                        PrevUpdatedTime = DateTime.Now,
                    };
                    _PosLocationRepository.Insert(mod);
                }

                int count = UnitOfWork.SaveChanges();
                return OperationHelper.ReturnOperationResult(count > 0, opera);
            }, "刷新定位");
        }
    }
}

