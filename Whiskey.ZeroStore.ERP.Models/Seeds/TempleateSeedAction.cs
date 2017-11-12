using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using Whiskey.Core.Data.Entity.Migrations;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class TempleateSeedAction : ISeedAction
    {
        public int Order
        {
            get
            {
                return 20;
            }
        }

        public void Action(DbContext context)
        {
            #region 主题模板

            List<TemplateTheme> listTT = new List<TemplateTheme>()
            {
                new TemplateTheme() { Name="原始主题" },
                new TemplateTheme() { Name="默认主题",Path="/Theme/default/",IsDefault=true, },
            };
            context.Set<TemplateTheme>().AddOrUpdate(listTT.ToArray());
            context.SaveChanges();

            #endregion

            #region 通知模块模板

            var listT = new Template[] {
                new Template() {TemplateName="配货通知",TemplateType=6,TemplateHtml="$sendName 配到 $receiveName 的配货单，需要处理",IsDefault=true,templateNotification=new TemplateNotification() {Name="配货通知",NotifciationType= TemplateNotificationType.Picking,Notes="1、$sendName：发货店铺<br/>2、$receiveName：收货店铺<br/>3、$sendId：发货店铺Id<br/>4、$receiveId：收货店铺Id<br/>5、$sendAddress：发货店铺地址<br/>6、$receiveAddress：收货店铺地址<br/>7、$sendPhone：发货店铺电话（有电话取电话,否则取手机号）<br/>8、$receivePhone：收货店铺电话（有电话取电话,否则取手机号）<br/>9、$sendTime：发货时间<br/>" } },
                new Template() {TemplateName="储值积分调整通知",TemplateType=6,TemplateHtml="会员：$memberName 手机号：$memberPhone 有储值调整：$balance 有积分调整：$score",IsDefault=true,templateNotification=new TemplateNotification() {Name="储值积分调整通知",NotifciationType= TemplateNotificationType.ChangeRecharge,Notes="1、$storeId：所属店铺Id<br/>2、$storeName：会员所属店铺<br/>3、$storePhone：所属店铺电话（有电话取电话,否则取手机号）<br/>4、$storeAddress：所属店铺地址<br/>5、$memberId：会员Id<br/>6、$memberName：会员昵称<br/>7、$memberPhone：会员电话<br/>8、$balance：储值金额<br/>9、$score：储值积分<br/>10、$sendTime：申请时间<br/>" } },
                new Template() {TemplateName="入职审核通知",TemplateType=6,TemplateHtml="您有一个入职申请需要处理！",IsDefault=true,templateNotification=new TemplateNotification() {Name="入职通知",NotifciationType= TemplateNotificationType.Entry,Notes="1、$entryName：入职人<br/>2、$entryDep：入职人部门<br/>3、$ToExamine：审核人<br/>4、$Sex：性别<br/>5、$InterviewEvaluation：面试评价" }, },
                new Template() {TemplateName="入职通知",TemplateType=6,TemplateHtml="热烈欢迎$entryDep的$entryName加入我们零时尚大家庭！！！$entryName是一位优秀的$Sex生，人物小传：$InterviewEvaluation",IsDefault=true,templateNotification=new TemplateNotification() {Name="入职通知",NotifciationType= TemplateNotificationType.Entry,Notes="1、$entryName：入职人<br/>2、$entryDep：入职人部门<br/>3、$ToExamine：审核人<br/>4、$Sex：性别<br/>5、$InterviewEvaluation：面试评价" }, },
                new Template() {TemplateName="入职审核未通过",TemplateType=6,TemplateHtml="您有一个入职申请审核未通过！",IsDefault=true,templateNotification=new TemplateNotification() {Name="入职审核未通过",NotifciationType= TemplateNotificationType.EntryRefuse,Notes="1、$entryName：入职人<br/>2、$entryDep：入职人部门<br/>3、$ToExamine：审核人<br/>4、$Sex：性别<br/>5、$InterviewEvaluation：面试评价" }, },
                new Template() {TemplateName="离职审核通知",TemplateType=6,TemplateHtml="您有一个离职审核需要处理！",IsDefault=true,templateNotification=new TemplateNotification() {Name="离职通知",NotifciationType= TemplateNotificationType.Resignation,Notes="1、$entryName：离职人<br/>2、$entryDep：离职人部门<br/>3、$ToExamine：审核人<br/>" }, },
                new Template() {TemplateName="离职通知",TemplateType=6,TemplateHtml="$entryDep的$entryName于$time已离职.",IsDefault=true,templateNotification=new TemplateNotification() {Name="离职通知",NotifciationType= TemplateNotificationType.ResignationFinanice,Notes="1、$entryName：离职人<br/>2、$entryDep：离职人部门<br/>3、$ToExamine：审核人<br/>4、$time：离职日期<br/>" }, },
                new Template() {TemplateName="离职审核未通过通知",TemplateType=6,TemplateHtml="您有一个离职申请审核未通过！",IsDefault=true,templateNotification=new TemplateNotification() {Name="离职审核未通过",NotifciationType= TemplateNotificationType.ResignationRefuse,Notes="1、$entryName：离职人<br/>2、$entryDep：离职人部门<br/>3、$ToExamine：审核人<br/>" }, },
                new Template() {TemplateName="储值积分调整审核通知",TemplateType=6,TemplateHtml="会员：$memberName 手机号：$memberPhone 储值调整：$balance 积分调整：$score 审核状态：$VerifyType",IsDefault=true,templateNotification=new TemplateNotification() {Name="储值积分调整审核通知",NotifciationType= TemplateNotificationType.Changecheck,Notes="1、$storeId：所属店铺Id<br/>2、$storeName：会员所属店铺<br/>3、$storePhone：所属店铺电话（有电话取电话,否则取手机号）<br/>4、$storeAddress：所属店铺地址<br/>5、$memberId：会员Id<br/>6、$memberName：会员昵称<br/>7、$memberPhone：会员电话<br/>8、$balance：储值金额<br/>9、$score：储值积分<br/>10、$sendTime：申请时间<br/>11、$VerifyType：审核状态<br/>12、$checktime：审核时间<br/>" }, },
            };

            context.Set<Template>().AddOrUpdate(listT);
            context.SaveChanges();

            #endregion
        }
    }
}
