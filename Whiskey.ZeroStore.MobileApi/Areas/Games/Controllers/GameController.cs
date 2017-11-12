using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Class;
using Whiskey.Utility.Data;
using Whiskey.Utility.Extensions;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Models.Enums.Members;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.MobileApi.Controllers;
using Whiskey.ZeroStore.MobileApi.Extensions.Attribute;

namespace Whiskey.ZeroStore.MobileApi.Areas.Games.Controllers
{
    // GET: Games/Game
    [License(CheckMode.Verify)]
    public class GameController : BaseController
    {
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(GameController));

        protected readonly IGameContract gameContract;
        protected readonly IMemberShareContract _memberShareContract;
        public GameController(
            IGameContract gameContract,
            IMemberShareContract _memberShareContract
            )
        {
            this.gameContract = gameContract;
            this._memberShareContract = _memberShareContract;
        }

        public JsonResult AddScore(int MemberId, string Tag, decimal Score)
        {
            var data = gameContract.AddScore(MemberId, Tag, Score);
            return Json(data);
        }

        public JsonResult Share(int MemberId, string Tag, ShareFlag Flag = ShareFlag.游戏)
        {
            var data = gameContract.Share(MemberId, Tag, Flag);
            return Json(data);
        }

        public JsonResult PlayRandom(int MemberId, string Tag)
        {
            var data = gameContract.PlayRandom(MemberId, Tag);
            return Json(data);
        }

        public JsonResult GetAwardInfo(string Tag, int Count = 3)
        {
            var data = gameContract.GetRandomAward(Tag, Count);
            return Json(data);
        }

        /// <summary>
        /// 获取游戏信息
        /// </summary>
        /// <param name="Tag"></param>
        /// <param name="MemberId"></param>
        /// <returns></returns>
        public JsonResult GetGameInfo(string Tag, int MemberId, bool onlyCount = false)
        {
            var result = new OperationResult(OperationResultType.Error);
            var mod = gameContract.Entities.FirstOrDefault(f => f.IsEnabled && !f.IsDeleted && f.Tag == Tag);
            if (mod.IsNull())
            {
                result.Message = "游戏不存在";
            }
            else
            {
                var sharecount = mod.MemberShares.Count(c => c.IsEnabled && !c.IsDeleted && c.MemberId == MemberId);
                var sharedaycount = mod.MemberShares.Count(c => c.IsEnabled && !c.IsDeleted && c.MemberId == MemberId && c.CreatedTime >= DateNowDate);

                result.ResultType = OperationResultType.Success;

                var PlayedCount = mod.GameRecords.Count(c => c.IsEnabled && !c.IsDeleted && c.MemberId == MemberId);
                var PlayedDayCount = mod.GameRecords.Count(c => c.IsEnabled && !c.IsDeleted && c.MemberId == MemberId && c.CreatedTime >= DateNowDate);
                var LimitCount = mod.LimitCount > 0 ? (mod.LimitCount + sharecount - PlayedCount) + "" : "无限制";
                var LimitDayCount = mod.LimitDayCount + sharedaycount - PlayedDayCount;

                if (onlyCount)
                {
                    result.Data = new
                    {
                        LimitCount = LimitCount,
                        LimitDayCount = LimitDayCount,
                        PlayedCount = PlayedCount,
                        PlayedDayCount = PlayedDayCount
                    };
                }
                else
                {
                    result.Data = new
                    {
                        mod.Name,
                        StartTime = mod.StartTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        EndTime = mod.EndTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "长期有效",
                        mod.Introduce,
                        LimitCount = LimitCount,
                        LimitDayCount = LimitDayCount,
                        PlayedCount = PlayedCount,
                        PlayedDayCount = PlayedDayCount
                    };
                }
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}