using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Filter;
using Whiskey.Utility.Logging;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using Whiskey.Core.Data.Extensions;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models.Entities.Collocations;
using Whiskey.Utility.Extensions;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Subjects.Controllers
{
    public class CollocationController : BaseController
    {
        #region 声明操作对象
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(CollocationController));

        protected readonly ICollocationContract _collocationContract;
        protected readonly IDepartmentContract _departmentContract;
        protected readonly IMemberContract _memberContract;
        protected readonly IStoreContract _storeContract;
        protected readonly IAdministratorContract _administratContract;

        public CollocationController(ICollocationContract collocationContract, IDepartmentContract departmentContract, IMemberContract memberContract, IStoreContract storeContract, IAdministratorContract administratContract)
        {
            _collocationContract = collocationContract;
            _departmentContract = departmentContract;
            _memberContract = memberContract;
            _administratContract = administratContract;
            _storeContract = storeContract;
        }
        #endregion

        #region 初始化界面
        /// <summary>
        /// 初始化操作界面
        /// </summary>
        /// <returns></returns>
        [Layout]
        public ActionResult Index()
        {
            ViewBag.Departments = CacheAccess.GetDepartmentListItem(_departmentContract, true);
            return View();
        }
        #endregion

      
        /// <summary>
        /// 获取数据列表
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> List()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<Collocation, bool>> predicate = FilterHelper.GetExpression<Collocation>(request.FilterGroup);
            int count = 0;
            var data = await Task.Run(() =>
            {
               
                var list = _collocationContract.Collocations.Where(predicate).ToList();
                var resul = list.Select(m => new
                  {
                      m.Id,
                      m.Numb,
                      m.State,
                      DepartmentName = GetCollocationInfo(m.Admini, "depart"),
                      Email = GetCollocationInfo(m.Admini, "email"),
                      PhoneNumb = GetCollocationInfo(m.Admini, "mobilePho"),
                      RealName = GetCollocationInfo(m.Admini, "realname"),
                      Note = m.Notes ?? "",
                      m.UpdatedTime,
                      AdminName = m.Operator == null ? "" : m.Operator.Member.MemberName,
                      m.IsDeleted,
                      m.IsEnabled
                  }).ToList();
                return new GridData<object>(resul, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        private string GetCollocationInfo(Administrator administrator, string infoname)
        {
            if (administrator != null)
            {
                switch (infoname)
                {
                    case "depart":
                        {
                            if (administrator.Department != null)
                            {
                                return administrator.Department.DepartmentName;
                            }
                            return null;
                        }
                    case "email":
                        {
                            return administrator.Member.Email;
                        }
                    case "mobilePho":
                        {
                            return administrator.Member.MobilePhone;
                        }
                    case "realname":
                        {
                            return administrator.Member.RealName;
                        }

                    default:
                        break;
                }


                return null;
            }
            return null;
        }   

        /// <summary>
        /// 载入创建数据
        /// </summary>
        /// <returns></returns>
        public ActionResult Create()
        {
            ViewBag.Departments = CacheAccess.GetDepartmentListItem(_departmentContract, false);
            return PartialView();
        }


        /// <summary>
        /// 创建数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(CollocationDto coldto, Administrator admidto)
        {
            Collocation coll = AutoMapper.Mapper.Map<Collocation>(coldto);
            coll.Numb = admidto.Member.UniquelyIdentifies;
            coll.IsDeleted = false;
            coll.IsEnabled = true;
            var admin = _administratContract.Administrators.Where(c => c.IsDeleted == false && c.IsEnabled == true && c.MemberId == admidto.MemberId).FirstOrDefault();
            if (admin != null)
            {
                coll.AdminiId = admin.Id;
            }
            else
            {
                Administrator adm = AutoMapper.Mapper.Map<Administrator>(admidto);
                adm.Member = _memberContract.Members.FirstOrDefault(c => c.Id == adm.MemberId);

                adm.IsLogin = true;
                adm.IsDeleted = false;
                adm.IsEnabled = true;
                adm.JobPositionId = 1;

                coll.Admini = adm;
            }

            var result = _collocationContract.Insert(coll);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
      
        public ActionResult DetailView(int Id)
        {
            ViewBag.collId = Id;
            return PartialView();
        }
        public ActionResult DetailList()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<EarningsDetail, bool>> predicate = FilterHelper.GetExpression<EarningsDetail>(request.FilterGroup);

            string idstr = Request.Params["Id"];
            List<object> li = new List<object>();
            int cou = 0;
            if (!string.IsNullOrEmpty(idstr))
            {
                int id = Convert.ToInt32(idstr);

                var collc = _collocationContract.Collocations.Where(c => c.IsDeleted == false && c.IsEnabled == true && c.Id == id).FirstOrDefault();
                if (collc != null && collc.EarningsDetail != null)
                {
                    cou = collc.EarningsDetail.Where(predicate.Compile()).Count();
                    li.AddRange(collc.EarningsDetail.Where(predicate.Compile()).Select(c => new
                     {
                         c.Id,
                         c.IsCloseAnAccount,
                         c.CloseAnAccountTime,
                         ConsumeDateTime = c.ConsumeOrder.CreatedTime,
                         c.ConsumeOrder.Member.MemberName,
                         c.Totalexpendamount,
                         c.EarningsType,
                         collc.Numb,
                         collc.Admini.Member.RealName,
                         c.EarningsPercent,
                         c.EarningsNotes
                     }).Skip(request.PageCondition.PageIndex).Take(request.PageCondition.PageSize).ToList());
                }
            }
            GridData<object> redat = new GridData<object>(li, cou, request.RequestInfo);
            return Json(redat);
        }

     
        public ActionResult MembList()
        {
            List<int?> stores = _memberContract.Members.Select(c => c.StoreId).Distinct().ToList();

            var li = _storeContract.Stores.Where(c => c.IsDeleted == false && c.IsEnabled == true && stores.Contains(c.Id)).Select(c => new SelectListItem()
            {
                Text = c.StoreName,
                Value = c.Id.ToString()
            }).ToList();
            li.Insert(0, new SelectListItem()
            {
                Text = "下拉选择",
                Value = ""
            });
            ViewBag.StoreIds = li;
            var colls = _collocationContract.Collocations.Where(c => c.IsDeleted == false && c.IsEnabled == true).Select(c => new SelectListItem()
            {
                Value = c.Id.ToString(),
                Text = c.CollocationName,
            }).ToList();
            colls.Insert(0, new SelectListItem()
            {
                Text = "下拉选择",
                Value = ""
            });
            ViewBag.CollocationIds = colls;
            ViewBag.Departments = CacheAccess.GetDepartmentListItem(_departmentContract, true);
            return PartialView();
        }
        public ActionResult GetInfoById(int Id)
        {
            OperationResult resul = new OperationResult(OperationResultType.Error);
            var memb = _memberContract.Members.Where(c => c.Id == Id).Select(c => new
            {

                c.Id,
                MembNum = c.UniquelyIdentifies,
                c.RealName,
                c.MobilePhone,
                c.MemberName,
                c.MemberPass,
                c.CardNumber,
                c.IDCard,
                c.Email,
                c.Gender,

            }).FirstOrDefault();
            //判断将要添加的员工是否已存在
            bool exisAdmi = _administratContract.Administrators.Where(c => (c.Member.MemberName == memb.MemberName || c.Member.UniquelyIdentifies == memb.MembNum) && c.IsDeleted == false && c.IsEnabled == true).Count() > 0;
            bool exisColl = false;
            if (exisAdmi)
            {
                exisColl = _collocationContract.Collocations.Where(c => c.Numb == memb.MembNum && c.IsDeleted == false && c.IsEnabled == true).Count() > 0;
            }

            if (exisColl)
            {
                resul = new OperationResult(OperationResultType.Error, "该编号的搭配师已经存在") { Data = memb };
            }
            else
            {
                resul = new OperationResult(OperationResultType.Success, "") { Data = memb };
            }
            return Json(resul);

        }
        /// <summary>
        /// 载入修改数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult Update(int Id)
        {
            var result = _collocationContract.Collocations.Where(c => c.IsDeleted == false && c.IsEnabled == true && c.Id == Id).FirstOrDefault();

            if (result != null && result.Admini != null)
            {
                var deps = CacheAccess.GetDepartmentListItem(_departmentContract, false);
                string seldepId = result.Admini.DepartmentId != null ? result.Admini.DepartmentId.ToString() : null;
                var selcDep = deps.Where(c => c.Value == seldepId).FirstOrDefault();
                selcDep.Selected = true;

                ViewBag.Departments = deps;
                ViewBag.stat = result.State;
                return PartialView(result);
            }
            return Json(new OperationResult(OperationResultType.Error));
        }

        /// <summary>
        /// 提交数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Update(Collocation colldto)
        {
            OperationResult resul = new OperationResult(OperationResultType.Error);
            var origpass = Request["origPass"];
            Administrator admin = _administratContract.Administrators.Where(c => c.IsDeleted == false && c.IsEnabled == true && c.Id == colldto.Admini.Id).FirstOrDefault();
            Collocation coll = _collocationContract.Collocations.Where(c => c.IsDeleted == false && c.IsEnabled == true && c.Numb == colldto.Admini.Member.UniquelyIdentifies).FirstOrDefault();

            if (admin != null&&coll!=null)
            {
                if (!string.IsNullOrEmpty(origpass))
                {
                    origpass = origpass.MD5Hash();
                    if (admin.Member.MemberPass == origpass)
                    {
                        admin.Member.MemberPass = colldto.Admini.Member.MemberPass.MD5Hash();
                    }
                    else {
                        resul = new OperationResult(OperationResultType.Error, "原密码不正确");
                    }
                }
                admin.Member = Utils.UpdateNavMemberInfo(colldto.Admini,_memberContract);
                var cId = coll.Id;
                coll = AutoMapper.Mapper.Map(colldto, coll);
                coll.Id = cId;
                coll.Admini=admin;
                coll.AdminiId = admin.Id;
                coll.Numb = admin.Member.UniquelyIdentifies;
                resul = _collocationContract.Update(coll);
            }
            else {
                resul = new OperationResult(OperationResultType.Error, "指定的修改对象不存在");
            }
            return Json(resul, JsonRequestBehavior.AllowGet);
        }
        //yxk 2016-1-27
        /// <summary>
        /// 根据搭配师编号获取搭配师信息
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public ActionResult GetCollcationInfo(string num)
        {
            num=InputHelper.SafeInput(num);
            OperationResult res=new OperationResult(OperationResultType.Error);
           var coll= _collocationContract.Collocations.Where(c => c.IsEnabled && !c.IsDeleted && c.Numb == num).Select(x=>new
           {
               x.Id,
               x.Numb,
               name=x.Admini.Member.RealName
           }).FirstOrDefault();
            if (coll != null)
            {
                res=new OperationResult(OperationResultType.Success){Data = coll};
            }
            return Json(res);
        }


        #region 查看数据
        /// <summary>
        /// 查看数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        public ActionResult View(int Id)
        {
            var result = _collocationContract.Collocations.Where(c => c.Id == Id).FirstOrDefault();
            return PartialView(result);
        }
        #endregion

        #region 移除数据
        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public ActionResult Remove(int[] Id)
        {
            var result = _collocationContract.Remove(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 删除数据
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public ActionResult Delete(int[] Id)
        {
            var result = _collocationContract.Delete(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 恢复数据
        /// <summary>
        /// 恢复数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public ActionResult Recovery(int[] Id)
        {
            var result = _collocationContract.Recovery(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 启用数据
        /// <summary>
        /// 启用数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public ActionResult Enable(int[] Id)
        {
            var result = _collocationContract.Enable(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 禁用数据
        /// <summary>
        /// 禁用数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public ActionResult Disable(int[] Id)
        {
            var result = _collocationContract.Disable(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}