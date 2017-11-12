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
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    //yxk 2016-1-8 店铺的商品活动
    class SalesCampaignService : ServiceBase, ISalesCampaignContract
    {
        private readonly IRepository<SalesCampaign, int> _salesCampaignRepository;
        static object lockobj=new object();
        public SalesCampaignService(IRepository<SalesCampaign, int> salesCampaignRepository)
            :base(salesCampaignRepository.UnitOfWork)
        {
            _salesCampaignRepository = salesCampaignRepository; ;
        }

        public IQueryable<SalesCampaign> SalesCampaigns
        {
            get {return _salesCampaignRepository.Entities; }
        }

        public Utility.Data.OperationResult Insert(bool trans,params SalesCampaign[] sales)
        {
            _salesCampaignRepository.UnitOfWork.TransactionEnabled = trans;
            lock (lockobj) {
                int maxnum = 100000;
                if (_salesCampaignRepository.Entities.Count() > 0) {
                    maxnum = _salesCampaignRepository.Entities.Max(c => c.CampaignNumber);
                }
               foreach (var item in sales)
               {
                   item.CampaignNumber = maxnum + 1;
               }
            }
            int res= _salesCampaignRepository.Insert((IEnumerable<SalesCampaign>)sales);
            if(res>0)
                return new Utility.Data.OperationResult(Utility.Data.OperationResultType.Success,sales.Count()+"条数据受影响");
            return new Utility.Data.OperationResult(Utility.Data.OperationResultType.Error);
        }

        public Utility.Data.OperationResult Update(params SalesCampaign[] sales)
        {
            
            return  _salesCampaignRepository.Update(sales);
        }


        //在启用 、禁用、移除、恢复之前判断该活动是否已经超出了结束时间，如果不可用，则不允许操作
        public OperationResult Disable(params int[] ids)
        {
            OperationResult resul=new OperationResult(OperationResultType.Error);
           var ents= _salesCampaignRepository.Entities.Where(c => ids.Contains(c.Id));
            var endtime = DateTime.Now;
           var cou= ents.Where(c => c.CampaignEndTime < endtime).Select(c=>c.CampaignNumber).ToList();
            if (cou.Any())
            {
                string nums = string.Join(",", cou);
                resul.Message = "截止当前时间已经结束的活动不允许进行修改、移除、禁用、启用等操作，编号：" + nums;
            }
            else
            {
                ents.Each(c =>
                {
                    c.IsEnabled = false;
                    c.UpdatedTime = DateTime.Now;
                    c.OperatorId = AuthorityHelper.OperatorId;
                    
                });
                resul = _salesCampaignRepository.Update(ents.ToList());
            }
            return resul;
        }

        public OperationResult Enable(params int[] ids)
        {
            OperationResult resul = new OperationResult(OperationResultType.Error);
            var ents = _salesCampaignRepository.Entities.Where(c => ids.Contains(c.Id));
            var endtime = DateTime.Now;
            var cou = ents.Where(c => c.CampaignEndTime < endtime).Select(c => c.CampaignNumber).ToList();
            if (cou.Any())
            {
                string nums = string.Join(",", cou);
                resul.Message = "截止当前时间已经结束的活动不允许进行修改、移除、禁用、启用等操作，编号：" + nums;
            }
            else
            {
                ents.Each(c => {
                    c.IsEnabled = true;
                    c.UpdatedTime = DateTime.Now;
                    c.OperatorId = AuthorityHelper.OperatorId;

                });
                resul = _salesCampaignRepository.Update(ents.ToList());
            }
            return resul;
        }

        public OperationResult Remove(params int[] ids)
        {
            OperationResult resul = new OperationResult(OperationResultType.Error);
            var ents = _salesCampaignRepository.Entities.Where(c => ids.Contains(c.Id));
            var endtime = DateTime.Now;
            var cou = ents.Where(c => c.CampaignEndTime < endtime).Select(c => c.CampaignNumber).ToList();
            if (cou.Any())
            {
                string nums = string.Join(",", cou);
                resul.Message = "截止当前时间已经结束的活动不允许进行修改、移除、禁用、启用等操作，编号：" + nums;
            }
            else
            {
                ents.Each(c => {
                    c.IsDeleted = true;
                    c.UpdatedTime = DateTime.Now;
                    c.OperatorId = AuthorityHelper.OperatorId;

                });
                resul = _salesCampaignRepository.Update(ents.ToList());
            }
            return resul;
        }

        public OperationResult Recovery(params int[] ids)
        {
            OperationResult resul = new OperationResult(OperationResultType.Error);
            var ents = _salesCampaignRepository.Entities.Where(c => ids.Contains(c.Id));
            var endtime = DateTime.Now;
            var cou = ents.Where(c => c.CampaignEndTime < endtime).Select(c => c.CampaignNumber).ToList();
            if (cou.Any())
            {
                string nums = string.Join(",", cou);
                resul.Message = "截止当前时间已经结束的活动不允许进行修改、移除、禁用、启用等操作，编号：" + nums;
            }
            else
            {
                ents.Each(c => {
                    c.IsDeleted = false;
                    c.UpdatedTime = DateTime.Now;
                    c.OperatorId = AuthorityHelper.OperatorId;

                });
                resul = _salesCampaignRepository.Update(ents.ToList());
            }
            return resul;
        }


        public List<SalesCampaign> GetAvailableSalesCampaignsByStore(int storeId)
        {
            if (storeId <= 0)
            {
                throw new Exception("参数异常");
            }
            var date = DateTime.Now;
            return _salesCampaignRepository.Entities.Where(s => !s.IsDeleted
                                                    && s.IsEnabled
                                                    && s.StoresIds.Contains(storeId.ToString())
                                                    && s.CampaignStartTime <= date
                                                    && s.CampaignEndTime >= date
            ).ToList();
        }
    }
}
