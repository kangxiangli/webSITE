using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Data;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Models.DTO;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Areas.Offices.Models;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Offices.Controllers
{
    public class AdminProfileController : Controller
    {
        protected readonly IAdministratorContract _administratorContract;
        protected readonly IDepartmentContract _departmentContract;
        //protected readonly IAdministratorTypeContract AdministratorTypeContract;
        protected readonly IStoreContract _storeContract;
        protected readonly IStorageContract _storageContract;
        protected readonly IMemberContract _memberContract;
        protected readonly ICollocationContract _collocationContract;
        protected readonly IRoleContract _roleContract;
        protected readonly IPermissionContract _permissionContract;
        protected readonly IModuleContract _moduleContract;
        protected readonly IJobPositionContract _jobPositionContract;
        protected readonly IWorkTimeDetaileContract _workTimeDetaileContract;
        protected readonly IWorkTimeContract _workTimeContract;
        public readonly IEntryContract _entryContract;
        public AdminProfileController(
           IAdministratorContract administratorContract,
           IDepartmentContract departmentContract,
           IStoreContract storeContract,
           IStorageContract storageContract,
           IMemberContract memberContract,
           ICollocationContract collocationContract,
           IRoleContract roleContract,
           IPermissionContract permissionContract,
           IModuleContract moduleContract,
           IJobPositionContract jobPositionContract,
           //IAdministratorTypeContract AdministratorTypeContract,
           IWorkTimeDetaileContract workTimeDetaileContract,
           IWorkTimeContract workTimeContract,
           IEntryContract entryContract)
        {
            _administratorContract = administratorContract;
            _departmentContract = departmentContract;
            _storeContract = storeContract;
            _storageContract = storageContract;
            _memberContract = memberContract;
            _collocationContract = collocationContract;
            _roleContract = roleContract;
            _permissionContract = permissionContract;
            _moduleContract = moduleContract;
            _jobPositionContract = jobPositionContract;
            _workTimeDetaileContract = workTimeDetaileContract;
            _workTimeContract = workTimeContract;
            //this.AdministratorTypeContract = AdministratorTypeContract;
            _entryContract = entryContract;
        }

        // GET: Offices/PersonProfile
        [Layout]
        public ActionResult Index()
        {
            ViewBag.PageFlags = JsonHelper.ToJson(PageFlag());
            return View();
        }

        /// <summary>
        /// 载入创建数据
        /// </summary>
        /// <returns></returns>
        public ActionResult Create()
        {
            return PartialView();
        }
        private string SaveImg(string imgBase64Str)
        {
            var mimeType = imgBase64Str.Substring(0, imgBase64Str.IndexOf(';'));
            var extention = mimeType.Substring(mimeType.IndexOf('/') + 1);
            var serverPath = "/Content/Images/AdminProfile/" + Guid.NewGuid().ToString("N") + "." + extention;
            bool res = ImageHelper.SaveBase64Image(imgBase64Str, serverPath);
            return serverPath;
        }
        public ActionResult View(int id)
        {
            var model = _administratorContract.View(id);
            var adminProfile = _entryContract.Entrys.FirstOrDefault(e => e.MemberId == model.MemberId.Value);
            if (adminProfile == null)
            {
                adminProfile = new Entry() { MemberId = model.MemberId.Value };
            }
            ViewBag.Profile = adminProfile;
            return PartialView(model);
        }

        public ActionResult Update(int id)
        {
            var model = _administratorContract.View(id);
            var adminProfile = _entryContract.Entrys.FirstOrDefault(e => e.MemberId == model.MemberId.Value);
            if (adminProfile == null)
            {
                adminProfile = new Entry() { MemberId = model.MemberId.Value };
            }
            ViewBag.Profile = adminProfile;
            return PartialView(model);
        }

        public class ProfileUpdateEntry
        {
            public int AdminId { get; set; }
            public string BankcardImgPath { get; set; }
            public string IdCardImgPath { get; set; }
            public string HealthCertificateImgPath { get; set; }
            public string PhotoImgPath { get; set; }
            public string RegistFormPath { get; set; }
            public string LaborContractImgPath { get; set; }
            public string ResumeImgPath { get; set; }
        }

        [HttpPost]
        public ActionResult Update(ProfileUpdateEntry dto, Administrator dtoAdmin)
        {
            var model = _administratorContract.View(dto.AdminId);
            var adminProfile = _entryContract.Entrys.FirstOrDefault(e => e.MemberId == model.MemberId.Value);
            var action = "update";
            if (adminProfile == null)
            {
                action = "insert";

                // 补档操作
                adminProfile = new Entry()
                {
                    MemberId = model.MemberId.Value,
                    UpdatedTime = DateTime.Now,
                    EntryTime = model.CreatedTime,        // 入职时间
                    DepartmentId = model.DepartmentId,    // 部门
                    JobPositionId = model.JobPositionId,  // 职位
                    RoleJurisdiction = string.Join(",", model.Roles.Select(r => r.Id).ToList()), //角色ids
                    operationId = AuthorityHelper.OperatorId,
                    ToExamineResult = 3
                };
            }

            model.Member.Email = dtoAdmin.Member.Email;
            model.Member.MobilePhone = dtoAdmin.Member.MobilePhone;
            model.Member.RealName = dtoAdmin.Member.RealName;
            model.Member.Gender = dtoAdmin.Member.Gender;
            model.Member.DateofBirth = dtoAdmin.Member.DateofBirth;
            model.Notes = dtoAdmin.Notes;
            model.EntryTime = dtoAdmin.EntryTime;
            

            if (!string.IsNullOrEmpty(dto.BankcardImgPath) && dto.BankcardImgPath.Length > 0 && dto.BankcardImgPath.IndexOf("base64") > 0)
            {
                adminProfile.BankcardImgPath = SaveImg(dto.BankcardImgPath);
            }

            if (!string.IsNullOrEmpty(dto.IdCardImgPath) && dto.IdCardImgPath.Length > 0 && dto.IdCardImgPath.IndexOf("base64") > 0)
            {
                adminProfile.IdCardImgPath = SaveImg(dto.IdCardImgPath);
            }

            if (!string.IsNullOrEmpty(dto.HealthCertificateImgPath) && dto.HealthCertificateImgPath.Length > 0 && dto.HealthCertificateImgPath.IndexOf("base64") > 0)
            {
                adminProfile.HealthCertificateImgPath = SaveImg(dto.HealthCertificateImgPath);
            }


            if (!string.IsNullOrEmpty(dto.PhotoImgPath) && dto.PhotoImgPath.Length > 0 && dto.PhotoImgPath.IndexOf("base64") > 0)
            {
                adminProfile.PhotoImgPath = SaveImg(dto.PhotoImgPath);
            }

            if (!string.IsNullOrEmpty(dto.RegistFormPath) && dto.RegistFormPath.Length > 0 && dto.RegistFormPath.IndexOf("base64") > 0)
            {
                adminProfile.RegistFormPath = SaveImg(dto.RegistFormPath);
            }

            if (!string.IsNullOrEmpty(dto.LaborContractImgPath) && dto.LaborContractImgPath.Length > 0 && dto.LaborContractImgPath.IndexOf("base64") > 0)
            {
                adminProfile.LaborContractImgPath = SaveImg(dto.LaborContractImgPath);
            }

            if (!string.IsNullOrEmpty(dto.ResumeImgPath) && dto.ResumeImgPath.Length > 0 && dto.ResumeImgPath.IndexOf("base64") > 0)
            {
                adminProfile.ResumeImgPath = SaveImg(dto.ResumeImgPath);
            }

            var oper = _administratorContract.Update(model);
            if (oper.ResultType == OperationResultType.Success)
            {
                if (action == "update")
                {
                    var res = _entryContract.Update(adminProfile);
                    return Json(res);
                }
                else
                {
                    var res = _entryContract.Insert(adminProfile);
                    return Json(res);
                }
            }
            return Json(oper);
        }


        private List<string> PageFlag()
        {
            var area = RouteData.DataTokens.ContainsKey("area") ? RouteData.DataTokens["area"].ToString() : string.Empty;
            var controller = RouteData.Values["controller"].ToString();

            var pageUrl = string.Format("{0}/{1}/Index", area, controller);

            try
            {
                var listpers = PermissionHelper.GetCurrentUserPageNoPermission(pageUrl, _administratorContract, _moduleContract, _permissionContract)
                    .ToList();

                return listpers.Where(p => !string.IsNullOrEmpty(p.OnlyFlag))
                    .Select(p => p.OnlyFlag)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("error");
            }

        }


        /// <summary>
        /// 查询数据
        /// </summary>
        /// <returns></returns>
        public ActionResult List(string name, string mobilePhone, DateTime? startDate, DateTime? endDate, bool isEnabled = true, int pageIndex = 1, int pageSize = 10)
        {

            var query = _administratorContract.Administrators;
            query = query.Where(e => e.IsEnabled == isEnabled && e.DepartmentId.HasValue && e.Department.DepartmentType == DepartmentTypeFlag.公司);
            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(e => e.Member.MemberName.StartsWith(name) || e.Member.RealName.StartsWith(name));
            }

            if (!string.IsNullOrEmpty(mobilePhone) && mobilePhone.Length > 0)
            {
                query = query.Where(e => e.Member.MobilePhone.StartsWith(mobilePhone));
            }

            if (startDate.HasValue)
            {
                query = query.Where(e => e.CreatedTime >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                query = query.Where(e => e.CreatedTime <= endDate.Value);
            }
            var memberEntryQuery = _entryContract.Entrys.Where(e => !e.IsDeleted && e.IsEnabled);
            var resQuery = query.GroupJoin(memberEntryQuery, admin => admin.MemberId, entry => entry.MemberId, (admin, entryCollection) => new
            {
                admin.Id,
                admin.Department.DepartmentName,
                admin.JobPosition.JobPositionName,
                TypeName = admin.Department.DepartmentType + "",
                admin.Member.RealName,
                admin.Member.MobilePhone,
                admin.Member.Gender,
                MemberId = admin.MemberId.Value,
                admin.IsDeleted,
                admin.IsEnabled,
                admin.UpdatedTime,
                admin.CreatedTime,
                admin.EntryTime,
                HasRegistForm = entryCollection.Any(e => !string.IsNullOrEmpty(e.RegistFormPath)),
                HasCertificate = entryCollection.Any(e => !string.IsNullOrEmpty(e.BankcardImgPath) || !string.IsNullOrEmpty(e.IdCardImgPath) || !string.IsNullOrEmpty(e.HealthCertificateImgPath) || !string.IsNullOrEmpty(e.PhotoImgPath)),
                HasLaborContract = entryCollection.Any(e => !string.IsNullOrEmpty(e.LaborContractImgPath)),
                HasResume = entryCollection.Any(e => !string.IsNullOrEmpty(e.ResumeImgPath)),
            });

            var list = resQuery.OrderByDescending(e => e.UpdatedTime).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();

            var res = new OperationResult(OperationResultType.Success, string.Empty, new
            {
                pageData = list,
                pageInfo = new PageDto
                {
                    pageIndex = pageIndex,
                    pageSize = pageSize,
                    totalCount = resQuery.Count(),
                }
            });

            return Json(res, JsonRequestBehavior.AllowGet);
        }





    }
}