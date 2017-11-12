using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using Whiskey.Core.Data.Entity.Migrations;
using System.Linq;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models
{
    /// <summary>
    /// 初始化模块
    /// </summary>
    public class ModuleSeedAction : ISeedAction
    {
        /// <summary>
        /// 获取 操作排序，数值越小越先执行
        /// </summary>
        public int Order { get { return 999; } }
        public void Action(DbContext context)
        {
            List<Module> listM = new List<Module>()
            {
                #region 异业联盟模块

                new Module(){Icon="icon-trophy",ModuleName="异业联盟模块",ModuleType=1,IsShow=true,Children= new Module[]{
                    new Module(){ ModuleName="商家管理",ModuleType=1,PageUrl="/Offices/Partner/Index", PageArea="Offices", PageController="Partner", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="供应商等级",ModuleType=1,PageUrl="/Offices/PartnerLevel/Index", PageArea="Offices", PageController="PartnerLevel", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="卡券管理",ModuleType=1,PageUrl="/Coupons/Coupon/Index", PageArea="Coupons", PageController="Coupon", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="加盟商管理",ModuleType=1,PageUrl="/Offices/PartnerManage/Index", PageArea="Offices", PageController="PartnerManage", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="加盟商统计",ModuleType=1,PageUrl="/Offices/PartnerStatistics/Index", PageArea="Offices", PageController="PartnerStatistics", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="临时选货",ModuleType=1,PageUrl="/Offices/TempOnline/Index", PageArea="Offices", PageController="TempOnline", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="临时选货车",ModuleType=1,PageUrl="/Stores/Online/CartIndex", PageArea="Stores", PageController="Online", PageAction="CartIndex",IsShow=true },
                }},

                #endregion

                #region 办公管理模块

                new Module(){Icon="fa-area-chart",ModuleName="办公管理模块",ModuleType=1,IsShow=true,Children= new Module[]{
                    new Module(){ ModuleName="职位管理",ModuleType=1,PageUrl="/Offices/JobPosition/Index", PageArea="Offices", PageController="JobPosition", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="员工管理",ModuleType=1,PageUrl="/Authorities/Administrator/Index", PageArea="Authorities", PageController="Administrator", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="搭配师管理",ModuleType=1,PageUrl="/Subjects/Collocation/Index", PageArea="Subjects", PageController="Collocation", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="工作时间管理",ModuleType=1,PageUrl="/Offices/WorkTime/Index", PageArea="Offices", PageController="WorkTime", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="年假管理",ModuleType=1,PageUrl="/Offices/AnnualLeave/Index", PageArea="Offices", PageController="AnnualLeave", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="员工考勤管理",ModuleType=1,PageUrl="/Offices/StaffAttendance/Index", PageArea="Offices", PageController="StaffAttendance", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="请假审核管理",ModuleType=1,PageUrl="/Offices/LeaveVerify/Index", PageArea="Offices", PageController="LeaveVerify", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="外勤审核管理",ModuleType=1,PageUrl="/Offices/FieldVerify/Index", PageArea="Offices", PageController="FieldVerify", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="加班审核管理",ModuleType=1,PageUrl="/Offices/OvertimeVerify/Index", PageArea="Offices", PageController="OvertimeVerify", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="休假管理",ModuleType=1,PageUrl="/Offices/Rest/Index", PageArea="Offices", PageController="Rest", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="离职管理",ModuleType=1,PageUrl="/Authorities/Resignation/Index", PageArea="Authorities", PageController="Resignation", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="工作日志管理",ModuleType=1,PageUrl="/Offices/WorkLog/Index", PageArea="Offices", PageController="WorkLog", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="项目类型",ModuleType=1,PageUrl="/Offices/WorkLogAttribute/Index", PageArea="Offices", PageController="WorkLogAttribute", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="补卡审核",ModuleType=1,PageUrl="/Offices/AttendanceRepair/Index", PageArea="Offices", PageController="AttendanceRepair", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="部门考勤管理",ModuleType=1,PageUrl="/Offices/DepartAtten/Index", PageArea="Offices", PageController="DepartAtten", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="入职管理",ModuleType=1,PageUrl="/Offices/EntryManagement/Index", PageArea="Offices", PageController="EntryManagement", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="调班审核",ModuleType=1,PageUrl="/Offices/ToExamine/Index", PageArea="Offices", PageController="ToExamine", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="调班申请",ModuleType=1,PageUrl="/Offices/ClassApplication/Index", PageArea="Offices", PageController="ClassApplication", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="员工类型",ModuleType=1,PageUrl="/Authorities/AdministratorType/Index", PageArea="Authorities", PageController="AdministratorType", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="工单类别管理",ModuleType=1,PageUrl="/Offices/WorkOrderCategory/Index", PageArea="Offices", PageController="WorkOrderCategory", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="工单管理",ModuleType=1,PageUrl="/Offices/WorkOrder/Index", PageArea="Offices", PageController="WorkOrder", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="我的工单管理",ModuleType=1,PageUrl="/Offices/MyWorkOrder/Index", PageArea="Offices", PageController="MyWorkOrder", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="试卷管理",ModuleType=1,PageUrl="/Offices/Exam/Index", PageArea="Offices", PageController="Exam", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="题库管理",ModuleType=1,PageUrl="/Offices/ExamQuestion/Index", PageArea="Offices", PageController="ExamQuestion", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="岗位培训管理",ModuleType=1,PageUrl="/Offices/Training/Index", PageArea="Offices", PageController="Training", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="培训内容管理",ModuleType=1,PageUrl="/Offices/TrainingBlog/Index", PageArea="Offices", PageController="TrainingBlog", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="我的培训",ModuleType=1,PageUrl="/Offices/MyTraining/Index", PageArea="Offices", PageController="MyTraining", PageAction="Index",IsShow=true },
                }},

                #endregion

                #region 基础属性模块

                new Module(){Icon="icon-cogs",ModuleName="基础属性模块",ModuleType=1,IsShow=true,Children= new Module[]{
                    new Module(){ ModuleName="品类管理",ModuleType=1,PageUrl="/Properties/Category/Index", PageArea="Properties", PageController="Category", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="色彩管理",ModuleType=1,PageUrl="/Properties/Color/Index", PageArea="Properties", PageController="Color", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="尺码管理",ModuleType=1,PageUrl="/Properties/Size/Index", PageArea="Properties", PageController="Size", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="供应商管理",ModuleType=1,PageUrl="/Properties/Brand/Index", PageArea="Properties", PageController="Brand", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="季节管理",ModuleType=1,PageUrl="/Properties/Season/Index", PageArea="Properties", PageController="Season", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="商品提成管理",ModuleType=1,PageUrl="/Properties/ProductCommission/Index", PageArea="Properties", PageController="ProductCommission", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="商品活动管理",ModuleType=1,PageUrl="/Properties/SalesCampaign/Index", PageArea="Properties", PageController="SalesCampaign", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="积分规则管理",ModuleType=1,PageUrl="/Properties/ScoreRule/Index", PageArea="Properties", PageController="ScoreRule", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="其他属性管理",ModuleType=1,PageUrl="/Properties/Maintain/Index", PageArea="Properties", PageController="Maintain", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="打印机设置",ModuleType=1,PageUrl="/Properties/PrintSet/Index", PageArea="Properties", PageController="PrintSet", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="人群分类管理",ModuleType=1,PageUrl="/Properties/ProductCrowd/Index", PageArea="Properties", PageController="ProductCrowd", PageAction="Index",IsShow=true },
                }},

                #endregion

                #region 卡券活动模块

                new Module(){Icon="fa-train",ModuleName="卡券活动模块",ModuleType=1,IsShow=true,Children= new Module[]{
                    new Module(){ ModuleName="优惠券管理",ModuleType=1,PageUrl="/Coupons/Coupon/Index", PageArea="Coupons", PageController="Coupon", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="优惠券发送管理",ModuleType=1,PageUrl="/Coupons/CouponSend/Index", PageArea="Coupons", PageController="CouponSend", PageAction="Index",IsShow=true },
                }},

                #endregion

                #region 商品档案模块

                new Module(){Icon="fa-th-list",ModuleName="商品档案模块",IsShow=true,Children= new Module[]{
                    new Module(){ ModuleName="商品管理",ModuleType=1,PageUrl="/Products/Product/Index", PageArea="Products", PageController="Product", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="会员搭配管理",ModuleType=1,PageUrl="/Products/MemberCollocation/Index", PageArea="Products", PageController="MemberCollocation", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="搭配属性管理",ModuleType=1,PageUrl="/Products/ProductAttribute/Index", PageArea="Products", PageController="ProductAttribute", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="会员单品管理",ModuleType=1,PageUrl="/Products/MemberSingleProduct/Index", PageArea="Products", PageController="MemberSingleProduct", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="店铺搭配管理",ModuleType=1,PageUrl="/StoreCollocation/Store/Index", PageArea="StoreCollocation", PageController="Store", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="商品追踪",ModuleType=1,PageUrl="/ProductTracks/ProductTrack/Index", PageArea="ProductTracks", PageController="ProductTrack", PageAction="Index",IsShow=true },
                }},

                #endregion

                #region 图库素材模块

                new Module(){Icon="icon-legal",ModuleName="图库素材模块",ModuleType=1,IsShow=true,Children= new Module[]{
                    new Module(){ ModuleName="图片管理",ModuleType=1,PageUrl="/Galleries/Gallery/Index", PageArea="Galleries", PageController="Gallery", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="背景素材管理",ModuleType=1,PageUrl="/Galleries/BackgroundMaterial/Index", PageArea="Galleries", PageController="BackgroundMaterial", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="商品素材管理",ModuleType=1,PageUrl="/Galleries/ProductMaterial/Index", PageArea="Galleries", PageController="ProductMaterial", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="图片属性管理",ModuleType=1,PageUrl="/Galleries/GalleryAttribute/Index", PageArea="Galleries", PageController="GalleryAttribute", PageAction="Index",IsShow=true },
                }},

                #endregion

                #region 专题管理模块

                new Module(){Icon="fa-file-word-o",ModuleName="专题管理模块",IsShow=true,Children= new Module[]{
                    new Module(){ ModuleName="专题管理",ModuleType=1,PageUrl="/Subjects/Subject/Index", PageArea="Subjects", PageController="Subject", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="专题属性管理",ModuleType=1,PageUrl="/Subjects/SubjectAttribute/Index", PageArea="Subjects", PageController="SubjectAttribute", PageAction="Index",IsShow=true },
                }},

                #endregion

                #region 库房管理模块

                new Module(){Icon="fa-legal",ModuleName="库房管理模块",IsShow=true,Children= new Module[]{
                    new Module(){ ModuleName="分仓管理",ModuleType=1,PageUrl="/Warehouses/Storage/Index", PageArea="Warehouses", PageController="Storage", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="入库管理",ModuleType=1,PageUrl="/Warehouses/AddProduct/Index", PageArea="Warehouses", PageController="AddProduct", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="库存管理",ModuleType=1,PageUrl="/Warehouses/Inventory/Index", PageArea="Warehouses", PageController="Inventory", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="盘点商品",ModuleType=1,PageUrl="/Warehouses/Checker/Index", PageArea="Warehouses", PageController="Checker", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="盘点管理",ModuleType=1,PageUrl="/Warehouses/Checked/Index", PageArea="Warehouses", PageController="Checked", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="校对管理",ModuleType=1,PageUrl="/Warehouses/Checkup/Index", PageArea="Warehouses", PageController="Checkup", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="采购管理",ModuleType=1,PageUrl="/Warehouses/Purchase/Index", PageArea="Warehouses", PageController="Purchase", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="配货管理",ModuleType=1,PageUrl="/Warehouses/Orderblank/Index", PageArea="Warehouses", PageController="Orderblank", PageAction="Index",IsShow=false },
                    new Module(){ ModuleName="创建销售单",ModuleType=1,PageUrl="/Warehouses/Sell/Index", PageArea="Warehouses", PageController="Sell", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="销售记录查询",ModuleType=1,PageUrl="/Warehouses/Sale/Index", PageArea="Warehouses", PageController="Sale", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="调拨商品",ModuleType=1,PageUrl="/Warehouses/Transfer/Index", PageArea="Warehouses", PageController="Transfer", PageAction="Index",IsShow=false },
                    new Module(){ ModuleName="调拨记录查询",ModuleType=1,PageUrl="/Warehouses/Transfered/Index", PageArea="Warehouses", PageController="Transfered", PageAction="Index",IsShow=false },
                    new Module(){ ModuleName="入库记录管理",ModuleType=1,PageUrl="/Warehouses/InventoryRecord/Index", PageArea="Warehouses", PageController="InventoryRecord", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="在线选货管理",ModuleType=1,PageUrl="/Warehouses/OnlinePurchaseProduct/Index", PageArea="Warehouses", PageController="OnlinePurchaseProduct", PageAction="Index",IsShow=true },
                }},

                #endregion

                #region 店铺管理模块

                new Module(){Icon="fa-glass",ModuleName="店铺管理模块",IsShow=true,Children= new Module[]{
                    new Module(){ ModuleName="店铺管理",ModuleType=1,PageUrl="/Stores/Store/Index", PageArea="Stores", PageController="Store", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="在线选货",ModuleType=1,PageUrl="/Stores/Online/Index", PageArea="Stores", PageController="Online", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="我的选货车",ModuleType=1,PageUrl="/Stores/Online/OrderCartIndex", PageArea="Stores", PageController="Online", PageAction="OrderCartIndex",IsShow=true },
                    new Module(){ ModuleName="商品零售",ModuleType=1,PageUrl="/Stores/Retail/Index", PageArea="Stores", PageController="Retail", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="零售记录查询",ModuleType=1,PageUrl="/Stores/RetailDetail/Index", PageArea="Stores", PageController="RetailDetail", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="商品退货",ModuleType=1,PageUrl="/Stores/Return/Index", PageArea="Stores", PageController="Return", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="退货记录查询",ModuleType=1,PageUrl="/Stores/Returned/Index", PageArea="Stores", PageController="Returned", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="商品返仓",ModuleType=1,PageUrl="/Stores/Restore/Index", PageArea="Stores", PageController="Restore", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="商品返仓记录查询",ModuleType=1,PageUrl="/Stores/Restored/Index", PageArea="Stores", PageController="Restored", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="打印条码",ModuleType=1,PageUrl="/Stores/Barcode/Index", PageArea="Stores", PageController="Barcode", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="会员信息管理",ModuleType=1,PageUrl="/Stores/MembSearch/index", PageArea="Stores", PageController="MembSearch", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="库存查找",ModuleType=1,PageUrl="/Stores/InventorySearch/index", PageArea="Stores", PageController="InventorySearch", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="店铺活动",ModuleType=1,PageUrl="/Stores/StoreActivity/Index", PageArea="Stores", PageController="StoreActivity", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="排班管理",ModuleType=1,PageUrl="/Stores/WorkforceManagement/Index", PageArea="Stores", PageController="WorkforceManagement", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="店铺类型",ModuleType=1,PageUrl="/Stores/StoreType/Index", PageArea="Stores", PageController="StoreType", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="店铺等级",ModuleType=1,PageUrl="/Stores/StoreLevel/Index", PageArea="Stores", PageController="StoreLevel", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="店铺付款记录",ModuleType=1,PageUrl="/Stores/StoreDeposit/Index", PageArea="Stores", PageController="StoreDeposit", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="考察项目管理",ModuleType=1,PageUrl="/Stores/StoreCheck/Index", PageArea="Stores", PageController="StoreCheck", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="店铺考察管理",ModuleType=1,PageUrl="/Stores/StoreCheckRecord/Index", PageArea="Stores", PageController="StoreCheckRecord", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="预约管理",ModuleType=1,PageUrl="/Stores/Appointment/Index", PageArea="Stores", PageController="Appointment", PageAction="Index",IsShow=true },
                }},

                #endregion

                #region 会员管理模块

                new Module(){Icon="fa-user",ModuleName="会员管理模块",IsShow=true,Children= new Module[]{
                    new Module(){ ModuleName="会员档案管理",ModuleType=1,PageUrl="/Members/Member/Index", PageArea="Members", PageController="Member", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="消费记录管理",ModuleType=1,PageUrl="/Members/MemberConsume/Index", PageArea="Members", PageController="MemberConsume", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="充值记录管理",ModuleType=1,PageUrl="/Members/MemberDeposit/Index", PageArea="Members", PageController="MemberDeposit", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="礼品兑换记录",ModuleType=1,PageUrl="/Members/MemberGift/Index", PageArea="Members", PageController="MemberGift", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="会员等级管理",ModuleType=1,PageUrl="/Members/MemberLevel/Index", PageArea="Members", PageController="MemberLevel", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="储值卡管理",ModuleType=1,PageUrl="/Members/MemberCard/Index", PageArea="Members", PageController="MemberCard", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="会员活动",ModuleType=1,PageUrl="/Members/MemberActivity/Index", PageArea="Members", PageController="MemberActivity", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="会员类型",ModuleType=1,PageUrl="/Members/MemberType/Index", PageArea="Members", PageController="MemberType", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="奖励物品",ModuleType=1,PageUrl="/Members/Prize/Index", PageArea="Members", PageController="Prize", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="签到规则",ModuleType=1,PageUrl="/Members/SignRule/Index", PageArea="Members", PageController="SignRule", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="储值积分维护",ModuleType=1,PageUrl="/Members/AdjustDeposit/Index", PageArea="Members", PageController="AdjustDeposit", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="会员热度",ModuleType=1,PageUrl="/Members/MemberHeat/Index", PageArea="Members", PageController="MemberHeat", PageAction="Index",IsShow=true },
                }},

                #endregion

                #region 信息管理模块

                new Module(){Icon="fa-inbox",ModuleName="信息管理模块",IsShow=true,Children= new Module[]{
                    new Module(){ ModuleName="考勤统计",ModuleType=1,PageUrl="/Offices/Attendance/Index", PageArea="Offices", PageController="Attendance", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="消息管理",ModuleType=1,PageUrl="/Notices/Messager/Index", PageArea="Notices", PageController="Messager", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="通知管理",ModuleType=1,PageUrl="/Notices/Notification/Index", PageArea="Notices", PageController="Notification", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="店铺统计",ModuleType=1,PageUrl="/Notices/StoreStatistics/Index", PageArea="Notices", PageController="StoreStatistics", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="运营统计",ModuleType=1,PageUrl="/Notices/StoreSpendStatistics/index", PageArea="Notices", PageController="StoreSpendStatistics", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="通知查看",ModuleType=1,PageUrl="/Notices/NotificationView/Index", PageArea="Notices", PageController="NotificationView", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="库存统计",ModuleType=1,PageUrl="/Notices/StoreStatistics/InventoryStat", PageArea="Notices", PageController="StoreStatistics", PageAction="Index",IsShow=true },
                }},

                #endregion

                #region 权限管理模块

                new Module(){Icon="fa-sitemap",ModuleName="权限管理模块",ModuleType=1,IsShow=true,Children= new Module[]{
                    new Module(){ ModuleName="部门管理",ModuleType=1,PageUrl="/Authorities/Department/Index", PageArea="Authorities", PageController="Department", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="角色管理",ModuleType=1,PageUrl="/Authorities/Role/Index", PageArea="Authorities", PageController="Role", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="模块管理",ModuleType=1,PageUrl="/Authorities/Module/Index", PageArea="Authorities", PageController="Module", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="许可管理",ModuleType=1,PageUrl="/Authorities/Permission/Index", PageArea="Authorities", PageController="Permission", PageAction="Index",IsShow=true },
                }},

                #endregion

                #region 公共管理模块

                new Module(){Icon="fa-cog",ModuleName="公共管理模块",IsShow=true,Children= new Module[]{
                    new Module(){ ModuleName="基础设置",ModuleType=1,PageUrl="/Commons/Setting/Index", PageArea="Commons", PageController="Setting", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="折扣管理",ModuleType=1,PageUrl="/Commons/Discount/Index", PageArea="Commons", PageController="Discount", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="快递管理",ModuleType=1,PageUrl="/Commons/Express/Index", PageArea="Commons", PageController="Express", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="支付管理",ModuleType=1,PageUrl="/Commons/Payment/Index", PageArea="Commons", PageController="Payment", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="省份管理",ModuleType=1,PageUrl="/Commons/Province/Index", PageArea="Commons", PageController="Province", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="城市管理",ModuleType=1,PageUrl="/Commons/City/Index", PageArea="Commons", PageController="City", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="操作日志管理",ModuleType=1,PageUrl="/Commons/Log/Index", PageArea="Commons", PageController="Log", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="圈子管理",ModuleType=1,PageUrl="/Commons/Circle/Index", PageArea="Commons", PageController="Circle", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="超时管理",ModuleType=1,PageUrl="/Commons/TimeoutSetting/Index", PageArea="Commons", PageController="TimeoutSetting", PageAction="Index",IsShow=true },
                }},

                #endregion

                #region 文章管理模块

                new Module(){Icon="fa-angellist",ModuleName="文章管理模块",IsShow=true,Children= new Module[]{
                    new Module(){ ModuleName="文章管理",ModuleType=1,PageUrl="/Articles/Article/Index", PageArea="Articles", PageController="Article", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="栏目管理",ModuleType=1,PageUrl="/Articles/ArticleItem/Index", PageArea="Articles", PageController="ArticleItem", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="文章属性管理",ModuleType=1,PageUrl="/Articles/ArticleAttribute/Index", PageArea="Articles", PageController="ArticleAttribute", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="App文章",ModuleType=1,PageUrl="/Articles/AppArticle/Index", PageArea="Articles", PageController="AppArticle", PageAction="Index",IsShow=true },
                }},

                #endregion

                #region 模版管理模块

                new Module(){Icon="fa-arrow-circle-o-right",ModuleName="模版管理模块",IsShow=true,Children= new Module[]{
                    new Module(){ ModuleName="商品模板管理",ModuleType=1,PageUrl="/Templates/ProductTemplate/Index", PageArea="Templates", PageController="ProductTemplate", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="文章模板管理",ModuleType=1,PageUrl="/Templates/Template/Index", PageArea="Templates", PageController="Template", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="首页模板管理",ModuleType=1,PageUrl="/Templates/WebsiteTemplate/Index", PageArea="Templates", PageController="WebsiteTemplate", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="栏目模板管理",ModuleType=1,PageUrl="/Templates/SectionTemplate/Index", PageArea="Templates", PageController="SectionTemplate", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="栏目列表模板管理",ModuleType=1,PageUrl="/Templates/SectionListTemplate/Index", PageArea="Templates", PageController="SectionListTemplate", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="模版JS管理",ModuleType=1,PageUrl="/Templates/JS/Index", PageArea="Templates", PageController="JS", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="模版CSS管理",ModuleType=1,PageUrl="/Templates/CSS/Index", PageArea="Templates", PageController="CSS", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="模版图片管理",ModuleType=1,PageUrl="/Templates/Image/Index", PageArea="Templates", PageController="Image", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="商品列表",ModuleType=1,PageUrl="/Templates/ProductListTemplate/Index", PageArea="Templates", PageController="ProductListTemplate", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="通知模块管理",ModuleType=1,PageUrl="/Templates/NotificationTemplate/Index", PageArea="Templates", PageController="NotificationTemplate", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="主题管理",ModuleType=1,PageUrl="/Templates/TemplateTheme/Index", PageArea="Templates", PageController="TemplateTheme", PageAction="Index",IsShow=true },
                }},

                #endregion

                #region 财务管理模块

                new Module(){Icon="fa-credit-card",ModuleName="财务管理模块",ModuleType=1,IsShow=true,PageUrl="/Finance/StoredValue/Index",PageArea="Finance",PageController="StoredValue",PageAction="Index" ,Children= new Module[]{
                    new Module(){ ModuleName="充值规则管理",ModuleType=1,PageUrl="/Finance/StoredValue/Index", PageArea="Finance", PageController="StoredValue", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="违规记录管理",ModuleType=1,PageUrl="/Finance/PunishScoreRecord/Index", PageArea="Finance", PageController="PunishScoreRecord", PageAction="Index",IsShow=true },
                }},

                #endregion

                #region 视频监控管理

                new Module(){Icon="fa-video",ModuleName="视频监控模块",ModuleType=1,IsShow=true,Children= new Module[]{
                    new Module(){ ModuleName="监控设备管理",ModuleType=1,PageUrl="/Video/VideoEquipment/Index", PageArea="Video", PageController="VideoEquipment", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="查看监控",ModuleType=1,PageUrl="/Video/MonitorPlayback/Index", PageArea="Video", PageController="MonitorPlayback", PageAction="Index",IsShow=true },
                }},

                #endregion

                #region 商城管理模块

                new Module(){Icon="fa-image",ModuleName="商城管理模块",IsShow=true,Children= new Module[]{
                    new Module(){ ModuleName="商城款号列表",ModuleType=1,PageUrl="/Stores/StoreRecommend/Index", PageArea="Stores", PageController="StoreRecommend", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="商城款号管理",ModuleType=1,PageUrl="/Stores/StoreRecommend/Create", PageArea="Stores", PageController="StoreRecommend", PageAction="Create",IsShow=true },
                }},

                #endregion

                #region 工厂管理模块

                new Module(){Icon="icon-briefcase",ModuleName="工厂管理模块",ModuleType=1,IsShow=true,Children= new Module[]{
                    new Module(){ ModuleName="设计师管理",ModuleType=1,PageUrl="/Factory/Designer/Index", PageArea="Factory", PageController="Designer", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="样品管理",ModuleType=1,PageUrl="/Factory/Sample/Index", PageArea="Factory", PageController="Sample", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="工厂管理",ModuleType=1,PageUrl="/Factory/Factorys/Index", PageArea="Factory", PageController="Factorys", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="样品发布管理",ModuleType=1,PageUrl="/Factory/Release/Index", PageArea="Factory", PageController="Release", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="供货管理",ModuleType=1,PageUrl="/Factory/Supply/Index", PageArea="Factory", PageController="Supply", PageAction="Index",IsShow=true },
                }},

                #endregion

                #region 数据分析模块

                new Module(){Icon="icon-pencil",ModuleName="数据分析模块",ModuleType=1,IsShow=true,Children= new Module[]{
                    new Module(){ ModuleName="销售分析",ModuleType=1,PageUrl="/DataStat/Sale/Index", PageArea="DataStat", PageController="Sale", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="品牌分析",ModuleType=1,PageUrl="/DataStat/Brand/Index", PageArea="DataStat", PageController="Brand", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="品类分析",ModuleType=1,PageUrl="/DataStat/Category/Index", PageArea="DataStat", PageController="Category", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="款式分析",ModuleType=1,PageUrl="/DataStat/BigProductNum/Index", PageArea="DataStat", PageController="BigProductNum", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="会员分析",ModuleType=1,PageUrl="/DataStat/Member/Index", PageArea="DataStat", PageController="Member", PageAction="Index",IsShow=true },
                    new Module(){ ModuleName="加盟商分析",ModuleType=1,PageUrl="/DataStat/Partner/Index", PageArea="DataStat", PageController="Partner", PageAction="Index",IsShow=true },
                }},

                #endregion

                #region 游戏管理模块

                new Module(){Icon="fa-gamepad",ModuleName="游戏管理模块",ModuleType=1,IsShow=true,Children= new Module[]{
                    new Module(){ ModuleName="游戏管理",ModuleType=1,PageUrl="/Games/Game/Index", PageArea="Games", PageController="Game", PageAction="Index",IsShow=true },
                }},

	            #endregion

            };

            context.Set<Module>().AddOrUpdate(listM.ToArray());
            context.SaveChanges();

            List<Permission> listPer = new List<Permission>();

            foreach (var item in listM.SelectMany(s => s.Children))
            {
                var listP = new List<Permission>()
                {
                    new Permission() {ModuleId=item.Id, PermissionName="加载页面",Identifier="Index",Icon="fa-search",Style="btn btn-primary btn-padding-right",ActionName="Index",AreaName=item.PageArea,ControllName=item.PageController,Gtype= PermissionGroupType.查看 },
                    new Permission() {ModuleId=item.Id, PermissionName="加载数据",Identifier="List",Icon="fa-search",Style="btn btn-primary btn-padding-right",ActionName="List",AreaName=item.PageArea,ControllName=item.PageController,Gtype= PermissionGroupType.查看 },
                    new Permission() {ModuleId=item.Id, PermissionName="新增",Identifier="Create",Icon="fa-search",Style="btn btn-primary btn-padding-right",ActionName="Create",OnlyFlag="#Create",AreaName=item.PageArea,ControllName=item.PageController },
                    new Permission() {ModuleId=item.Id, PermissionName="修改",Identifier="Update",Icon="fa-pencil",Style="btn btn-primary btn-padding-right",ActionName="Update",OnlyFlag="#Update",AreaName=item.PageArea,ControllName=item.PageController },
                    new Permission() {ModuleId=item.Id, PermissionName="查看数据详情",Identifier="View",Icon="fa-pencil",Style="btn btn-primary btn-padding-right",ActionName="View",OnlyFlag="#View",AreaName=item.PageArea,ControllName=item.PageController,Gtype= PermissionGroupType.查看 },
                    new Permission() {ModuleId=item.Id, PermissionName="禁用数据",Identifier="Disable",Icon="fa-pencil",Style="btn btn-primary btn-padding-right",ActionName="Disable",OnlyFlag="#Disable",AreaName=item.PageArea,ControllName=item.PageController,Gtype= PermissionGroupType.禁用 },
                    new Permission() {ModuleId=item.Id, PermissionName="移除数据",Identifier="Remove",Icon="fa-pencil",Style="btn btn-primary btn-padding-right",ActionName="Remove",OnlyFlag="#Remove",AreaName=item.PageArea,ControllName=item.PageController,Gtype= PermissionGroupType.删除 },
                    new Permission() {ModuleId=item.Id, PermissionName="打印预览数据",Identifier="Print",Icon="fa-pencil",Style="btn btn-primary btn-padding-right",ActionName="Print",OnlyFlag="#Print",AreaName=item.PageArea,ControllName=item.PageController,Gtype= PermissionGroupType.查看 },
                    new Permission() {ModuleId=item.Id, PermissionName="导出数据",Identifier="Export",Icon="fa-pencil",Style="btn btn-primary btn-padding-right",ActionName="Export",OnlyFlag="#Export",AreaName=item.PageArea,ControllName=item.PageController,Gtype= PermissionGroupType.查看 },
                    new Permission() {ModuleId=item.Id, PermissionName="批量删除",Identifier="RemoveAll",Icon="fa-pencil",Style="btn btn-primary btn-padding-right",ActionName="RemoveAll",OnlyFlag="#RemoveAll",AreaName=item.PageArea,ControllName=item.PageController,Gtype= PermissionGroupType.删除 },
                    new Permission() {ModuleId=item.Id, PermissionName="恢复",Identifier="Recovery",Icon="fa-reply",Style="btn btn-primary btn-padding-right",ActionName="Recovery",OnlyFlag="#Recovery",AreaName=item.PageArea,ControllName=item.PageController,Gtype= PermissionGroupType.删除 },
                    new Permission() {ModuleId=item.Id, PermissionName="启用",Identifier="Enable",Icon="fa-check",Style="btn btn-primary btn-padding-right",ActionName="Enable",OnlyFlag="#Enable",AreaName=item.PageArea,ControllName=item.PageController,Gtype= PermissionGroupType.禁用 },
                };

                foreach (var item2 in listP)
                {
                    var arp = new ARolePermissionRelation() { RoleId = 1, IsShow = true, Permission = item2 };
                    item2.ARolePermissionRelations = new List<ARolePermissionRelation>() { arp };
                }

                listPer.AddRange(listP);
            }

            context.Set<Permission>().AddOrUpdate(listPer.ToArray());
            context.SaveChanges();

            #region 后台首页单独添加

            var modM = new Module()
            {
                ModuleName = "后台首页",
                IsShow = false,
                Children = new Module[]{
                    new Module(){ ModuleName="登录首页",ModuleType=1,PageUrl="/Authorities/Login/index", PageArea="Authorities", PageController="Login", PageAction="Index",IsShow=false,OverrideUrl="admin_login" },
                }
            };
            context.Set<Module>().AddOrUpdate(modM);
            context.SaveChanges();

            #endregion
        }
    }
}
