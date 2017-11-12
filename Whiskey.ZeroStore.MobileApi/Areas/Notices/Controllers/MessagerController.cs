using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Data;
using Whiskey.Utility.Extensions;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Notices;
using Whiskey.ZeroStore.MobileApi.Controllers;

namespace Whiskey.ZeroStore.MobileApi.Areas.Notices.Controllers
{
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
        
        #region 获取消息数量
        public JsonResult GetMsgCount(int? AdminId = null)
        {
            if(AdminId==null)
            {
                return Json(0, JsonRequestBehavior.AllowGet);
            }

            int msgCount = 0;

            #region 走缓存
            object obj = CacheHelper.GetCache("MsgCache");
            if (AdminId.HasValue)
            {
                if (obj == null)
                {
                    SetCache(new List<int>() { AdminId.Value });
                    obj = CacheHelper.GetCache("MsgCache");
                }
                Dictionary<int, int> dic = obj as Dictionary<int, int>;
                if (!dic.TryGetValue(AdminId.Value, out msgCount))
                {
                    SetCache(new List<int>() { AdminId.Value });
                    obj = CacheHelper.GetCache("MsgCache");
                    Dictionary<int, int> dicc = obj as Dictionary<int, int>;
                    dicc.TryGetValue(AdminId.Value, out msgCount);
                }
            }

            #endregion

            return Json(msgCount, JsonRequestBehavior.AllowGet);
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
    }
}