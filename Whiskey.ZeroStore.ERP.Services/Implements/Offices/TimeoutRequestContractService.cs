
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Utility;
using Whiskey.Utility.Data;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Notices;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    public class TimeoutRequestService : ServiceBase, ITimeoutRequestContract
    {

        private readonly IRepository<TimeoutRequest, int> _repo;
        private readonly IRepository<Administrator, int> _adminRepo;
        private readonly ITimeoutSettingContract _timeoutSettingContract;
        private readonly IModuleContract _moduleContract;
        private readonly INotificationContract _notificationContract;

        public TimeoutRequestService(IRepository<TimeoutRequest, int> repo,
            IRepository<Administrator, int> adminRepo,
            ITimeoutSettingContract timeoutSettingContract,
            IModuleContract moduleContract,
            INotificationContract notificationContract

            ) : base(repo.UnitOfWork)
        {
            _repo = repo;
            _adminRepo = adminRepo;
            _timeoutSettingContract = timeoutSettingContract;
            _moduleContract = moduleContract;
            _notificationContract = notificationContract;
        }
        public IQueryable<TimeoutRequest> Entities => _repo.Entities;


        public OperationResult Insert(params TimeoutRequest[] entities)
        {
            return _repo.Insert(entities, e =>
            {
                e.CreatedTime = DateTime.Now;
                e.UpdatedTime = DateTime.Now;
                e.OperatorId = AuthorityHelper.OperatorId;
            });
        }
        public OperationResult Delete(params TimeoutRequest[] entities)
        {
            var count = _repo.Delete(entities);
            if (count > 0)
            {
                return OperationResult.OK();
            }
            return OperationResult.Error("删除失败");
        }
        public OperationResult Update(ICollection<TimeoutRequest> entities)
        {
            return _repo.Update(entities, e =>
            {
                e.UpdatedTime = DateTime.Now;
                e.OperatorId = AuthorityHelper.OperatorId;
            });
        }

        public TimeoutRequest Edit(int id)
        {
            return _repo.GetByKey(id);
        }

        public OperationResult DeleteOrRecovery(bool delete, params int[] ids)
        {
            return OperationHelper.Try((opera) =>
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _repo.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = delete;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                }
                return OperationHelper.ReturnOperationResult(UnitOfWork.SaveChanges() > 0, opera);
            }, delete ? Operation.Delete : Operation.Recovery);
        }

        public OperationResult EnableOrDisable(bool enable, params int[] ids)
        {
            return OperationHelper.Try((opera) =>
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _repo.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = enable;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                }
                return OperationHelper.ReturnOperationResult(UnitOfWork.SaveChanges() > 0, opera);
            }, enable ? Operation.Enable : Operation.Disable);
        }

        public TimeoutRequest View(int Id)
        {
            return _repo.GetByKey(Id);
        }

        public OperationResult Update(params TimeoutRequest[] entities)
        {
            return _repo.Update(entities, e =>
            {
                e.OperatorId = AuthorityHelper.OperatorId;
                e.UpdatedTime = DateTime.Now;
            });
        }

        public const string CONFIG_KEY = "timeoutrequest:config";
        public OperationResult GetConfig()
        {
            var key = CONFIG_KEY;
            var dict = RedisCacheHelper.Get<Dictionary<string, string>>(key);
            if (dict == null || !dict.ContainsKey("limitdays"))
            {
                dict = new Dictionary<string, string>
                {
                    {"limitdays","10" }
                };
                RedisCacheHelper.Set(key, dict);
            }
            return OperationResult.OK(dict);
        }

        public OperationResult UpdateConfig(Dictionary<string, string> dict)
        {
            var isOk = RedisCacheHelper.Set(CONFIG_KEY, dict);
            if (!isOk)
            {
                return OperationResult.Error("修改失败");
            }
            return OperationResult.OK();
        }
        public OperationResult Create(TimeoutRequest dto, Action<List<int>> sendNotificationAction)
        {
            try
            {
                var adminEntity = _adminRepo.Entities.FirstOrDefault(a => !a.IsDeleted && a.IsEnabled && a.Id == AuthorityHelper.OperatorId.Value);
                var timeoutSettingEntity = _timeoutSettingContract.View(dto.TimeoutSettingId);
                if (adminEntity == null || timeoutSettingEntity == null)
                {
                    return OperationResult.Error("参数错误");
                }

                var entity = new TimeoutRequest()
                {
                    CreatedTime = DateTime.Now,
                    Number = dto.Number,
                    DepartmentId = adminEntity.DepartmentId.Value,
                    Notes = dto.Notes,
                    State = TimeoutRequestState.审核中,
                    RequestAdminId = adminEntity.Id,
                    TimeoutSettingId = timeoutSettingEntity.Id
                };

                entity.Timeouts = _timeoutSettingContract.ComputeTimeouts(timeoutSettingEntity.Name, dto.Number);
                if (entity.Timeouts <= 0)
                {
                    return OperationResult.Error("未检测到超时");
                }

                // 同一个人,申请数量限制
                var historyQuery = _repo.Entities.Where(e => !e.IsDeleted && e.IsEnabled)
                                                     .Where(e => e.RequestAdminId == adminEntity.Id && !e.IsUsed);

                if (historyQuery.Any(e => e.State == TimeoutRequestState.审核中))
                {
                    return OperationResult.Error("您此前已提交超时申请,请等待审核结果");
                }

                if (historyQuery.Any(e => e.State == TimeoutRequestState.已通过))
                {
                    return OperationResult.Error("您有审核通过且未使用的超时申请,使用后才能申请");
                }

                // 同一个单号,多人同时申请限制
                var existQuery = _repo.Entities.Where(e => !e.IsDeleted && e.IsEnabled)
                                                   .Where(e => e.Number == dto.Number && !e.IsUsed)
                                                   .Where(e => e.State == TimeoutRequestState.审核中 || e.State == TimeoutRequestState.已通过).Select(e => new { e.RequestAdmin.Member.RealName, e.State, e.CreatedTime })
                                                   .FirstOrDefault();

                if (existQuery != null)
                {
                    return OperationResult.Error($"您申请的单号在{existQuery.CreatedTime}已被[{existQuery.RealName}]申请");
                }


                var res = Insert(entity);
                if (res.ResultType == OperationResultType.Success)
                {
                    // 获取有审核权限且管辖部门包含申请人所在部门的管理员
                    var _filterFlags = new string[] { "#pass", "#nopass" };

                    var adminIds =  _moduleContract.GetPermissionedAdminIds("TimeoutRequest", "Offices", entity.DepartmentId, _filterFlags);
                

                    if (adminIds != null && adminIds.Any())
                    {
                        var notiDto = new NotificationDto
                        {
                            AdministratorIds = adminIds,
                            NoticeType = NoticeFlag.Immediate,
                            NoticeTargetType = (int)NoticeTargetFlag.Admin,
                            Title = "超时申请",
                            Description = $"有一条新的{timeoutSettingEntity.Name}申请需要处理",
                            IsEnableApp = false
                        };
                        _notificationContract.Insert(sendNotificationAction, notiDto);
                    }
                }
                return res;
            }
            catch (Exception e)
            {

                return OperationResult.Error(e.Message);
            }




        }

    }
}
