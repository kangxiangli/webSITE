using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;

namespace Whiskey.Utility.Data
{
    //yxk 2015-10-
    /// <summary>
    /// 封装cache操作
    /// </summary>
    public static class CacheHelper
    {
        static System.Web.Caching.Cache cache = HttpRuntime.Cache;
        #region cache基本操作
        public static object GetCache(string key)
        {

            return cache[key];
        }

        /// <summary>
        /// 设置数据缓存
        /// </summary>
        public static void SetCache(string key, object obj)
        {

            cache.Insert(key, obj);
        }

        /// <summary>
        /// 设置数据缓存
        /// </summary>
        public static void SetCache(string key, object obj, TimeSpan timeout)
        {

            cache.Insert(key, obj, null, DateTime.MaxValue, timeout, System.Web.Caching.CacheItemPriority.NotRemovable, null);

        }
        public static void SetCache(string key, object obj, CacheDependency depend)
        {

            cache.Insert(key, obj, depend);


        }

        /// <summary>
        /// 设置数据缓存
        /// </summary>
        public static void SetCache(string key, object obj, DateTime absoluteExpiration, TimeSpan slidingExpiration)
        {

            cache.Insert(key, obj, null, absoluteExpiration, slidingExpiration);
        }
        public static void SetCache(string key, object obj, CacheDependency depend, TimeSpan timespanExpiration)
        {

            cache.Insert(key, obj, depend, Cache.NoAbsoluteExpiration, timespanExpiration);


        }

        /// <summary>
        /// 移除指定数据缓存
        /// </summary>
        public static void RemoveAllCache(string key, bool contains = false)
        {
            key.CheckNotNullOrEmpty("key");
            if (!contains)
            {
                cache.Remove(key);
            }
            else
            {
                IDictionaryEnumerator CacheEnum = cache.GetEnumerator();
                while (CacheEnum.MoveNext())
                {
                    var strCurKey = CacheEnum.Key.ToString();
                    if (strCurKey.IndexOf(key) > -1)
                    {
                        cache.Remove(strCurKey);
                    }
                }
            }
        }

        /// <summary>
        /// 移除指定数据缓存
        /// </summary>
        public static void RemoveCache(params string[] keys)
        {
            foreach (var key in keys)
            {
                cache.Remove(key);
            }
           
        }

        /// <summary>
        /// 移除全部缓存
        /// </summary>
        public static void RemoveAllCache()
        {
            IDictionaryEnumerator CacheEnum = cache.GetEnumerator();
            while (CacheEnum.MoveNext())
            {
                cache.Remove(CacheEnum.Key.ToString());
            }
        }

        #endregion

    }

}
