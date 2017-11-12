using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Globalization;
using AutoMapper;
using Antlr3;
using Antlr3.ST;
using Antlr3.ST.Language;
using Antlr3.ST.Extensions;
using Newtonsoft.Json;
using Whiskey.Utility.Class;
using Whiskey.Utility.Data;
using Whiskey.Utility.Filter;
using Whiskey.Utility.Logging;
using Whiskey.Utility.Extensions;
using Whiskey.Web.Helper;
using Whiskey.Web.Mvc.Binders;
using Whiskey.Core.Data;
using Whiskey.Core.Data.Extensions;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Notices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Notices.Controllers
{

    [License(CheckMode.Verify)]
    public class MessagerController : BaseController
    {

        #region 初始化操作对象

        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(MessagerController));

        protected readonly IMessagerContract _messagerContract;

        protected readonly IAdministratorContract _adminContract;

        public MessagerController(IMessagerContract messagerContract,
            IAdministratorContract adminContract)
        {
            _messagerContract = messagerContract;
            _adminContract = adminContract;
        }
        #endregion

        #region 初始化界面

        /// <summary>
        /// 视图数据
        /// </summary>
        /// <returns></returns>
        [Layout]
        public ActionResult Index(int ActiveType = 0)
        {
            int adminId = AuthorityHelper.OperatorId ?? 0;
            ViewBag.AdminId = adminId;
            ViewBag.ActiveType = ActiveType;
            return View();
        }
        #endregion

        #region 添加数据

        /// <summary>
        /// 载入创建数据
        /// </summary>
        /// <returns></returns>
        public ActionResult Create()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            ViewBag.ReceiverIds = list;
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
        public ActionResult Create(MessagerDto dto)
        {
            dto.Status = (int)MessagerStatusFlag.UnRead;
            dto.SenderId = AuthorityHelper.OperatorId ?? 0;
            List<MessagerDto> listDto = new List<MessagerDto>();
            foreach (int id in dto.ReceiverIds)
            {
                MessagerDto entity = DeepClone(dto);
                entity.ReceiverId = id;
                listDto.Add(entity);
            }
            var result = _messagerContract.Insert(true, sendMessageAction, listDto.ToArray());
            if (result.ResultType == OperationResultType.Success)
            {
                SetCache(dto.ReceiverIds);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 修改数据

        /// <summary>
        /// 提交数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Update(MessagerDto dto)
        {
            var result = _messagerContract.Update(sendMessageAllAction, dto);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 载入修改数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult Update(int Id)
        {
            var result = _messagerContract.Edit(Id);
            return PartialView(result);
        }

        public ActionResult DeleteMyMsg(params int[] Id)
        {
            OperationResult oResult = new OperationResult(OperationResultType.Error);
            var listmessager = _messagerContract.Messagers.Where(w => Id.Contains(w.Id));
            if (listmessager.IsNotNullOrEmptyThis())
            {
                List<MessagerDto> listdto = new List<MessagerDto>();
                foreach (var entity in listmessager)
                {
                    entity.Status = (int)MessagerStatusFlag.Read;
                    var dto = Mapper.Map<Messager, MessagerDto>(entity);
                    listdto.Add(dto);
                }
                oResult = _messagerContract.Update(null, listdto.ToArray());
                if (oResult.ResultType == OperationResultType.Success)
                {
                    SetCache(listmessager.Select(s => s.ReceiverId.Value).ToList(), -1);
                }
            }
            return Json(oResult, JsonRequestBehavior.DenyGet);
        }

        #endregion

        #region 查看数据

        /// <summary>
        /// 查看数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        public ActionResult View(int Id, bool IsRead)
        {
            var result = _messagerContract.View(Id);
            if (!IsRead)
            {
                DeleteMyMsg(Id);
            }
            return PartialView(result);
        }
        #endregion

        #region 获取数据列表

        /// <summary>
        /// 获取数据列表
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> List()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<Messager, bool>> predicate = FilterHelper.GetExpression<Messager>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                var count = 0;
                var list = _messagerContract.Messagers.Where<Messager, int>(predicate, request.PageCondition, out count).Select(m => new
                {
                    SenderName = m.Sender.Member.RealName,
                    //m.MessageType,
                    //m.Description,
                    ReceiverName = m.Receiver.Member.RealName,
                    m.Status,
                    m.MessageTitle,
                    m.Id,
                    m.IsDeleted,
                    m.IsEnabled,
                    m.Sequence,
                    m.UpdatedTime,
                    m.CreatedTime,
                    m.Operator.Member.MemberName,
                }).ToList();
                return new GridData<object>(list, count, request.RequestInfo);

            });
            return Json(data, JsonRequestBehavior.AllowGet);
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
            var result = _messagerContract.Remove(null, Id);
            if (result.ResultType == OperationResultType.Success)
            {
                SetCache(_messagerContract.Messagers.Where(w => Id.Contains(w.Id)).Select(s => s.ReceiverId.Value).ToList(), -1);
            }
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
            var result = _messagerContract.Delete(null, Id);
            if (result.ResultType == OperationResultType.Success)
            {
                SetCache(_messagerContract.Messagers.Where(w => Id.Contains(w.Id)).Select(s => s.ReceiverId.Value).ToList(), -1);
            }
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
            var result = _messagerContract.Recovery(null, Id);
            if (result.ResultType == OperationResultType.Success)
            {
                SetCache(_messagerContract.Messagers.Where(w => Id.Contains(w.Id)).Select(s => s.ReceiverId.Value).ToList());
            }
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
            var result = _messagerContract.Enable(null, Id);
            if (result.ResultType == OperationResultType.Success)
            {
                SetCache(_messagerContract.Messagers.Where(w => Id.Contains(w.Id)).Select(s => s.ReceiverId.Value).ToList());
            }
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
            var result = _messagerContract.Disable(null, Id);
            if (result.ResultType == OperationResultType.Success)
            {
                SetCache(_messagerContract.Messagers.Where(w => Id.Contains(w.Id)).Select(s => s.ReceiverId.Value).ToList(), -1);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 打印数据

        /// <summary>
        /// 打印数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        public ActionResult Print(int[] Id)
        {
            var path = Path.Combine(HttpRuntime.AppDomainAppPath, EnvironmentHelper.TemplatePath(this.RouteData));
            var list = _messagerContract.Messagers.Where(m => Id.Contains(m.Id)).OrderByDescending(m => m.Id).ToList();
            var group = new StringTemplateGroup("all", path, typeof(TemplateLexer));
            var st = group.GetInstanceOf("Printer");
            st.SetAttribute("list", list);
            return Json(new { html = st.ToString() }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 导出数据

        /// <summary>
        /// 导出数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        public ActionResult Export(int[] Id)
        {
            var path = Path.Combine(HttpRuntime.AppDomainAppPath, EnvironmentHelper.TemplatePath(this.RouteData));
            var list = _messagerContract.Messagers.Where(m => Id.Contains(m.Id)).OrderByDescending(m => m.Id).ToList();
            var group = new StringTemplateGroup("all", path, typeof(TemplateLexer));
            var st = group.GetInstanceOf("Exporter");
            st.SetAttribute("list", list);
            return Json(new { version = EnvironmentHelper.ExcelVersion(), html = st.ToString() }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 获取员工
        /// <summary>
        /// 初始化员工界面
        /// </summary>
        /// <returns></returns>
        public ActionResult Admin()
        {
            return PartialView();
        }

        /// <summary>
        /// 获取员工数据
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> AdminList()
        {
            GridRequest request = new GridRequest(Request);
            var rules = request.FilterGroup.Rules.Where(x => string.IsNullOrEmpty(x.Field)).ToList();
            foreach (FilterRule item in rules)
            {
                request.FilterGroup.Rules.Remove(item);
            }
            Expression<Func<Administrator, bool>> predicate = FilterHelper.GetExpression<Administrator>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                var count = 0;
                var list = _adminContract.Administrators.Where<Administrator, int>(predicate, request.PageCondition, out count).Select(m => new
                {
                    m.Id,
                    m.Member.MemberName,
                    m.Member.RealName,
                    m.JPushRegistrationID
                }).ToList();
                return new GridData<object>(list, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 将消息放入缓存中
        /// <summary>
        /// 写入缓存
        /// </summary>
        /// <param name="ReceiverIds"></param>
        /// <param name="addorsubCount">每个Id要增加或减少的消息数量，正数为加，负数为减</param>
        private void SetCache(List<int> ReceiverIds, int addorsubCount = 1)
        {
            #region old版本

            //List<Messager> listMsg = _messagerContract.Messagers.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.Status == (int)MessagerStatusFlag.UnRead).ToList();
            //List<Administrator> listAdmin = _adminContract.Administrators.Where(x => x.IsDeleted == false && x.IsEnabled == true).ToList();
            //Dictionary<int, int> dicMsg = new Dictionary<int, int>();
            //foreach (Administrator admin in listAdmin)
            //{
            //    int msgCount = listMsg.Where(x => x.ReceiverId == admin.Id).Count();
            //    if (dicMsg.ContainsKey(admin.Id))
            //    {
            //        //int count = dicMsg[admin.Id];
            //        dicMsg[admin.Id] = msgCount;
            //    }
            //    else
            //    {
            //        dicMsg.Add(admin.Id, msgCount);
            //    }
            //}
            //CacheHelper.RemoveAllCache("MsgCache");
            //CacheHelper.SetCache("MsgCache", dicMsg);

            #endregion

            #region new版本

            Dictionary<int, int> dicMsgNew = new Dictionary<int, int>();
            object obj = CacheHelper.GetCache("MsgCache");
            if (obj == null)
            {
                Dictionary<int, int> dicMsg = _messagerContract.Messagers.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.Status == (int)MessagerStatusFlag.UnRead && ReceiverIds.Contains(x.ReceiverId.Value)).GroupBy(g => g.ReceiverId.Value).ToDictionary(t => t.Key, t => t.Count());
                foreach (var item in ReceiverIds)
                {
                    int count = 0;
                    dicMsgNew.Add(item, count);
                    if (dicMsg.TryGetValue(item, out count))
                    {
                        dicMsgNew[item] = count;
                    }
                }
            }
            else
            {
                dicMsgNew = obj as Dictionary<int, int>;
                List<int> listLostIds = ReceiverIds.Where(w => !dicMsgNew.Keys.Contains(w)).ToList();//不存在的receiverids
                List<int> listHasIds = ReceiverIds.Except(listLostIds).ToList();
                if (listLostIds != null && listLostIds.Count > 0)
                {
                    Dictionary<int, int> dicMsgLost = _messagerContract.Messagers.Where(x => !x.IsDeleted && x.IsEnabled && x.Status == (int)MessagerStatusFlag.UnRead && listLostIds.Contains(x.ReceiverId.Value)).GroupBy(g => g.ReceiverId.Value).ToDictionary(t => t.Key, t => t.Count());
                    foreach (var item in listLostIds)
                    {
                        int count = 0;
                        dicMsgNew.Add(item, count);
                        if (dicMsgLost.TryGetValue(item, out count))
                        {
                            dicMsgNew[item] = count;
                        }
                    }
                }
                if (!listHasIds.IsEmpty())
                {
                    foreach (var item in listHasIds)
                    {
                        if (dicMsgNew.ContainsKey(item))
                        {
                            dicMsgNew[item] += addorsubCount; dicMsgNew[item] = dicMsgNew[item] >= 0 ? dicMsgNew[item] : 0;
                        }
                        else
                        {
                            //应该不会出现，防止丢失单独走一次（正常情况不走else）
                            int count = _messagerContract.Messagers.Where(x => !x.IsDeleted && x.IsEnabled && x.Status == (int)MessagerStatusFlag.UnRead && x.ReceiverId == item).Count();
                            dicMsgNew.Add(item, count);
                        }
                    }
                }
            }
            CacheHelper.RemoveAllCache("MsgCache");
            CacheHelper.SetCache("MsgCache", dicMsgNew);

            sendMessageAction(ReceiverIds);

            #endregion
        }
        #endregion

        #region 获取消息数量
        public JsonResult GetMsgCount(int? AdminId=null)
        {
            var adminId = AdminId ?? AuthorityHelper.OperatorId;
            int msgCount = 0;

            #region 走缓存
            object obj = CacheHelper.GetCache("MsgCache");
            if (adminId.HasValue)
            {
                if (obj == null)
                {
                    SetCache(new List<int>() { adminId.Value });
                    obj = CacheHelper.GetCache("MsgCache");
                }
                Dictionary<int, int> dic = obj as Dictionary<int, int>;
                if (!dic.TryGetValue(adminId.Value, out msgCount))
                {
                    SetCache(new List<int>() { adminId.Value });
                    obj = CacheHelper.GetCache("MsgCache");
                    Dictionary<int, int> dicc = obj as Dictionary<int, int>;
                    dicc.TryGetValue(adminId.Value, out msgCount);
                }
            }

            #endregion

            return Json(msgCount, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 深拷贝优惠券详细信息
        /// <summary>
        /// 深拷贝优惠券详细信息
        /// </summary>
        /// <param name="couponItem"></param>
        /// <returns></returns>
        private MessagerDto DeepClone(MessagerDto dto)
        {
            using (Stream stream = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, dto);
                stream.Seek(0, SeekOrigin.Begin);
                MessagerDto entityDto = formatter.Deserialize(stream) as MessagerDto;
                return entityDto;
            }
        }
        #endregion

    }
}
