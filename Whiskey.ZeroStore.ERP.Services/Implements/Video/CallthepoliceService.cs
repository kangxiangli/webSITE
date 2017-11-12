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
  public  class CallthepoliceService : ServiceBase, ICallthepoliceContract
    {
        protected readonly IRepository<Callthepolice, int> _callthepoliceRepository;
        public CallthepoliceService(IRepository<Callthepolice, int> callthepoliceRepository) :base(callthepoliceRepository.UnitOfWork)
        {
            _callthepoliceRepository = callthepoliceRepository;
        }

        public IQueryable<Callthepolice> Callthepolices
        {
            get { return _callthepoliceRepository.Entities; }
        }
        public OperationResult Insert(params Callthepolice[] rules)
        {
            OperationResult resul = new OperationResult(OperationResultType.Error);
            foreach (var rule in rules)
            {
                rule.OperatorId = AuthorityHelper.OperatorId;
                resul = _callthepoliceRepository.Insert(rule) > 0 ? new OperationResult(OperationResultType.Success) : new OperationResult(OperationResultType.Error);
            }
            return resul;
        }
    }
}
