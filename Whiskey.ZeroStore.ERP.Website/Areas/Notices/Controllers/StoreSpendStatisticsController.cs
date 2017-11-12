using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Ajax.Utilities;
using Whiskey.Utility.Filter;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models.Entities.Notices;
using Whiskey.ZeroStore.ERP.Services;
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Areas.Notices.Models;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Notices.Controllers
{
    public class StoreSpendStatisticsController : BaseController
    {
        protected readonly IStoreSpendStatisticsContract _storeSpendStatisticsContract;
        protected readonly IStorageContract _storageContract;
        protected readonly IStoreStatisticsContract _storeStatisticsContract;
        protected readonly IAdministratorContract _administratorContract;
        private readonly IStoreContract _storeContract;
        public StoreSpendStatisticsController(IStoreSpendStatisticsContract storeSpendStatisticsContract,
             IAdministratorContract administratorContract,
             IStoreContract storeContract,
            IStorageContract storageContract, IStoreStatisticsContract storeStatisticsContract)
        {
            _storeSpendStatisticsContract = storeSpendStatisticsContract;
            _storageContract = storageContract;
            _storeStatisticsContract = storeStatisticsContract;
            _storeContract = storeContract;
            this._administratorContract = administratorContract;
        }

        //店铺开支统计
        // GET: /Notices/StoreSpendStatistics/
        [Layout]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult List(DateTime startDate, DateTime endDate)
        {
            var rq = new GridRequest(Request);
            var startTime = startDate.Date;
            var endTime = endDate.Date.AddDays(1).AddSeconds(-1);
            var pred = FilterHelper.GetExpression<StoreStatistics>(rq.FilterGroup);
            var spendpred = FilterHelper.GetExpression<StoreSpendStatistics>(rq.FilterGroup);
            var storeids = _storeContract.QueryManageStoreId(AuthorityHelper.OperatorId.Value);

            var alldat = _storeStatisticsContract.StoreStatistics
                                                .Where(pred)
                                                .Where(c => c.CreatedTime >= startTime && c.CreatedTime <= endTime)
                                                .Where(c => storeids.Contains(c.StoreId))
                                                .GroupBy(c => c.StoreId)
                                                .OrderBy(c => c.Key);
            var storeStatData = alldat.Skip(rq.PageCondition.PageIndex).Take(rq.PageCondition.PageSize).ToList().Select(c => new
            {
                c.FirstOrDefault().Store.Id,
                c.FirstOrDefault().Store.StoreName,
                Gain = c.Sum(g => g.Gain),
                Cost = c.Sum(g => g.Cost),
            }).ToList();

            var allspenddata = _storeSpendStatisticsContract.StoreStatistics
                 .Where(spendpred)
                 .Where(c => c.CreatedTime >= startTime && c.CreatedTime <= endTime)
                 .Where(c => storeids.Contains(c.StoreId))
                 .GroupBy(c => c.StoreId)
                 .OrderBy(c => c.Key);
            var spenddat =
                allspenddata.Skip(rq.PageCondition.PageIndex)
                    .Take(rq.PageCondition.PageSize)
                    .ToList()
                    .Select(c => new
                    {
                        StoreId = c.Key,
                        Amount = c.GroupBy(g => g.SpendType).Select(g => new
                        {
                            SpendType = g.Key,
                            Amount = g.Sum(t => t.Amount),
                        })
                    }).ToList();

            List<Dictionary<string, string>> dicList = new List<Dictionary<string, string>>();
            foreach (var statItem in storeStatData)
            {
                Dictionary<string, string> dic = new Dictionary<string, string>();
                var storeid = statItem.Id;
                dic.Add("StoreId", storeid.ToString());
                dic.Add("StoreName", statItem.StoreName);
                dic.Add("Gain", statItem.Gain.ToString());
                dic.Add("Cost", statItem.Cost.ToString());
                var spendStat = spenddat.Find(c => c.StoreId == storeid);
                float spendAoun = 0;
                if (spendStat != null)
                {
                    spendAoun = spendStat.Amount.Sum(c => c.Amount);
                    foreach (var amount in spendStat.Amount)
                    {
                        dic.Add(Enum.GetName(typeof(SpendType), amount.SpendType), amount.Amount.ToString());
                    }

                }
                else
                {
                    dic["WaterRate"] = "";
                    dic["ElectricCharge"] = "";
                    dic["Chummage"] = "";
                    dic["Incidentals"] = "";
                    dic["Taxes"] = "";
                    dic["SalaryCoun"] = "";
                    dic["Tending"] = "";
                    dic["Other"] = "";
                }

                var profit = (float)statItem.Gain - spendAoun;
                dic.Add("Profit", profit.ToString());
                dicList.Add(dic);
            }
            List<object> li = new List<object>();
            var te = "";
            foreach (var dic in dicList)
            {
                li.Add(new
                {
                    Id = dic["StoreId"],
                    StoreName = dic["StoreName"],
                    Gain = dic["Gain"],
                    Cost = dic["Cost"],
                    Profit = dic["Profit"],
                    WaterRate = dic.TryGetValue("WaterRate", out te) ? te : "",
                    ElectricCharge = dic.TryGetValue("ElectricCharge", out te) ? te : "",
                    Chummage = dic.TryGetValue("Chummage", out te) ? te : "",
                    Incidentals = dic.TryGetValue("Incidentals", out te) ? te : "",
                    Taxes = dic.TryGetValue("Taxes", out te) ? te : "",
                    SalaryCoun = dic.TryGetValue("SalaryCoun", out te) ? te : "",
                    Tending = dic.TryGetValue("Tending", out te) ? te : "",
                    Other = dic.TryGetValue("Other", out te) ? te : "",
                });
            }

            int cou = alldat.Count();
            GridData<object> data = new GridData<object>(li, cou, Request);
            return Json(data);
        }

        [HttpPost]
        public ActionResult Create(StoreSpendStatisticsDto storeSpenddto)
        {
            var storespend = AutoMapper.Mapper.Map<StoreSpendStatistics>(storeSpenddto);
            storespend.StartYear = storeSpenddto.StartTime.Year;
            storespend.StartMonth = (byte)storeSpenddto.StartTime.Month;
            storespend.StartDay = (byte)storeSpenddto.StartTime.Day;

            storespend.EndYear = storeSpenddto.EndTime.Year;
            storespend.EndMonth = (byte)storeSpenddto.EndTime.Month;
            storespend.EndDay = (byte)storeSpenddto.EndTime.Day;

            var res = _storeSpendStatisticsContract.Insert(storespend);
            return Json(res);
        }

        /// <summary>
        /// 添加店铺消费记录
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Create()
        {
            ViewBag.SpendType = CacheAccess.GetSpendType();

            
            return PartialView();
        }

    }
}