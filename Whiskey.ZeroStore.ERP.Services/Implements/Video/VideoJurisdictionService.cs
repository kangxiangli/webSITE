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

namespace Whiskey.ZeroStore.ERP.Services.Implements.Video
{
   public class VideoJurisdictionService : ServiceBase, IVideoJurisdictionContract
    {
        protected readonly IRepository<VideoJurisdiction, int> _videoJurisdictionRepository;
        public VideoJurisdictionService(IRepository<VideoJurisdiction, int> videoJurisdictionRepository) :base(videoJurisdictionRepository.UnitOfWork)
        {
            _videoJurisdictionRepository = videoJurisdictionRepository;
        }

        public IQueryable<VideoJurisdiction> VideoJurisdictions
        {
            get { return _videoJurisdictionRepository.Entities; }
        }
        public OperationResult Insert(params VideoJurisdiction[] rules)
        {
            OperationResult resul = new OperationResult(OperationResultType.Error);
            foreach (var rule in rules)
            {
                rule.OperatorId = AuthorityHelper.OperatorId;
                resul = _videoJurisdictionRepository.Insert(rule) > 0 ? new OperationResult(OperationResultType.Success) : new OperationResult(OperationResultType.Error);
            }
            return resul;
        }

        public OperationResult Update(params VideoJurisdictionDto[] rules)
        {
            var resul = _videoJurisdictionRepository.Update(rules, null, (dto, ent) =>
            {
                ent = AutoMapper.Mapper.Map<VideoJurisdiction>(dto);
                ent.UpdatedTime = DateTime.Now;
                ent.OperatorId = AuthorityHelper.OperatorId;
                ent.IsEnabled = false;
                return ent;
            });
            return resul;
        }
        public OperationResult Remove(params int[] ids)
        {
            var entys = _videoJurisdictionRepository.Entities.Where(c => ids.Contains(c.Id));
            int result = _videoJurisdictionRepository.Delete(entys);
            if (result > 0)
            {
                return new OperationResult(OperationResultType.Success, "删除成功！", "");
            }
            else
            {
                return new OperationResult(OperationResultType.Error, "删除失败！", "");
            }
        }
    }
}
