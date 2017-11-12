﻿using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Whiskey.Utility.Extensions;
using Whiskey.Utility.Data;
using Whiskey.Utility.Helper;

namespace Whiskey.ZeroStore.ERP.WebMember
{
    [HubName("AutoExitHub")]
    public class AutoExitHub : BaseHub<AutoExitHub>
    {
        public AutoExitHub() : base(_hubInfo) { }
        public static HubList<HubInfo> _hubInfo = new HubList<HubInfo>();

        #region 私有方法

        //tcid = 11.1.1.111;Windows 10;Chrome;2016/11/25 10:30:15
        /// <summary>
        /// 获取客户端tcid
        /// </summary>
        /// <param name="AdminId"></param>
        /// <returns></returns>
        private string GetTcId(string AdminId)
        {
            if (Context.RequestCookies.ContainsKey("tcid"))
            {
                Cookie tcidCookie = Context.RequestCookies["tcid"];
                if (tcidCookie.IsNotNull())
                {
                    string strtcid = tcidCookie.Value.Replace(" ", "+");
                    return strtcid;
                    //return AesHelper.Decrypt(strtcid, AdminId);
                }
            }
            return string.Empty;
        }

        #endregion

        public void flushSocketLink(string MemberId, string browserId)
        {
            Task.Run(() =>
            {
                if (MemberId.IsNotNullAndEmpty())
                {
                    string cid = Context.ConnectionId;
                    string tcid = GetTcId(MemberId);

                    var modcon = _hubInfo.FirstOrDefault(f => f.connectionId == cid);

                    if (modcon.IsNotNull())
                    {
                        modcon.MemberId = MemberId;
                        if (tcid.IsNotNullAndEmpty())//防止用户清除tcid
                        {
                            modcon.uuid = tcid;
                        }
                        modcon.browserId = browserId;
                        modcon.CreateTime = DateTime.Now;//时间被刷新
                    }
                    #region 这块代码使用不到
                    //var modadmin = _hubInfo.FirstOrDefault(f => f.uuid == AdminId);
                    //else if (modadmin.IsNotNull())
                    //{
                    //    modadmin.connectionId = cid;
                    //    modadmin.CreateTime = DateTime.Now;
                    //}
                    #endregion
                    else
                    {
                        modcon = new HubInfo();
                        modcon.MemberId = MemberId;
                        modcon.uuid = tcid;
                        modcon.connectionId = cid;
                        modcon.browserId = browserId;
                        modcon.CreateTime = DateTime.Now;
                        _hubInfo.Add(modcon);
                    }

                    if (!ConfigurationHelper.EnableManyDevice)
                    {
                        ExitOtherDevice(MemberId, modcon.browserId);
                    }
                }
            });
        }

        /// <summary>
        /// 退出当前用户的登录
        /// </summary>
        /// <param name="AdminId">用户id</param>
        /// <param name="clearAll">是否清除所有登录</param>
        /// <param name="tcid">指定的tcid</param>
        /// <returns></returns>
        public static OperationResult AutoExit(string MemberId, bool clearAll, string tcid)
        {
            OperationResult oResult = new OperationResult(OperationResultType.Error, "取消授权登录失败");
            var Clients = GetClients();
            if (clearAll)
            {
                var curall = GetCurAdminAllLogin(MemberId);
                if (curall.Count > 0)
                {
                    oResult.ResultType = OperationResultType.Success;
                    oResult.Message = "账号被取消授权登录";
                    foreach (var item in curall)
                    {
                        var client = Clients.Client(item.connectionId);
                        client.exitLogin(oResult);
                    }
                }
            }
            else
            {
                if (tcid.IsNotNullAndEmpty())
                {
                    var autoExitHub = _hubInfo.FirstOrDefault(f => f.MemberId == MemberId && (f.uuid == tcid || f.uuid.IsNullOrEmpty()));//uuid=""为用户恶意操作，所以强制退出
                    if (autoExitHub.IsNotNull())
                    {
                        var listBrowsers = _hubInfo.Where(w => w.MemberId == MemberId && w.browserId == autoExitHub.browserId).ToList();//uuid在同一个浏览器多次登录后不是唯一的

                        oResult.ResultType = OperationResultType.Success;
                        oResult.Message = "账号被取消授权登录";
                        foreach (var item in listBrowsers)
                        {
                            var client = Clients.Client(item.connectionId);
                            client.exitLogin(oResult);
                        }
                    }
                }
            }
            return oResult;
        }

        /// <summary>
        /// 退出其它设备的登录
        /// </summary>
        /// <param name="AdminId"></param>
        /// <param name="connectionId"></param>
        /// <returns></returns>
        public static void ExitOtherDevice(string MemberId, string browserId)
        {
            Task.Run(() =>
            {
                if (MemberId.IsNotNullAndEmpty())
                {
                    var Clients = GetClients();
                    var listOtherDevices = _hubInfo.Where(w => w.MemberId == MemberId && w.browserId != browserId).ToList();//添加browserId原因,同一个浏览器登录多次视为一次
                    if (listOtherDevices.Count > 0)
                    {
                        OperationResult oResult = new OperationResult(OperationResultType.Success, "账号在其它设备登录，如非本人操作请及时修改密码");
                        foreach (var device in listOtherDevices)
                        {
                            var client = Clients.Client(device.connectionId);
                            client.exitLogin(oResult);
                        }
                    }
                }
            });
        }

        public static List<HubInfo> GetCurAdminAllLogin(string MemberId)
        {
            if (MemberId.IsNotNullAndEmpty())
            {
                return _hubInfo.Where(w => w.MemberId == MemberId).ToList();
            }
            return new List<HubInfo>();
        }
    }
}