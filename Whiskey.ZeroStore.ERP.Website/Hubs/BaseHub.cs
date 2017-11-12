using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Website
{
    public class HubBase<T, THubInfo> : Hub where T : IHub where THubInfo : BaseHubInfo
    {
        private HubList<THubInfo> _hubInfo = new HubList<THubInfo>();

        public HubBase(HubList<THubInfo> _hubInfo)
        {
            this._hubInfo = _hubInfo;
        }
        /// <summary>
        /// 当前连接标识
        /// </summary>
        public string CurrentConnectionId { get { return Context.ConnectionId; } }

        /// <summary>
        /// 当前连接的设备
        /// </summary>
        public dynamic CurrentClient { get { return AllClients.Client(CurrentConnectionId); } }

        /// <summary>
        /// 所有连接的设备
        /// </summary>
        protected static IHubConnectionContext<dynamic> AllClients
        {
            get
            {
                return GlobalHost.ConnectionManager.GetHubContext<T>().Clients;
            }
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            _hubInfo.RemoveAll(f => f.connectionId == CurrentConnectionId);
            return base.OnDisconnected(stopCalled);
        }
    }

    public class BaseHub<T> : HubBase<T, HubInfo> where T : IHub
    {
        public BaseHub(HubList<HubInfo> _hubInfo) : base(_hubInfo)
        {
        }
    }

    public delegate void HubListChangedEventHandler<T>();
    public delegate void HubListItemChangedEventHandler<T>(params T[] t);

    public class HubList<T> : List<T>
    {
        /// <summary>
        /// 开启异步事件(默认开启)
        /// </summary>
        public bool enableAsyncEvent = true;

        /// <summary>
        /// 当List数量发生变化时会触发此事件
        /// </summary>
        public event HubListChangedEventHandler<T> ListChanged;
        public event HubListItemChangedEventHandler<T> ItemAdded;
        public event HubListItemChangedEventHandler<T> ItemRemoved;

        private void ItemChangedEvent(bool isAdd, params T[] t)
        {
            if (enableAsyncEvent)
            {
                Task.Run(() =>
                {
                    ItemChangedEventFunc(isAdd, t);
                });
            }
            else
            {
                ItemChangedEventFunc(isAdd, t);
            }
        }

        protected void ItemChangedEventFunc(bool isAdd, params T[] t)
        {
            if (isAdd)
                ItemAdded?.Invoke(t);
            else
                ItemRemoved?.Invoke(t);

            ListChanged?.Invoke();
        }

        public new void Add(T t)
        {
            base.Add(t);
            ItemChangedEvent(true, t);
        }

        public new void AddRange(IEnumerable<T> collection)
        {
            base.AddRange(collection);
            ItemChangedEvent(true, collection.ToArray());
        }
        public new void InsertRange(int index, IEnumerable<T> collection)
        {
            base.InsertRange(index, collection);
            ItemChangedEvent(true, collection.ToArray());
        }
        public new void Insert(int index, T t)
        {
            base.Insert(index, t);
            ItemChangedEvent(true, t);
        }
        public new bool Remove(T t)
        {
            var bo = base.Remove(t);
            if (bo)
                ItemChangedEvent(false, t);
            return bo;
        }
        public new int RemoveAll(Predicate<T> match)
        {
            var willdelete = this.Where(w => match(w)).ToArray();
            var co = base.RemoveAll(match);
            if (co > 0)
                ItemChangedEvent(false, willdelete);
            return co;
        }
        public new void RemoveAt(int index)
        {
            var willdelete = this[index];
            base.RemoveAt(index);
            ItemChangedEvent(false, willdelete);
        }
        public new void RemoveRange(int index, int count)
        {
            var willdelete = this.Skip(index).Take(count).ToArray();
            base.RemoveRange(index, count);
            ItemChangedEvent(false, willdelete);
        }
        public new void Clear()
        {
            var willdelete = this.ToArray();
            base.Clear();
            if (willdelete.Count() > 0)
                ItemChangedEvent(false, willdelete);
        }
        
    }

    public class BaseHubInfo
    {
        /// <summary>
        /// 连接标识
        /// </summary>
        public string connectionId { get; set; }
    }

    public class HubInfo : BaseHubInfo
    {
        public string uuid { get; set; }
        public string AdminId { get; set; }
        public string browserId { get; set; }
        public DateTime CreateTime { get; set; }
    }
    /// <summary>
    /// 设备信息
    /// </summary>
    public class DeviceHubInfo : BaseHubInfo
    {
        /// <summary>
        /// 设备标识
        /// </summary>
        public string IMEI { get; set; }
    }
}