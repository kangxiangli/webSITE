using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using AutoMapper;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Web.Helper;
using Whiskey.Web.SignalR;
using Whiskey.Web.Http;
using Whiskey.Web.Extensions;
using Whiskey.Utility;
using Whiskey.Utility.Helper;
using Whiskey.Utility.Data;
using Whiskey.Utility.Web;
using Whiskey.Utility.Class;
using Whiskey.Utility.Extensions;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using System.Text;
using System.IO;
using Whiskey.ZeroStore.ERP.Transfers.APIEntities.MemberCollo;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Product;
using Whiskey.ZeroStore.ERP.Models.Enums;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    /// <summary>
    /// 会员搭配图片业务层
    /// </summary>
    public class MemberColloEleService : ServiceBase, IMemberColloEleContract
    {
        #region 初始化数据层操作对象
        //日志记录
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(MemberColloEleService));

        private readonly IRepository<MemberColloEle, int> _memberColloEleRepository;
        public MemberColloEleService(IRepository<MemberColloEle, int> memberColloEleRepository)
            : base(memberColloEleRepository.UnitOfWork)
        {
            _memberColloEleRepository = memberColloEleRepository;
        }
        #endregion

        #region 获取单个数据
        /// <summary>
        /// 获取单个数据
        /// </summary>
        /// <param name="Id">主键ID</param>
        /// <returns></returns>
        public MemberColloEle View(int Id)
        {
            MemberColloEle memberColloEle = _memberColloEleRepository.GetByKey(Id);
            return memberColloEle;
        }
       
        /// <summary>
        /// 获取单个DTO数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public MemberColloEleDto Edit(int Id)
        {
            var entity = _memberColloEleRepository.GetByKey(Id);
            Mapper.CreateMap<MemberColloEle, MemberColloEleDto>();
            var dto = Mapper.Map<MemberColloEle, MemberColloEleDto>(entity);
            return dto;
        }
        #endregion

        #region 获取数据集

        /// <summary>
        /// 获取数据集
        /// </summary>
        public IQueryable<MemberColloEle> MemberColloEles { get { return _memberColloEleRepository.Entities; } }

        #endregion

        #region 按条件检查数据是否存在
        /// <summary>
        /// 按条件检查数据是否存在
        /// </summary>
        /// <param name="predicate">检查谓语表达式</param>
        /// <param name="id">更新的编号</param>
        /// <returns>是否存在</returns>
        public bool CheckExists(Expression<Func<MemberColloEle, bool>> predicate, int id = 0)
        {
            return _memberColloEleRepository.ExistsCheck(predicate, id);
        }
        #endregion

        #region 添加数据

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dtos">要添加的DTO数据</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Insert(params MemberColloEleDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                OperationResult result = _memberColloEleRepository.Insert(dtos,
                dto =>
                {

                },
                (dto, entity) =>
                {
                    entity.CreatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    List<MemberColloEle> list = new List<MemberColloEle>();
                    foreach (var item in dto.MemberColloEleDtos)
                    {
                       Mapper.CreateMap<MemberColloEleDto,MemberColloEle>();
                       var dto1 = Mapper.Map<MemberColloEleDto, MemberColloEle>(item);
                        list.Add(dto1);
                    }
                    entity.Children = list;
                    return entity;
                });
                return result;
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "添加失败！错误如下：" + ex.Message);
            }
        }

        /// <summary>
        /// 添加搭配元素
        /// </summary>
        /// <param name="MemberColloId"></param>
        /// <param name="Image"></param>
        /// <param name="listColloImage"></param>
        /// <param name="listText"></param>
        /// <returns></returns>
        public OperationResult Insert(int MemberColloId, string Image, List<ImageList> listColloImage, List<TextList> listText)
        {
            try
            {
                //保存路径
                string configPath = ConfigurationHelper.GetAppSetting("SaveMemberCollocation");
                string strDate = DateTime.Now.Year + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day + "/";
                StringBuilder sbPath = new StringBuilder();
                StringBuilder sbSaveFileName = new StringBuilder();
                MemberColloEleDto MemberColloEleDto = new MemberColloEleDto();
                //文件唯一名称
                Guid gid = Guid.NewGuid();
                string fileName = gid.ToString() + ".png";
                string savePath = configPath + strDate + fileName;
                var saveResult = SaveImage(Image, savePath);
                if (saveResult.ResultType == OperationResultType.Success)
                {
                    MemberColloEleDto.ImagePath = savePath;
                    MemberColloEleDto.MemberColloId = MemberColloId;
                    MemberColloEleDto.EleType = (int)MemberColloEleFlag.ImageEle;
                    if (listColloImage != null && listColloImage.Count > 0)
                    {
                        List<MemberColloEleDto> listMemberColloEleDto = new List<MemberColloEleDto>();                        
                        foreach (var item in listColloImage)
                        {
                            MemberColloEleDto mDto = new MemberColloEleDto()
                            {
                                EleInfo = item.Frame,
                                SpinInfo = item.Spin,
                                Level = item.Level,
                                EleType = MemberColloEleFlag.ImageEle,
                                MemberColloId = MemberColloId,                                 
                                ProductId = item.ProductId,
                                ProductSource = item.ProductSource,
                                ProductType = item.ProductType
                            };                             
                            if (item.ProductSource == ProductSourceFlag.UploadProduct)
                            {
                                //文件唯一名称
                                Guid guid = Guid.NewGuid();
                                string imageName = gid.ToString() + ".png";
                                string path = configPath + strDate + fileName;
                                var saveImageResult = SaveImage(item.Image, path);
                                if (saveResult.ResultType != OperationResultType.Success)
                                {
                                    FileHelper.Delete(savePath);
                                    return new OperationResult(OperationResultType.Error, "上传图片失败");
                                }
                                else
                                {
                                    mDto.ImagePath = path;
                                }                               
                                listMemberColloEleDto.Add(mDto); 
                            }
                            
                        }
                        if (listText != null && listText.Count() > 0)
                        {
                            foreach (var item in listText)
                            {
                                MemberColloEleDto eleDto = new MemberColloEleDto();
                                eleDto.TextColor = item.Color;
                                eleDto.TextFont = item.FontSize;
                                eleDto.TextInfo = item.Text;
                                eleDto.EleInfo = item.Frame;
                                eleDto.SpinInfo = item.Spin;
                                eleDto.EleType = MemberColloEleFlag.TextEle;
                                eleDto.MemberColloId = MemberColloId;
                                listMemberColloEleDto.Add(eleDto);
                            }
                        }
                        MemberColloEleDto.MemberColloEleDtos = listMemberColloEleDto;
                        var insertResult = Insert(MemberColloEleDto);
                        if (insertResult.ResultType !=OperationResultType.Success)
                        {
                            FileHelper.Delete(MemberColloEleDto.ImagePath);
                            foreach (var item in listMemberColloEleDto)
                            {
                                if (!string.IsNullOrEmpty(item.ImagePath))
                                {
                                    FileHelper.Delete(item.ImagePath);    
                                }                                
                            }
                        }
                        return insertResult;                        
                    }
                    else
                    {
                        return new OperationResult(OperationResultType.Error, "请选择要上传的图片");
                    }
                }
                else
                {
                    return saveResult;
                }
            }
            catch (Exception ex)
            {
                _Logger.Error(ex.ToString());
                return new OperationResult(OperationResultType.Error, "程序异常");
            }

        }



        #region 注释代码
        ///// <summary>
        ///// 添加数据
        ///// </summary>
        ///// <param name="MemberColloId"></param>
        ///// <param name="ImageListInfo"></param>
        ///// <param name="ImageList"></param>
        ///// <param name="ColloImage"></param>
        ///// <returns></returns>
        //public  OperationResult Insert(int MemberColloId, ColloImage[] ImageListInfo, HttpPostedFileBase[] ImageList, HttpPostedFileBase ColloImage)
        //{
        //    string configPath = ConfigurationHelper.GetAppSetting("SaveMemberCollocation");
        //    string strDate = DateTime.Now.Year + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day + "/";
        //    StringBuilder sbPath = new StringBuilder();
        //    StringBuilder sbSaveFileName = new StringBuilder();
        //    long maxSize = 3500480;
        //    long minSize = 300000;
        //    string pngType = "image/png";
        //    bool resultType = true;
        //    string suffix = ".png";
        //    MemberColloEleDto MemberColloEleDto = new MemberColloEleDto();
        //    if (ColloImage!=null)
        //    {
        //        if (ColloImage.InputStream.Length > maxSize) return new OperationResult(OperationResultType.Error, "上传图片过大");
        //        Guid gid = Guid.NewGuid();
        //        string fileName = gid.ToString().Substring(0, 15) + suffix;
        //        string savePath = configPath + strDate + fileName;
        //        string coverImageType = ColloImage.ContentType;
        //        if (pngType == coverImageType)
        //        {
        //            double ratio = Math.Round((double)minSize / ColloImage.InputStream.Length, 2);
        //            ratio = ratio >= 1 ? 1 : ratio;
        //            resultType = ImageHelper.PercentImage(ColloImage.InputStream, ratio, savePath);
        //            if (!resultType) return new OperationResult(OperationResultType.Error, "上传图片失败");
        //        }
        //        else
        //        {
        //            int flag = 100;
        //            flag = ColloImage.InputStream.Length > minSize ? 40 : flag;
        //            resultType = ImageHelper.GetPicThumbnail(ColloImage.InputStream, savePath, flag);
        //            if (!resultType) return new OperationResult(OperationResultType.Error, "上传图片失败");
        //        }                
        //        if (resultType)
        //        {
        //            MemberColloEleDto.ImagePath = savePath;
        //            MemberColloEleDto.MemberColloId = MemberColloId;                    
        //        }
        //        else
        //        {
        //            return new OperationResult(OperationResultType.Error, "保存图片失败，请重新上传");
        //        }
        //    }
        //    Dictionary<string, MemberColloEleDto> dicMemberCollo = new Dictionary<string, MemberColloEleDto>();
        //    if (ImageListInfo != null && ImageListInfo.Length > 0)
        //    {
        //        for (int i = 0; i < ImageListInfo.Length; i++)
        //        {
        //            dicMemberCollo.Add(ImageListInfo[i].FileName, new MemberColloEleDto()
        //            {
        //                Level = ImageListInfo[i].Level,
        //                XAxis = ImageListInfo[i].XAxis,
        //                YAxis = ImageListInfo[i].YAxis,
        //                SpinAngle = ImageListInfo[i].SpinAngle,
        //                Heigh = ImageListInfo[i].Heigh,
        //                Width = ImageListInfo[i].Width,
        //                SpinDirection = ImageListInfo[i].SpinDirection,
        //                MemberColloId = MemberColloId
        //            });
        //        }
        //    }
        //    if (ImageList != null)
        //    {                
        //        List<MemberColloEleDto> listMemberColloEleDto = new List<MemberColloEleDto>();
        //        for (int i = 0; i < ImageList.Length; i++)
        //        {
        //            if (ImageList[i].InputStream.Length > maxSize) return new OperationResult(OperationResultType.Error, "上传图片过大");
        //            string key = ImageList[i].FileName;
        //            Guid gid = Guid.NewGuid();
        //            sbSaveFileName.Append(gid.ToString().Substring(0, 15) + suffix);
        //            sbPath.Append(configPath + strDate + sbSaveFileName);

        //            string imageType = ImageList[i].ContentType;
        //            if (pngType == imageType)
        //            {
        //                double ratio = Math.Round((double)minSize / ImageList[i].InputStream.Length, 2);
        //                ratio = ratio >= 1 ? 1 : ratio;
        //                resultType = ImageHelper.PercentImage(ImageList[i].InputStream, ratio, sbPath.ToString());
        //                if (!resultType) return new OperationResult(OperationResultType.Error, "上传图片失败");
        //            }
        //            else
        //            {
        //                int flag = 100;
        //                flag = ImageList[i].InputStream.Length > minSize ? 40 : flag;
        //                resultType = ImageHelper.GetPicThumbnail(ImageList[i].InputStream, sbPath.ToString(), flag);
        //                if (!resultType) return new OperationResult(OperationResultType.Error, "上传图片失败");
        //            }
        //            if (resultType)
        //            {
        //                dicMemberCollo[key].ImagePath = sbPath.ToString();
        //                dicMemberCollo[key].MemberColloId = MemberColloId;
        //                listMemberColloEleDto.Add(dicMemberCollo[key]);
        //            }
        //            else
        //            {
        //                return new OperationResult(OperationResultType.Error, "上传图片失败，请稍后重试");
        //            }
        //            sbSaveFileName.Clear();
        //            sbPath.Clear();
        //        }
        //        MemberColloEleDto.MemberColloEleDtos = listMemberColloEleDto;
        //        var result=Insert(MemberColloEleDto);
        //        return result;
        //    }
        //    else
        //    {
        //        return new OperationResult(OperationResultType.Error, "请选择要上传的图片");
        //    }
        //}

        #endregion

        #region 注释-添加搭配元素
        /// <summary>
        /// 添加搭配元素
        /// </summary>
        /// <param name="MemberColloId"></param>
        /// <param name="Image"></param>
        /// <param name="listColloImage"></param>
        /// <param name="listText"></param>
        /// <returns></returns>
        //public OperationResult Insert(int MemberColloId, string Image, List<ImageList> listColloImage,List<TextList> listText)
        //{
        //    try
        //    {
        //        //保存路径
        //        string configPath = ConfigurationHelper.GetAppSetting("SaveMemberCollocation");
        //        string strDate = DateTime.Now.Year + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day + "/";
        //        StringBuilder sbPath = new StringBuilder();
        //        StringBuilder sbSaveFileName = new StringBuilder();
        //        MemberColloEleDto MemberColloEleDto = new MemberColloEleDto();
        //        //文件唯一名称
        //        Guid gid = Guid.NewGuid();
        //        string fileName = gid.ToString() + ".png";
        //        string savePath = configPath + strDate + fileName;
        //        var saveResult = SaveImage(Image, savePath);
        //        if (saveResult.ResultType == OperationResultType.Success)
        //        {
        //            MemberColloEleDto.ImagePath = savePath;
        //            MemberColloEleDto.MemberColloId = MemberColloId;
        //            MemberColloEleDto.EleType = (int)MemberColloEleType.ImageEle;
        //            if (listColloImage != null && listColloImage.Count > 0)
        //            {
        //                List<MemberColloEleDto> listMemberColloEleDto = new List<MemberColloEleDto>();
        //                //是否出错
        //                bool isError = false;
        //                for (int i = 0; i < listColloImage.Count; i++)
        //                {
        //                    Guid guid = Guid.NewGuid();
        //                    sbSaveFileName.Append(guid.ToString().Substring(0, 15) + ".png");
        //                    sbPath.Append(configPath + strDate + sbSaveFileName);
        //                    var result = SaveImage(listColloImage[i].Image, sbPath.ToString());
        //                    if (result.ResultType == OperationResultType.Success)
        //                    {
        //                        MemberColloEleDto mDto = new MemberColloEleDto()
        //                        {
        //                            EleInfo = listColloImage[i].Frame,
        //                            SpinInfo = listColloImage[i].Spin,
        //                            Level = listColloImage[i].Level,
        //                            EleType = (int)MemberColloEleType.ImageEle,
        //                            MemberColloId = MemberColloId,
        //                            ImagePath = sbPath.ToString()
        //                        };
        //                        listMemberColloEleDto.Add(mDto);
        //                        sbSaveFileName.Clear();
        //                        sbPath.Clear();
        //                    }
        //                    else
        //                    {
        //                        //图片保存失败时跳出循环
        //                        isError = true;
        //                        break;
        //                    }

        //                }
        //                //图片保存失败，删除已经上传的；否侧，写入数据库
        //                if (isError)
        //                {
        //                    if (listMemberColloEleDto.Count > 0)
        //                    {
        //                        foreach (var item in listMemberColloEleDto)
        //                        {
        //                            if (!string.IsNullOrEmpty(item.ImagePath))
        //                            {
        //                                FileHelper.Delete(item.ImagePath);
        //                            }
        //                        }
        //                    }
        //                    return new OperationResult(OperationResultType.Error, "上传失败！"); ;
        //                }
        //                else
        //                {
        //                    if (listText != null && listText.Count() > 0)
        //                    {
        //                        foreach (var item in listText)
        //                        {
        //                            MemberColloEleDto eleDto = new MemberColloEleDto();
        //                            eleDto.TextColor = item.Color;
        //                            eleDto.TextFont = item.FontSize;
        //                            eleDto.TextInfo = item.Text;
        //                            eleDto.EleInfo = item.Frame;
        //                            eleDto.SpinInfo = item.Spin;
        //                            eleDto.EleType = (int)MemberColloEleType.TextEle;
        //                            eleDto.MemberColloId = MemberColloId;
        //                            listMemberColloEleDto.Add(eleDto);
        //                        }
        //                    }
        //                    MemberColloEleDto.MemberColloEleDtos = listMemberColloEleDto;
        //                    var insertResult = Insert(MemberColloEleDto);
        //                    if (insertResult.ResultType != OperationResultType.Success)
        //                    {
        //                        if (listMemberColloEleDto.Count > 0)
        //                        {
        //                            foreach (var item in listMemberColloEleDto)
        //                            {
        //                                if (!string.IsNullOrEmpty(item.ImagePath))
        //                                {
        //                                    FileHelper.Delete(item.ImagePath);
        //                                }
        //                            }
        //                        }
        //                    }
        //                    return insertResult;
        //                }
        //            }
        //            else
        //            {
        //                return new OperationResult(OperationResultType.Error, "请选择要上传的图片");
        //            }
        //        }
        //        else
        //        {
        //            return saveResult;
        //        }   
        //    }
        //    catch (Exception ex)
        //    {
        //        _Logger.Error(ex.ToString());
        //        return new OperationResult(OperationResultType.Error, "程序异常");                
        //    }
                     
        //}
        #endregion

        /// <summary>
        /// 保存上传图片
        /// </summary>
        /// <param name="image"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public OperationResult SaveImage(string image, string path)
        {
            try
            {
                //最大保存字节
                long maxSize = 3500480;
                if (string.IsNullOrEmpty(image))
                {
                    return new OperationResult(OperationResultType.Error, "请添加需要上传的图片");
                }
                else
                {
                    byte[] bufferCollo = Convert.FromBase64String(image);
                    MemoryStream msCollo = new MemoryStream(bufferCollo);
                    if (msCollo.Length > maxSize)
                    {
                        msCollo.Dispose();//释放IO资源
                        return new OperationResult(OperationResultType.Error, "上传图片过大！");
                    }
                    else
                    {
                        //string saveResult = ImageHelper.MakeThumbnail(msCollo, path, 670, 1000, "W", "Png");
                        bool saveResult = FileHelper.SaveUpload(msCollo, path);
                        //释放IO资源
                        msCollo.Dispose();
                        if (!saveResult)
                        {
                            return new OperationResult(OperationResultType.Error, "上传图片失败！");
                        }
                        else
                        {
                            return new OperationResult(OperationResultType.Success, "上传成功！");
                        }
                    }                    
                }
                
            }
            catch (Exception ex)
            {
                _Logger.Error(ex.ToString());
                return new OperationResult(OperationResultType.Error, "服务器忙，请稍后重试！");
            }
            
        }

        #endregion

        #region 更新数据
        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="dtos">包含更新数据的DTO数据</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Update(params MemberColloEleDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                OperationResult result = _memberColloEleRepository.Update(dtos,
                    dto =>
                    {

                    },
                    (dto, entity) =>
                    {
                        entity.UpdatedTime = DateTime.Now;
                        entity.OperatorId = AuthorityHelper.OperatorId;
                        return entity;
                    });
                return result;
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "更新失败！错误如下：" + ex.Message);
            }
        }
        #endregion

        #region 移除数据
        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="ids">要移除的编号</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Remove(params int[] ids)
        {
            try
            {                
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _memberColloEleRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _memberColloEleRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "移除成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "移除失败！错误如下：" + ex.Message);
            }
        }
        #endregion

        #region 恢复数据
        /// <summary>
        /// 恢复数据
        /// </summary>
        /// <param name="ids">要恢复的编号</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Recovery(params int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _memberColloEleRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _memberColloEleRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "恢复成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "恢复失败！错误如下：" + ex.Message);
            }
        }
        #endregion

        #region 删除数据
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="ids">要删除的编号</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Delete(params int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                OperationResult result = _memberColloEleRepository.Delete(ids);
                return result;
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "删除失败！错误如下：" + ex.Message);
            }

        }

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="MemberColloIds">搭配ID</param>
        /// <returns></returns>
        public OperationResult DeleteByMemberColloId(params int[] MemberColloIds)
        {
            IQueryable<MemberColloEle> listColloImage = MemberColloEles.Where(x => MemberColloIds.Contains(x.MemberColloId??0));
            if (listColloImage != null)
            {
                foreach (var colloImage in listColloImage)
                {
                    if (!string.IsNullOrEmpty(colloImage.ImagePath))
                    {
                        bool isDel = FileHelper.Delete(colloImage.ImagePath);
                        if (!isDel)
                        {
                            return new OperationResult(OperationResultType.Error, "删除失败");
                        }
                    }
                }
                var ids = listColloImage.Select(x => x.Id).ToList();
                var result = _memberColloEleRepository.Delete(listColloImage);
                if (result>0)
                {
                    return new OperationResult(OperationResultType.Success, "删除成功");
                }
                else
                {
                    return new OperationResult(OperationResultType.Error, "删除失败");    
                }
            }
            else
            {
                return new OperationResult(OperationResultType.NoChanged, "没有任何操作");
            }
        }

        #endregion

        #region 启用数据
        /// <summary>
        /// 启用数据
        /// </summary>
        /// <param name="ids">要启用的编号</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Enable(params int[] ids)
        {

            try
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _memberColloEleRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _memberColloEleRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "启用成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "启用失败！错误如下：" + ex.Message);
            }
        }
        #endregion

        #region 禁用数据
        /// <summary>
        /// 禁用数据
        /// </summary>
        /// <param name="ids">要禁用的编号</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Disable(params int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _memberColloEleRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _memberColloEleRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "禁用成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "禁用失败！错误如下：" + ex.Message);
            }
        }
        #endregion

        public void Update(List<MemberColloEle> listEle)
        {
            _memberColloEleRepository.Update(listEle);
        }
    }
}
