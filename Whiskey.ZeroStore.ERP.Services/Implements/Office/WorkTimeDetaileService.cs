using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using System.Web.Mvc;
using Whiskey.Utility.Logging;
using Whiskey.Utility;
using Whiskey.Utility.Data;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Web.Helper;
using AutoMapper;

namespace Whiskey.ZeroStore.ERP.Services
{
    public class WorkTimeDetaileService : ServiceBase, IWorkTimeDetaileContract
    {
        protected readonly ILogger _Logger = LogManager.GetLogger(typeof(WorkTimeDetaileService));

        private readonly IRepository<WorkTimeDetaile, int> _workTimeDetaileRepository;

        public WorkTimeDetaileService(IRepository<WorkTimeDetaile, int> workTimeDetaileRepository)
            : base(workTimeDetaileRepository.UnitOfWork)
        {
            _workTimeDetaileRepository = workTimeDetaileRepository;
        }

        public IQueryable<WorkTimeDetaile> WorkTimeDetailes
        {
            get { return _workTimeDetaileRepository.Entities; }
        }

        public OperationResult Insert(params WorkTimeDetaile[] rules)
        {
            OperationResult resul = new OperationResult(OperationResultType.Error);
            foreach (var rule in rules)
            {
                rule.OperatorId = AuthorityHelper.OperatorId;
                resul = _workTimeDetaileRepository.Insert(rule) > 0 ? new OperationResult(OperationResultType.Success) : new OperationResult(OperationResultType.Error);
            }
            return resul;
        }
        public OperationResult TrueRemove(int year, int month, params int[] ids)
        {
            var entys = _workTimeDetaileRepository.Entities.Where(c => ids.Contains(c.WorkTimeId.Value) && c.Year == year && c.Month == month);
            int result = _workTimeDetaileRepository.Delete(entys);
            if (result >= 0)
            {
                return new OperationResult(OperationResultType.Success, "删除成功！", "");
            }
            else
            {
                return new OperationResult(OperationResultType.Error, "删除失败！", "");
            }

        }
        public OperationResult TrueRemoveByWorkTimeId(params int[] ids)
        {
            var entys = _workTimeDetaileRepository.Entities.Where(c => ids.Contains(c.WorkTimeId.Value));
            int result = _workTimeDetaileRepository.Delete(entys);
            if (result > 0)
            {
                return new OperationResult(OperationResultType.Success, "删除成功！", "");
            }
            else
            {
                return new OperationResult(OperationResultType.Error, "删除失败！", "");
            }

        }

        public OperationResult Update(params WorkTimeDetaileDto[] rules)
        {
            var resul = _workTimeDetaileRepository.Update(rules, null, (dto, ent) =>
            {
                ent = AutoMapper.Mapper.Map<WorkTimeDetaile>(dto);
                ent.UpdatedTime = DateTime.Now;
                ent.OperatorId = AuthorityHelper.OperatorId;
                ent.IsEnabled = true;
                return ent;
            });
            return resul;
        }
        public OperationResult UpdateWorkType(int ToExamineStatues, int day, params int[] ids)
        {
            var enty = _workTimeDetaileRepository.Entities.Where(c => ids.Contains(c.Id));
            enty.Each(c =>
            {
                c.WorkTimeType = ToExamineStatues;
                c.WorkDay = day;
                c.OperatorId = AuthorityHelper.OperatorId;
                c.UpdatedTime = DateTime.Now;
            });
            return _workTimeDetaileRepository.Update(enty.ToList());
        }

        public WorkTimeDetaileDto Edit(int Id)
        {
            var entity = _workTimeDetaileRepository.GetByKey(Id);
            Mapper.CreateMap<WorkTimeDetaile, WorkTimeDetaileDto>();
            var dto = Mapper.Map<WorkTimeDetaile, WorkTimeDetaileDto>(entity);
            return dto;
        }
    }
}
