
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Utility;
using Whiskey.Utility.Data;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.Utility.Extensions;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    public class AppointmentGenService : ServiceBase, IAppointmentGenContract
    {
        private readonly IRepository<AppointmentGen, int> _AppointmentGenRepository;
        private readonly IRepository<Product, int> _ProductRepository;
        private readonly IRepository<Member, int> _MemberRepository;
        private readonly IRepository<Appointment, int> _AppointmentRepository;
        public AppointmentGenService(
            IRepository<AppointmentGen, int> _AppointmentGenRepository,
            IRepository<Product, int> _ProductRepository,
            IRepository<Appointment, int> _AppointmentRepository,
            IRepository<Member, int> _MemberRepository
            ) : base(_AppointmentGenRepository.UnitOfWork)
        {
            this._AppointmentGenRepository = _AppointmentGenRepository;
            this._ProductRepository = _ProductRepository;
            this._MemberRepository = _MemberRepository;
            this._AppointmentRepository = _AppointmentRepository;
        }

        public IQueryable<AppointmentGen> Entities
        {
            get
            {
                return _AppointmentGenRepository.Entities;
            }
        }


        public OperationResult EnableOrDisable(bool enable, params int[] ids)
        {
            return OperationHelper.Try((opera) =>
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _AppointmentGenRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = enable;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                }
                return OperationHelper.ReturnOperationResult(UnitOfWork.SaveChanges() > 0, opera);
            }, enable ? Operation.Enable : Operation.Disable);
        }

        public OperationResult Insert(params AppointmentGen[] entities)
        {
            return OperationHelper.Try((opera) =>
            {
                entities.CheckNotNull("entities");
                OperationResult result = _AppointmentGenRepository.Insert(entities,
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
                var entities = _AppointmentGenRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = delete;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                }
                return OperationHelper.ReturnOperationResult(UnitOfWork.SaveChanges() > 0, opera);
            }, delete ? Operation.Delete : Operation.Recovery);
        }

        public OperationResult Update(params AppointmentGen[] entities)
        {
            return OperationHelper.Try((opera) =>
            {
                entities.CheckNotNull("entities");
                UnitOfWork.TransactionEnabled = true;
                OperationResult result = _AppointmentGenRepository.Update(entities,
                entity =>
                {
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                });
                int count = UnitOfWork.SaveChanges();
                return OperationHelper.ReturnOperationResult(count > 0, opera);
            }, Operation.Update);
        }

        public AppointmentGen View(int Id)
        {
            return _AppointmentGenRepository.GetByKey(Id);
        }

        public OperationResult Insert(params AppointmentGenDto[] dtos)
        {
            return Insert(null, dtos);
        }

        public OperationResult Insert(Action<bool, string, int[]> process, params AppointmentGenDto[] dtos)
        {
            var sendpeople = new int[] { AuthorityHelper.OperatorId ?? 0 };//弹窗通知人
            return OperationHelper.Try(() =>
            {
                dtos.CheckNotNull("dtos");
                UnitOfWork.TransactionEnabled = true;

                var list = new List<AppointmentGen>();

                #region dto转换entity

                process?.Invoke(false, "数据转换...", sendpeople);

                var queryp = _ProductRepository.Entities.Where(w => w.IsEnabled && !w.IsDeleted);
                var querym = _MemberRepository.Entities.Where(w => w.IsEnabled && !w.IsDeleted && w.StoreId.HasValue);
                foreach (var dto in dtos)
                {
                    var ent = Mapper.Map<AppointmentGen>(dto);
                    ent.Products = queryp.Where(w => dto.ProductIds.Contains(w.Id)).ToList();
                    ent.Members = querym.Where(w => dto.MemberIds.Contains(w.Id)).ToList();
                    ent.AllCount = ent.Products.Count;
                    ent.SuccessCount = ent.AllCount;
                    ent.CreatedTime = DateTime.Now;
                    ent.OperatorId = AuthorityHelper.OperatorId;
                    list.Add(ent);
                }
                OperationResult result = _AppointmentGenRepository.Insert(list, a => { });

                #endregion

                var querya = _AppointmentRepository.Entities.Where(w => w.IsEnabled && !w.IsDeleted);

                process?.Invoke(false, "数据准备...", sendpeople);

                foreach (var item in list)
                {
                    var listmemberIds = item.Members.Select(s => new { s.Id, s.StoreId }).OrderBy(o => Guid.NewGuid()).ToList();
                    var listproductIds = item.Products.Select(s => new { s.Id, s.ProductNumber }).OrderBy(o => Guid.NewGuid()).ToList();
                    item.SuccessCount = listproductIds.Count;

                    if (listproductIds.Count == 0 || listmemberIds.Count == 0)
                    {
                        item.SuccessCount = 0;
                        continue;
                    }

                    if (listmemberIds.Count > listproductIds.Count)
                    {
                        listmemberIds = listmemberIds.Take(listproductIds.Count).ToList();
                    }

                    var partcount = listmemberIds.Count;//份数
                    var avgcount = (int)Math.Floor((double)listproductIds.Count / partcount);//最小每份数量
                    var listavg = new List<int>();
                    for (int i = 1; i < partcount; i++)
                    {
                        listavg.Add(avgcount);
                    }
                    listavg.Add(listproductIds.Count - avgcount * (partcount - 1));
                    listavg.ListRandom();

                    process?.Invoke(false, "生成数据...", sendpeople);

                    var _count = 0;
                    for (int i = 0; i < partcount; i++)
                    {
                        var tacount = listavg[i];
                        var cm = listmemberIds[i];
                        var ps = listproductIds.Skip(_count).Take(tacount).ToList();
                        _count += tacount;

                        foreach (var item2 in ps)
                        {
                            if (querya.Any(a => a.MemberId == cm.Id && a.ProductNumber == item2.ProductNumber && a.StoreId == cm.StoreId))
                            {
                                --item.SuccessCount;
                            }
                            else
                            {
                                Appointment app = new Appointment();
                                app.MemberId = cm.Id;
                                app.StoreId = cm.StoreId.Value;
                                app.ProductNumber = item2.ProductNumber;
                                app.State = AppointmentState.已处理;
                                app.CreatedTime = RandomHelper.random.NextDateTime(item.StartTime, item.EndTime);
                                app.Notes = "已处理";
                                _AppointmentRepository.Insert(app);
                            }
                        }
                    }
                }

                _AppointmentGenRepository.Update(list, a => { });

                process?.Invoke(false, "保存数据...", sendpeople);
                int count = UnitOfWork.SaveChanges();
                process?.Invoke(true, "操作完成...", sendpeople);

                return OperationHelper.ReturnOperationResult(count > 0, Operation.Add);
            }, (ex) => { process?.Invoke(true, "", sendpeople); return OperationHelper.ReturnOperationExceptionResult(ex, Operation.Add); });
        }

        public OperationResult Update(params AppointmentGenDto[] dtos)
        {
            return OperationHelper.Try((opera) =>
            {
                dtos.CheckNotNull("dtos");
                UnitOfWork.TransactionEnabled = true;
                OperationResult result = _AppointmentGenRepository.Update(dtos, a => { },
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

        public AppointmentGenDto Edit(int Id)
        {
            var entity = _AppointmentGenRepository.GetByKey(Id);
            Mapper.CreateMap<AppointmentGen, AppointmentGenDto>();
            var dto = Mapper.Map<AppointmentGen, AppointmentGenDto>(entity);
            return dto;
        }

        public OperationResult BatchImpot(Action<bool, string, int[]> process, params AppointmentGenBatchDto[] dtos)
        {
            var sendpeople = new int[] { AuthorityHelper.OperatorId ?? 0 };//弹窗通知人
            return OperationHelper.Try(() =>
            {
                dtos.CheckNotNull("dtos");
                UnitOfWork.TransactionEnabled = true;

                #region 数据结构转换

                process?.Invoke(false, "数据转换...", sendpeople);

                var queryp = _ProductRepository.Entities.Where(w => w.IsEnabled && !w.IsDeleted);
                var querym = _MemberRepository.Entities.Where(w => w.IsEnabled && !w.IsDeleted && w.StoreId.HasValue);
                var listphone = dtos.Select(s => s.MobilePhone).Distinct();
                var listpro = dtos.Select(s => s.ProductNumber).Distinct();
                var listp = queryp.Where(w => listpro.Contains(w.ProductNumber)).ToList();
                var listm = querym.Where(w => listphone.Contains(w.MobilePhone)).DistinctBy(d => d.MobilePhone).ToList();

                var ent = new AppointmentGen();
                ent.Products = listp;
                ent.Members = listm;
                ent.AllCount = dtos.Length;
                ent.SuccessCount = ent.AllCount;
                ent.StartTime = dtos.Min(m => m.AppointmentTime);
                ent.EndTime = dtos.Max(m => m.AppointmentTime);
                ent.CreatedTime = DateTime.Now;
                ent.OperatorId = AuthorityHelper.OperatorId;
                _AppointmentGenRepository.Insert(ent);

                #endregion

                if (ent.AllCount > 0)
                {
                    process?.Invoke(false, "数据生成...", sendpeople);

                    var querya = _AppointmentRepository.Entities.Where(w => w.IsEnabled && !w.IsDeleted);
                    var listapp = new List<Appointment>();
                    foreach (var dto in dtos)
                    {
                        var modm = ent.Members.FirstOrDefault(s => s.MobilePhone == dto.MobilePhone);
                        var modp = ent.Products.FirstOrDefault(f => f.ProductNumber == dto.ProductNumber);
                        if (modm.IsNull() || modp.IsNull())
                        {
                            --ent.SuccessCount;
                            continue;
                        }

                        if (listapp.Any(a => a.MemberId == modm.Id && a.ProductNumber == modp.ProductNumber && a.StoreId == modm.StoreId)
                            || querya.Any(a => a.MemberId == modm.Id && a.ProductNumber == modp.ProductNumber && a.StoreId == modm.StoreId))
                        {
                            --ent.SuccessCount;
                        }
                        else
                        {
                            Appointment app = new Appointment();
                            app.MemberId = modm.Id;
                            app.StoreId = modm.StoreId.Value;
                            app.ProductNumber = modp.ProductNumber;
                            app.State = AppointmentState.已处理;
                            app.CreatedTime = RandomHelper.random.NextDateTime(dto.AppointmentTime.Date.AddHours(9), dto.AppointmentTime.Date.AddHours(18));
                            app.Notes = "已处理";
                            listapp.Add(app);
                        }
                    }
                    _AppointmentRepository.Insert(listapp, a => { });
                    _AppointmentGenRepository.Update(ent);
                }

                process?.Invoke(false, "保存数据...", sendpeople);
                int count = UnitOfWork.SaveChanges();
                process?.Invoke(true, "操作完成...", sendpeople);

                return OperationHelper.ReturnOperationResult(count > 0, "批量导入");
            }, ex =>
            {
                process?.Invoke(true, "", sendpeople); return OperationHelper.ReturnOperationExceptionResult(ex, "批量导入");
            });
        }
    }
}

