using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Filter;
using Whiskey.ZeroStore.ERP.Models.Entities;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using Whiskey.Core.Data.Extensions;
using Whiskey.ZeroStore.ERP.Transfers;
using AutoMapper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using System.Web.Script.Serialization;
using ExpressionParser;
using Whiskey.Utility.Data;
using System.Web.Caching;
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Website.Areas.Authorities.Models;
using Whiskey.Utility.Extensions;


namespace Whiskey.ZeroStore.ERP.Website.Areas.Authorities.Controllers
{
    public class DepartmentController : BaseController
    {
        //
        // GET: /Authorities/Department/
        protected readonly IDepartmentContract _departmentContract;
        protected readonly IAdministratorContract _administratorContract;
        protected readonly IPermissionContract _permissionContract;
        protected readonly IModuleContract _moduleContract;


        public DepartmentController(IDepartmentContract departmentContract, 
            IAdministratorContract administratorContract, 
            IPermissionContract permissionContract,
            IModuleContract moduleContract)
        {
            _departmentContract = departmentContract;
            _administratorContract = administratorContract;
            _permissionContract = permissionContract;
            _moduleContract = moduleContract;
            var li = CacheAccess.GetDepartmentListItem(_departmentContract, true);
            ViewBag.Departments = li;
        }
        [Layout]
        [Log]
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Create()
        {
            return PartialView();
        }
        [HttpPost]
        public ActionResult Create(DepartmentDto dto)
        {
            #region 部门权限已弃用

            //var right = Request["right"];
            //var rigshowstr = Request["rightShow"] ?? "";
            //if (!string.IsNullOrEmpty(right))
            //{
            //    List<int> ids = right.Replace("c", "").Split(",", true).ToList().ConvertAll(c => Convert.ToInt32(c));
            //    List<int> pershowids = rigshowstr.Replace("c", "").Split(",", true).ToList().ConvertAll(c => Convert.ToInt32(c));
            //    //ids = _permissionContract.Permissions.Where(c => ids.Contains(c.Id)).Select(s => s.Id).ToList();//最好做一次数据的校验，防止数据库中不存在传入的数据
            //    if (dto.ADepartmentPermissionRelations.IsNullThis())
            //        dto.ADepartmentPermissionRelations = new List<ADepartmentPermissionRelation>();
            //    ids.ForEach(s =>
            //    {
            //        dto.ADepartmentPermissionRelations.Add(new ADepartmentPermissionRelation()
            //        {
            //            DepartmentId = dto.Id,
            //            PermissionsId = s,
            //            IsShow = pershowids.Exists(e => e == s)
            //        });
            //    });
            //}

            #endregion

            #region 注释代码

            //bool isEnabled = Request["IsEnabled"] == "1" ? true : false;
            //string Description = Request["Description"];
            //dto.IsEnabled = isEnabled;
            //Department dep = new Department()
            //{
            //    //DepartmentName = DepartmentName,
            //    CreatedTime = DateTime.Now,
            //    UpdatedTime = DateTime.Now,
            //    IsDeleted = false,
            //    IsEnabled = isEnabled,
            //    Description = Description,
            //    Permissions = li,
            //    OperatorId = AuthorityHelper.OperatorId
            //};
            #endregion

            OperationResult resul = _departmentContract.Insert(dto);
            return Json(resul);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Log]
        public ActionResult GetUserList()
        {
            var ismulti = Request["allowmulti"];
            ViewBag.Multi = ismulti == null ? "-1" : "1";
           
            return PartialView();
        }
        //yxk 2015-9-17
        /// <summary>
        /// 获取部门和部门下的人员列表
        /// </summary>
        /// <returns></returns>
       
        public JsonResult GetDepartMember()
        {

            GridRequest request = new GridRequest(Request);
            var pred = FilterHelper.GetExpression<Administrator>(request.FilterGroup);
            var alladmi = _administratorContract.Administrators.Where(pred).Where(c => !c.IsDeleted && c.IsEnabled).GroupBy(c => c.DepartmentId);
            var readmin =
                alladmi.OrderByDescending(c => c.Max(g => g.DepartmentId))
                    .Skip(request.PageCondition.PageIndex)
                    .Take(request.PageCondition.PageSize)
                    .Select(c => new
                    {
                        departmentId = c.Key,
                        child = c.Select(g => new
                        {
                            g.Id,
                            g.Member.RealName,
                            g.Member.MemberName,
                            g.Member.Gender,
                            g.Member.MobilePhone,
                            g.Notes,
                            g.IsDeleted,
                            g.IsEnabled
                        })
                    });

            var deparIds= readmin.Select(c => c.departmentId).ToArray();
           var depar= _departmentContract.Departments.Where(c => deparIds.Contains(c.Id)).Select(c => new
            {
                c.Id,
                c.DepartmentName
            }).ToList();
            List<object> lida=new List<object>();
            foreach (var admin in readmin)
            {
                var depa = depar.First(c => c.Id == admin.departmentId);
               
                lida.Add(new
                {
                    ParentId = "",
                    Id="DE"+admin.departmentId,
                    RealName = depa.DepartmentName,
                    AttrName = "",
                    Gender="",
                    Telephone = "",
                    Description = "",
                    IsEnabled = true,
                    IsDeleted = false
                });
                lida.AddRange(admin.child.Select(c=>new
                {
                    ParentId ="DE"+ admin.departmentId,
                    c.Id,
                    RealName = c.RealName,
                    AttrName = c.MemberName,
                    Gender=c.Gender.ToString(),
                    Telephone=c.MobilePhone,
                    Description= c.Notes,
                    c.IsEnabled,
                    c.IsDeleted
                   
                }));
            }
            // ParentId = "", RealName = dep.DepartmentName, AttrName = "", Id = "DE" + dep.Id, Description = "", Gender = "", Telephone = "", IsEnabled = true, IsDeleted = false }



            //FilterGroup fg = new JavaScriptSerializer().Deserialize(Request["Conditions"], typeof(FilterGroup)) as FilterGroup;

            //ExpressionAccessHelp<Administrator> exp = new ExpressionAccessHelp<Administrator>();
            //ExpressionAccessHelp<Department> exp1 = new ExpressionAccessHelp<Department>();

            //foreach (FilterRule rule in fg.Rules)
            //{

            //    if (rule.Field == "AttributeName"&&rule.Value!=null&&rule.Value.ToString().Trim()!="")
            //    {
            //        exp.OrWithContains(new Tuple<string, object>[] {
            //     new Tuple<string,object>("AdminName",rule.Value),
            //     new Tuple<string,object>("RealName",rule.Value)
                 
            //    });
            //    }
            //    if (rule.Field == "Department" && rule.Value!= null && rule.Value.ToString().Trim() != "")
            //    {
            //        exp1.Equal("Id", Convert.ToInt32(rule.Value).ToString().Trim());
            //    }

            //}

            //var departments = CacheAccess.GetDepartments(_departmentContract).Where(pred).Where(c => c.IsDeleted == false && c.IsEnabled == true).OrderBy(c => c.Id);

            //List<object> adminList = new List<object>();
            //foreach (Department dep in departments)
            //{

            //    var adlist = _administratorContract.Administrators.Where(exp.GetExpression()).Where(c => c.IsDeleted == false && c.IsEnabled == true && c.DepartmentId == dep.Id).Select(c => new { ParentId = "DE" + c.DepartmentId, AttrName = c.AdminName, Id = c.Id, Description = c.Notes, Gender = c.Gender, RealName = c.RealName, Telephone = c.MobilePhone, IsDeleted = c.IsDeleted, IsEnabled = c.IsEnabled }).ToList();

            //    adminList.Add(new { ParentId = "", RealName = dep.DepartmentName, AttrName = "", Id = "DE" + dep.Id, Description = "", Gender = "", Telephone = "", IsEnabled = true, IsDeleted = false });
            //    adminList.AddRange(adlist);
            //}

            //测试数据  测试通过
            //var adminList1 = new List<object>()
            //{
            //  new{ParentId="",AttrName="自然",Id=1,Description = "",Telephone = "", IsEnabled=true,IsDeleted=false},
            //  new{ParentId=1,AttrName="测试",Id=3,Description = "",Telephone = "", IsEnabled=true,IsDeleted=false},
            //  new{ParentId=1,AttrName="测试",Id=4,Description = "",Telephone = "", IsEnabled=true,IsDeleted=false},
            //  new{ParentId=1,AttrName="测试",Id=5,Description = "",Telephone = "", IsEnabled=true,IsDeleted=false},
            //  new{ParentId="",AttrName="测试",Id=6,Description = "",Telephone = "", IsEnabled=true,IsDeleted=false},
            //  new{ParentId=6,AttrName="测试",Id=7,Description = "",Telephone = "", IsEnabled=true,IsDeleted=false},
            //  new{ParentId=6,AttrName="测试",Id=8,Description = "",Telephone = "", IsEnabled=true,IsDeleted=false},
            //};
            var data = new GridData<object>(lida, alladmi.Count(), request.RequestInfo);
            return Json(data, JsonRequestBehavior.AllowGet);

        }
        //yxk 2015-11
        /// <summary>
        /// 获取部门列表
        /// </summary>
        /// <returns></returns>
        public  async Task<ActionResult> List()
        {
            //List<Department> li = CacheAccess.GetDepartments(departmentContract);

            GridRequest request = new GridRequest(Request);
            Expression<Func<Department, bool>> predicate = FilterHelper.GetExpression<Department>(request.FilterGroup);
            var data = await Task.Run(() =>
            {                
                int count = 0;

                var list = (from c in _departmentContract.Departments.Where<Department, int>(predicate, request.PageCondition, out count)
                            let modJobAdmin = _administratorContract.Administrators.FirstOrDefault(f => f.DepartmentId == c.Id && f.JobPosition.IsLeader && f.IsEnabled && !f.IsDeleted)
                            let realName = modJobAdmin != null ? modJobAdmin.Member != null ? modJobAdmin.Member.RealName : string.Empty : string.Empty
                            select new
                            {
                                c.Id,
                                Notes = c.Description,
                                c.CreatedTime,
                                c.UpdatedTime,
                                c.DepartmentName,
                                RealName = realName,
                                userCoun = _administratorContract.Administrators.Where(x => x.DepartmentId == c.Id && x.IsDeleted == false && x.IsEnabled == true).Count(),
                                c.IsDeleted,
                                c.IsEnabled,
                                c.MacAddress,
                                c.Stores.Count,
                                DepartmentType = c.DepartmentType.ToString()
                            }).ToList();

                return new GridData<object>(list, count, request.RequestInfo);               
            });
            return Json(data, JsonRequestBehavior.AllowGet);

        }
        /// <summary>
        /// 获取所有部门
        /// </summary>
        /// <returns></returns>
        public JsonResult GetAllDepartment()
        {
            return Json(CacheAccess.GetDepartmentListItem(_departmentContract, false), JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 选择部门列表（SelectListItem）
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> GetDepartmentSelectList()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<Department, bool>> predicate = FilterHelper.GetExpression<Department>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                var list = (from c in _departmentContract.Departments.Where(predicate)
                            select new SelectListItem
                            {
                                Value = c.Id + "",
                                Text = c.DepartmentName
                            }).ToList();

                return list;
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取指定部门类型的Ids
        /// </summary>
        /// <param name="flag"></param>
        /// <returns></returns>
        public JsonResult GetTypeDepartmentIds(DepartmentTypeFlag? flag)
        {
            var query = _departmentContract.Departments.Where(c => c.IsDeleted == false && c.IsEnabled == true);
            if (flag.HasValue)
            {
                query = query.Where(w => w.DepartmentType == flag);
            }
            var list = query.Select(s => s.Id).ToList();
            return Json(list);
        }

        private Administrator GetAdmin(int? admid)
        {
            Administrator admin=new Administrator();
            if (admid != null)
            {
                admin = _administratorContract.Administrators.FirstOrDefault(c => c.IsEnabled && !c.IsDeleted&&c.Id==admid);
               
            }
            
            return admin;
        }
        public ActionResult View(int Id)
        {
            var part = _departmentContract.Departments.Single(c => c.Id == Id);
            if (part != null)
            {
                ViewBag.RealName = _administratorContract.Administrators.FirstOrDefault(f => f.DepartmentId == part.Id && f.JobPosition.IsLeader && f.IsEnabled && !f.IsDeleted)?.Member?.RealName;

                #region 加载部门权限已弃用

                //List<ModuPermission> li = new List<ModuPermission>();
                //foreach (var item in part.ADepartmentPermissionRelations.Select(s => s.Permission))
                //{
                //    int id = item.Module.Id;
                //    ModuPermission mod = li.SingleOrDefault(c => c.Id == id);
                //    if (mod == null)
                //    {
                //        li.Add(new ModuPermission()
                //        {
                //            Id = id,
                //            Name = item.Module.ModuleName,
                //            Description = item.Module.Description,
                //            Child = new List<ModuPermission>(){
                //          new ModuPermission(){
                //           Id=item.Id,
                //           Name=item.PermissionName,
                //           Description=item.Description,
                //           Child=null
                //          }
                //         }
                //        });
                //    }
                //    else
                //    {
                //        mod.Child.Add(new ModuPermission()
                //        {
                //            Id = item.Id,
                //            Name = item.PermissionName,
                //            Description = item.Description,
                //            Child = null
                //        });
                //    }
                //}
                //ViewBag.da = li;

                #endregion

                string strChild = string.Empty;
                if (part.Children!=null)
                {
                    strChild = part.Children.Select(x => x.DepartmentName).ToList().ExpandAndToString();
                }
                ViewBag.Children = strChild;
            }
            return PartialView(part);

        }
        public ActionResult GetMember()
        {
            ViewBag.DepartmentID = Request["did"];
            return PartialView();
        }
        /// <summary>
        /// 根据部门id获取员工信息
        /// </summary>
        /// <returns></returns>
        public ActionResult GetMemberByDeparId()
        {
            var id = Request["did"];
            GridData<object> dat = new GridData<object>(new List<object>(), 0, Request);
            if (!string.IsNullOrEmpty(id))
            {
                int _id = Convert.ToInt32(id);
                var li = _administratorContract.Administrators.Where(c => c.DepartmentId
                    == _id && c.IsDeleted == false && c.IsEnabled == true).Select(x => new
                    {
                        adminName = x.Member.MemberName,
                        realName = x.Member.RealName,
                        createTime = x.CreatedTime,
                        note = x.Notes,
                        //isAdmin = (_departmentContract.Departments.Where(s => s.Id == _id).FirstOrDefault().AdministratorId) == x.Id
                        isAdmin =x.JobPosition.IsLeader
                    }).ToList();
                li = li.OrderByDescending(c => c.isAdmin).ToList();
                dat = new GridData<object>(li, li.Count(), Request);
            }
            return Json(dat);

        }
        public ActionResult Update(int Id)
        {
            IQueryable<Department> listDepart = _departmentContract.Departments;
            var part = listDepart.Single(c => c.Id == Id);
            if (part != null)
            {
                //ViewBag.RealName = part.JobPositions.FirstOrDefault(x => x.IsLeader == true) == null ? string.Empty : part.JobPositions.FirstOrDefault(x => x.IsLeader == true).Administrators.FirstOrDefault().RealName;
                //ViewBag.RealName = part.Administrator == null ? "" : part.Administrator.RealName;
            }
            Mapper.CreateMap<Department, DepartmentDto>();
            var dto = Mapper.Map<Department, DepartmentDto>(part);
            //dto.DepartmentIds = part.Children.Select(x => x.Id).ToList().ExpandAndToString();
            //IQueryable<Administrator> listAdmin = _administratorContract.Administrators.Where(c => c.IsDeleted == false && c.IsEnabled == true && c.DepartmentId == Id);
            #region 注释代码-
                        
            //List<SelectListItem> li = listAdmin.Select(c => new SelectListItem()
            //{
            //    Text = c.RealName,
            //    Value = c.Id.ToString(),
            //    Selected = c.Id == part.AdministratorId
            //}).ToList();
            //if (li.Where(c => c.Selected).Count() == 0)
            //{
            //    li.Insert(0, new SelectListItem()
            //    {
            //        Text = "请下拉选择",
            //        Value = ""
            //    });
            //}
            #endregion
            //int departId=part.Id;
            //string title=string.Empty;

            //List<SelectListItem> listEntity = _departmentContract.SelectList(title, departId);
            //List<SelectListItem> list = new List<SelectListItem>();
            //if (part.Children!=null)
            //{                
            //    list = listAdmin.Select(x => new SelectListItem { 
            //      Value=x.Id.ToString(),
            //      Text = x.Member.RealName,
            //      Selected = x.Id == part.SubordinateId,
            //    }).ToList();
            //}
            //List<SelectListItem> departments = new List<SelectListItem>();
            //departments.AddRange(listEntity);            
            //ViewBag.Subordinate = list;
            //ViewBag.Departs = listEntity;
            //ViewBag.Departments = departments;
            //ViewBag.Admin = li;
            return PartialView(dto);
        }
        [HttpPost]
        public ActionResult Update(DepartmentDto dto)
        {
            //int id = Convert.ToInt32(Request["Id"]);

            //string Description = Request["Description"];
            //bool isEnabled = Request["IsEnabled"] == "1" ? true : false;
            //bool isDelet = Request["IsDeleted"] == "1" ? true : false;
            //string adminId = Request["AdminId"];
            //string strDepartIds = Request["DepartmentIds"];
            //dto.DepartmentIds = strDepartIds;
            //dto.IsEnabled = isEnabled;
            //dto.IsDeleted = isDelet;
            //string strMac = Request["MacAddress"];
            //Department dep = new Department()
            //{
            //    Id = id,
            //    DepartmentName = DepartmentName,
            //    CreatedTime = DateTime.Now,
            //    UpdatedTime = DateTime.Now,
            //    IsDeleted = isDelet,
            //    IsEnabled = isEnabled,
            //    Description = Description,
            //    MacAddress=strMac,
            //    OperatorId = AuthorityHelper.OperatorId
            //};   
            //if (!string.IsNullOrEmpty(adminId))
            //{
            //    dto.AdministratorId = Convert.ToInt32(adminId);
            //}

            Department department = _departmentContract.Departments.FirstOrDefault(f => f.Id == dto.Id);
            department = AutoMapper.Mapper.Map(dto, department);

            #region 部门权限已弃用

            //var rigstr = Request["right"];
            //var rigshowstr = Request["rightShow"] ?? "";
            //if (!string.IsNullOrEmpty(rigstr))
            //{
            //    List<int> perids = rigstr.Replace("c", "").Split(",", true).ToList().ConvertAll(c => Convert.ToInt32(c));
            //    List<int> pershowids = rigshowstr.Replace("c", "").Split(",", true).ToList().ConvertAll(c => Convert.ToInt32(c));

            //    var allperssionIds = new List<int>();
            //    if (department.ADepartmentPermissionRelations.IsNotNullOrEmptyThis())
            //    {
            //        allperssionIds = department.ADepartmentPermissionRelations.Select(s => s.PermissionsId.Value).ToList();
            //    }
            //    var addperids = perids.Except(allperssionIds).ToList();
            //    var removeids = allperssionIds.Except(perids).ToList();
            //    if (department.ADepartmentPermissionRelations.IsNullThis())
            //        department.ADepartmentPermissionRelations = new List<ADepartmentPermissionRelation>();
            //    addperids.ForEach(s =>
            //    {
            //        department.ADepartmentPermissionRelations.Add(new ADepartmentPermissionRelation()
            //        {
            //            DepartmentId = department.Id,
            //            PermissionsId = s,
            //            IsShow = pershowids.Exists(e => e == s)
            //        });
            //    });

            //    foreach (var item in removeids)
            //    {
            //        var per = department.ADepartmentPermissionRelations.FirstOrDefault(f => f.PermissionsId == item);
            //        if (per.IsNotNull())
            //            department.ADepartmentPermissionRelations.Remove(per);
            //    }
            //    department.ADepartmentPermissionRelations.Where(w => pershowids.Contains(w.PermissionsId.Value)).Each(e => e.IsShow = true);//需要修改为显示
            //    department.ADepartmentPermissionRelations.Where(w => !pershowids.Contains(w.PermissionsId.Value)).Each(e => e.IsShow = false);//需要修改为不显示的
            //}
            //else
            //    department.ADepartmentPermissionRelations.Clear();

            #endregion

            OperationResult resul = _departmentContract.Update(department);
            return Json(resul);
        }
        public ActionResult Edit()
        {
            //parId:parId, partName: parName, notes: notes, admin: admi, isdel: isdel, isenab: isenab 
            OperationResult resul = new OperationResult(OperationResultType.Error);
            int parId = Convert.ToInt32(Request["parId"]);
            string parName = InputHelper.SafeInput(Request["partName"]);
            string notes = InputHelper.SafeInput(Request["notes"]);
            string adminId = Request["admin"];
            bool isdele = Request["isdel"] == "1" ? true : false;
            bool isenabl = Request["isenab"] == "1" ? true : false;
            var par = _departmentContract.Departments.Single(c => c.Id == parId);
            if (par != null)
            {
                par.UpdatedTime = DateTime.Now;
                par.OperatorId = AuthorityHelper.OperatorId;
                par.IsDeleted = isdele;
                par.IsEnabled = isenabl;
                if (!string.IsNullOrEmpty(parName))
                    par.DepartmentName = parName;

                if (!string.IsNullOrEmpty(notes))
                    par.Description = notes;
                if (!string.IsNullOrEmpty(adminId))
                {
                    int admi = Convert.ToInt32(adminId);
                    //par.AdministratorId = admi;
                }
            }
            resul = _departmentContract.Update(par);
            return Json(resul);
        }
        public ActionResult Disable(int Id)
        {
            OperationResult resul = new OperationResult(OperationResultType.Error);
            var dep = _departmentContract.Departments.FirstOrDefault(c => c.Id == Id);
            if (dep != null)
            {
                dep.IsEnabled = false;
                dep.UpdatedTime = DateTime.Now;
                resul = _departmentContract.Update(dep);
            }
            return Json(resul);
        }
        public ActionResult Enable(int Id)
        {
            OperationResult resul = new OperationResult(OperationResultType.Error);
            var dep = _departmentContract.Departments.FirstOrDefault(c => c.Id == Id);
            if (dep != null)
            {
                dep.IsEnabled = true;
                dep.UpdatedTime = DateTime.Now;
                resul = _departmentContract.Update(dep);
            }
            return Json(resul);
        }
        public ActionResult Remove(int Id)
        {
            OperationResult resul = new OperationResult(OperationResultType.Error);
            var dep = _departmentContract.Departments.FirstOrDefault(c => c.Id == Id);
            if (dep != null)
            {
                dep.IsDeleted = true;
                dep.UpdatedTime = DateTime.Now;
                resul = _departmentContract.Update(dep);
            }
            return Json(resul);
        }
        public ActionResult Recovery(int Id)
        {
            OperationResult resul = new OperationResult(OperationResultType.Error);
            var dep = _departmentContract.Departments.Single(c => c.Id == Id);
            if (dep != null)
            {
                dep.IsDeleted = false;
                dep.UpdatedTime = DateTime.Now;
                resul = _departmentContract.Update(dep);
            }
            return Json(resul);
        }

        public ActionResult GetChilById()
        {
            //GetChilById?_pid
            OperationResult resul = new OperationResult(OperationResultType.Error);
            var parid = InputHelper.SafeInput(Request["_pid"]);
            if (!string.IsNullOrEmpty(parid))
            {
                int pid = Convert.ToInt32(parid);

                var pmodu = CacheAccess.GetModules(_moduleContract).Single(c => c.Id == pid && c.IsDeleted == false && c.IsEnabled == true);
                if (pmodu != null)
                {
                    var chil = pmodu.Permissions.Select(c => new RightTree()
                    {
                        id = "c" + c.Id,
                        text = c.PermissionName,
                        _checked = false,
                        url = "",
                        msg = c.Description
                    });
                    return Json(new ResJson()
                    {
                        obj = chil,
                        msg = "",
                        success = true,

                    });
                }
            }
            return Json(resul);
        }

        #region 部门权限已弃用

        //public ActionResult LoadPermissionTree()
        //{
        //    return PartialView();
        //}
        //public ActionResult GetTree()
        //{
        //    int depid = Convert.ToInt32(Request["did"]);
        //    var model = new List<Module>();
        //    ResJson json = new ResJson() { };

        //    var dep = _departmentContract.Departments.FirstOrDefault(c => c.Id == depid);
        //    if (dep.IsNotNull() && dep.ADepartmentPermissionRelations.IsNotNullThis())
        //    {
        //        var rolepers = dep.ADepartmentPermissionRelations.ToList();
        //        List<Permission> li = new List<Permission>();
        //        if (dep.ADepartmentPermissionRelations.IsNotNullOrEmptyThis())
        //            li = dep.ADepartmentPermissionRelations.Select(s => s.Permission).ToList();

        //        json = new ResJson()
        //        {
        //            msg = "测试",
        //            obj = GetNode(li, rolepers),
        //            success = true,
        //            type = "json"
        //        };

        //    }
        //    return Json(json);
        //}

        //private RightTree GetNode(List<Permission> perli, List<ADepartmentPermissionRelation> rolepers)
        //{
        //    RightTree retree = new RightTree()
        //    {
        //        id = "0",
        //        _checked = false,
        //        text = "选择权限",
        //        url = "",
        //        children = new List<RightTree>()
        //    };
        //    var parmodules = CacheAccess.GetModules(_moduleContract).Where(c => c.ParentId == null && c.IsEnabled == true && c.IsDeleted == false).ToList();
        //    foreach (var item in parmodules)
        //    {
        //        var tre = new RightTree()
        //        {
        //            id = item.Id + "",
        //            text = item.ModuleName,
        //            url = "",
        //            children = GetChild(item.Id, perli, rolepers),
        //            _checked = false,
        //            msg = item.Description
        //        };
        //        tre._checked = tre.children.Count(c => c._checked) > 0;
        //        tre._isShow = tre.children.Count(x => x._isShow) > 0;
        //        retree.children.Add(tre);
        //    }
        //    bool chec = retree.children.Any(c => c._checked == true);
        //    retree._checked = chec;
        //    return retree;
        //}
        //private List<RightTree> GetChild(int parid, List<Permission> perli, List<ADepartmentPermissionRelation> rolepers)
        //{
        //    List<RightTree> li = new List<RightTree>();

        //    var mods = CacheAccess.GetModules(_moduleContract).Where(c => c.ParentId == parid && c.IsDeleted == false && c.IsEnabled == true);
        //    List<bool> ches = new List<bool>();
        //    foreach (var c in mods)
        //    {

        //        var tr = new RightTree()
        //        {
        //            id = c.Id + "",
        //            text = c.ModuleName,
        //            //text=GetCheckPermiss(c.Id,perli,out ch),
        //            url = "",
        //            children = GetPermiss(c.Id, perli, rolepers),
        //            _checked = false,
        //            msg = c.Description
        //        };
        //        tr._checked = tr.children.Count(x => x._checked) > 0;
        //        tr._isShow = tr.children.Count(x => x._isShow) > 0;
        //        li.Add(tr);
        //    }

        //    return li;
        //}

        //private List<RightTree> GetPermiss(int twoModId, List<Permission> perli, List<ADepartmentPermissionRelation> rolepers)
        //{
        //    var pers = CacheAccess.GetPermissions(_permissionContract).Where(c => c.ModuleId == twoModId && c.IsEnabled == true && c.IsDeleted == false).ToList();

        //    return pers.Select(c => new RightTree()
        //     {
        //         id = "c" + c.Id,
        //         text = c.PermissionName,
        //         url = "",
        //         msg = c.Description,
        //         _checked = perli.Select(x => x.Id).Contains(c.Id),
        //        _isShow = rolepers.Count(cc => cc.PermissionsId == c.Id && cc.IsShow != false) > 0,
        //         _gtype = (int?)c.Gtype
        //     }).ToList();
        //}
        //public string GetCheckPermiss(int twoModId, List<Permission> perli, out bool che)
        //{
        //    var pers = _permissionContract.Permissions.Where(c => c.ModuleId == twoModId && c.IsEnabled == true && c.IsDeleted == false).ToList();
        //    string tx = "";
        //    List<bool> ches = new List<bool>();
        //    foreach (var item in pers)
        //    {
        //        bool _checked = perli.Select(x => x.Id).Contains(item.Id);
        //        ches.Add(_checked);
        //        if (_checked)
        //        {
        //            tx += "<label title='" + item.Description + "'><input checked='checked' type='checkbox' value='c'" + item.Id + "/>" + item.PermissionName + "</label>";

        //        }
        //        else
        //        {
        //            tx += "<label title='" + item.Description + "'><input type='checkbox' value='c'" + item.Id + "/>" + item.PermissionName + "</label>";

        //        }

        //    }
        //    che = ches.Any(c => c == true);
        //    return tx;
        //}

        #endregion

        #region 获取部门人员
        public JsonResult GetAdmin(int DepartId)
        {
           IQueryable<Administrator> listAdmin =  _administratorContract.Administrators.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.DepartmentId == DepartId);
           var entity = listAdmin.Select(x => new { 
             x.Id,
             x.Member.RealName,
           });
           return Json(entity, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 部分视图界面
        public ActionResult Department()
        {
            return PartialView();
        }
        #endregion
    }
}
