using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using Whiskey.Core.Data.Entity.Migrations;
using Whiskey.ZeroStore.ERP.Models.Entities.Products;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class AuthoritieseedAction : ISeedAction
    {
        /// <summary>
        /// 获取 操作排序，数值越小越先执行
        /// </summary>
        public int Order { get { return 0; } }

        /// <summary>
        /// 定义种子数据初始化过程
        /// </summary>
        /// <param name="context">数据上下文</param>
        public void Action(DbContext context)
        {
            #region 会员类型 MemberType

            MemberType modMT = new MemberType() { MemberTypeName = "普通会员", MemberTypeDiscount = 1m };

            #endregion

            #region 会员 Member

            Member modM = new Member()
            {
                UniquelyIdentifies = "000001",
                CardNumber = "xxxxxx",
                IDCard = "xxxxxxxxxxxxxxxxxx",
                DateofBirth = DateTime.Now,
                MemberName = "零时尚",
                MemberPass = "b13955235245b249",
                RealName = "零时尚",
                UserPhoto = "/Content/Images/logo-_03.png",
                MemberType = modMT,
            };
            context.Set<Member>().AddOrUpdate(modM);
            context.SaveChanges();

            #endregion

            #region 店铺类型 StoreType

            StoreType modST = new StoreType() { TypeName = "直营店", IsReseller = true };

            #endregion

            #region 店铺 Store

            Store modS = new Store() { StoreType = modST, StoreName = "零时尚总店", Description = "零时尚总部仓库资源整合部", Telephone = "18500020068" };
            context.Set<Store>().AddOrUpdate(modS);
            context.SaveChanges();

            #endregion

            #region 部门 Department

            Department modD = new Department() { DepartmentName = "网络部", Description = "网络部", DepartmentType = DepartmentTypeFlag.公司 };
            context.Set<Department>().AddOrUpdate(modD);
            context.SaveChanges();
            #endregion

            #region 工作时间 WorkTime

            List<WorkTime> listWorkTime = new List<WorkTime>() {
              new WorkTime(){ WorkTimeName="普通工作时间",IsFlexibleWork=false, AmStartTime="9:00:00",AmEndTime="12:00:00",PmStartTime="13:00:00",PmEndTime="18:00:00",Sequence=0,WorkHour=8,WorkWeek="1,2,3,4,5"},
              new WorkTime(){ WorkTimeName="弹性工作时间", AmStartTime="00:00:00",AmEndTime="00:00:00",PmStartTime="00:00:00",PmEndTime="00:00:00",IsFlexibleWork=true,Sequence=0,WorkHour=0,WorkWeek="1,2,3,4,5"},
            };
            context.Set<WorkTime>().AddOrUpdate(listWorkTime.ToArray());
            context.SaveChanges();

            #endregion

            #region 年假 AnnualLeave

            List<AnnualLeave> listAnnualLeave = new List<AnnualLeave>() {
               new AnnualLeave(){AnnualLeaveName="普通年假",ParentId=null,Sequence=0 , StartYear=0,EndYear=0,Days=0,Children=new List<AnnualLeave>(){
                 new AnnualLeave(){AnnualLeaveName="0-1年普通年假",Sequence=0 , StartYear=0,EndYear=1,Days=5},
                 new AnnualLeave(){AnnualLeaveName="1-3年普通年假",Sequence=0, StartYear=1,EndYear=3,Days=10},
               }}
            };
            context.Set<AnnualLeave>().AddOrUpdate(listAnnualLeave.ToArray());
            context.SaveChanges();

            #endregion

            #region 初始化职位

            JobPosition modJ = new JobPosition() { AnnualLeaveId = 1, WorkTimeId = 1, DepartmentId = 1, JobPositionName = "系统管理员", CheckLogin = false, CheckMac = false, IsLeader = true, AllowPwd = true };
            context.Set<JobPosition>().AddOrUpdate(modJ);
            context.SaveChanges();

            #endregion

            #region 买手说标签 BuysaidAttribute

            List<BuysaidAttribute> buysaidAttributes=new List<BuysaidAttribute>()
            {
                new BuysaidAttribute(){AttriName = "单品来历",Descri = "表达单品象征意味或含义，单品故事描述或点开链接相关专题"},
                new BuysaidAttribute(){AttriName = "色彩分析",Descri = "色彩所传达的气质"},
                new BuysaidAttribute(){AttriName = "面料分析",Descri = "面料的质感、考究、贵重、舒适"},
                new BuysaidAttribute(){AttriName = "款式分析",Descri = "设计细节分析"},
                new BuysaidAttribute(){AttriName = "做工",Descri = "工艺的复杂性、精妙"},
                new BuysaidAttribute(){AttriName = "试穿体验",Descri = "版型、舒适度"},
                new BuysaidAttribute(){AttriName = "扬长避短",Descri = "塑造体型、掩饰特型、凸显优势"},
                new BuysaidAttribute(){AttriName = "实用性",Descri = "方便实用、易搭配、造型丰富、不挑体型"},
          
            };
            context.Set<BuysaidAttribute>().AddOrUpdate(buysaidAttributes.ToArray());
            context.SaveChanges();

            #endregion

            #region 角色 Role

            Role modR = new Role() { RoleName = "超级管理员", Description = "对整个系统进行管理及操控。", Weight = 150 };
            context.Set<Role>().AddOrUpdate(modR);

            #endregion

            //#region 员工类型 AdministratorType

            //AdministratorType modAT = new AdministratorType() { TypeName = "公司", UnChangeable = true };
            //context.Set<AdministratorType>().AddOrUpdate(modAT);
            //AdministratorType[] listAT = new AdministratorType[] { new AdministratorType { TypeName = "加盟商", UnChangeable = true }, new AdministratorType { TypeName = "设计师", UnChangeable = true } };
            //context.Set<AdministratorType>().AddOrUpdate(listAT);

            //#endregion

            #region 员工 Administrator

            Administrator modA = new Administrator() { EntryTime = DateTime.Now, Department = modD, JobPosition = modJ, Member = modM, Roles = new List<Role>() { modR }, Stores = new List<Store>() { modS } };
            context.Set<Administrator>().AddOrUpdate(modA);
            context.SaveChanges();

            #endregion

        }
    }
}