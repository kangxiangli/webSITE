using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.ZeroStore.ERP.Website.Models;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Warehouse;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.Web.Helper;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Warehouses.Controllers
{
    [CheckStoreIsClosed]
    public class AddProductController : BaseController
    {

        #region 初始化操作对象

        protected readonly IStoreContract _storeContract;
        protected readonly IBrandContract _brandContract;
        protected readonly IStorageContract _storageContract;
        protected readonly ICheckerContract _checkerContract;
        protected readonly IAdministratorContract _adminContract;
        protected readonly IDesignerContract _designerContract;

        public AddProductController(IStoreContract storeContract,
            IBrandContract brandContract, IAdministratorContract adminContract,
            IStorageContract storageContract,
            IDesignerContract _designerContract,
            ICheckerContract checkerContract)
        {
            _adminContract = adminContract;
            _storeContract = storeContract;
            _brandContract = brandContract;
            _storageContract = storageContract;
            _checkerContract = checkerContract;
            this._designerContract = _designerContract;
        }
        #endregion
        // GET: /Warehouses/AddProduct/
        [Layout]
        [HttpGet]
        public ActionResult Index()
        {
            ViewBag.ScanInvalidCount = 0;
            ViewBag.ScanValidCount = 0;

            var list = new List<SelectListItem>();
            var listStores = new List<SelectListItem>();

            var mod = _designerContract.SelectDesigner.Where(w => w.IsEnabled && !w.IsDeleted && w.AdminId == AuthorityHelper.OperatorId.Value).Select(s => new
            {
                s.Factory.StoreId,
                s.Factory.Store.StoreName,
                s.Factory.StorageId,
                s.Factory.Storage.StorageName,
            }).FirstOrDefault();
            if (mod != null)
            {
                list.Add(new SelectListItem()
                {
                    Value = mod.StorageId + "",
                    Text = mod.StorageName,
                });
                listStores.Add(new SelectListItem()
                {
                    Value = mod.StoreId + "",
                    Text = mod.StoreName,
                });
            }

            ViewBag.Stores = listStores;
            ViewBag.Storages = list;
            ViewBag.IsDesigner = mod != null;

            return View();
        }
        /// <summary>
        /// 获取指定的店铺下当前用户有权限操作的仓库
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetEnableAddProductStorageById(int id)
        {
            var li = CacheAccess.GetManagedStorageByStoreId(_storageContract, _adminContract, id, true).ToList();
            return Json(li);
        }
        //yxk 2015-9
        //upload file
        public JsonResult ExcelFileUpload()
        {

            OperationResult resul = new OperationResult(OperationResultType.Error);
            //string fileName = uploadfile.FileName;
            //string savepath = "./Warehouses/Content/uploadFiles/" + fileName;
            //uploadfile.SaveAs(savepath);
            int te = Request.Files.Count;
            if (System.Web.HttpContext.Current.Request.Files.Count > 0)
            {

                var file = Request.Files[0];
                string fileName = file.FileName;
                string savePath = Server.MapPath("/Content/UploadFiles/Excels/") + DateTime.Now.ToString("yyyyMMddHH");
                if (!Directory.Exists(savePath))
                {
                    Directory.CreateDirectory(savePath);
                }
                string fullName = savePath + "\\" + fileName;

                if (System.IO.File.Exists(fullName))
                {
                    System.IO.File.Delete(fullName);
                }
                file.SaveAs(fullName);
                var reda = ExcelToJson(fullName);
                System.IO.File.Delete(fullName);
                if (reda.Any())
                    resul = new OperationResult(OperationResultType.Success);
            }
            return Json(resul);
        }

        private List<List<String>> ExcelToJson(string fileName)
        {
            if (System.IO.File.Exists(fileName))
            {
                var da = new List<List<String>>();
                if (Path.GetExtension(fileName) == ".txt")
                {
                    string st = System.IO.File.ReadAllText(fileName);
                    var retda = st.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    var li = new List<List<string>>();
                    retda.Each(c =>
                    {
                        var t = new List<string>() { c };
                        li.Add(t);
                    });
                    da = li;
                }
                else
                {
                    YxkSabri.ExcelUtility excel = new YxkSabri.ExcelUtility();
                    da = excel.ExcelToArray(fileName, 0, 0);
                    var _key = "sess_excel_inpor_instorage_11";
                    SessionAccess.Set(_key, da, true);

                }
                return da;
            }
            return null;
        }
        [HttpPost]
        public void UnloadPg()
        {
            Session.Remove("ScanInvalid");
            Session.Remove("ScanValid");
        }

        #region 校验选择选择店铺和仓库是否在盘点中
        public JsonResult IsChecker(int StoreId, int StorageId)
        {
            var isChecking = _storeContract.IsInCheckingStat(StoreId);
            int count = isChecking ? 1 : 0;
            return Json(count);
        }
        #endregion
    }
}