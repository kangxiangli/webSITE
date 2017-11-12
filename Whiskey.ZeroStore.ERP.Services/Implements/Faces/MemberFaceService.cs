using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Utility;
using Whiskey.Utility.Data;
using Whiskey.Utility.Extensions;
using Whiskey.Utility.Helper;
using Whiskey.Utility.Logging;
using Whiskey.Web.FacePlus;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    public class MemberFaceService : ServiceBase, IMemberFaceContract
    {
        string MemberFacePhotoPath = ConfigurationHelper.GetAppSetting("SaveMemberFacePhoto", "/Content/Images/MemberFace/");

        protected readonly ILogger _Logger = LogManager.GetLogger(typeof(MemberFaceService));

        private readonly IRepository<MemberFace, int> _MemberFaceRepository;
        private readonly IRepository<Member, int> _MemberRepository;
        public MemberFaceService(
            IRepository<MemberFace, int> _MemberFaceRepository,
            IRepository<Member, int> _MemberRepository
            ) : base(_MemberFaceRepository.UnitOfWork)
        {
            this._MemberFaceRepository = _MemberFaceRepository;
            this._MemberRepository = _MemberRepository;
        }

        public IQueryable<MemberFace> Entities
        {
            get
            {
                return _MemberFaceRepository.Entities;
            }
        }

        public OperationResult DeleteOrRecovery(bool delete, params int[] ids)
        {
            return OperationHelper.Try((opera) =>
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _MemberFaceRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = delete;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                }
                return OperationHelper.ReturnOperationResult(UnitOfWork.SaveChanges() > 0, opera);
            }, delete ? Operation.Delete : Operation.Recovery);
        }

        public OperationResult EnableOrDisable(bool enable, params int[] ids)
        {
            return OperationHelper.Try((opera) =>
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _MemberFaceRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = enable;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                }
                return OperationHelper.ReturnOperationResult(UnitOfWork.SaveChanges() > 0, opera);
            }, enable ? Operation.Enable : Operation.Disable);
        }

        public OperationResult Insert(params MemberFace[] entities)
        {
            return OperationHelper.Try((opera) =>
            {
                entities.CheckNotNull("entities");
                OperationResult result = _MemberFaceRepository.Insert(entities,
                entity =>
                {
                    entity.CreatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                });
                return result;
            }, Operation.Add);
        }

        public OperationResult Update(params MemberFace[] entities)
        {
            return OperationHelper.Try((opera) =>
            {
                entities.CheckNotNull("entities");
                UnitOfWork.TransactionEnabled = true;
                OperationResult result = _MemberFaceRepository.Update(entities,
                entity =>
                {
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                });
                int count = UnitOfWork.SaveChanges();
                return OperationHelper.ReturnOperationResult(count > 0, opera);
            }, Operation.Update);
        }

        public MemberFace View(int Id)
        {
            return _MemberFaceRepository.GetByKey(Id);
        }

        /// <summary>
        /// 获取会员所属店铺Id
        /// </summary>
        /// <param name="MemberId"></param>
        /// <param name="orginMember">来源表,true会员表,false人脸表</param>
        /// <returns></returns>
        private int? GetOut_Id(int MemberId, bool orginMember = false)
        {
            if (orginMember)
            {
                return _MemberRepository.Entities.Where(w => w.Id == MemberId).Select(s => s.StoreId).FirstOrDefault();
            }

            var storeid = _MemberFaceRepository.Entities.Where(w => w.MemberId == MemberId && w.IsEnabled && !w.IsDeleted).OrderByDescending(o => o.CreatedTime).Select(s => s.StoreId).FirstOrDefault();
            if (storeid == 0)
            {
                return null;
            }
            return storeid;
        }

        public OperationResult AddFace(int MemberId, string imgUrl, Stream imgStream = null)
        {
            var dresult = new OperationResult(OperationResultType.Error, "加入识别库失败");

            var result = FaceHelper.Detect(imgUrl, imgStream);

            if (result.Item1)
            {
                var outer_id = GetOut_Id(MemberId, true);
                if (!outer_id.HasValue)
                {
                    dresult.Message = "会员没有归属店铺";
                    return dresult;
                }
                var strouter_id = outer_id.Value + "";
                var hasStore = Entities.Any(a => a.IsEnabled && !a.IsDeleted && a.StoreId == outer_id.Value);
                var result2 = new Tuple<bool, string>(false, string.Empty);
                if (!hasStore)
                {
                    result2 = FaceHelper.FaceSetCreate(strouter_id, result.Item2);
                }
                else
                {
                    #region 判断有没有超过允许的最大值
                    var querym = Entities.Where(w => w.IsEnabled && !w.IsDeleted && w.MemberId == MemberId);
                    if (querym.Count() >= 5)
                    {
                        var mod = querym.OrderBy(o => o.CreatedTime).FirstOrDefault();
                        var strfacetokens = mod.FaceToken;

                        result2 = FaceHelper.FaceSetRemoveFace(mod.StoreId + "", strfacetokens);
                        if (result2.Item1)
                        {
                            var res = DeleteOrRecovery(true, mod.Id);
                            if (res.ResultType == OperationResultType.Success)
                            {
                                //foreach (var item in list.Select(s => s.ImgPath))
                                //{
                                //    FileHelper.Delete(item);
                                //}

                                result2 = FaceHelper.FaceSetAddFace(strouter_id, result.Item2);
                            }
                            else
                            {
                                result2 = new Tuple<bool, string>(false, res.Message);
                            }
                        }
                    }
                    else
                    {
                        result2 = FaceHelper.FaceSetAddFace(strouter_id, result.Item2);
                    }

                    #endregion
                }

                if (result2.Item1)
                {
                    var filepath = $"{MemberFacePhotoPath}{DateTime.Now.ToString("yyyy/MM/dd")}/{MemberId}_{result.Item2}.jpg";

                    var mod = new MemberFace();
                    mod.ImgPath = imgUrl;
                    mod.MemberId = MemberId;
                    mod.StoreId = outer_id.Value;
                    mod.FaceToken = result.Item2;

                    if (imgStream != null && FileHelper.SaveUpload(imgStream, filepath))
                    {
                        mod.ImgPath = filepath;
                    }
                    dresult = Insert(mod);
                }
                else
                {
                    dresult.Message = result2.Item2;
                }
            }
            else
            {
                dresult.Message = result.Item2;
            }
            return dresult;
        }

        public OperationResult CompareFace(int MemberId, string imgUrl, Stream imgStream = null)
        {
            var dresult = new OperationResult(OperationResultType.Error, "匹配失败");
            var outer_id = GetOut_Id(MemberId);
            if (!outer_id.HasValue)
            {
                dresult.Message = "会员没有归属店铺";
                return dresult;
            }
            var res = FaceHelper.Search(outer_id + "", null, imgUrl, imgStream);
            if (res.Item1)
            {
                dresult.ResultType = OperationResultType.Success;
                dresult.Message = "匹配成功";
            }
            else
            {
                dresult.Message = res.Item2;
            }
            return dresult;
        }

        public OperationResult RemoveFace(int MemberId, string FaceToken)
        {
            var dresult = new OperationResult(OperationResultType.Error);

            var mod = Entities.Where(w => w.IsEnabled && !w.IsDeleted && w.MemberId == MemberId && w.FaceToken == FaceToken).FirstOrDefault();
            if (mod.IsNotNull())
            {
                var res = FaceHelper.FaceSetRemoveFace(mod.StoreId + "", mod.FaceToken);
                if (res.Item1)
                {
                    dresult = DeleteOrRecovery(true, mod.Id);
                    if (dresult.ResultType == OperationResultType.Success)
                    {
                        #region 删除用户保存图像文件

                        //foreach (var item in list.Where(w => w.ImgPath != null && !w.ImgPath.StartsWith("http")).Select(s => s.ImgPath))
                        //{
                        //    FileHelper.Delete(item);
                        //}

                        #endregion
                    }
                }
                else
                {
                    dresult.Message = res.Item2;
                }
            }
            else
            {
                dresult.Message = "FaceToken不存在";
            }

            return dresult;
        }

        public OperationResult RemoveFaceAll(int MemberId)
        {
            var dresult = new OperationResult(OperationResultType.Success);
            var list = Entities.Where(w => w.IsEnabled && !w.IsDeleted && w.MemberId == MemberId).OrderBy(o => o.CreatedTime).Skip(0).Take(5).ToList();
            if (list.IsNotNullOrEmptyThis())
            {
                var res = new Tuple<bool, string>(false, "");
                foreach (var item in list.GroupBy(g => g.StoreId))
                {
                    res = FaceHelper.FaceSetRemoveFace(item.Key + "", item.Select(s => s.FaceToken).Aggregate((str, next) => { return $"{str},{next}"; }));
                    if (res.Item1)
                    {
                        var listitems = item.Select(s => s).ToList();
                        foreach (var it in listitems)
                        {
                            it.IsDeleted = true;
                            it.UpdatedTime = DateTime.Now;
                        }

                        dresult = _MemberFaceRepository.Update(listitems);
                        if (dresult.ResultType == OperationResultType.Success)
                        {
                            #region 删除用户保存图像文件

                            //foreach (var itemimg in listitems.Where(w => w.ImgPath != null && !w.ImgPath.StartsWith("http")).Select(s => s.ImgPath))
                            //{
                            //    FileHelper.Delete(itemimg);
                            //}

                            #endregion
                        }
                    }
                    else
                    {
                        dresult.ResultType = OperationResultType.Error;
                        dresult.Message = res.Item2;
                        break;
                    }
                }
            }
            
            return dresult;
        }

        public OperationResult GetFace(int MemberId, int Count = 3)
        {
            var list = Entities.Where(w => w.IsEnabled && !w.IsDeleted && w.MemberId == MemberId).OrderByDescending(o => o.CreatedTime).Skip(0).Take(Count).Select(s => new
            {
                s.FaceToken,
                ImgPath = s.ImgPath != null && !s.ImgPath.StartsWith("http") ? ApiUrl + s.ImgPath : s.ImgPath,
            }).ToList();
            return new OperationResult(OperationResultType.Success, "", list);
        }

        public OperationResult SearchMemberIds(int storeId, string imgUrl, Stream imgStream = null)
        {
            return OperationHelper.Try(() =>
            {
                var dresult = new OperationResult(OperationResultType.Error);
                string outer_id = storeId + "";

                var res = FaceHelper.Detect(imgUrl, imgStream);
                if (res.Item1)
                {
                    var result = FaceHelper.SearchFaceToken(outer_id, null, imgUrl, imgStream);
                    if (result.Item1)
                    {
                        var listf = result.Item3;
                        var listMemberIds = _MemberFaceRepository.Entities.Where(w => w.IsEnabled && !w.IsDeleted && listf.Contains(w.FaceToken)).Select(s => s.MemberId).Distinct().ToList();
                        if (listMemberIds != null && listMemberIds.Count > 0)
                        {
                            dresult.ResultType = OperationResultType.Success;
                            dresult.Data = listMemberIds;
                        }
                        else
                        {
                            dresult.Message = "未匹配到相关信息";
                        }
                    }
                    else
                    {
                        dresult.Message = result.Item2;
                    }
                }
                else
                {
                    dresult.Message = res.Item2;
                }

                return dresult;
            }, ex =>
            {
                return OperationHelper.ReturnOperationExceptionResult(ex, "匹配");
            });
        }

        public OperationResult MoveMemberToNewFaceSet(int MemberId, int? NewStoreId)
        {
            var dresult = new OperationResult(OperationResultType.Success);
            if (ConfigurationHelper.IsDevelopment()) { return dresult; }
            var listf = _MemberFaceRepository.Entities.Where(w => w.IsEnabled && !w.IsDeleted && w.MemberId == MemberId).OrderBy(o => o.CreatedTime).Skip(0).Take(5).ToList();
            if (listf != null && listf.Count > 0)
            {
                if (NewStoreId.HasValue)
                {
                    listf = listf.Where(w => w.StoreId != NewStoreId).ToList();
                    if (listf.IsNotNullOrEmptyThis())
                    {
                        var strtokens = listf.Select(s => s.FaceToken).Aggregate((str, next) => { return $"{str},{next}"; });
                        var res = FaceHelper.FaceSetCreate(NewStoreId + "", strtokens);
                        if (res.Item1)
                        {
                            foreach (var item in listf.GroupBy(g => g.StoreId))
                            {
                                var listfa = item.Select(s => s).ToList();
                                var strtokens2 = listfa.Select(s => s.FaceToken).Aggregate((str, next) => { return $"{str},{next}"; });
                                var res2 = FaceHelper.FaceSetRemoveFace(item.Key + "", strtokens2);

                                listfa.ForEach(f => { f.StoreId = NewStoreId.Value; f.UpdatedTime = DateTime.Now; });
                                _MemberFaceRepository.Update(listfa);
                            }
                        }
                        else
                        {
                            dresult.ResultType = OperationResultType.Error;
                            dresult.Message = res.Item2;
                        }
                    }
                }
                else
                {
                    foreach (var item in listf.GroupBy(g => g.StoreId))
                    {
                        var listfa = item.Select(s => s).ToList();
                        var strtokens2 = listfa.Select(s => s.FaceToken).Aggregate((str, next) => { return $"{str},{next}"; });
                        var res2 = FaceHelper.FaceSetRemoveFace(item.Key + "", strtokens2);
                        if (res2.Item1)
                        {
                            listfa.ForEach(f => { f.IsDeleted = true; f.UpdatedTime = DateTime.Now; });
                            _MemberFaceRepository.Update(listfa);
                        }
                    }
                }
            }
            return dresult;
        }

        public IQueryable<Member> SearchedMembers(int storeId, string imgUrl, Stream imgStream = null)
        {
            var result = SearchMemberIds(storeId, imgUrl, imgStream);
            if (result.ResultType == OperationResultType.Success)
            {
                var listids = result.Data as List<int>;
                return _MemberRepository.Entities.Where(w => w.IsEnabled && !w.IsDeleted && listids.Contains(w.Id));
            }
            return _MemberRepository.Entities.Where(w => w.Id == 0);
        }
    }
}
