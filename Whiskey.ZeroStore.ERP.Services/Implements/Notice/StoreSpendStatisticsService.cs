
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Utility.Data;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models.Entities.Notices;
using Whiskey.ZeroStore.ERP.Services.Contracts;

namespace Whiskey.ZeroStore.ERP.Services.Implements.Notice
{
    public class StoreSpendStatisticsService : ServiceBase, IStoreSpendStatisticsContract
    {
        private readonly IRepository<StoreSpendStatistics, int> _storeSpendStatisticsRepository;

        public StoreSpendStatisticsService(IRepository<StoreSpendStatistics, int> storeSpendStatisticsRepository)
            : base(storeSpendStatisticsRepository.UnitOfWork)
        {
            _storeSpendStatisticsRepository = storeSpendStatisticsRepository;
        }

        public IQueryable<Models.Entities.Notices.StoreSpendStatistics> StoreStatistics
        {
            get { return _storeSpendStatisticsRepository.Entities; }
        }

        public Utility.Data.OperationResult Insert(params Models.Entities.Notices.StoreSpendStatistics[] ents)
        {
            ents.Each(c =>
            {
                c.OperatorId = c.OperatorId?? AuthorityHelper.OperatorId;
                c.CreatedTime = c.UpdatedTime = DateTime.Now;
            });
            return _storeSpendStatisticsRepository.Insert((IEnumerable<StoreSpendStatistics>) ents) > 0
                ? new OperationResult(OperationResultType.Success)
                : new OperationResult(OperationResultType.Error);
        }
    }
}