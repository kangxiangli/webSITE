using AutoMapper;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Utility;
using Whiskey.Utility.Data;
using Whiskey.Utility.Extensions;
using Whiskey.Utility.Helper;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    public class AppointmentService : ServiceBase, IAppointmentContract
    {
        IRepository<Appointment, int> _repo;
        protected readonly IMemberContract _memberContract;
        protected readonly IProductContract _productContract;
        protected readonly IBrandContract _brandContract;
        protected readonly ISalesCampaignContract _salesCampaignContract;
        protected readonly ITemplateContract _templateContract;
        protected readonly ICollocationPlanContract _collocationPlanContract;
        private readonly IRepository<Storage, int> _storageRepository;
        private readonly IRepository<AppointmentPacking, int> _appointmentPackingRepository;
        private readonly IRepository<AppointmentFeedback, int> _feedbackRepo;
        private static object _syncObject = new object();
        public AppointmentService(IMemberContract memberContract,
            IProductContract productContract,
            IBrandContract brandContract,
            ISalesCampaignContract salesCampaignContract,
            ITemplateContract _templateContract,
            ICollocationPlanContract collocationPlanContract,
            IRepository<Storage, int> storageRepository,
            IRepository<AppointmentPacking, int> appointmentPackingRepository,
            IRepository<AppointmentFeedback, int> feedbackRepo,
            IRepository<Appointment, int> repo) : base(repo.UnitOfWork)
        {

            _memberContract = memberContract;
            _productContract = productContract;
            _brandContract = brandContract;
            _salesCampaignContract = salesCampaignContract;
            _repo = repo;
            this._templateContract = _templateContract;
            _collocationPlanContract = collocationPlanContract;
            _storageRepository = storageRepository;
            _appointmentPackingRepository = appointmentPackingRepository;
            _feedbackRepo = feedbackRepo;
        }
        public IQueryable<Appointment> Entities => _repo.Entities.Where(e => !e.IsDeleted && e.IsEnabled);



        public OperationResult Update(params Appointment[] entities)
        {
            var res = _repo.Update(entities, entity =>
            {
                entity.UpdatedTime = DateTime.Now;
                entity.OperatorId = AuthorityHelper.OperatorId;
            });
            return res;
        }

        public OperationResult UpdateState(Dictionary<int, string> ids)
        {
            var entities = _repo.Entities.Where(e => ids.Keys.Contains(e.Id)).ToList();
            entities.ForEach(e =>
            {
                e.State = AppointmentState.已处理;
                e.Notes = ids[e.Id] ?? String.Empty;
            });
            var res = _repo.Update(entities, entity =>
            {
                entity.UpdatedTime = DateTime.Now;
                entity.OperatorId = AuthorityHelper.OperatorId;
            });
            return res;
        }

        public OperationResult GetOptions()
        {
            var options = RedisCacheHelper.Get<AppointmentOption>("appointment:options");
            if (options == null)
            {
                throw new Exception("数据未找到");
            }
            return new OperationResult(OperationResultType.Success, string.Empty, options);
        }

        /// <summary>
        /// 生成预约号
        /// </summary>
        /// <returns></returns>
        private string GenerateNumber()
        {
            lock (_syncObject)
            {
                var number = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                Thread.Sleep(1);
                return number;
            }

        }

        /// <summary>
        /// 新增预约
        /// </summary>
        /// <param name="memberId">会员id</param>
        /// <param name="notes">会员留言</param>
        /// <param name="likeNumbers">中意的货号</param>
        /// <param name="dislikeNumbers">不中意的货号</param>
        /// <param name="checkOptions">预约项</param>
        /// <returns></returns>
        public OperationResult Add(int memberId, string notes, string[] likeNumbers, string[] dislikeNumbers, Dictionary<string, string> checkOptions)
        {
            if (likeNumbers == null || likeNumbers.Length <= 0)
            {
                return OperationResult.Error("请选择预约商品");
            }

            var options = RedisCacheHelper.Get<AppointmentOption>("appointment:options");
            if (checkOptions != null && checkOptions.Keys.Any())
            {
                foreach (var item in checkOptions)
                {
                    // key validate
                    if (string.IsNullOrEmpty(item.Key))
                    {
                        return OperationResult.Error("key参数不可为空");
                    }

                    if (!options.ContainsKey(item.Key))
                    {
                        return OperationResult.Error($"{item.Key}不存在");
                    }

                    // value validate
                    if (string.IsNullOrEmpty(item.Value))
                    {
                        return OperationResult.Error($"{item.Key}值不能为空");
                    }


                    var optionEntry = options[item.Key];
                    var arr = item.Value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    if (!optionEntry.Multiple && arr.Length != 1)
                    {
                        return OperationResult.Error($"{item.Key}值数目无效");
                    }
                    if (arr.Any(o => !optionEntry.Options.ContainsKey(o)))
                    {
                        return OperationResult.Error($"{item.Key},{item.Value}值无效");
                    }

                }
            }


            // 归属店铺校验
            var storeId = _memberContract.Members.Where(m => !m.IsDeleted && m.IsEnabled && m.Id == memberId).Select(m => m.StoreId).FirstOrDefault();
            if (!storeId.HasValue)
            {
                return new OperationResult((OperationResultType)1002, "未绑定归属店铺");
                //return OperationResult.Error("会员尚未绑定归属店铺,暂时无法预约");
            }

            // 校验是否有进行中的预约
            if (_repo.Entities.Any(e => !e.IsDeleted && e.IsEnabled && e.State == AppointmentState.预约中 && e.MemberId == memberId))
            {
                return OperationResult.Error("尚有未完成的预约,暂时无法进行新的预约");
            }


            // 批量预约时,多个货号存入一条预约信息中
            likeNumbers = likeNumbers.Distinct().ToArray();
            dislikeNumbers = dislikeNumbers.Distinct().ToArray();

            if (!likeNumbers.Any())
            {
                return OperationResult.Error("请选择要预约的商品");
            }

            // 校验中意商品货号
            var numberQuery = _productContract.Products.Where(p => !p.IsDeleted && p.IsEnabled).Select(p => p.ProductNumber);

            if (!likeNumbers.All(n => numberQuery.Any(number => number == n)))
            {
                return OperationResult.Error("中意商品中货号有误");
            }

            // 校验不中意商品货号
            if (dislikeNumbers != null && dislikeNumbers.Length > 0)
            {

                if (!dislikeNumbers.All(dislike => numberQuery.Any(number => number == dislike)))
                {
                    return OperationResult.Error("不中意商品中货号有误");
                }
            }

            var dtnow = DateTime.Now;
            var entitiesToAdd = new Appointment()
            {
                Number = GenerateNumber(),
                MemberId = memberId,
                StoreId = storeId.Value,
                ProductNumber = string.Join(",", likeNumbers),
                DislikeProductNumbers = string.Join(",", dislikeNumbers),
                State = AppointmentState.预约中,
                CreatedTime = dtnow,
                UpdatedTime = dtnow,
                Notes = notes,
            };


            if (checkOptions != null && checkOptions.Keys.Any())
            {
                entitiesToAdd.Quantity = checkOptions["Quantity"];
                entitiesToAdd.Budget = checkOptions["Budget"];
                entitiesToAdd.Top = checkOptions["Top"];
                entitiesToAdd.Bottom = checkOptions["Bottom"];
                entitiesToAdd.Jumpsuit = checkOptions["Jumpsuit"];
                entitiesToAdd.Situation = checkOptions["Situation"];
                entitiesToAdd.Style = checkOptions["Style"];
                entitiesToAdd.Fabric = checkOptions["Fabric"];
                entitiesToAdd.Color = checkOptions["Color"];
                entitiesToAdd.Season = checkOptions["Season"];
            }


            var count = _repo.Insert(entitiesToAdd);
            if (count <= 0)
            {
                return OperationResult.Error("插入失败");
            }

            OperationHelper.Try(() =>
            {
                var modt = _templateContract.GetNotificationTemplate(TemplateNotificationType.MemberAppointment);
                if (modt != null)
                {
                    var title = modt.TemplateName;
                    var modM = _memberContract.View(memberId);
                    var membername = modM.MemberName ?? modM.RealName;
                    var storename = modM.Store?.StoreName ?? string.Empty;
                    var storeaddress = modM.Store?.Address ?? string.Empty;
                    Dictionary<string, object> dic = new Dictionary<string, object>();
                    dic.Add("MemberName", membername);
                    dic.Add("MemberPhone", modM.MobilePhone);
                    dic.Add("AppointmentTime", dtnow);
                    dic.Add("StoreName", storename);
                    dic.Add("StoreAddress", storeaddress);
                    var content = NVelocityHelper.Generate(modt.TemplateHtml, dic);
                    _memberContract.SendAppNotification(title, content, JPushFlag.我的预约, memberId);
                }
            });

            return OperationResult.OK();
        }

        public Dictionary<string, object> GetItems(int memberId, int pageIndex = 1, int pageSize = 10, AppointmentState? stat = null)
        {
            var memberQuery = _memberContract.Members.Where(m => !m.IsDeleted && m.IsEnabled && m.Id == memberId);
            if (!memberQuery.Any())
            {
                throw new Exception("会员不存在");
            }
            var storeId = memberQuery.Select(m => m.StoreId).FirstOrDefault();
            if (!storeId.HasValue)
            {
                throw new Exception("会员尚未绑定归属店铺");
            }

            var query = _repo.Entities.Where(s => !s.IsDeleted && s.IsEnabled && s.MemberId == memberId);
            if (stat.HasValue)
            {
                query = query.Where(w => w.State == stat.Value);
            }
            var allCount = query.Count();
            var pendingCount = query.Count(s => s.State != AppointmentState.已预约);
            var finishCount = allCount - pendingCount;
            var list = query.OrderByDescending(o => o.UpdatedTime)
                            .Skip((pageIndex - 1) * pageSize)
                            .Take(pageSize)
                            .Select(s => new
                            {
                                s.SelectedPlanId,
                                s.SelectedPlan.CoverImg,
                                CollocationPlans = s.CollocationPlans.Select(p => new { p.Id, p.CoverImg }),
                                s.Number,
                                s.Id,
                                s.ProductNumber,
                                s.DislikeProductNumbers,
                                s.CreatedTime,
                                s.State,
                                s.Notes,
                                s.Store.StoreName,
                                s.Bottom,
                                s.Budget,
                                s.Color,
                                s.Fabric,
                                s.Jumpsuit,
                                s.Quantity,
                                s.Season,
                                s.Situation,
                                s.Style,
                                s.Top,

                            }).ToList()
                            .Select(s => new
                            {
                                CoverImg = s.CoverImg,
                                s.Number,
                                Id = s.Id,
                                DislikeProductNumbers = s.DislikeProductNumbers,
                                ProductNumber = s.ProductNumber,
                                CreatedTime = s.CreatedTime.ToUnixTime().ToString(),
                                State = s.State.ToString(),
                                Notes = s.Notes ?? string.Empty,
                                s.StoreName,
                                s.Bottom,
                                s.Budget,
                                s.Color,
                                s.Fabric,
                                s.Jumpsuit,
                                s.Quantity,
                                s.Season,
                                s.Situation,
                                s.Style,
                                s.Top,
                            }).ToList();
            return new Dictionary<string, object>
            {
                { "PageSize" , pageSize },
                { "AllCount",allCount},
                { "PendingCount",pendingCount},
                { "FinishCount",finishCount},
                { "ResultType",OperationResultType.Success},
                { "Message",string.Empty},
                {"Data",list }

            };



        }

        public OperationResult GetPlans(int id)
        {
            var entity = _repo.Entities.Include(a => a.CollocationPlans).FirstOrDefault(a => !a.IsDeleted && a.IsEnabled && a.Id == id);
            if (entity == null)
            {
                return OperationResult.Error("预约信息有误");
            }

            if (entity.State == AppointmentState.预约中)
            {
                return OperationResult.Error("预约尚未处理,请耐心等待预约结果");
            }

            var plans = entity.CollocationPlans.Where(c => !c.IsDeleted && c.IsEnabled).ToList()
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    CoverImg = c.CoverImg ?? string.Empty,
                    c.Desc,
                    c.Rules,
                    c.UseCount,
                    c.Tags,
                    c.SuggestionCount,
                    c.Suggestions,
                    c.RuleCount,
                }).ToList();

            // 已预约时返回已选择的方案
            if (entity.State == AppointmentState.已预约)
            {
                plans = plans.Where(p => p.Id == entity.SelectedPlanId.Value).ToList();
            }
            return new OperationResult(OperationResultType.Success, string.Empty, plans);
        }
        public OperationResult GetPlans(string number)
        {
            var entity = _repo.Entities.Include(a => a.CollocationPlans).FirstOrDefault(a => !a.IsDeleted && a.IsEnabled && a.Number == number);
            if (entity == null)
            {
                return OperationResult.Error("预约信息有误");
            }

            if (entity.State == AppointmentState.预约中)
            {
                return OperationResult.Error("预约尚未处理,请耐心等待预约结果");
            }

            var plans = entity.CollocationPlans.Where(c => !c.IsDeleted && c.IsEnabled).ToList()
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    CoverImg = c.CoverImg ?? string.Empty,
                    c.Desc,
                    c.Rules,
                    c.UseCount,
                    c.Tags,
                    c.SuggestionCount,
                    c.Suggestions,
                    c.RuleCount,
                }).ToList();
            // 已预约时返回已选择的方案
            if (entity.State >= AppointmentState.已预约)
            {
                plans = plans.Where(p => p.Id == entity.SelectedPlanId.Value).ToList();
            }
            var feedbackedNumbers = _feedbackRepo.Entities.Where(f => !f.IsDeleted && f.IsEnabled && f.AppointmentId == entity.Id).Select(f => f.ProductNumber).ToList();
            return new OperationResult(OperationResultType.Success, string.Empty, new { plans, feedbackedNumbers });
        }

        public OperationResult ConfirmPlans(int id, int planId, DateTime start, DateTime end)
        {
            using (var transaction = _repo.GetTransaction())
            {
                var entity = _repo.Entities.Include(a => a.CollocationPlans).FirstOrDefault(a => !a.IsDeleted && a.IsEnabled && a.Id == id);
                if (entity == null)
                {
                    return OperationResult.Error("预约信息有误");
                }

                if (entity.State == AppointmentState.预约中)
                {
                    return OperationResult.Error("预约尚未处理,请耐心等待预约结果");
                }
                else if (entity.State != AppointmentState.已处理)
                {
                    return OperationResult.Error("预约状态有误");
                }

                if (start < DateTime.Now || end < DateTime.Now)
                {
                    return OperationResult.Error("预约时间段必须超过当前时间");
                }

                if (end <= start)
                {
                    return OperationResult.Error("预约时间段结束时间必须超过开始时间");

                }
                if ((end - DateTime.Now.Date).TotalDays > 16)
                {
                    return OperationResult.Error("预约时间段超过了15天");

                }

                // 校验plan是否存在
                var plan = _collocationPlanContract.Entities.FirstOrDefault(c => !c.IsDeleted && c.IsEnabled && c.Id == planId);


                // 校验planid是否是推荐的planid
                var entityPlanIds = entity.CollocationPlans.Where(c => !c.IsDeleted && c.IsEnabled).Select(c => c.Id).ToList();
                if (!entityPlanIds.Contains(planId))
                {
                    return OperationResult.Error("提交的搭配方案与预约中的搭配方案不匹配");
                }

                // plan的使用次数加1
                plan.UseCount += 1;

                _collocationPlanContract.Update(plan);

                entity.SelectedPlanId = planId;
                entity.State = AppointmentState.已预约;
                entity.StartTime = start;
                entity.EndTime = end;

                var cnt = _repo.Update(entity);
                if (cnt <= 0)
                {
                    return OperationResult.Error("操作失败");
                }


                transaction.Commit();

                return OperationResult.OK();
            }

        }


        public int GetPackingId(int id)
        {
            var entity = _repo.Entities.Where(e => !e.IsDeleted && e.IsEnabled && e.Id == id).FirstOrDefault();
            if (entity == null)
            {
                throw new Exception("预约信息不存在");
            }
            if (entity.State < AppointmentState.已预约)
            {
                throw new Exception("预约状态有误");
            }
            if (entity.AppointmentPackingId.HasValue)
            {
                return entity.AppointmentPacking.Id;
            }
            else // 初始化
            {
                using (var transaction = _repo.GetTransaction())
                {
                    var plan = entity.SelectedPlan;
                    var rules = JsonHelper.FromJson<List<CollocationRulesEntry>>(plan.Rules);
                    var productNumbers = rules.Where(r => !string.IsNullOrEmpty(r.ProductNumber)).Select(r => r.ProductNumber).Distinct().ToList();
                    if (!productNumbers.Any())
                    {
                        throw new Exception("搭配方案内单品不能为空");
                    }


                    var storageEntry = _storageRepository.Entities.Where(x => !x.IsDeleted && x.IsEnabled && x.IsOrderStorage)
                                                            .Select(s => new
                                                            {
                                                                s.Id,
                                                                s.StoreId
                                                            }).FirstOrDefault();
                    if (storageEntry == null)
                    {
                        throw new Exception("采购仓库不存在");
                    }
                    var dict = _productContract.Products.Where(p => !p.IsDeleted && p.IsEnabled && productNumbers.Contains(p.ProductNumber))
                                                       .Select(p => new { p.Id, p.ProductNumber })
                                                       .ToDictionary(p => p.ProductNumber, p => p.Id);
                    var packing = new AppointmentPacking
                    {
                        FromStorageId = storageEntry.Id,
                        FromStoreId = storageEntry.StoreId,
                        State = AppointmentPackingState.装箱中,
                        ToStoreId = entity.StoreId,
                        AppointmentNumber = entity.Number
                    };

                    foreach (var item in dict)
                    {
                        packing.AppointmentPackingItem.Add(new AppointmentPackingItem
                        {
                            ProductNumber = item.Key,
                            ProductId = item.Value,
                        });
                    }
                    _appointmentPackingRepository.Insert(packing);
                    entity.AppointmentPackingId = packing.Id;
                    _repo.Update(entity);
                    transaction.Commit();
                    return packing.Id;
                }



            }




        }


        public async Task<OperationResult> GetLikes(string number)
        {
            if (string.IsNullOrEmpty(number))
            {
                throw new ArgumentException("商品货号不能为空", nameof(number));
            }

            var entity = _repo.Entities.FirstOrDefault(a => !a.IsDeleted && a.IsEnabled && a.Number == number);
            if (entity == null)
            {
                return OperationResult.Error("预约信息有误");
            }

            var likeNumbers = entity.ProductNumber;
            if (string.IsNullOrEmpty(likeNumbers))
            {
                return OperationResult.Error("中意单品为空");
            }
            var numberArr = likeNumbers.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var webUrl = ConfigurationHelper.WebUrl;
            var data = await _productContract.Products.Where(p => !p.IsDeleted && p.IsEnabled && numberArr.Contains(p.ProductNumber))
                .Select(p => new
                {
                    p.ProductNumber,
                    p.Color.ColorName,
                    p.Size.SizeName,
                    ProductCollocationImg = p.ProductCollocationImg ?? p.ProductOriginNumber.ProductCollocationImg
                }).ToListAsync();
            var data2 = data.Select(p => new
            {
                p.ProductNumber,
                p.ColorName,
                p.SizeName,
                ProductCollocationImg = string.IsNullOrEmpty(p.ProductCollocationImg) ? string.Empty : webUrl + p.ProductCollocationImg
            }).ToList();
            return new OperationResult(OperationResultType.Success, string.Empty, data2);
        }


        /// <summary>
        /// 取消预约
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public OperationResult Cancel(int id)
        {
            var entity = _repo.Entities.FirstOrDefault(a => !a.IsDeleted && a.IsEnabled && a.Id == id);
            if (entity == null)
            {
                return OperationResult.Error("预约信息有误");
            }
            if (entity.State == AppointmentState.已撤销)
            {
                return OperationResult.OK();
            }
            else if (entity.State == AppointmentState.已预约)
            {
                return OperationResult.Error("无法撤销已完成的预约");
            }

            entity.State = AppointmentState.已撤销;
            var res = _repo.Update(entity);
            if (res > 0)
            {
                return OperationResult.OK();
            }
            return OperationResult.Error("撤销失败");
        }

        public OperationResult RejectAllPlans(int id)
        {
            var entity = _repo.Entities.FirstOrDefault(a => !a.IsDeleted && a.IsEnabled && a.Id == id);
            if (entity == null)
            {
                return OperationResult.Error("预约信息有误");
            }

            if (entity.State == AppointmentState.预约中)
            {
                return OperationResult.Error("预约尚未处理,请耐心等待预约结果");
            }

            // 预约状态fallback
            entity.SelectedPlanId = null;
            entity.State = AppointmentState.预约中;
            var cnt = _repo.Update(entity);
            if (cnt <= 0)
            {
                return OperationResult.Error("操作失败");
            }
            return OperationResult.OK();
        }

        public OperationResult ClearItem(int memberId)
        {
            try
            {
                var items = _repo.Entities.Where(s => s.MemberId == memberId).ToList();
                if (items == null || items.Count <= 0)
                {
                    return OperationResult.OK();
                }

                _repo.Delete(items);
                return OperationResult.OK();
            }
            catch (Exception e)
            {
                return OperationResult.Error(e.Message);
            }

        }

        public OperationResult BatchInsert(IEnumerable<Appointment> entities)
        {
            _repo.InsertBulk(entities);
            return OperationResult.OK();
        }

        public async Task<OperationResult> GetBoxToAccept(int storeId, AppointmentState filter)
        {
            var data = await _repo.Entities.Where(a => !a.IsDeleted && a.IsEnabled && a.State == filter)
                .Select(a => new
                {
                    a.State,
                    CoverImg = a.SelectedPlan.CoverImg,
                    Store = a.Store.StoreName,
                    Member = a.Member.RealName,
                    AppointmentNumber = a.Number,
                    Quantity = a.AppointmentPacking.Orderblank.OrderblankItems.Count,
                    StartTime = a.StartTime.Value,
                    EndTime = a.EndTime.Value,
                    Detail = a.AppointmentPacking.Orderblank.OrderblankItems.Select(i => new
                    {
                        i.Product.ProductNumber,
                        Color = i.Product.Color.ColorName,
                        Size = i.Product.Size.SizeName,
                        Quantity = i.Quantity,
                        Barcode = i.OrderBlankBarcodes,
                        CollocationImg = i.Product.ProductCollocationImg ?? i.Product.ProductOriginNumber.ProductCollocationImg
                    })
                }).ToListAsync();
            var res = data.Select(a => new
            {
                State = a.State.ToString(),
                a.CoverImg,
                a.Store,
                a.Member,
                a.AppointmentNumber,
                a.Quantity,
                StartTime = a.StartTime.ToUnixTime(),
                EndTime = a.EndTime.ToUnixTime(),
                a.Detail
            });
            return new OperationResult(OperationResultType.Success, string.Empty, res);

        }

        public OperationResult QuickAdd(int memberId, DateTime start, DateTime end)
        {
            return OperationHelper.Try(() =>
            {
                var storeId = _memberContract.Members.Where(m => !m.IsDeleted && m.IsEnabled && m.Id == memberId).Select(m => m.StoreId).FirstOrDefault();
                if (!storeId.HasValue)
                {
                    return new OperationResult((OperationResultType)1002, "未绑定归属店铺");
                }
                if (_repo.Entities.Any(e => !e.IsDeleted && e.IsEnabled && e.State == AppointmentState.预约中 && e.MemberId == memberId && e.AppointmentType == AppointmentType.快速))
                {
                    return OperationResult.Error("尚有未完成的预约,暂时无法进行新的预约");
                }

                var dtnow = DateTime.Now;
                var entitiesToAdd = new Appointment()
                {
                    Number = GenerateNumber(),
                    MemberId = memberId,
                    StoreId = storeId.Value,
                    State = AppointmentState.预约中,
                    CreatedTime = dtnow,
                    UpdatedTime = dtnow,
                    StartTime = start,
                    EndTime = end,
                    AppointmentType = AppointmentType.快速,
                };
                var count = _repo.Insert(entitiesToAdd);
                if (count <= 0)
                {
                    return OperationResult.Error("预约未成功");
                }
                OperationHelper.Try(() =>
                {
                    var modt = _templateContract.GetNotificationTemplate(TemplateNotificationType.MemberAppointment);
                    if (modt != null)
                    {
                        var title = modt.TemplateName;
                        var modM = _memberContract.View(memberId);
                        var membername = modM.MemberName ?? modM.RealName;
                        var storename = modM.Store?.StoreName ?? string.Empty;
                        var storeaddress = modM.Store?.Address ?? string.Empty;
                        Dictionary<string, object> dic = new Dictionary<string, object>();
                        dic.Add("MemberName", membername);
                        dic.Add("MemberPhone", modM.MobilePhone);
                        dic.Add("AppointmentTime", $"{start.ToString("yyyy/MM/dd HH:mm")}~{end.ToString("yyyy/MM/dd HH:mm")}");
                        dic.Add("StoreName", storename);
                        dic.Add("StoreAddress", storeaddress);
                        var content = NVelocityHelper.Generate(modt.TemplateHtml, dic);
                        _memberContract.SendAppNotification(title, content, JPushFlag.我的预约, memberId);
                    }
                });
                return OperationHelper.ReturnOperationResult(true, "");

            }, "预约失败");
        }
    }
}

