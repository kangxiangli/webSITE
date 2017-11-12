using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack.Redis;
using Whiskey.Utility.Helper;
using System.Configuration;
using ServiceStack.Caching;
using System.Collections;

namespace Whiskey.Utility.Data
{
    public class RedisCacheHelper
    {
        private static readonly RedisManagerPool _clientManager;
        private static IRedisClient _client
        {
            get
            {
                return _clientManager.GetClient();
            }
        }

        /// <summary>
        /// 会员jpush
        /// </summary>
        public const string KEY_MEMBER_JPUSH = "member_jpush";

        /// <summary>
        /// 所有店铺
        /// </summary>
        public const string KEY_ALL_STORE = "allstore";

        /// <summary>
        /// 所有仓库
        /// </summary>
        public const string KEY_ALL_STORAGE = "allstorage";

        /// <summary>
        /// 权限店铺
        /// </summary>
        public const string KEY_ADMIN_MANAGE_STORE = "admin:manage:store";

        /// <summary>
        /// 权限部门
        /// </summary>
        public const string KEY_ADMIN_MANAGE_DEPARTMENT = "admin:manage:department";

        /// <summary>
        /// 权限仓库
        /// </summary>
        public const string KEY_ADMIN_MANAGE_STORAGE = "admin:manage:storage";

        /// <summary>
        /// 会员[app确认登录]状态
        /// </summary>
        public const string KEY_MEMBER_LOGIN_STAT_PREFIX = "member_login_stat_";

        /// <summary>
        /// 零售会员登录
        /// </summary>
        public const string KEY_MEMBER_RETAIL_LOGIN_PREFIX = "retail:login:member:";

        /// <summary>
        /// 在线选货配置
        /// </summary>
        public const string kEY_ONLINEPURCHASE_CONFIG = "online_purchase:config";


        /// <summary>
        /// 商城配置
        /// </summary>
        public const string kEY_ONLINESTOREE_CONFIG = "online_store:config";

        /// <summary>
        /// 在线选货单中款号的新品状态
        /// </summary>
        public const string KEY_ONLINEPURCHASE_BIGPRODNUM_STATE = "online_purchase:bigprodnum_state";

        /// <summary>
        /// app商城中款号的新品状态
        /// </summary>
        public const string KEY_ONLINESTORE_BIGPRODNUM_STATE = "online_store:bigprodnum_state";

        public const string EnterpriseMemberTypeId = "member_type_id_bind_to_enterprise";

        #region 连接字符串
        /// <summary>
        /// 正式环境配置节KEY
        /// </summary>
        public const string PRODUCTION_KEY = "RedisHosts_PRODUCTION";

        /// <summary>
        /// 开发环境配置节KEY
        /// </summary>
        public const string Development_KEY = "RedisHosts_DEV";
        #endregion
        static RedisCacheHelper()
        {

            var appSettingKey = PRODUCTION_KEY;
            if (ConfigurationHelper.IsDevelopment())
            {
                appSettingKey = Development_KEY;
            }

            var connStr = ConfigurationHelper.GetAppSetting(appSettingKey);

            if (string.IsNullOrEmpty(connStr))
            {
                throw new Exception($"Redis配置信息未找到 key:{appSettingKey}");
            }
            var hosts = connStr.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            _clientManager = new RedisManagerPool(hosts, new RedisPoolConfig()
            {
                MaxPoolSize = 1000
            });

        }

        #region 原子操作
        public static long Increment(string key, uint amount)
        {


            return _client.Increment(key, amount);

        }

        public static long Decrement(string key, uint amount)
        {


            return _client.Decrement(key, amount);

        }
        #endregion

        #region 字符串GET,SET 基础操作
        public static bool Set<T>(string key, T value, DateTime expiresAt)
        {


            return _client.Set(key, value, expiresAt);

        }
        public static bool Set<T>(string key, T value)
        {


            return _client.Set(key, value);

        }
        public static bool Set<T>(string key, T value, TimeSpan expiresIn)
        {


            return _client.Set(key, value, expiresIn);

        }

        /// <summary>
        /// 批量保存key value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        public static void SetAll<T>(IDictionary<string, T> values)
        {
            _client.SetAll(values);
        }

        /// <summary>
        /// 仅在key不存在的情况下插入成功,如果KEY已存在,返回false
        /// </summary>
        public static bool SetNX<T>(string key, T value, DateTime expiresAt)
        {
            return _client.Add(key, value, expiresAt);
        }

        /// <summary>
        ///仅在key不存在的情况下插入成功,如果KEY已存在,返回false
        /// </summary>
        public static bool SetNX<T>(string key, T value)
        {

            return _client.Add(key, value);

        }

        /// <summary>
        ///仅在key不存在的情况下插入成功,如果KEY已存在,返回false
        /// </summary>
        public static bool SetNX<T>(string key, T value, TimeSpan expiresIn)
        {
            return _client.Add(key, value, expiresIn);
        }

        public static T Get<T>(string key)
        {

            return _client.Get<T>(key);

        }

        /// <summary>
        /// SADD 操作
        /// </summary>
        /// <param name="setId"></param>
        /// <param name="item"></param>
        public static void AddItemToSet(string setId, string item)
        {
            _client.AddItemToSet(setId, item);
        }

        /// <summary>
        /// SMEMBERS 操作
        /// </summary>
        public static HashSet<string> GetAllItemsFromSet(string setId)
        {
            return _client.GetAllItemsFromSet(setId);
        }

        /// <summary>
        /// 批量获取
        /// </summary>
        public static IDictionary<string, T> GetAll<T>(IEnumerable<string> keys)
        {
            return _client.GetAll<T>(keys);
        }
        #endregion

        #region 删除操作
        public static bool Remove(string key)
        {
            return _client.Remove(key);
        }
        public static void RemoveAll(IEnumerable<string> keys)
        {
            _client.RemoveAll(keys);
        }
        #endregion

        #region Entity相关操作,默认将entity序列化为json字符串
        public static void Delete<T>(T entity)
        {
            _client.Delete(entity);
        }
        public static void DeleteAll<TEntity>()
        {
            _client.DeleteAll<TEntity>();
        }
        public static void DeleteById<T>(object id)
        {
            _client.DeleteById<T>(id);
        }
        public static void DeleteByIds<T>(ICollection ids)
        {
            _client.DeleteByIds<T>(ids);
        }
        public static T GetById<T>(object id)
        {
            return _client.GetById<T>(id);
        }
        public static IList<T> GetByIds<T>(ICollection ids)
        {
            return _client.GetByIds<T>(ids);
        }

        /// <summary>
        /// 后台保存的key: urn:<T>:<id>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static T Store<T>(T entity)
        {
            return _client.Store(entity);
        }
        public static void StoreAll<TEntity>(IEnumerable<TEntity> entities)
        {   
            _client.StoreAll(entities);
        }
        #endregion


        /// <summary>
        /// HSET 操作
        /// </summary>
        public static bool SetEntryInHash(string hashId, string key, string value)
        {
            return _client.SetEntryInHash(hashId, key, value);
        }

        /// <summary>
        /// HSETNX 操作
        /// </summary>
        /// <param name="hashId"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool SetEntryInHashIfNotExists(string hashId, string key, string value)
        {
            return _client.SetEntryInHashIfNotExists(hashId, key, value);
        }


        /// <summary>
        /// HMSET 操作
        /// </summary>
        /// <param name="hashId"></param>
        /// <param name="keyValuePairs"></param>
        public static void SetRangeInHash(string hashId, IEnumerable<KeyValuePair<string, string>> keyValuePairs)
        {
            _client.SetRangeInHash(hashId, keyValuePairs);
        }

        /// <summary>
        /// HGETALL
        /// </summary>
        public static Dictionary<string, string> GetAllEntriesFromHash(string hashId)
        {
            return _client.GetAllEntriesFromHash(hashId);
        }
        /// <summary>
        /// HDEL
        /// </summary>
        /// <param name="hashId"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool RemoveEntryFromHash(string hashId, string key)
        {
            return _client.RemoveEntryFromHash(hashId, key);
        }

        /// <summary>
        /// HDEL
        /// </summary>
        /// <param name="hashId"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static void RemoveAllEntryFromHash(string hashId, params string[] keys)
        {
            using (var pipeline = _client.CreatePipeline())
            {
                for (int i = 0; i < keys.Length; i++)
                {
                    pipeline.QueueCommand(r => r.RemoveEntryFromHash(hashId, keys[i]));
                }
                pipeline.Flush();
            }
        }

        /// <summary>
        /// HGET操作
        /// </summary>
        /// <param name="hashId"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetValueFromHash(string hashId, string key)
        {
            return _client.GetValueFromHash(hashId, key);
        }

        /// <summary>
        /// HGET操作，泛型版
        /// </summary>
        /// <param name="hashId"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T GetValueFromHash<T>(string hashId, string key) where T : class
        {
            var str = _client.GetValueFromHash(hashId, key);
            if (string.IsNullOrEmpty(str) || str.Length <= 0)
            {
                return null;
            }
            try
            {
                var res = JsonHelper.FromJson<T>(str);
                return res;
            }
            catch (Exception)
            {
                return null;
            }

        }

        /// <summary>
        /// HMGET 操作
        /// </summary>
        /// <param name="hashId"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public static List<string> GetValuesFromHash(string hashId, params string[] keys)
        {
            return _client.GetValuesFromHash(hashId, keys);
        }

        public static bool ContainsKey(string key)
        {
            return _client.ContainsKey(key);
        }

        public static bool HashContainsEntry(string hashId, string key)
        {
            return _client.HashContainsEntry(hashId, key);
        }

        public static bool ExpireEntryIn(string key, TimeSpan expireIn)
        {
            return _client.ExpireEntryIn(key, expireIn);
        }


        public static void ResetCacheAllStore()
        {
            _client.Remove(KEY_ALL_STORE);
        }
        public static void ResetCacheAllStorage()
        {
            _client.Remove(KEY_ALL_STORAGE);
        }

        /// <summary>
        /// 重置管理员的权限缓存
        /// </summary>
        /// <param name="adminIds"></param>
        public static void ResetManageStoreDepartmentCache(params int[] adminIds)
        {
            var arr = adminIds.Select(i => i.ToString()).ToArray();
            RemoveAllEntryFromHash(KEY_ADMIN_MANAGE_STORE, arr);
            RemoveAllEntryFromHash(KEY_ADMIN_MANAGE_DEPARTMENT, arr);
        }


        /// <summary>
        /// 清除店铺仓库部门及权限店铺,权限仓库,权限部门相关cache
        /// </summary>
        /// <param name="adminIds"></param>
        public static void ResetStoreDepartmentMangeStoreCache()
        {
            var keys = new List<string>();
            keys.AddRange(_client.SearchKeys("all*"));
            keys.AddRange(_client.SearchKeys("admin:manage*"));
            _client.RemoveAll(keys);
            
        }


        /// <summary>
        /// 短信发送配置对象
        /// </summary>
        public const string KEY_SMS_CONFIG = "config:sms";


        /// <summary>
        /// 获取短信发送配置
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, string> GetSMSConfig()
        {
            var dict = _client.GetAllEntriesFromHash(KEY_SMS_CONFIG);
            return dict;
        }

        /// <summary>
        /// 初始化短信发送配置
        /// </summary>
        /// <returns></returns>
        public static void SetSMSConfig(Dictionary<string,string> dict)
        {
            _client.SetRangeInHash(KEY_SMS_CONFIG, dict);
        }

        public static void LogExcepition(string key,string val)
        {
            _client.AddItemToList(key, val);
        }

        

    }
}
