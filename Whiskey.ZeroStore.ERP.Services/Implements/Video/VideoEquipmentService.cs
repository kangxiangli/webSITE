using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Utility.Data;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{
   public class VideoEquipmentService : ServiceBase, IVideoEquipmentContract
    {
        protected readonly IRepository<VideoEquipment, int> _videoEquipmentRepository;
        public VideoEquipmentService(IRepository<VideoEquipment, int> videoEquipmentRepository):base(videoEquipmentRepository.UnitOfWork)
        {
            _videoEquipmentRepository = videoEquipmentRepository;
        }

        public IQueryable<VideoEquipment> VideoEquipments
        {
            get { return _videoEquipmentRepository.Entities; }
        }
        public OperationResult Insert(params VideoEquipment[] rules)
        {
            OperationResult resul = new OperationResult(OperationResultType.Error);
            foreach (var rule in rules)
            {
                rule.OperatorId = AuthorityHelper.OperatorId;
                resul = _videoEquipmentRepository.Insert(rule) > 0 ? new OperationResult(OperationResultType.Success) : new OperationResult(OperationResultType.Error);
            }
            return resul;
        }

        public OperationResult Update(params VideoEquipmentDto[] rules)
        {
            var resul = _videoEquipmentRepository.Update(rules, null, (dto, ent) =>
            {
                ent = AutoMapper.Mapper.Map<VideoEquipment>(dto);
                ent.UpdatedTime = DateTime.Now;
                ent.OperatorId = AuthorityHelper.OperatorId;
                ent.IsEnabled = true;
                return ent;
            });
            return resul;
        }

        //删除 或恢复
        public OperationResult Remove(bool state, params int[] ids)
        {
            var entys = _videoEquipmentRepository.Entities.Where(c => ids.Contains(c.Id));
            entys.Each(c =>
            {
                c.IsDeleted = state;
                c.OperatorId = AuthorityHelper.OperatorId;
                c.UpdatedTime = DateTime.Now;
            });
            return _videoEquipmentRepository.Update(entys.ToList());
        }

        //禁用或恢复
        public OperationResult Disable(bool state, params int[] ids)
        {
            var entys = _videoEquipmentRepository.Entities.Where(c => ids.Contains(c.Id));
            entys.Each(c =>
            {
                c.IsEnabled = state;
                c.OperatorId = AuthorityHelper.OperatorId;
                c.UpdatedTime = DateTime.Now;
            });
            return _videoEquipmentRepository.Update(entys.ToList());
        }
    }
}
