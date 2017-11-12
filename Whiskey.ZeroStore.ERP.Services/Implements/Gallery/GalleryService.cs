




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
using Whiskey.Web.Extensions;
using Whiskey.Utility;
using Whiskey.Utility.Data;
using Whiskey.Utility.Web;
using Whiskey.Utility.Class;
using Whiskey.Utility.Extensions;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{

    public class GalleryService : ServiceBase, IGalleryContract
    {
        #region GalleryService

		private readonly IRepository<Gallery, int> _galleryRepository;

        private readonly IRepository<GalleryAttribute, int> _galleryAttributeRepository;

        private readonly IRepository<Color, int> _colourRepository;

		public GalleryService(
			IRepository<Gallery, int> galleryRepository,
            IRepository<GalleryAttribute, int> galleryAttributeRepository,
            IRepository<Color, int> colourRepository
		): base(galleryRepository.UnitOfWork)
		{
			_galleryRepository = galleryRepository;
            _galleryAttributeRepository = galleryAttributeRepository;
            _colourRepository = colourRepository;

		}


        /// <summary>
        /// 获取单个数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		public Gallery View(int Id){
			var entity=_galleryRepository.GetByKey(Id);
            return entity;
		}


        /// <summary>
        /// 获取单个DTO数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		public GalleryDto Edit(int Id){
			var entity=_galleryRepository.GetByKey(Id);
            Mapper.CreateMap<Gallery, GalleryDto>();
            var dto = Mapper.Map<Gallery, GalleryDto>(entity);
            dto.Attributes = entity.GalleryAttributes.Select(m => m.Id).ToList().ExpandAndToString();
            dto.Colours = entity.Colors.Select(c => c.Id).ToList().ExpandAndToString();
            var colourHtml="";
            foreach (var colour in entity.Colors) {
                //colourHtml += "<li data-id='" + colour.Id + "' class='selected'><a href='javascript:void(0);' style='background-color:" + colour.RGB + ";' title='颜色：" + colour.ColorName + "，" + Math.Round(colour.MaxHue * 100) + "Deg，纯度：" + Math.Round(colour.MaxSaturation * 100) + "%,明度：" + Math.Round(colour.MaxLightness * 100) + "%'></a><i></i></li>";
                colourHtml += "<li data-id='" + colour.Id + "' class='selected'><a href='javascript:void(0);'  title='颜色：" + colour.ColorName + "'></a><i></i></li>";
            }
            dto.ColoursHtml = colourHtml;
            return dto;
		}


        /// <summary>
        /// 获取数据集
        /// </summary>
        public IQueryable<Gallery> Gallerys { get { return _galleryRepository.Entities; } }



        /// <summary>
        /// 按条件检查数据是否存在
        /// </summary>
        /// <param name="predicate">检查谓语表达式</param>
        /// <param name="id">更新的编号</param>
        /// <returns>是否存在</returns>
        public bool CheckExists(Expression<Func<Gallery, bool>> predicate, int id = 0)
        {
            return _galleryRepository.ExistsCheck(predicate, id);
        }



        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dtos">要添加的DTO数据</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Insert(params GalleryDto[] dtos)
        {
            try
            {
				dtos.CheckNotNull("dtos");
				OperationResult result = _galleryRepository.Insert(dtos,
				dto =>
				{
				},
				(dto, entity) =>
				{
                    if (dto.OriginalPath != null && dto.OriginalPath.Length > 0)
                    {
                        //var largeSavePath = EnvironmentHelper.ProductPath + "/g_0" + DateTime.Now.ToString("HHmmssffff", System.Globalization.DateTimeFormatInfo.InvariantInfo) + ".jpg";
                        //ImageHelper.MakeThumbnail(dto.OriginalPath, largeSavePath, 670, 1000, "W", "Jpg");

                        var thumbnailPath = EnvironmentHelper.GalleryPath + "/g_1" + DateTime.Now.ToString("HHmmssffff", System.Globalization.DateTimeFormatInfo.InvariantInfo) + ".jpg";
                        ImageHelper.MakeThumbnail(dto.OriginalPath, thumbnailPath, 204, 325, "W", "Jpg");
                        entity.ThumbnailPath = thumbnailPath;
                        //entity.OriginalPath = largeSavePath;
                    }
                    var attributeNewIds = (dto.Attributes != null && dto.Attributes.Length>0) ? dto.Attributes.Split(',').ToList() : new List<string>();
                    if (attributeNewIds.Count > 0)
                    {
                        var categories = _galleryAttributeRepository.Entities.Where(m => attributeNewIds.Contains(m.Id.ToString()));
                        foreach (var category in categories)
                        {
                            entity.GalleryAttributes.Add(category);
                        }
                    }

                    var colourNewIds = (dto.Colours != null && dto.Colours.Length > 0) ? dto.Colours.Split(',').ToList() : new List<string>();
                    if (colourNewIds.Count > 0)
                    {
                        var colours = _colourRepository.Entities.Where(m => colourNewIds.Contains(m.Id.ToString()));
                        foreach (var colour in colours)
                        {
                            entity.Colors.Add(colour);
                        }
                    }

					entity.CreatedTime = DateTime.Now;
					entity.OperatorId=AuthorityHelper.OperatorId;
					return entity;
				});
				return result;
            }catch (Exception ex){
                return new OperationResult(OperationResultType.Error, "添加失败！错误如下：" + ex.Message,ex.ToString());
            }
        }



		/// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="dtos">包含更新数据的DTO数据</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Update(params GalleryDto[] dtos)
        {
            try
            {
				dtos.CheckNotNull("dtos");
				OperationResult result = _galleryRepository.Update(dtos,
					dto =>
					{

					},
					(dto, entity) => {
                        if (dto.OriginalPath != null && dto.OriginalPath.Length > 0)
                        {
                            //var largeSavePath = EnvironmentHelper.ProductPath + "/g_0" + DateTime.Now.ToString("HHmmssffff", System.Globalization.DateTimeFormatInfo.InvariantInfo) + ".jpg";
                            //ImageHelper.MakeThumbnail(dto.OriginalPath, largeSavePath, 670, 1000, "W", "Jpg");

                            var thumbnailPath = EnvironmentHelper.GalleryPath + "/g_1" + DateTime.Now.ToString("HHmmssffff", System.Globalization.DateTimeFormatInfo.InvariantInfo) + ".jpg";
                            ImageHelper.MakeThumbnail(dto.OriginalPath, thumbnailPath, 204, 325, "W", "Jpg");

                            entity.ThumbnailPath = thumbnailPath;
                            //entity.OriginalPath = largeSavePath;
                        }
                        var attributeNewIds = (dto.Attributes != null && dto.Attributes.Length>0) ? dto.Attributes.Split(',').ToList() : new List<string>();
                        var attributeOldIds = (entity.GalleryAttributes != null && entity.GalleryAttributes.Select(m => m.Id).Count() > 0) ? entity.GalleryAttributes.Select(m => m.Id.ToString()).ToList() : new List<string>();
                        if (attributeNewIds.Count > 0)
                        {
                            var exceptAttributeIds = attributeOldIds.Intersect(attributeNewIds, StringComparer.OrdinalIgnoreCase).ToList();
                            var createAttributeIds = attributeNewIds.Except(exceptAttributeIds, StringComparer.OrdinalIgnoreCase).ToList();
                            var deleteAttributeIds = attributeOldIds.Except(exceptAttributeIds, StringComparer.OrdinalIgnoreCase).ToList();
                            var insertAttributeEntities = _galleryAttributeRepository.Entities.Where(m => createAttributeIds.Contains(m.Id.ToString()));
                            
                            var deleteAttributeEntities = entity.GalleryAttributes;
                            foreach (var removeId in deleteAttributeIds)
                            {
                                if (removeId.Length > 0)
                                {
                                    entity.GalleryAttributes.Remove(deleteAttributeEntities.FirstOrDefault(m => m.Id.ToString() == removeId));
                                }
                            }

                            foreach (var attribute in insertAttributeEntities) { 
                                entity.GalleryAttributes.Add(attribute);
                            }

                        }


                        var colourNewIds = (dto.Colours != null && dto.Colours.Length>0) ? dto.Colours.Split(',').ToList() : new List<string>();
                        var colourOldIds = (entity.Colors != null && entity.Colors.Select(m => m.Id).Count() > 0) ? entity.Colors.Select(m => m.Id.ToString()).ToList() : new List<string>();
                        if (colourNewIds.Count > 0)
                        {
                            var exceptColorIds = colourOldIds.Intersect(colourNewIds, StringComparer.OrdinalIgnoreCase).ToList();
                            var createColorIds = colourNewIds.Except(exceptColorIds, StringComparer.OrdinalIgnoreCase).ToList();
                            var deleteColorIds = colourOldIds.Except(exceptColorIds, StringComparer.OrdinalIgnoreCase).ToList();
                            var insertColorEntities = _colourRepository.Entities.Where(m => createColorIds.Contains(m.Id.ToString()));

                            var deleteColorEntities = entity.Colors;
                            foreach (var removeId in deleteColorIds)
                            {
                                if (removeId.Length > 0)
                                {
                                    entity.Colors.Remove(deleteColorEntities.FirstOrDefault(m => m.Id.ToString() == removeId));
                                }
                            }

                            foreach (var colour in insertColorEntities) { 
                                entity.Colors.Add(colour);
                            }
                        }

						entity.UpdatedTime = DateTime.Now;
                        entity.OperatorId = AuthorityHelper.OperatorId;
						return entity;
					});
				return result;
            }catch (Exception ex){
                return new OperationResult(OperationResultType.Error, "更新失败！错误如下：" + ex.Message,ex.ToString());
            }
        }



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
				var entities = _galleryRepository.Entities.Where(m => ids.Contains(m.Id));
				foreach (var entity in entities) {
					entity.IsDeleted = true;
					entity.UpdatedTime = DateTime.Now;
					entity.OperatorId=AuthorityHelper.OperatorId;
					_galleryRepository.Update(entity);
				}
				return UnitOfWork.SaveChanges() > 0? new OperationResult(OperationResultType.Success, "移除成功！"): new OperationResult(OperationResultType.NoChanged,"数据没有变化！");
            }catch (Exception ex){
                return new OperationResult(OperationResultType.Error, "移除失败！错误如下：" + ex.Message,ex.ToString());
            }
        }


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
				var entities = _galleryRepository.Entities.Where(m => ids.Contains(m.Id));
				foreach (var entity in entities) {
					entity.IsDeleted = false;
					entity.UpdatedTime = DateTime.Now;
					entity.OperatorId=AuthorityHelper.OperatorId;
					_galleryRepository.Update(entity);
				}
				return UnitOfWork.SaveChanges() > 0? new OperationResult(OperationResultType.Success, "恢复成功！"): new OperationResult(OperationResultType.NoChanged,"数据没有变化！");
            }catch (Exception ex){
                return new OperationResult(OperationResultType.Error, "恢复失败！错误如下：" + ex.Message,ex.ToString());
            }
        }


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

                UnitOfWork.TransactionEnabled = true;
                var entities = _galleryRepository.Entities.Where(m => ids.Contains(m.Id));
                var images = new List<ProductImage>();
                foreach (var entity in entities)
                {
                    entity.GalleryAttributes.Clear();
                    if (entity.OriginalPath != null)
                    {
                        FileHelper.Delete(entity.OriginalPath);
                        FileHelper.Delete(entity.ThumbnailPath);
                    }
                }
                _galleryRepository.Delete(ids);
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "删除成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }catch (Exception ex){
                return new OperationResult(OperationResultType.Error, "删除失败！错误如下：" + ex.Message,ex.ToString());
            }

        }


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
				var entities = _galleryRepository.Entities.Where(m => ids.Contains(m.Id));
				foreach (var entity in entities) {
					entity.IsEnabled = true;
					entity.UpdatedTime = DateTime.Now;
					entity.OperatorId=AuthorityHelper.OperatorId;
					_galleryRepository.Update(entity);
				}
				return UnitOfWork.SaveChanges() > 0? new OperationResult(OperationResultType.Success, "启用成功！"): new OperationResult(OperationResultType.NoChanged,"数据没有变化！");
            }catch (Exception ex){
                return new OperationResult(OperationResultType.Error, "启用失败！错误如下：" + ex.Message,ex.ToString());
            }
		}


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
				var entities = _galleryRepository.Entities.Where(m => ids.Contains(m.Id));
				foreach (var entity in entities) {
					entity.IsEnabled = false;
					entity.UpdatedTime = DateTime.Now;
					entity.OperatorId=AuthorityHelper.OperatorId;
					_galleryRepository.Update(entity);
				}
				return UnitOfWork.SaveChanges() > 0? new OperationResult(OperationResultType.Success, "禁用成功！"): new OperationResult(OperationResultType.NoChanged,"数据没有变化！");
            }catch (Exception ex){
                return new OperationResult(OperationResultType.Error, "禁用失败！错误如下：" + ex.Message,ex.ToString());
            }
		}




        #endregion
    }
}
