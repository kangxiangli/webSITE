using System.IO;
using System.Web.Mvc;
using Whiskey.Utility.Class;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.MobileApi.Controllers;
using Whiskey.ZeroStore.MobileApi.Extensions.Attribute;

// GET: Api/Intelligents/Face
namespace Whiskey.ZeroStore.MobileApi.Areas.Intelligents.Controllers
{
    [License(CheckMode.Verify)]
    public class FaceController : BaseController
    {
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(FaceController));
        protected readonly IMemberContract _MemberContract;
        protected readonly IMemberFaceContract _MemberFaceContract;

        public FaceController(
            IMemberContract _MemberContract,
            IMemberFaceContract _MemberFaceContract
            )
        {
            this._MemberContract = _MemberContract;
            this._MemberFaceContract = _MemberFaceContract;
        }

        /// <summary>
        /// 获取文件流中第一张的图片
        /// </summary>
        /// <returns></returns>
        private Stream GetFileStream()
        {
            var Files = Request.Files;
            Stream ImgStream = null;
            if (Files != null && Files.Count > 0)
            {
                var file = Files[0];
                ImgStream = file.InputStream;
            }
            return ImgStream;
        }

        /// <summary>
        /// 添加会员人脸图像
        /// </summary>
        public JsonResult AddFace(int MemberId, string imgUrl)
        {
            var imgStream = GetFileStream();
            var result = _MemberFaceContract.AddFace(MemberId, imgUrl, imgStream);

            return Json(result);
        }

        /// <summary>
        /// 移除会员人脸图像
        /// </summary>
        /// <param name="MemberId"></param>
        /// <param name="FaceToken"></param>
        /// <returns></returns>
        public JsonResult RemoveFace(int MemberId, string FaceToken)
        {
            var result = _MemberFaceContract.RemoveFace(MemberId, FaceToken);
            return Json(result);
        }

        /// <summary>
        /// 移除会员人脸图像
        /// </summary>
        /// <param name="MemberId"></param>
        /// <param name="FaceToken"></param>
        /// <returns></returns>
        public JsonResult RemoveFaceAll(int MemberId)
        {
            var result = _MemberFaceContract.RemoveFaceAll(MemberId);
            return Json(result);
        }

        /// <summary>
        /// 会员脸部图像对比
        /// </summary>
        /// <param name="MemberId"></param>
        /// <param name="imgUrl"></param>
        /// <returns></returns>
        public JsonResult CompareFace(int MemberId, string imgUrl)
        {
            var imgStream = GetFileStream();
            var result = _MemberFaceContract.CompareFace(MemberId, imgUrl, imgStream);
            return Json(result);
        }

        /// <summary>
        /// 获取会员人脸图像
        /// </summary>
        /// <param name="MemberId"></param>
        /// <param name="Count"></param>
        /// <returns></returns>
        public JsonResult GetFace(int MemberId,int Count=3)
        {
            var result = _MemberFaceContract.GetFace(MemberId, Count);
            return Json(result, JsonRequestBehavior.AllowGet);

        }
    }
}