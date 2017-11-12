using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using Whiskey.Utility.Extensions;

namespace Whiskey.Web.Helper
{
    /// <summary>
    /// 发起网络请求
    /// </summary>
    public class HttpRequestHelper
    {
        //private static HttpClient client { get { return new HttpClient(); } }

        /// <summary>
        /// Post请求
        /// </summary>
        /// <param name="url">请求连接地址</param>
        /// <param name="PostVars">请求的参数</param>
        /// <returns></returns>
        public static TResult Post<TResult>(string url, Dictionary<string, string> PostVars)
        {
            var client = new HttpClient();
            var content = new FormUrlEncodedContent(PostVars);
            var response = client.PostAsync(url, content).Result;
            var result = response.Content.ReadAsStringAsync().Result;
            return result.FromJsonString<TResult>();
        }

        /// <summary>
        /// Post请求
        /// </summary>
        /// <param name="url">请求连接地址</param>
        /// <param name="PostVars">发送的参数将被序列化为JSON</param>
        /// <returns></returns>
        public static TResult PostJson<T, TResult>(string url, T PostVars)
        {
            var client = new HttpClient();
            var response = client.PostAsJsonAsync(url, PostVars).Result;
            var result = response.Content.ReadAsStringAsync().Result;
            return result.FromJsonString<TResult>();
        }
        /// <summary>
        /// 返回是JSON形式时使用
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="url"></param>
        /// <returns></returns>
        public static TResult Get<TResult>(string url)
        {
            var client = new HttpClient();
            var result = client.GetStringAsync(url).Result;
            return result.FromJsonString<TResult>();
        }
        /// <summary>
        /// 返回不是JSON形式时调用
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string Get(string url)
        {
            var client = new HttpClient();
            var result = client.GetStringAsync(url).Result;
            return result;
        }

        /// <summary>
        /// Post请求,已form-data形式提交
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="url">请求url</param>
        /// <param name="imgs">文件</param>
        /// <param name="PostVars">参数</param>
        /// <returns></returns>
        public static TResult PostFormData<TResult>(string url, Dictionary<string, byte[]> files, Dictionary<string, string> PostVars)
        {
            using (var client = new HttpClient())
            using (var content = new MultipartFormDataContent())
            {
                if (files != null)
                {
                    foreach (var item in files.Where(w => w.Value != null))
                    {
                        var fileContent = new ByteArrayContent(item.Value);
                        content.Add(fileContent, item.Key, item.Key);
                    }
                }
                if (PostVars != null)
                {
                    foreach (var item in PostVars.Where(w => w.Value != null))
                    {
                        content.Add(new StringContent(item.Value), string.Format("\"{0}\"", item.Key));
                    }
                }

                var response = client.PostAsync(url, content).Result;
                var result = response.Content.ReadAsStringAsync().Result;
                return result.FromJsonString<TResult>();
            }
        }

        /// <summary>
        /// Post请求,已form-data形式提交
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="url">请求url</param>
        /// <param name="imgs">文件</param>
        /// <param name="PostVars">参数</param>
        /// <returns></returns>
        public static TResult PostFormFile<TResult>(string url, Dictionary<string, HttpFileCollectionBase> files, Dictionary<string, string> PostVars)
        {
            using (var client = new HttpClient())
            using (var content = new MultipartFormDataContent())
            {
                if (files != null)
                {
                    //foreach (var item in files.Where(w => w.Value != null))
                    //{
                    //    var fileContent = new ByteArrayContent(item.Value);
                    //    content.Add(fileContent, item.Key, item.Key);
                    //}
                    foreach (var item in files.Where(w => w.Value != null))
                    {
                        var fileContent = new ObjectContent(item.Value.GetType(), item.Value, new JsonMediaTypeFormatter());
                        content.Add(fileContent, item.Key);
                    }
                }
                if (PostVars != null)
                {
                    foreach (var item in PostVars.Where(w => w.Value != null))
                    {
                        content.Add(new StringContent(item.Value), string.Format("\"{0}\"", item.Key));
                    }
                }

                //var response = client.PostAsync(url, content).Result;
                var response = client.PutAsJsonAsync(url, content).Result;
                var result = response.Content.ReadAsStringAsync().Result;
                return result.FromJsonString<TResult>();
            }
        }
    }
}
