using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whiskey.Web.Mvc;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;

namespace Whiskey.ZeroStore.ERP.Website.Areas.ProductTracks.Controllers
{
    public class ProductTrackController : BaseController
    {
        // GET: ProductTracks/ProductTrack

        protected readonly IProductTrackContract _productTrackContract;

        public ProductTrackController(IProductTrackContract productTrackContract)
        {
            _productTrackContract = productTrackContract;
        }

        [Layout]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult List()
        {
            GridRequest request = new GridRequest(Request);
            string ProductBarcode = Request["ProductBarcode"];
            var list = new List<ProductTrack>();
            if (!string.IsNullOrEmpty(ProductBarcode))
            {
                var number = ProductBarcode.Substring(0, ProductBarcode.Length - 3);
                var parItem = _productTrackContract.Tracks.Where(x => x.ProductNumber == number &&
                string.IsNullOrEmpty(x.ProductBarcode)).FirstOrDefault();
                if (parItem != null)
                {
                    list.Add(parItem);
                }
                var child = _productTrackContract.Tracks.Where(x => x.ProductBarcode == ProductBarcode).ToList();
                if (child.Any())
                {
                    list.AddRange(child);
                }
            }
            var da = list.OrderByDescending(x => x.CreatedTime).Skip(request.PageCondition.PageIndex).Take(request.PageCondition.PageSize).Select(x =>
            new
            {
                Id = x.Id,
                x.Describe,
                ProductBarcode = string.IsNullOrEmpty(x.ProductBarcode) ? x.ProductNumber : x.ProductBarcode,
                x.CreatedTime,
                Name = x.Operator.Member.MemberName

            }).ToList();
            GridData<object> data = new GridData<object>(da, list.Count(), request.RequestInfo);
            return Json(data);
        }
    }
}