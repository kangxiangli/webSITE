using System;
using System.Collections.Generic;
using System.Data.Entity.SqlServer;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;

// GET: FaceTest
namespace Whiskey.ZeroStore.MobileApi.Controllers
{
    public class FaceTestController : BaseController
    {
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(FaceTestController));

        protected readonly IMemberFaceContract _MemberFaceContract;
        protected readonly IMemberContract _MemberContract;
        protected readonly IMemberFigureContract _MemberFigureContract;

        public FaceTestController(
            IMemberFaceContract _MemberFaceContract,
            IMemberContract _MemberContract,
            IMemberFigureContract _MemberFigureContract
            )
        {
            this._MemberFaceContract = _MemberFaceContract;
            this._MemberContract = _MemberContract;
            this._MemberFigureContract = _MemberFigureContract;
        }

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

        public ActionResult Index()
        {
            return View();
        }

        public JsonResult SearchMemberIds(int storeId, string imgUrl)
        {
            var imgStream = GetFileStream();
            var result = _MemberFaceContract.SearchMemberIds(storeId, imgUrl, imgStream);
            if (result.ResultType == Utility.Data.OperationResultType.Success)
            {
                var memberIds = result.Data as List<int>;
                var data = (from s in _MemberContract.Members.Where(w => w.IsEnabled && !w.IsDeleted).Where(w => memberIds.Contains(w.Id))
                            let f = _MemberFigureContract.MemberFigures.Where(w => w.IsEnabled && !w.IsDeleted && w.MemberId == s.Id).OrderByDescending(o => o.Id)
                            select new
                            {
                                s.MemberName,
                                Gender = (GenderFlag_CN)s.Gender,
                                s.MobilePhone,
                                s.CardNumber,
                                MemberType = s.MemberType.MemberTypeName,
                                s.Store.StoreName,
                                DateofBirth = s.DateofBirth,
                                UserPhoto = s.UserPhoto != null ? WebUrl + s.UserPhoto : s.UserPhoto,
                                MemberFigure = f.Select(ss => new
                                {
                                    ss.ApparelSize,
                                    Birthday = ss.Birthday,
                                    ss.PreferenceColor,
                                    ss.Bust,
                                    ss.FigureDes,
                                    ss.FigureType,
                                    ss.Gender,
                                    ss.Height,
                                    ss.Hips,
                                    ss.Shoulder,
                                    ss.Waistline,
                                    ss.Weight
                                }).FirstOrDefault()

                            }).ToList();
                result.Data = data;
            }
            return Json(result);
        }
    }
}