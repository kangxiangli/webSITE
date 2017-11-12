using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Web;
using AutoMapper;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Web.Helper;
using Whiskey.Web.SignalR;
using Whiskey.Web.Http;
using Whiskey.Web.Extensions;
using Whiskey.Utility;
using Whiskey.Utility.Data;
using Whiskey.Utility.Web;
using Whiskey.Utility.Class;
using Whiskey.Utility.Extensions;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using System.Security.Cryptography;
using System.Web.Mvc;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Authority;
using Whiskey.ZeroStore.ERP.Services.Extensions.Helper;
using Whiskey.ZeroStore.ERP.Services.Content;
using System.Data.Entity;
using Whiskey.jpush.api;
using Whiskey.Utility.Helper;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{

    public class AdministratorService : ServiceBase, IAdministratorContract
    {
        #region AdministratorService


        private readonly IRepository<Administrator, int> _administratorRepository;

        private readonly IRepository<Member, int> _memberRepository;
        private readonly IRepository<Designer, int> _designerRepository;
        private readonly ITemplateContract _TemplateContract;
        private readonly IRepository<MobileInfo, int> _mobileInfoRepo;
        public AdministratorService(
            IRepository<Administrator, int> administratorRepository,
            IRepository<Designer, int> _designerRepository,
            ITemplateContract _TemplateContract,
            IRepository<Member, int> memberRepository,
            IRepository<MobileInfo, int> mobileInfoRepo)
            : base(administratorRepository.UnitOfWork)
        {
            _administratorRepository = administratorRepository;
            _memberRepository = memberRepository;
            this._designerRepository = _designerRepository;
            this._TemplateContract = _TemplateContract;
            _mobileInfoRepo = mobileInfoRepo;
        }


        /// <summary>
        /// 获取单个数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public Administrator View(int Id)
        {
            var entity = _administratorRepository.GetByKey(Id);
            return entity;
        }


        /// <summary>
        /// 获取单个DTO数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public AdministratorDto Edit(int Id)
        {
            var entity = _administratorRepository.GetByKey(Id);
            Mapper.CreateMap<Administrator, AdministratorDto>();
            var dto = Mapper.Map<Administrator, AdministratorDto>(entity);
            return dto;
        }


        /// <summary>
        /// 获取数据集
        /// </summary>
        public IQueryable<Administrator> Administrators { get { return _administratorRepository.Entities; } }



        /// <summary>
        /// 按条件检查数据是否存在
        /// </summary>
        /// <param name="predicate">检查谓语表达式</param>
        /// <param name="id">更新的编号</param>
        /// <returns>是否存在</returns>
        public bool CheckExists(Expression<Func<Administrator, bool>> predicate, int id = 0)
        {
            return _administratorRepository.ExistsCheck(predicate, id);
        }



        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dtos">要添加的DTO数据</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Insert(params AdministratorDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                OperationResult result = _administratorRepository.Insert(dtos,
                dto =>
                {

                },
                (dto, entity) =>
                {
                    //entity.AdminPass = dto.AdminPass.MD5Hash();
                    entity.CreatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    return entity;
                });
                return result;
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "添加失败！错误如下：" + ex.Message, ex.ToString());
            }
        }

        public OperationResult Insert(params Administrator[] admins)
        {
            UnitOfWork.TransactionEnabled = true;
            foreach (Administrator admin in admins)
            {
                Member member = _memberRepository.Entities.FirstOrDefault(x => x.Id == admin.MemberId);
                if (member != null)
                {
                    member.Email = admin.Member.Email;
                    member.MobilePhone = admin.Member.MobilePhone;
                    member.Gender = admin.Member.Gender;
                    member.UpdatedTime = DateTime.Now;
                    member.RealName = admin.Member.RealName;
                    _memberRepository.Update(member);
                }

            }
            #region 员工来自会员,添加会员时已进行过MD5加密
            //admins.Each(c =>
            //{
            //    if (!string.IsNullOrEmpty(c.Member.MemberPass)) c.Member.MemberPass = c.Member.MemberPass.MD5Hash();
            //    else
            //    {
            //        //c.AdminPass = c.Member.MemberPass;
            //    }
            //});
            #endregion

            _administratorRepository.Insert((IEnumerable<Administrator>)admins);
            int count = UnitOfWork.SaveChanges();

            return count > 0 ? new OperationResult(OperationResultType.Success, "添加成功") : new OperationResult(OperationResultType.Error, "添加失败");
        }


        public OperationResult Update(params Administrator[] admins)
        {
            try
            {
                admins.CheckNotNull("admins");
                UnitOfWork.TransactionEnabled = true;
                admins.Each(e => e.UpdatedTime = DateTime.Now);

                _administratorRepository.Update(admins);
                int count = UnitOfWork.SaveChanges();

                var result = count > 0 ? new OperationResult(OperationResultType.Success, "修改成功") : new OperationResult(OperationResultType.Error, "修改失败");
                if (result.ResultType == OperationResultType.Success)
                {
                    var adminIds = admins.Select(s => s.Id).ToArray();
                    CacheAccess.ClearPermissionCache(adminIds);
                    RedisCacheHelper.ResetManageStoreDepartmentCache(adminIds);
                }
                return result;
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "更新失败！错误如下：" + ex.Message, ex.ToString());
            }
        }

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="dtos">包含更新数据的DTO数据</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Update(params AdministratorDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                OperationResult result = _administratorRepository.Update(dtos,
                    dto =>
                    {

                    },
                    (dto, entity) =>
                    {
                        entity.UpdatedTime = DateTime.Now;
                        entity.OperatorId = AuthorityHelper.OperatorId;
                        return entity;
                    });
                if (result.ResultType == OperationResultType.Success)
                {
                    CacheAccess.ClearPermissionCache(dtos.Select(s => s.Id).ToArray());
                }
                return result;
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "更新失败！错误如下：" + ex.Message, ex.ToString());
            }
        }



        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="ids">要移除的编号</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Remove(params int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _administratorRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _administratorRepository.Update(entity);
                }
                var result = UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "移除成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
                if (result.ResultType == OperationResultType.Success)
                {
                    CacheAccess.ClearPermissionCache(ids);
                }
                return result;
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "移除失败！错误如下：" + ex.Message, ex.ToString());
            }
        }


        /// <summary>
        /// 恢复数据
        /// </summary>
        /// <param name="ids">要恢复的编号</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Recovery(params int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _administratorRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _administratorRepository.Update(entity);
                }
                var result = UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "恢复成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
                if (result.ResultType == OperationResultType.Success)
                {
                    CacheAccess.ClearPermissionCache(ids);
                }
                return result;
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "恢复失败！错误如下：" + ex.Message, ex.ToString());
            }
        }


        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="ids">要删除的编号</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Delete(params int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                OperationResult result = _administratorRepository.Delete(ids);
                if (result.ResultType == OperationResultType.Success)
                {
                    CacheAccess.ClearPermissionCache(ids);
                }
                return result;
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "删除失败！错误如下：" + ex.Message, ex.ToString());
            }

        }


        /// <summary>
        /// 启用数据
        /// </summary>
        /// <param name="ids">要启用的编号</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Enable(params int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _administratorRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _administratorRepository.Update(entity);
                }
                var result = UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "启用成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
                if (result.ResultType == OperationResultType.Success)
                {
                    CacheAccess.ClearPermissionCache(ids);
                }
                return result;
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "启用失败！错误如下：" + ex.Message, ex.ToString());
            }
        }


        /// <summary>
        /// 禁用数据
        /// </summary>
        /// <param name="ids">要禁用的编号</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Disable(params int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _administratorRepository.Entities.Where(m => ids.Contains(m.Id)).ToList();
                foreach (var entity in entities)
                {
                    entity.IsEnabled = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    //_administratorRepository.Update(entity);
                }
                var result = UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "禁用成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
                if (result.ResultType == OperationResultType.Success)
                {
                    CacheAccess.ClearPermissionCache(ids);
                }
                return result;
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "禁用失败！错误如下：" + ex.Message, ex.ToString());
            }
        }



        /// <summary>
        /// 检测用户名是否存在
        /// </summary>
        /// <param name="AdminName"></param>
        /// <returns></returns>
        public bool CheckNameExist(string AdminName)
        {

            bool isExist = false;
            try
            {
                var adminName = InputHelper.SafeInput(AdminName).Trim().ToLower();
                var entity = _administratorRepository.Entities.FirstOrDefault(m => m.Member.MemberName.Trim().ToLower() == adminName);
                if (entity != null)
                {
                    isExist = true;
                }
            }
            catch
            {
                isExist = false;
            }

            return isExist;
        }


        /// <summary>
        /// 检测登录
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public OperationResult CheckLogin(Administrator dto)
        {

            OperationResult result = new OperationResult(OperationResultType.Error, "你输入的密码不正确！");
            try
            {
                IQueryable<Administrator> listAdmin = this.Administrators.Where(x => x.IsDeleted == false && x.IsEnabled == true);
                List<Administrator> listEntity = new List<Administrator>();
                if (!string.IsNullOrEmpty(dto.Member.MemberName))
                {
                    string adminName = InputHelper.SafeInput(dto.Member.MemberName).Trim().ToLower();
                    //Administrator entity = listAdmin.FirstOrDefault(m => m.Member.MemberName.Trim().ToLower() == adminName);
                    Member entity = _memberRepository.Entities.FirstOrDefault(x => x.IsDeleted == false && x.IsEnabled == true && x.MemberName.Trim().ToLower() == adminName);
                    if (entity == null)
                    {
                        result.Message = "你输入的帐号不存在！";
                    }
                    else
                    {
                        Administrator admin = listAdmin.Where(x => x.MemberId == entity.Id)
                            .Include(x => x.Department)
                            .Include(x => x.Department.Stores)
                            .FirstOrDefault();
                        if (admin == null)
                        {
                            result.Message = "你输入的帐号不存在！";
                        }
                        else
                        {
                            listEntity.Add(admin);
                        }
                    }
                }
                if (!string.IsNullOrEmpty(dto.Member.MobilePhone) && dto.Member.MobilePhone.IsMobileNumber())
                {
                    string telPhone = InputHelper.SafeInput(dto.Member.MobilePhone).Trim().ToLower();
                    Administrator entity = listAdmin
                        .Where(m => m.Member.MobilePhone.Trim().ToLower() == telPhone)
                            .Include(x => x.Department)
                            .Include(x => x.Department.Stores)
                        .FirstOrDefault();
                    if (entity == null)
                    {
                        result = new OperationResult(OperationResultType.Error, "你输入的手机不存在！");
                    }
                    else
                    {
                        listEntity.Add(entity);
                    }
                }
                foreach (var entity in listEntity)
                {
                    if (entity.Member.MemberPass.Trim().ToLower() == dto.Member.MemberPass.MD5Hash().Trim().ToLower())
                    {
                        result = new OperationResult(OperationResultType.Success, "你已经登录成功！");
                        entity.LoginTime = DateTime.Now;
                        entity.LoginCount += 1;
                        entity.JPushRegistrationID = dto.JPushRegistrationID;
                        result.Data = entity;
                        _administratorRepository.Update(entity);
                        break;
                    }
                    else
                    {
                        result = new OperationResult(OperationResultType.Error, "你输入的密码不正确！");
                    }
                }
            }
            catch (Exception ex)
            {
                result = new OperationResult(OperationResultType.Error, "登录出现异常！错误如下：" + ex.Message, ex.ToString());
            }

            return result;
        }


        /// <summary>
        /// 找回密码
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public OperationResult FindPassword(Administrator dto)
        {

            var entity = _administratorRepository.Entities.FirstOrDefault(m => m.Member.MemberName == dto.Member.MemberName && m.Member.Email == dto.Member.Email);

            if (entity != null)
            {
                string randomPassword = RandomHelper.GetRandomPassword(6);
                OperationResult result = EmailHelper.SendMail(dto.Member.Email, "零时尚轻奢汇（重置员工密码）", "你的新员工密码是：" + randomPassword + "，请登录后立即修改！");
                if (result.ResultType == OperationResultType.Success)
                {
                    entity.Member.MemberPass = randomPassword.MD5Hash();
                    _administratorRepository.Update(entity);
                    return new OperationResult(OperationResultType.Success, "我们已将你的密码重置，并发送至（" + dto.Member.Email + "）邮箱，请注意查收。");
                }
                else
                {
                    return new OperationResult(OperationResultType.Error, "邮件发送失败，错误如下：" + result.Message);
                }
            }
            else
            {
                return new OperationResult(OperationResultType.Error, "你输入的信息有误或此员工不存在！");
            }
        }


        public List<SelectListItem> SelectList(string title)
        {
            IQueryable<Administrator> listAdmin = this.Administrators.Where(x => x.IsDeleted == false && x.IsEnabled == true);
            List<SelectListItem> list = new List<SelectListItem>();
            list = listAdmin.Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Member.RealName }).ToList();
            if (!string.IsNullOrEmpty(title))
            {
                list.Insert(0, new SelectListItem { Value = string.Empty, Text = title });
            }
            return list;
        }

        #region 更新数据
        public OperationResult Update(int AdminId, string KeyWord, AdminUpdateFlag flag)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error);
            try
            {
                Administrator entity = this.View(AdminId);
                if (entity == null)
                {
                    oper.Message = "会员不存在";
                    return oper;
                }

                UnitOfWork.TransactionEnabled = true;
                Member member = new Member();
                member = _memberRepository.Entities.FirstOrDefault(x => x.Id == entity.MemberId);
                switch (flag)
                {
                    case AdminUpdateFlag.DateOfBirth:
                        {
                            DateTime date = DateTime.Parse(KeyWord);
                            member.DateofBirth = date;
                            _memberRepository.Update(member);
                            int cnt = UnitOfWork.SaveChanges();
                            if (cnt > 0)
                            {
                                oper.ResultType = OperationResultType.Success;
                                oper.Message = "修改成功";
                            }
                            else
                            {
                                oper.Message = "修改失败";
                                return oper;
                            }
                        }
                        break;

                    case AdminUpdateFlag.RealName:
                    case AdminUpdateFlag.Gender:

                    case AdminUpdateFlag.Email:

                    case AdminUpdateFlag.MacAddress:
                    case AdminUpdateFlag.MobilePhone:
                        {
                            //参数校验
                            oper = AdminParamsHelper.CheckParams(entity, KeyWord, flag);
                            if (oper.ResultType != OperationResultType.Success)
                            {
                                return oper;
                            }

                            //手机号排重
                            if (flag == AdminUpdateFlag.MobilePhone)
                            {
                                int resCount = _memberRepository.Entities.Where(x => x.MobilePhone == KeyWord).Count();
                                if (resCount > 0)
                                {
                                    oper.ResultType = OperationResultType.Error;
                                    oper.Message = "手机号码已经存在";
                                    return oper;
                                }
                            }

                        }
                        break;
                    case AdminUpdateFlag.IDCard:
                        {
                            KeyWord = KeyWord.Trim();
                            if (KeyWord.Length != 18)
                            {
                                return new OperationResult(OperationResultType.Error, "身份证格式不正确");
                            }
                            oper.ResultType = OperationResultType.Success;
                            member.IDCard = KeyWord;

                        }
                        break;
                    default:
                        break;
                }

                //更新
                //member.Email = entity.Member.Email;
                //member.Gender = entity.Member.Gender;
                //member.MobilePhone = entity.Member.MobilePhone;
                //member.RealName = entity.Member.RealName;
                member.UpdatedTime = DateTime.Now;
                entity.UpdatedTime = DateTime.Now;
                _memberRepository.Update(member);
                _administratorRepository.Update(entity);
                int count = UnitOfWork.SaveChanges();
                if (count > 0)
                {
                    oper.ResultType = OperationResultType.Success;
                    oper.Message = "修改成功";
                }
                else
                {
                    oper.Message = "修改失败";
                }

            }
            catch (Exception)
            {
                oper.Message = "服务器忙，请稍后重试";
            }
            if (oper.ResultType == OperationResultType.Success)
            {
                CacheAccess.ClearPermissionCache(AdminId);
            }
            return oper;
        }
        #endregion

        #region 修改密码
        public OperationResult UpdatePass(int AdminId, string OldPass, string PassWord)
        {
            PassWord = PassWord.Trim();
            OperationResult oper = new OperationResult(OperationResultType.Error);
            if (string.IsNullOrEmpty(PassWord))
            {
                oper.Message = "密码不能为空";
                return oper;
            }
            else
            {
                if (PassWord.Length < 6 || PassWord.Length > 16)
                {
                    oper.Message = "密码长度6~16位";
                    return oper;
                }
            }

            UnitOfWork.TransactionEnabled = true;
            Administrator admin = this.View(AdminId);
            OldPass = OldPass.MD5Hash();
            if (admin.Member.MemberPass == OldPass)
            {
                if (admin == null)
                {
                    oper.Message = "员工不存在";
                }
                else
                {
                    admin.UpdatedTime = DateTime.Now;
                    admin.OperatorId = AdminId;
                    admin.Member.MemberPass = PassWord.MD5Hash();
                    _administratorRepository.Update(admin);
                    int count = UnitOfWork.SaveChanges();
                    if (count > 0)
                    {
                        oper.ResultType = OperationResultType.Success;
                        oper.Message = "修改成功";
                    }
                    else
                    {
                        oper.Message = "修改失败";
                    }
                }
            }
            else
            {
                oper.Message = "原始密码错误";
            }
            return oper;
        }
        #endregion

        #region 获取工作时间
        public OperationResult GetWorkTime(int adminId)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error);
            Administrator admin = this.Administrators.Where(x => x.Id == adminId).FirstOrDefault();
            WorkTime workTime = new WorkTime();
            if (admin == null)
            {
                oper.Message = "员工不存在";
                return oper;
            }
            else
            {
                if (admin.IsPersonalTime)
                {
                    workTime = admin.WorkTime;
                }
                else
                {
                    if (admin.JobPositionId == null)
                    {
                        oper.Message = "请添加职位";
                        return oper;
                    }
                    else
                    {
                        workTime = admin.JobPosition.WorkTime;
                    }
                }
            }
            oper.ResultType = OperationResultType.Success;
            oper.Data = workTime;
            return oper;
        }

        #endregion
        #endregion


        public int GetMemberId(int adminId)
        {
            if (adminId <= 0)
            {
                throw new Exception("参数错误");
            }
            return _administratorRepository.Entities.Where(a => !a.IsDeleted && a.IsEnabled && a.Id == adminId)
                .Select(a => a.MemberId.Value)
                .FirstOrDefault();
        }


        /// <summary>
        /// 获取管理员的会员信息
        /// </summary>
        /// <param name="adminId"></param>
        /// <returns></returns>
        public OperationResult GetMemberProfile(int adminId)
        {
            if (adminId <= 0)
            {
                throw new Exception("参数错误");
            }
            var memberEntity = _administratorRepository.Entities.Where(a => !a.IsDeleted && a.IsEnabled && a.Id == adminId)
                .Select(a => new
                {
                    a.Member.Id,
                    a.Member.MobilePhone,
                    a.Member.UniquelyIdentifies,
                    a.Member.MemberPass,
                   

                })
                .FirstOrDefault();

            var mobileInfoQuery = _mobileInfoRepo.Entities.Where(m => !m.IsDeleted && m.IsEnabled && m.MemberId == memberEntity.Id); 
            if (memberEntity == null)
            {
                throw new Exception("没有数据");

            }
            var strcode = mobileInfoQuery.OrderByDescending(o => o.Id).Select(s => s.DeviceToken).FirstOrDefault();
            string num = $"{memberEntity.MobilePhone}{memberEntity.UniquelyIdentifies}{memberEntity.MemberPass}{strcode}".MD5Hash();

            var res = new
            {
                MemberId = memberEntity.Id,
                U_Num = num
            };
            return new OperationResult(OperationResultType.Success, string.Empty, res);


        }

        public bool IsDesigner(int AdminId)
        {
            return _designerRepository.Entities.Any(a => a.AdminId == AdminId && a.IsEnabled && !a.IsDeleted);
        }

        public Tuple<bool, List<StoreSelectItem>> GetDesignerStoreStorage(int AdminId)
        {
            var isdesigner = IsDesigner(AdminId);
            var list = new List<StoreSelectItem>();
            if (isdesigner)
            {
                list = _designerRepository.Entities.Where(a => a.AdminId == AdminId && a.IsEnabled && !a.IsDeleted).Select(s => new StoreSelectItem()
                {
                    StoreId = s.Factory.StoreId,
                    StoreName = s.Factory.Store.StoreName,
                    StoreType = s.Factory.Store.StoreType.TypeName,
                    Storages = new List<StorageSelectItem> { new StorageSelectItem { StorageId = s.Factory.StorageId, StorageName = s.Factory.Storage.StorageName } },
                }).ToList();
            }
            return new Tuple<bool, List<StoreSelectItem>>(isdesigner, list);
        }

        public Tuple<bool, List<SelectListItem>, List<SelectListItem>> GetDesignerStoreStorageList(int AdminId)
        {
            var isdesigner = IsDesigner(AdminId);
            var liststore = new List<SelectListItem>();
            var liststorage = new List<SelectListItem>();
            if (isdesigner)
            {
                var mod = _designerRepository.Entities.Where(a => a.AdminId == AdminId && a.IsEnabled && !a.IsDeleted).Select(s => s.Factory).FirstOrDefault();
                if (mod.IsNotNull())
                {
                    liststore.Add(new SelectListItem() { Text = mod.Store.StoreName, Value = mod.StoreId + "" });
                    liststorage.Add(new SelectListItem() { Text = mod.Storage.StorageName, Value = mod.StorageId + "" });
                }
            }
            return new Tuple<bool, List<SelectListItem>, List<SelectListItem>>(isdesigner, liststore, liststorage);
        }

        public void SendAdminBirthdayNoti(int AdminId)
        {
            try
            {
                var query = _administratorRepository.Entities.Where(w => w.IsEnabled && !w.IsDeleted);

                var modAdmin = query.Where(w => w.Id == AdminId && w.Member.DateofBirth.HasValue).FirstOrDefault();
                if (modAdmin.IsNotNull())
                {
                    var bir = modAdmin.Member.DateofBirth.Value;
                    var now = DateTime.Now;
                    if (bir.Month == now.Month && bir.Day == now.Day)
                    {
                        var temp = _TemplateContract.GetNotificationTemplate(TemplateNotificationType.AdminBirthday);
                        if (temp.IsNotNull())
                        {
                            var title = temp.TemplateName;
                            var content = temp.TemplateHtml;
                            if (content.IsNullOrEmpty())
                            {
                                return;
                            }

                            var dic = new Dictionary<string, object>();
                            dic.Add("DepartName", modAdmin.Department?.DepartmentName ?? string.Empty);
                            dic.Add("AdminName", modAdmin.Member.RealName ?? modAdmin.Member.MemberName);
                            dic.Add("Birthday", modAdmin.Member.DateofBirth.Value.ToString("MM月dd日"));

                            var entrytime = "";

                            var allmonths = (double)(now.Year - modAdmin.EntryTime.Year) * 12 + now.Month - modAdmin.EntryTime.Month;//入职总月数
                            var year = Math.Floor(allmonths / 12);//入职年数
                            var months = allmonths - year * 12;//入职月数
                            if (year > 0)
                            {
                                entrytime += $"{year}年";
                            }
                            if (months > 0)
                            {
                                entrytime += $"{months}个月";
                            }
                            else if (year == 0 && months == 0)
                            {
                                entrytime = "一个月内";
                            }

                            dic.Add("EntryYear", entrytime);

                            content = NVelocityHelper.Generate(content, dic);

                            if (!temp.EnabledPerNotifi)
                            {
                                var depids = _TemplateContract.GetNotificationDepartIds(temp.Id);
                                query = query.Where(w => depids.Contains(w.DepartmentId.Value));
                            }

                            var listids = query.Select(s => s.Id + "").ToArray();

                            if (listids.IsNotNull() && listids.Count() > 0)
                            {
                                Whiskey.jpush.api.push.mode.Audience audience = Whiskey.jpush.api.push.mode.Audience.s_tag("yuangong");
                                audience.alias(listids);
                                JpushApi.XIAODIE(now.Date.AddHours(10), JpushApiPlatform.All, audience, content, title, content, false);
                            }
                        }
                    }
                }
            }
            catch { }
        }
    }
}
