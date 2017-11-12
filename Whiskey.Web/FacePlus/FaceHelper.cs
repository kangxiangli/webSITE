using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Whiskey.Utility.Helper;
using Whiskey.Web.Helper;
using Whiskey.Utility.Extensions;

namespace Whiskey.Web.FacePlus
{
    public partial class FaceHelper
    {
        static FaceHelper()
        {
            if (ConfigurationHelper.EnableFaceDev)
            {
                var str = ConfigurationHelper.GetAppSetting("Face_Api_Dev");
                if (str.IsNotNullAndEmpty())
                {
                    var arr = str.Split(";");
                    Api_Key = arr[0];
                    Api_Secret = arr[1];
                }
            }
        }
        /// <summary>
        /// 默认可信度
        /// </summary>
        public static float defaultConfidence = 80;

        #region API地址

        private static string BaseRoot = "https://api-cn.faceplusplus.com";
        private static string BaseUrl = $"{BaseRoot}/facepp/v3";

        private static string Api_Key = "kOr0sbHeIt9siZLeCiqNrPEN5HjY0GBJ";
        private static string Api_Secret = "8RLx4LRbx2mkgsuIflfvDDYbpsbwZagM";

        #region 人脸识别

        /// <summary>
        /// 人脸检测和人脸分析
        /// </summary>
        public static string Url_Detect = $"{BaseUrl}/detect";
        
        /// <summary>
        /// 在Faceset中找出与目标人脸最相似的一张或多张人脸。支持传入face_token或者直接传入图片进行人脸搜索
        /// </summary>
        public static string Url_Search = $"{BaseUrl}/search";
        /// <summary>
        /// 将两个人脸进行比对，来判断是否为同一个人。支持传两张图片进行比对，或者一张图片与一个已知的face_token比对，也支持两个face_token进行比对。
        /// </summary>
        public static string Url_Compare = $"{BaseUrl}/compare";

        #region FaceSet

        public class FaceSet
        {
            /// <summary>
            /// 创建一个人脸的集合 FaceSet，用于存储人脸标识 face_token。一个 FaceSet 能够存储 10000 个 face_token
            /// </summary>
            public static string Url_Faceset_Create = $"{BaseUrl}/faceset/create";

            /// <summary>
            /// 为一个已经创建的 FaceSet 添加人脸标识 face_token。一个 FaceSet 最多存储10000个 face_token。
            /// </summary>
            public static string Url_Faceset_AddFace = $"{BaseUrl}/faceset/addface";
            /// <summary>
            /// 移除一个FaceSet中的某些或者全部face_token
            /// </summary>
            public static string Url_Faceset_RemoveFace = $"{BaseUrl}/faceset/removeface";
            /// <summary>
            /// 更新一个人脸集合的属性
            /// </summary>
            public static string Url_Faceset_Update = $"{BaseUrl}/faceset/update";
            /// <summary>
            /// 获取一个 FaceSet 的所有信息，包括此 FaceSet 的 faceset_token, outer_id, display_name 的信息，以及此 FaceSet 中存放的 face_token 数量与列表。
            /// 注意：2017年8月16日后，调用本接口将不会一次性返回全部的 face_token 列表。单次查询最多返回 100 个 face_token。如需获取全量数据，需要配合使用 start 和 next 参数。请尽快修改调整您的程序。
            /// </summary>
            public static string Url_Faceset_GetDetail = $"{BaseUrl}/facepp/v3/faceset/getdetail";
            /// <summary>
            /// 删除一个人脸集合。
            /// </summary>
            public static string Url_Faceset_Delete = $"{BaseUrl}/faceset/delete";
            /// <summary>
            /// 获取某一 API Key 下的 FaceSet 列表及其 faceset_token、outer_id、display_name 和 tags 等信息。
            /// 注意：2017年8月16日后，调用本接口将不会一次性返回全量的 FaceSet。单次查询最多返回 100 个 FaceSet。如需获取全量数据，需要配合使用 start 和 next 参数。请尽快修改调整您的程序。
            /// </summary>
            public static string Url_Faceset_GetFacesets = $"{BaseUrl}/faceset/getfacesets";

        }

        #endregion

        #region Face

        public class Face
        {
            /// <summary>
            /// 通过传入在Detect API检测出的人脸标识face_token，分析得出人脸的五官关键点，人脸属性和人脸质量判断信息。最多支持分析5个人脸。
            /// </summary>
            public static string Url_Face_Analyze = $"{BaseUrl}/face/analyze";
            /// <summary>
            /// 通过传入在Detect API检测出的人脸标识face_token，获取一个人脸的关联信息，包括源图片ID、归属的FaceSet。
            /// </summary>
            public static string Url_Face_GetDetail = $"{BaseUrl}/face/getdetail";
            /// <summary>
            /// 为检测出的某一个人脸添加标识信息，该信息会在Search接口结果中返回，用来确定用户身份。
            /// </summary>
            public static string Url_Face_SetUserId = $"{BaseUrl}/face/setuserid";
        }

        #endregion

        

        #endregion

        #region 图像识别

        /// <summary>
        /// 调用者提供图片文件或者图片URL，进行图片分析，识别图片场景和图片主体
        /// </summary>
        public static string Url_Detectsceneandobject = $"{BaseRoot}/imagepp/beta/detectsceneandobject";

        #endregion

        #endregion
    }

    public partial class FaceHelper
    {
        /// <summary>
        /// 进行人脸检测和人脸分析,两个参数至少传一个
        /// </summary>
        /// <param name="image_url"></param>
        /// <param name="img"></param>
        /// <returns>识别到的face_token每次请求返回不一样,返回null表示识别未通过</returns>
        public static Tuple<bool, string> Detect(string image_url = null, Stream img = null, int retry = 20)
        {
            var files = new Dictionary<string, byte[]>();
            if (img != null)
            {
                var filebytes = StreamHelper.StreamToBytes(img);
                files.Add("image_file", filebytes);
            }

            var pars = new Dictionary<string, string>();

            pars.Add("api_key", Api_Key);
            pars.Add("api_secret", Api_Secret);
            pars.Add("image_url", image_url);
            pars.Add("return_attributes", "facequality");

            var result = HttpRequestHelper.PostFormData<dynamic>(Url_Detect, files, pars);

            string strerror = result.error_message;

            if (strerror.IsNotNullAndEmpty())
            {
                if (strerror.Contains("CONCURRENCY_LIMIT_EXCEEDED") && retry-- > 0)
                {
                    return Detect(image_url, img, retry);
                }

                return new Tuple<bool, string>(false, strerror);
            }
            else
            {
                if (result.faces != null)
                {
                    foreach (var item in result.faces)
                    {
                        var attr = item.attributes;
                        var faq = attr.facequality;
                        //faq.threshold，可信度默认70.1
                        if (faq.value >= faq.threshold)
                        {
                            string token = item.face_token;
                            return new Tuple<bool, string>(true, token);
                        }
                    }
                }
            }
            return new Tuple<bool, string>(false, "未检测到人脸");
        }
        /// <summary>
        /// 创建FaceSet
        /// </summary>
        /// <param name="outer_id">唯一标识</param>
        /// <param name="face_tokens"></param>
        /// <returns></returns>
        public static Tuple<bool, string> FaceSetCreate(string outer_id, string face_tokens, int retry = 20)
        {
            var pars = new Dictionary<string, string>();

            pars.Add("api_key", Api_Key);
            pars.Add("api_secret", Api_Secret);
            pars.Add("outer_id", outer_id);
            pars.Add("face_tokens", face_tokens);//人脸标识face_token，可以是一个或者多个，用逗号分隔。最多不超过5个face_token
            pars.Add("force_merge", "1");

            var result = HttpRequestHelper.PostFormData<dynamic>(FaceSet.Url_Faceset_Create, null, pars);
            string strerror = result.error_message;

            if (strerror.IsNotNullAndEmpty())
            {
                if (strerror.Contains("CONCURRENCY_LIMIT_EXCEEDED") && retry-- > 0)
                {
                    return FaceSetCreate(outer_id, face_tokens, retry);
                }
                return new Tuple<bool, string>(false, strerror);
            }
            else
            {
                string token = result.faceset_token;
                return new Tuple<bool, string>(true, token);
            }
        }
        /// <summary>
        /// 添加Face_Token到FaceSet
        /// </summary>
        /// <param name="outer_id">唯一标识</param>
        /// <param name="face_tokens">可以以,分隔不能超过5个</param>
        /// <returns></returns>
        public static Tuple<bool, string> FaceSetAddFace(string outer_id, string face_tokens, int retry = 20)
        {
            var pars = new Dictionary<string, string>();

            pars.Add("api_key", Api_Key);
            pars.Add("api_secret", Api_Secret);
            pars.Add("outer_id", outer_id);
            pars.Add("face_tokens", face_tokens);//人脸标识face_token，可以是一个或者多个，用逗号分隔。最多不超过5个face_token

            var result = HttpRequestHelper.PostFormData<dynamic>(FaceSet.Url_Faceset_AddFace, null, pars);
            string strerror = result.error_message;

            if (strerror.IsNotNullAndEmpty())
            {
                if (strerror.Contains("CONCURRENCY_LIMIT_EXCEEDED") && retry-- > 0)
                {
                    return FaceSetAddFace(outer_id, face_tokens, retry);
                }
                return new Tuple<bool, string>(false, strerror);
            }
            else if (result.failure_detail != null && result.failure_detail.Count > 0)
            {
                StringBuilder strb = new StringBuilder();
                foreach (var item in result.failure_detail)
                {
                    strb.Append(item.reason + ",");
                }
                return new Tuple<bool, string>(false, strb.ToString().Trim(','));
            }
            else
            {
                int face_count = result.face_count;
                return new Tuple<bool, string>(true, face_count + "");
            }
        }
        /// <summary>
        /// 从FaceSet中移除Face_Token
        /// </summary>
        /// <param name="outer_id">唯一标识</param>
        /// <param name="face_tokens">可以以,分隔不能超过1000</param>
        /// <returns></returns>
        public static Tuple<bool, string> FaceSetRemoveFace(string outer_id, string face_tokens, int retry = 20)
        {
            var pars = new Dictionary<string, string>();

            pars.Add("api_key", Api_Key);
            pars.Add("api_secret", Api_Secret);
            pars.Add("outer_id", outer_id);
            pars.Add("face_tokens", face_tokens);//人脸标识face_token，可以是一个或者多个，用逗号分隔。最多不超过5个face_token

            var result = HttpRequestHelper.PostFormData<dynamic>(FaceSet.Url_Faceset_RemoveFace, null, pars);
            string strerror = result.error_message;

            if (strerror.IsNotNullAndEmpty())
            {
                if (strerror.Contains("CONCURRENCY_LIMIT_EXCEEDED") && retry-- > 0)
                {
                    return FaceSetRemoveFace(outer_id, face_tokens, retry);
                }
                return new Tuple<bool, string>(false, strerror);
            }
            else if (result.failure_detail != null && result.failure_detail.Count > 0)
            {
                StringBuilder strb = new StringBuilder();
                foreach (var item in result.failure_detail)
                {
                    strb.Append(item.reason + ",");
                }
                return new Tuple<bool, string>(false, strb.ToString().Trim(','));
            }
            else
            {
                int face_count = result.face_count;
                return new Tuple<bool, string>(true, face_count + "");
            }
        }
        /// <summary>
        /// 在Faceset中找出与目标人脸最相似的一张或多张人脸，后三个参数至少传入一个
        /// </summary>
        /// <param name="outer_id"></param>
        /// <param name="face_token"></param>
        /// <param name="image_url"></param>
        /// <param name="img"></param>
        /// <returns></returns>
        public static Tuple<bool, string> Search(string outer_id, string face_token = null, string image_url = null, Stream img = null, int retry = 20)
        {
            var files = new Dictionary<string, byte[]>();
            if (img != null)
            {
                var filebytes = StreamHelper.StreamToBytes(img);
                files.Add("image_file", filebytes);
            }

            var pars = new Dictionary<string, string>();

            pars.Add("api_key", Api_Key);
            pars.Add("api_secret", Api_Secret);
            pars.Add("outer_id", outer_id);
            pars.Add("face_token", face_token);
            pars.Add("image_url", image_url);
            pars.Add("return_result_count", "3");

            var result = HttpRequestHelper.PostFormData<dynamic>(Url_Search, files, pars);
            string strerror = result.error_message;

            if (strerror.IsNotNullAndEmpty())
            {
                if (strerror.Contains("CONCURRENCY_LIMIT_EXCEEDED") && retry-- > 0)
                {
                    return Search(outer_id, face_token, image_url, img, retry);
                }
                return new Tuple<bool, string>(false, strerror);
            }
            else if (result.results != null)
            {
                foreach (var item in result.results)
                {
                    float confid = item.confidence;
                    if (confid >= defaultConfidence)
                    {
                        return new Tuple<bool, string>(true, "匹配通过");
                    }
                }
                return new Tuple<bool, string>(false, "匹配未通过");
            }
            else
            {
                return new Tuple<bool, string>(false, "匹配未通过");
            }
        }
        /// <summary>
        /// 将两个人脸进行比对，来判断是否为同一个人
        /// </summary>
        /// <param name="image_url1"></param>
        /// <param name="image_url2"></param>
        /// <param name="img1"></param>
        /// <param name="img2"></param>
        /// <returns></returns>
        public static Tuple<bool, string> Compare(string image_url1, string image_url2, Stream img1 = null, Stream img2 = null, int retry = 20)
        {
            var files = new Dictionary<string, byte[]>();
            if (img1 != null)
            {
                var filebytes = StreamHelper.StreamToBytes(img1);
                files.Add("image_file1", filebytes);
            }
            if (img2 != null)
            {
                var filebytes = StreamHelper.StreamToBytes(img2);
                files.Add("image_file2", filebytes);
            }

            var pars = new Dictionary<string, string>();

            pars.Add("api_key", Api_Key);
            pars.Add("api_secret", Api_Secret);
            pars.Add("image_url1", image_url1);
            pars.Add("image_url2", image_url2);

            var result = HttpRequestHelper.PostFormData<dynamic>(Url_Compare, files, pars);
            string strerror = result.error_message;

            if (strerror.IsNotNullAndEmpty())
            {
                if (strerror.Contains("CONCURRENCY_LIMIT_EXCEEDED") && retry-- > 0)
                {
                    return Compare(image_url1, image_url2, img1, img2, retry);
                }
                return new Tuple<bool, string>(false, strerror);
            }
            else if (result.confidence != null)
            {
                float confid = result.confidence;
                return new Tuple<bool, string>(confid >= defaultConfidence, confid >= defaultConfidence ? "匹配通过" : "匹配未通过");
            }
            else
            {
                return new Tuple<bool, string>(false, "匹配未通过");
            }
        }

        /// <summary>
        /// 根据转入信息获取匹配的Out_Id
        /// </summary>
        /// <param name="image_url"></param>
        /// <param name="imgStream"></param>
        /// <returns></returns>
        public static Tuple<bool,string, List<int>> FaceGetDetail(string image_url, Stream imgStream = null, int retry = 20)
        {
            var dresult = new Tuple<bool, string, List<int>>(false, "未匹配到相关信息", null);

            var res = Detect(image_url, imgStream);
            if (res.Item1)
            {
                var pars = new Dictionary<string, string>();

                pars.Add("api_key", Api_Key);
                pars.Add("api_secret", Api_Secret);
                pars.Add("face_token", res.Item2);

                var result = HttpRequestHelper.PostFormData<dynamic>(Face.Url_Face_GetDetail, null, pars);
                string strerror = result.error_message;

                if (strerror.IsNotNullAndEmpty())
                {
                    if (strerror.Contains("CONCURRENCY_LIMIT_EXCEEDED") && retry-- > 0)
                    {
                        return FaceGetDetail(image_url, imgStream, retry);
                    }
                    dresult = new Tuple<bool, string, List<int>>(false, strerror, null);
                }
                else if (result.facesets != null && result.facesets.Count > 0)
                {
                    var listids = new List<int>();
                    foreach (var item in result.facesets)
                    {
                        int outid = item.outer_id;
                        listids.Add(outid);
                    }
                    dresult = new Tuple<bool, string, List<int>>(true, "匹配成功", listids);
                }
            }
            else
            {
                dresult = new Tuple<bool, string, List<int>>(false, res.Item2, null);
            }
            return dresult;
        }
        /// <summary>
        /// 根据转入信息获取匹配的所有FaceToken
        /// </summary>
        /// <param name="outer_id"></param>
        /// <param name="face_token"></param>
        /// <param name="image_url"></param>
        /// <param name="img"></param>
        /// <returns></returns>
        public static Tuple<bool,string, List<string>> SearchFaceToken(string outer_id, string face_token = null, string image_url = null, Stream img = null, int retry = 20)
        {
            var listfas = new List<string>();
            var files = new Dictionary<string, byte[]>();
            if (img != null)
            {
                var filebytes = StreamHelper.StreamToBytes(img);
                files.Add("image_file", filebytes);
            }

            var pars = new Dictionary<string, string>();

            pars.Add("api_key", Api_Key);
            pars.Add("api_secret", Api_Secret);
            pars.Add("outer_id", outer_id);
            pars.Add("face_token", face_token);
            pars.Add("image_url", image_url);
            pars.Add("return_result_count", "3");

            var result = HttpRequestHelper.PostFormData<dynamic>(Url_Search, files, pars);
            string strerror = result.error_message;

            if (strerror.IsNotNullAndEmpty())
            {
                if (strerror.Contains("CONCURRENCY_LIMIT_EXCEEDED") && retry-- > 0)
                {
                    return SearchFaceToken(outer_id, face_token, image_url, img, retry);
                }
                return new Tuple<bool, string, List<string>>(false, strerror, listfas);
            }
            else if (result.results != null)
            {
                foreach (var item in result.results)
                {
                    float confid = item.confidence;
                    if (confid >= defaultConfidence)
                    {
                        string facetoken = item.face_token;
                        listfas.Add(facetoken);
                    }
                }
                if (listfas.Count > 0)
                {
                    return new Tuple<bool, string, List<string>>(true, "匹配通过",listfas);
                }
                return new Tuple<bool, string, List<string>>(false, "匹配未通过", listfas);
            }
            else
            {
                return new Tuple<bool, string, List<string>>(false, "匹配未通过", listfas);
            }
        }
    }
}
