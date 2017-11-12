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
using Whiskey.Utility.Extensions;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    public class PartnerManageCheckService : ServiceBase, IPartnerManageCheckContract
    {
        private readonly IRepository<PartnerManageCheck, int> _PartnerManageCheckRepository;

        private readonly IRepository<Department, int> _DepartmentRepository;
        private readonly IRepository<JobPosition, int> _JobPositionRepository;
        private readonly IRepository<Store, int> _StoreRepository;
        private readonly IRepository<Storage, int> _StorageRepository;
        private readonly IRepository<Administrator, int> _AdministratorRepository;
        private readonly IMemberContract _MemberContract;
        public PartnerManageCheckService(
            IRepository<PartnerManageCheck, int> _PartnerManageCheckRepository
            ,IRepository<Department, int> _DepartmentRepository
            ,IRepository<JobPosition, int> _JobPositionRepository
            , IRepository<Store, int> _StoreRepository
            , IRepository<Storage, int> _StorageRepository
            , IRepository<Administrator, int> _AdministratorRepository
            , IMemberContract _MemberContract
            ) : base(_PartnerManageCheckRepository.UnitOfWork)
        {
            this._PartnerManageCheckRepository = _PartnerManageCheckRepository;
            this._DepartmentRepository = _DepartmentRepository;
            this._JobPositionRepository = _JobPositionRepository;
            this._StoreRepository = _StoreRepository;
            this._StorageRepository = _StorageRepository;
            this._AdministratorRepository = _AdministratorRepository;
            this._MemberContract = _MemberContract;
        }

        public IQueryable<PartnerManageCheck> Entities
        {
            get
            {
                return _PartnerManageCheckRepository.Entities;
            }
        }


        public OperationResult EnableOrDisable(bool enable, params int[] ids)
        {
            return OperationHelper.Try((opera) =>
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _PartnerManageCheckRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = enable;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                }
                return OperationHelper.ReturnOperationResult(UnitOfWork.SaveChanges() > 0, opera);
            }, enable ? Operation.Enable : Operation.Disable);
        }

        public OperationResult Insert(params PartnerManageCheck[] entities)
        {
            return OperationHelper.Try((opera) =>
            {
                entities.CheckNotNull("entities");

                UnitOfWork.TransactionEnabled = true;

                foreach (var item in entities)
                {
                    #region 参数校验
                    if (_DepartmentRepository.Entities.Any(a => a.DepartmentName == item.DepartmentName))
                    {
                        return new OperationResult(OperationResultType.ValidError, "部门名称已存在");
                    }
                    if (_StoreRepository.Entities.Any(a => a.StoreName == item.StoreName))
                    {
                        return new OperationResult(OperationResultType.ValidError, "店铺名称已存在");
                    }
                    if (_StorageRepository.Entities.Any(a => a.StorageName == item.StorageName))
                    {
                        return new OperationResult(OperationResultType.ValidError, "仓库名称已存在");
                    }
                    if (item.PartnerManage.MemberId.IsNull())
                    {
                        if (_AdministratorRepository.Entities.Any(a => a.Member.MemberName == item.PartnerManage.MemberName))
                        {
                            return new OperationResult(OperationResultType.ValidError, "会员昵称已存在");
                        }
                    }
                    else
                    {
                        if (_AdministratorRepository.Entities.Any(a => a.MemberId == item.PartnerManage.MemberId))
                        {
                            return new OperationResult(OperationResultType.ValidError, "会员已提升过员工,为保证数据正确性,已阻止本次审核");
                        }
                    }

                    #endregion

                    #region 创建部门

                    Department modDep = new Department();
                    modDep.DepartmentName = item.DepartmentName;
                    modDep.DepartmentType = DepartmentTypeFlag.加盟;
                    modDep.OperatorId = AuthorityHelper.OperatorId;
                    modDep.Description = item.PartnerManage.Notes;
                    modDep.CreatedTime = item.PartnerManage.CreatedTime;

                    #region 创建职位

                    JobPosition modJob = new JobPosition();
                    modJob.JobPositionName = "店长";
                    modJob.IsLeader = true;
                    //modJob.IsShopkeeper = true;
                    modJob.AllowPwd = true;
                    modJob.CheckLogin = false;
                    modJob.WorkTimeId = 4;//自由工作时间
                    modJob.AnnualLeaveId = 1;//普通年假
                    modJob.OperatorId = AuthorityHelper.OperatorId;
                    modJob.Notes = item.PartnerManage.Notes;
                    modJob.CreatedTime = item.PartnerManage.CreatedTime;

                    #region 创建员工

                    Administrator modAdmin = new Administrator();
                    //modAdmin.AdministratorTypeId = 2;//员工类型，加盟商
                    modAdmin.IsLogin = true;
                    modAdmin.Roles = item.Roles;
                    modAdmin.EntryTime = DateTime.Now;
                    modAdmin.Department = modDep;
                    modAdmin.OperatorId = AuthorityHelper.OperatorId;
                    modAdmin.Notes = item.PartnerManage.Notes;
                    modAdmin.CreatedTime = item.PartnerManage.CreatedTime;

                    #region 创建会员

                    if (item.PartnerManage.MemberId.IsNull())
                    {
                        Member modMem = new Member();
                        modMem.MemberName = item.PartnerManage.MemberName;
                        modMem.RealName = item.PartnerManage.MemberName;
                        modMem.UniquelyIdentifies = _MemberContract.RandomUniquelyId();
                        modMem.Gender = (int)item.PartnerManage.Gender;
                        modMem.Email = item.PartnerManage.Email;
                        modMem.MobilePhone = item.PartnerManage.MobilePhone;
                        modMem.UserPhoto = item.PartnerManage.StorePhoto;
                        modMem.CardNumber = _MemberContract.GetCardNum();
                        modMem.CreatedTime = item.PartnerManage.CreatedTime;
                        var pass = item.PartnerManage.MemberPass;

                        modMem.MemberPass = (pass.IsNullOrEmpty() ? "123456" : pass).MD5Hash();
                        modMem.MemberTypeId = 1;//普通会员
                        modMem.OperatorId = AuthorityHelper.OperatorId;
                        modMem.Notes = item.PartnerManage.Notes;
                        modAdmin.Member = modMem;
                    }
                    else
                    {
                        modAdmin.MemberId = item.PartnerManage.MemberId;
                    }

                    #endregion

                    modJob.Administrators.Add(modAdmin);

                    #endregion

                    modDep.JobPositions.Add(modJob);

                    #endregion

                    #region 创建店铺

                    Store modStore = new Store();
                    modStore.StoreName = item.StoreName;
                    modStore.StoreTypeId = item.StoreTypeId;
                    modStore.StoreCredit = 100;//店铺信誉
                    modStore.Balance = 0;
                    modStore.Telephone = item.PartnerManage.MobilePhone;
                    modStore.Province = item.PartnerManage.Province;
                    modStore.City = item.PartnerManage.City;
                    modStore.Address = item.PartnerManage.Address;
                    modStore.ZipCode = item.PartnerManage.ZipCode;
                    modStore.StorePhoto = item.PartnerManage.StorePhoto;
                    modStore.IsMainStore = false;//非官方总店
                    modStore.OperatorId = AuthorityHelper.OperatorId;
                    modStore.Notes = item.PartnerManage.Notes;
                    modStore.CreatedTime = item.PartnerManage.CreatedTime;
                    modStore.StoreDiscount = 1;//无折扣

                    #region 创建仓库

                    Storage modSto = new Storage();
                    modSto.StorageType = 1;//1为线下
                    modSto.StorageName = item.StorageName;
                    modSto.TelePhone = item.PartnerManage.MobilePhone;
                    modSto.StorageAddress = item.PartnerManage.Address;
                    modSto.IsDefaultStorage = true;
                    modSto.OperatorId = AuthorityHelper.OperatorId;
                    modSto.Description = item.PartnerManage.Notes;
                    modSto.CreatedTime = item.PartnerManage.CreatedTime;

                    modStore.Storages.Add(modSto);

                    #endregion

                    modDep.Stores.Add(modStore);

                    #endregion


                    _DepartmentRepository.Insert(modDep);

                    #endregion

                    item.PartnerManage.CheckNotes = "审核通过";
                    item.PartnerManage.CheckStatus = CheckStatusFlag.通过;
                }

                _PartnerManageCheckRepository.Insert(entities,
                entity =>
                {
                    entity.CreatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                });

                if (UnitOfWork.SaveChanges() > 0)
                {
                    RedisCacheHelper.ResetStoreDepartmentMangeStoreCache();
                    return new OperationResult(OperationResultType.Success);
                }
                else
                {
                    return new OperationResult(OperationResultType.Error, "审核失败");
                }
            }, Operation.Add);
        }

        public OperationResult DeleteOrRecovery(bool delete, params int[] ids)
        {
            return OperationHelper.Try((opera) =>
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _PartnerManageCheckRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = delete;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                }
                return OperationHelper.ReturnOperationResult(UnitOfWork.SaveChanges() > 0, opera);
            }, delete ? Operation.Delete : Operation.Recovery);
        }

        public OperationResult Update(params PartnerManageCheck[] entities)
        {
            return OperationHelper.Try((opera) =>
            {
                entities.CheckNotNull("entities");
                UnitOfWork.TransactionEnabled = true;
                OperationResult result = _PartnerManageCheckRepository.Update(entities,
                entity =>
                {
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                });
                int count = UnitOfWork.SaveChanges();
                return OperationHelper.ReturnOperationResult(count > 0, opera);
            }, Operation.Update);
        }

        public PartnerManageCheck View(int Id)
        {
            return _PartnerManageCheckRepository.GetByKey(Id);
        }

        public OperationResult Insert(params PartnerManageCheckDto[] dtos)
        {
            return OperationHelper.Try((opera) =>
            {
                dtos.CheckNotNull("dtos");
                UnitOfWork.TransactionEnabled = true;
                OperationResult result = _PartnerManageCheckRepository.Insert(dtos, a => { },
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

        public OperationResult Update(params PartnerManageCheckDto[] dtos)
        {
            return OperationHelper.Try((opera) =>
            {
                dtos.CheckNotNull("dtos");
                UnitOfWork.TransactionEnabled = true;
                OperationResult result = _PartnerManageCheckRepository.Update(dtos, a => { },
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

        public PartnerManageCheckDto Edit(int Id)
        {
            var entity = _PartnerManageCheckRepository.GetByKey(Id);
            Mapper.CreateMap<PartnerManageCheck, PartnerManageCheckDto>();
            var dto = Mapper.Map<PartnerManageCheck, PartnerManageCheckDto>(entity);
            return dto;
        }
    }
}
