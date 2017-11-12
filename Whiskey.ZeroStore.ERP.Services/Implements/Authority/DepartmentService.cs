using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Utility.Class;
using Whiskey.Utility.Data;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.Utility;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Services.Content;

namespace Whiskey.ZeroStore.ERP.Services.Implements.Authority
{
    //2015-9
    public class DepartmentService : ServiceBase,IDepartmentContract
    {
        private readonly IRepository<Department, int> _departmentRepository;

        protected readonly ILogger _Logger = LogManager.GetLogger(typeof(DepartmentService));
        public DepartmentService(IRepository<Department,int> departmentRepository):base(departmentRepository.UnitOfWork) {
            _departmentRepository = departmentRepository;
        }
        public Department View(int Id)
        {
            throw new NotImplementedException();
        }

        public Department Edit(int Id)
        {
            throw new NotImplementedException();
        }

        public OperationResult Insert(params DepartmentDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                List<Department> listDepartment = this.Departments.ToList();
                foreach (var dto in dtos)
                {
                    int count =listDepartment.Where(x => x.DepartmentName == dto.DepartmentName).Count();
                    if (count > 0)
                    {
                        return new OperationResult(OperationResultType.Error, "部门名称已经存在");
                    }
                    if (!string.IsNullOrEmpty(dto.MacAddress))
                    {
                       string[] arrMac = dto.MacAddress.Split(',');
                       List<string> listMac= listDepartment.Where(x=>!string.IsNullOrEmpty(x.MacAddress)).Select(x => x.MacAddress).ToList();
                       foreach (string mac in listMac)
                       {
                           string[] arrDres = mac.Split(',');
                           foreach (string item in arrDres)
                           {
                               foreach (string temp in arrMac)
                               {
                                   if (!string.IsNullOrEmpty(item) && !string.IsNullOrEmpty(item) && item ==temp)
                                   {
                                       return new OperationResult(OperationResultType.Error, "Mac地址已经存在");
                                   }
                               }
                           }                           
                       }
                    }
                }
                OperationResult result = _departmentRepository.Insert(dtos,
                dto =>
                {

                },
                (dto, entity) =>
                {
                    entity.CreatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    return entity;
                });
                return result;
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                _Logger.Error<string>(ex.ToString());
                return new OperationResult(OperationResultType.Error, "添加失败！" );
            }
        }
        public OperationResult Insert(params Department[] deps)
        {
           return _departmentRepository.Insert((IEnumerable<Department>)deps)>0?new OperationResult(OperationResultType.Success):new OperationResult(OperationResultType.Error);
        }
        public Utility.Data.OperationResult Update(params Department[] depas)
        {
            var result = _departmentRepository.Update(depas);
            if (result.ResultType == OperationResultType.Success)
            {
                CacheAccess.ClearPermissionCache();
            }
            return result;
        }

        public Utility.Data.OperationResult Remove(params int[] ids)
        {
            throw new NotImplementedException();
        }

        public Utility.Data.OperationResult Recovery(params int[] ids)
        {
            throw new NotImplementedException();
        }

        public Utility.Data.OperationResult Delete(params int[] ids)
        {
            throw new NotImplementedException();
        }

        public Utility.Data.OperationResult Enable(params int[] ids)
        {
            throw new NotImplementedException();
        }

        public Utility.Data.OperationResult Disable(params int[] ids)
        {
            throw new NotImplementedException();
        }

        public bool CheckNameExist(string AdminName)
        {
            throw new NotImplementedException();
        }

        public Utility.Data.OperationResult CheckLogin(DepartmentDto dto)
        {
            throw new NotImplementedException();
        }

        public Utility.Data.OperationResult FindPassword(DepartmentDto dto)
        {
            throw new NotImplementedException();
        }

        public bool CheckExists(System.Linq.Expressions.Expression<Func<Department, bool>> predicate, int id = 0)
        {
            throw new NotImplementedException();
        }
        public IQueryable<Department> Departments
        {
            get { return _departmentRepository.Entities; }
        }


        public IEnumerable<Values<string, string>> SelectDepartment(string title, Expression<Func<Department, bool>> predicate)
        {
            var list = _departmentRepository.Entities.Where(predicate).Select(m => new Values<string, string> {
              Key=m.Id.ToString(),
              Value=m.DepartmentName,
              IsDeleted=m.IsDeleted,
              IsEnabled=m.IsEnabled
            }).ToList();
            if (list.Count() > 0&&!string.IsNullOrEmpty(title)) {
                list.Insert(0, new Values<string, string> { Key = "", Value = title, IsDeleted = false, IsEnabled = true });
            }
            return list;
        }

        #region 获取选择列表

        public List<SelectListItem> SelectList(string title)
        {
            var list = _departmentRepository.Entities.Where(x => x.IsDeleted == false && x.IsEnabled == true).Select(m => new SelectListItem
            {
                Text = m.DepartmentName,
                Value = m.Id.ToString(),                                 
            }).ToList();
            if (!string.IsNullOrEmpty(title))
            {
                list.Insert(0, new SelectListItem { Text=title, Value = "", });
            }
            return list;
        }

        public List<SelectListItem> SelectList(string title, int departId)
        {
            List<Department> listDepart = this.Departments.Where(x => x.IsDeleted == false && x.IsEnabled == true).ToList();
            Department depart = listDepart.FirstOrDefault(x => x.Id == departId);
            listDepart.Remove(depart);
            Department parent = depart.Parent;
            while (true)
            {
                
                if (parent!=null)
                {
                    listDepart.Remove(parent);
                    parent = parent.Parent;
                }
                else
                {
                    break;
                }
            }
            List<SelectListItem> list = listDepart.Select(m => new SelectListItem
            {
                Text = m.DepartmentName,
                Value = m.Id.ToString(),                                 
            }).ToList();
            if (!string.IsNullOrEmpty(title))
            {
                list.Insert(0, new SelectListItem { Text=title, Value = "", });
            }
            return list;
        }
        #endregion


        #region 保存编辑数据
        /// <summary>
        /// 保存编辑数据
        /// </summary>
        /// <param name="dtos"></param>
        /// <returns></returns>
        
        public OperationResult Update(params DepartmentDto[] dtos)
        {
            dtos.CheckNotNull("dtos");
            try
            {
              IQueryable<Department> listDepartment = Departments;
              foreach (var dto in dtos)
              {
                   int count = listDepartment.Where(x => x.DepartmentName == dto.DepartmentName && x.Id!=dto.Id).Count();
                   if (count>0)
                   {
                       return new OperationResult(OperationResultType.Error, "部门名称已经存在"); 
                   }
                   if (!string.IsNullOrEmpty(dto.MacAddress))
                   {
                       string[] arrMac = dto.MacAddress.Split(',');
                       List<string> listMac = listDepartment.Where(x => !string.IsNullOrEmpty(x.MacAddress) && x.Id!=dto.Id).Select(x => x.MacAddress).ToList();
                       foreach (string mac in listMac)
                       {
                           string[] arrDres = mac.Split(',');
                           foreach (string item in arrDres)
                           {
                               foreach (string temp in arrMac)
                               {
                                   if (!string.IsNullOrEmpty(item) && !string.IsNullOrEmpty(item) && item == temp)
                                   {
                                       return new OperationResult(OperationResultType.Error, "Mac地址已经存在");
                                   }
                               }
                           }
                       }
                   }  
              }                
              OperationResult result = _departmentRepository.Update(dtos,
		      dto =>
		      {
                  
		      },
		      (dto, entity) => {
                  //var departIds = (dto.DepartmentIds != null && dto.DepartmentIds.Length > 0) ? dto.DepartmentIds.Split(',').ToList() : new List<string>();
                  //var children = this.Departments.Where(x => departIds.Contains(x.Id.ToString())).ToList();
                  //entity.Children = children;
		      	entity.UpdatedTime = DateTime.Now;
		      	entity.OperatorId=AuthorityHelper.OperatorId;                        
		      	return entity;
		      });
              if (result.ResultType == OperationResultType.Success)
              {
                  CacheAccess.ClearPermissionCache();
              }
			  return result;
            }
            catch (Exception ex){
                _Logger.Error<string>(ex.ToString());
                return new OperationResult(OperationResultType.Error, "更新失败" );
            }
        }
        #endregion


        
    }
}
