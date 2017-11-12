using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Data;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models.DTO;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
namespace Whiskey.ZeroStore.ERP.Website.Areas.DataStat.Controllers
{
    public class PartnerController : Controller
    {
        private readonly IPartnerStatisticsContract _statService;

        public PartnerController(IPartnerStatisticsContract statService)
        {
            _statService = statService;
        }
        [Layout]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult QueryByYear(ReqDTO req)
        {
            if (!req.Year.HasValue || req.Month.HasValue)
            {
                return Json(OperationResult.Error("参数错误"));
            }

            var statMonthes = new List<Tuple<DateTime, DateTime>>();
            DateTime monthStart = DateTime.Now;
            DateTime monthEnd = DateTime.Now;

            for (int i = 1; i <= 12; i++)
            {
                // 2017-04-01
                monthStart = new DateTime(year: req.Year.Value, month: i, day: 1);

                // 2017-5-01-00:00:01  =>   2017-04-30 23:59:59
                monthEnd = monthStart.AddMonths(1).AddSeconds(-1);

                statMonthes.Add(Tuple.Create(monthStart, monthEnd));
            }


            var startTime = statMonthes.First().Item1;
            var endTime = statMonthes.Last().Item2;
            var dataQuery = _statService.Entities.Where(s => !s.IsDeleted && s.IsEnabled)
                                                 .Where(s => s.CreatedTime >= startTime && s.CreatedTime <= endTime)
                                                 .ToList()
                                                 //.Select(s => new { s.PartnerCount, s.OrderCount, s.OrderMoney, s.SaleCount, s.SaleMoney, s.MemberCount,s.CreatedTime })
                                                 .GroupBy(s => s.CreatedTime.Month)
                                                 .ToList();


            if (!dataQuery.Any())
            {
                return Json(OperationResult.Error("没有数据"), JsonRequestBehavior.AllowGet);
            }

            List<ResDTO> monthlyData = new List<ResDTO>();
            //按月份统计出当月数据
            for (int i = 0; i < statMonthes.Count; i++)
            {
                var monthItem = statMonthes[i];

                var groupItem = dataQuery.FirstOrDefault(g => g.Key == monthItem.Item1.Month);
                if (groupItem == null)
                {
                    // 无信息
                    monthlyData.Add(new ResDTO()
                    {
                        IsAccumulate = req.IsAccumulate,
                        Year = monthItem.Item1.Year,
                        Month = monthItem.Item1.Month,
                        BeginTime = monthItem.Item1.ToString("yyyy-MM-dd HH:mm:ss"),
                        EndTime = monthItem.Item2.ToString("yyyy-MM-dd HH:mm:ss")
                    });
                }
                else
                {
                    var statData = new ResDTO
                    {
                        IsAccumulate = req.IsAccumulate,
                        Year = monthItem.Item1.Year,
                        Month = monthItem.Item1.Month,
                        BeginTime = monthItem.Item1.ToString("yyyy-MM-dd HH:mm:ss"),
                        EndTime = monthItem.Item2.ToString("yyyy-MM-dd HH:mm:ss"),
                        PartnerCount = groupItem.Sum(p => p.PartnerCount),
                        MemberCount = groupItem.Sum(p => p.MemberCount),
                        OrderCount = groupItem.Sum(p => p.OrderCount),
                        OrderMoney = groupItem.Sum(p => (decimal)p.OrderMoney),
                        SaleCount = groupItem.Sum(p => p.SaleCount),
                        SaleMoney = groupItem.Sum(p => (decimal)p.SaleMoney),
                    };
                    monthlyData.Add(statData);
                }
            }



            if (!req.IsAccumulate)
            {
                //非累加模式，返回每月的数据

                return Json(new OperationResult(OperationResultType.Success, string.Empty, monthlyData), JsonRequestBehavior.AllowGet);

            }
            else //累加模式
            {
                var orderedMonthData = monthlyData.OrderBy(r => r.Month).ToArray();
                var accumalatedData = orderedMonthData.Select(i =>
                {
                    var query = orderedMonthData.Where(r => r.Month <= i.Month);
                    return new ResDTO()
                    {
                        IsAccumulate = i.IsAccumulate,
                        BeginTime = i.BeginTime,
                        EndTime = i.EndTime,
                        Year = i.Year,
                        Month = i.Month,
                        MemberCount = query.Sum(q => q.MemberCount),
                        OrderCount = query.Sum(q => q.OrderCount),
                        OrderMoney = query.Sum(q => q.OrderMoney),
                        PartnerCount = query.Sum(q => q.PartnerCount),
                        SaleCount = query.Sum(q => q.SaleCount),
                        SaleMoney = query.Sum(q => q.SaleMoney)
                    };
                }).ToList();
                return Json(new OperationResult(OperationResultType.Success, string.Empty, accumalatedData), JsonRequestBehavior.AllowGet);

            }
        }

        public ActionResult QueryByMonth(ReqDTO req)
        {
            if (!req.Year.HasValue || !req.Month.HasValue)
            {
                return Json(OperationResult.Error("没有数据"), JsonRequestBehavior.AllowGet);

            }


            var statDays = new List<Tuple<DateTime, DateTime>>();
            var daysInMonth = DateTime.DaysInMonth(req.Year.Value, req.Month.Value);
            DateTime dayStart = DateTime.Now;
            DateTime dayEnd = DateTime.Now;

            for (int i = 1; i <= daysInMonth; i++)
            {
                // 2017-04-01
                dayStart = new DateTime(year: req.Year.Value, month: req.Month.Value, day: i);

                // 2017-04-02 - 00:00:01  =>   2017-04-01 23:59:59
                dayEnd = dayStart.AddDays(1).AddSeconds(-1);

                statDays.Add(Tuple.Create(dayStart, dayEnd));
            }
            var startTime = statDays.First().Item1;
            var endTime = statDays.Last().Item2;
            var dataQuery = _statService.Entities.Where(s => !s.IsDeleted && s.IsEnabled)
                                                 .Where(s => s.CreatedTime >= startTime && s.CreatedTime <= endTime)
                                                 .ToList()
                                                 //.Select(s => new { s.PartnerCount, s.OrderCount, s.OrderMoney, s.SaleCount, s.SaleMoney, s.MemberCount,s.CreatedTime })
                                                 .GroupBy(s => s.CreatedTime.Day)
                                                 .ToList();


            if (!dataQuery.Any())
            {
                return Json(OperationResult.Error("没有数据"), JsonRequestBehavior.AllowGet);
            }

            List<ResDTO> dailyData = new List<ResDTO>();
            //按月份统计出当月数据
            for (int i = 0; i < statDays.Count; i++)
            {
                var dayItem = statDays[i];

                var groupItem = dataQuery.FirstOrDefault(g => g.Key == dayItem.Item1.Day);
                if (groupItem == null)
                {
                    // 无信息
                    dailyData.Add(new ResDTO()
                    {
                        IsAccumulate = req.IsAccumulate,
                        Year = dayItem.Item1.Year,
                        Month = dayItem.Item1.Month,
                        Day = dayItem.Item1.Day,
                        BeginTime = dayItem.Item1.ToString("yyyy-MM-dd HH:mm:ss"),
                        EndTime = dayItem.Item2.ToString("yyyy-MM-dd HH:mm:ss")
                    });
                }
                else
                {
                    var statData = new ResDTO
                    {
                        IsAccumulate = req.IsAccumulate,
                        Year = dayItem.Item1.Year,
                        Month = dayItem.Item1.Month,
                        Day = dayItem.Item1.Day,
                        BeginTime = dayItem.Item1.ToString("yyyy-MM-dd HH:mm:ss"),
                        EndTime = dayItem.Item2.ToString("yyyy-MM-dd HH:mm:ss"),
                        PartnerCount = groupItem.Sum(p => p.PartnerCount),
                        MemberCount = groupItem.Sum(p => p.MemberCount),
                        OrderCount = groupItem.Sum(p => p.OrderCount),
                        OrderMoney = groupItem.Sum(p => (decimal)p.OrderMoney),
                        SaleCount = groupItem.Sum(p => p.SaleCount),
                        SaleMoney = groupItem.Sum(p => (decimal)p.SaleMoney),
                    };
                    dailyData.Add(statData);
                }
            }



            if (!req.IsAccumulate)
            {
                //非累加模式，返回每月的数据

                return Json(new OperationResult(OperationResultType.Success, string.Empty, dailyData), JsonRequestBehavior.AllowGet);

            }
            else //累加模式
            {
                var orderedMonthData = dailyData.OrderBy(r => r.Month).ToArray();
                var accumalatedData = orderedMonthData.Select(i =>
                {
                    var query = orderedMonthData.Where(r => r.Day <= i.Day);
                    return new ResDTO()
                    {
                        IsAccumulate = i.IsAccumulate,
                        BeginTime = i.BeginTime,
                        EndTime = i.EndTime,
                        Year = i.Year,
                        Month = i.Month,
                        Day = i.Day,
                        MemberCount = query.Sum(q => q.MemberCount),
                        OrderCount = query.Sum(q => q.OrderCount),
                        OrderMoney = query.Sum(q => q.OrderMoney),
                        PartnerCount = query.Sum(q => q.PartnerCount),
                        SaleCount = query.Sum(q => q.SaleCount),
                        SaleMoney = query.Sum(q => q.SaleMoney)
                    };
                }).ToList();
                return Json(new OperationResult(OperationResultType.Success, string.Empty, accumalatedData), JsonRequestBehavior.AllowGet);

            }
        }

        public class ReqDTO
        {
            public ReqDTO()
            {
                IsAccumulate = false;
            }
            public int? Year { get; set; }
            public int? Month { get; set; }

            /// <summary>
            /// 是否累计
            /// </summary>
            public bool IsAccumulate { get; set; }
        }

        public class ResDTO
        {
            public int Year { get; set; }
            public int Month { get; set; }
            public int? Day { get; set; }
            public bool IsAccumulate { get; set; }
            public string BeginTime { get; set; }
            public string EndTime { get; set; }
            public int PartnerCount { get; set; }
            public int OrderCount { get; set; }
            public decimal OrderMoney { get; set; }
            public int SaleCount { get; set; }
            public decimal SaleMoney { get; set; }

            public int MemberCount { get; set; }

        }
    }
}