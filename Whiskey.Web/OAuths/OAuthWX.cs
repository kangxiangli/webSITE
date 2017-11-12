using System;
using Whiskey.Web.Helper;
using Whiskey.Utility.Extensions;
using WxPayAPI;
using System.Collections.Generic;

namespace Whiskey.Web.OAuths
{
    public static class OAuthWX
    {
        //零时尚公众平台
        private static string appId = WxPayConfig.WX_0Fashion_APPID;// "wx844834db9123c470";
        private static string appSecret = WxPayConfig.APPSECRET;// "17836226f3dbca60de878759f1b018d5";
        /// <summary>
        /// 获取微信Code
        /// </summary>
        /// <param name="redirectUrl">返回的登录地址</param>
        /// <param name="isWap">true=微信内部登录 false=pc网页登录</param>
        public static string GetWeiXinCode(string redirectUrl, bool isWap = true)
        {
            if (redirectUrl.IsNotNullAndEmpty()) redirectUrl = System.Web.HttpUtility.UrlEncode(redirectUrl);
            if (isWap)
            {
                //微信OpenId授权
                return "https://open.weixin.qq.com/connect/oauth2/authorize?appid=" + appId + "&redirect_uri=" +
                      redirectUrl + "&response_type=code&scope=snsapi_userinfo&state=STATE#wechat_redirect";
            }
            else
            {
                //微信登录授权
                return "https://open.weixin.qq.com/connect/qrconnect?appid=" + appId + "&redirect_uri=" + redirectUrl +
                      "&response_type=code&scope=snsapi_login&state=STATE#wechat_redirect";
            }
        }
        /// <summary>
        /// 通过code获取access_token
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static WeiXinAccessTokenResult GetWeiXinAccessToken(string code)
        {
            string url = "https://api.weixin.qq.com/sns/oauth2/access_token?appid=" + appId + "&secret=" + appSecret +
              "&code=" + code + "&grant_type=authorization_code";
            string jsonStr = HttpRequestHelper.Get(url);
            var result = new WeiXinAccessTokenResult();
            if (jsonStr.Contains("errcode"))
            {
                var errorResult = jsonStr.FromJsonString<WeiXinErrorMsg>();
                result.ErrorResult = errorResult;
                result.Result = false;
            }
            else
            {
                var model = jsonStr.FromJsonString<WeiXinAccessTokenModel>();
                result.SuccessResult = model;
                result.Result = true;
            }
            return result;
        }
        /// <summary>
        /// 拉取用户信息
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="openId"></param>
        /// <returns></returns>
        public static WeiXinUserInfoResult GetWeiXinUserInfo(string accessToken, string openId)
        {
            string url = "https://api.weixin.qq.com/sns/userinfo?access_token=" + accessToken + "&openid=" + openId + "&lang=zh_CN";
            string jsonStr = HttpRequestHelper.Get(url);
            var result = new WeiXinUserInfoResult();
            if (jsonStr.Contains("errcode"))
            {
                var errorResult = jsonStr.FromJsonString<WeiXinErrorMsg>();
                result.ErrorMsg = errorResult;
                result.Result = false;
            }
            else
            {
                var userInfo = jsonStr.FromJsonString<WeiXinUserInfo>();
                result.UserInfo = userInfo;
                result.Result = true;
            }
            return result;
        }
    }
    /// <summary>
    /// 通过code获取access_token 请求成功的实体
    /// </summary>
    [Serializable]
    public class WeiXinAccessTokenModel
    {
        /// <summary>
        /// 接口调用凭证
        /// </summary>
        public string access_token { get; set; }
        /// <summary>
        /// access_token接口调用凭证超时时间，单位（秒）
        /// </summary>
        public int expires_in { get; set; }
        /// <summary>
        /// 用户刷新access_token
        /// </summary>
        public string refresh_token { get; set; }
        /// <summary>
        /// 授权用户唯一标识
        /// </summary>
        public string openid { get; set; }
        /// <summary>
        /// 用户授权的作用域，使用逗号（,）分隔
        /// </summary>
        public string scope { get; set; }
    }

    [Serializable]
    public class WeiXinAccessTokenResult
    {
        public WeiXinAccessTokenModel SuccessResult { get; set; }
        public bool Result { get; set; }

        public WeiXinErrorMsg ErrorResult { get; set; }
    }
    /// <summary>
    /// 获取微信用户信息
    /// </summary>
    [Serializable]
    public class WeiXinUserInfoResult
    {
        /// <summary>
        /// 微信用户信息
        /// </summary>
        public WeiXinUserInfo UserInfo { get; set; }
        /// <summary>
        /// 结果
        /// </summary>
        public bool Result { get; set; }
        /// <summary>
        /// 错误信息
        /// </summary>
        public WeiXinErrorMsg ErrorMsg { get; set; }
    }

    /// <summary>
    /// 微信错误访问的情况
    /// </summary>
    [Serializable]
    public class WeiXinErrorMsg
    {
        /// <summary>
        /// 错误编号
        /// </summary>
        public int errcode { get; set; }
        /// <summary>
        /// 错误提示消息
        /// </summary>
        public string errmsg { get; set; }
    }
    /// <summary>
    /// 微信授权成功后，返回的用户信息
    /// </summary>
    [Serializable]
    public class WeiXinUserInfo
    {
        /// <summary>
        /// 用户是否订阅该公众号标识，值为0时，代表此用户没有关注该公众号，拉取不到其余信息
        /// </summary>
        public int subscribe { get; set; }
        /// <summary>
        /// 用户的唯一标识
        /// </summary>
        public string openid { get; set; }
        /// <summary>
        /// 用户昵称
        /// </summary>
        public string nickname { get; set; }
        /// <summary>
        /// 用户的性别，值为1时是男性，值为2时是女性，值为0时是未知
        /// </summary>
        public int sex { get; set; }
        /// <summary>
        /// 用户个人资料填写的省份
        /// </summary>
        public string province { get; set; }
        /// <summary>
        /// 普通用户个人资料填写的城市
        /// </summary>
        public string city { get; set; }
        /// <summary>
        /// 国家，如中国为CN
        /// </summary>
        public string country { get; set; }
        /// <summary>
        /// 用户头像，最后一个数值代表正方形头像大小（有0、46、64、96、132数值可选，0代表640*640正方形头像），用户没有头像时该项为空
        /// </summary>
        public string headimgurl { get; set; }
    }
}
