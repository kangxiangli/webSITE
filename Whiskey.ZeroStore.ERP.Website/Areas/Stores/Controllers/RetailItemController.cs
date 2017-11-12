using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Filter;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Models.Entities;
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Stores.Controllers
{
    public class RetailItemController : Controller
    {
        //商品零售明细
        //
        // GET: /Stores/RetailItem/

        protected readonly IRetailItemContract _retailItemContract;
        protected readonly IRetailContract _retailContract;
        protected readonly IStorageContract _storageContract;

        public RetailItemController(IRetailItemContract retailItemContract, IRetailContract retailContract, IStorageContract storageContract)
        {
            _retailItemContract = retailItemContract;
            _retailContract = retailContract;
            _storageContract = storageContract;
        }
        [Layout]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult List()
        {
            GridRequest gr = new GridRequest(Request);
            Expression<Func<RetailItem, bool>> predict = FilterHelper.GetExpression<RetailItem>(gr.FilterGroup);
            var retailsAll = _retailItemContract.RetailItems.Where(predict).Select(c => new
              {
                  c.Id,
                  //c.OutStorage.StorageName,
                  c.OutStorageIds,
                  c.CreatedTime,
                  c.Product.ProductNumber,
                  c.ProductRetailPrice,
                  c.RetailCount,
                  c.RetailId,
                  c.Retail.RetailNumber,
                  c.IsDeleted,
                  c.IsEnabled

              }).GroupBy(x => x.RetailId).Select(x => new
              {
                  CreateTime = x.Max(t => t.CreatedTime),
                  x
              }).OrderByDescending(c => c.CreateTime);


            var retails = retailsAll.Skip(gr.PageCondition.PageIndex).Take(gr.PageCondition.PageSize).ToList();
            List<object> li = new List<object>();
            foreach (var reta in retails)
            {
                Retail reail = _retailContract.Retails.FirstOrDefault(c => c.Id == reta.x.Key);
                li.Add(new
                {
                    ParentId = "",
                    StoreId = "",
                    MemberNum = reail.Consumer.UniquelyIdentifies,
                    Id = "par" + reta.x.Key,
                    RetailNumber = reail.RetailNumber,
                    //reail.Operator.AdminName,
                    AdminName = reail.OperatorId,
                    CreateTime = reail.CreatedTime.ToString("yyyy-MM-dd hh:mm"),
                    RetailPrice = "",
                    RetailCount = "",
                    ProductNumber = "",
                    StorageName = "",
                    reail.IsEnabled,
                    reail.IsDeleted

                });
                var child = reta.x.Select(c => new
                {
                    c.Id,
                    ParentId = "par" + reta.x.Key,
                    MemberNum = "",
                    RetailNumber = "",
                    AdminName = "",
                    CreateTime = "",
                    RetailPrice = c.ProductRetailPrice,
                    RetailCount = c.RetailCount,
                    c.ProductNumber,
                    StorageName=GetStorageNames(c.OutStorageIds),
                    c.IsDeleted,
                    c.IsEnabled

                });
                li.AddRange(child);
            }
            GridData<object> data = new GridData<object>(li, retailsAll.Count(), gr.RequestInfo);
            return Json(data);
        }


        public ActionResult View(int Id)
        {
            _retailContract.Retails.Where(c => c.Id == Id).Select(c => new
            {
                c.Id,
                c.CreatedTime,
                c.IsDeleted,
                c.IsEnabled,
                c.Note,
                c.OutStorageDatetime,
                c.RetailNumber,
                c.ReturnMoney,
                c.ScoreConsume,
                c.StoredValueConsume,
                c.SwipeConsume,
                c.GetScore,
                c.CashConsume,
                c.CollocationNumber,
                c.ConsumeCount,
                c.Consumer.UniquelyIdentifies,
                
                

            });
            return PartialView();
        }


        private string GetStorageNames(string storageids)
        {
            int[] ids = storageids.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Select(c=>int.Parse(c)).ToArray();
            if (ids.Any())
            {
                var storagname=_storageContract.Storages.Where(c => ids.Contains(c.Id)).Select(c => c.StorageName);
               return string.Join(",", storagname);
            }
            return "";
        }
    }

}