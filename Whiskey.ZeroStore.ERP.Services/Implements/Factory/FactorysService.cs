using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Linq.Expressions;
using System.Web;
using System.Globalization;
using AutoMapper;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Web.Helper;
using Whiskey.Web.SignalR;
using Whiskey.Web.Http;
using Whiskey.Web.Extensions;
using Whiskey.Utility;
using Whiskey.Utility.Data;
using Whiskey.Utility.Web;
using Whiskey.Utility.Extensions;
using Whiskey.ZeroStore.ERP.Models.Entities.Properties;
using Whiskey.ZeroStore.ERP.Models.Entities.Warehouses;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Models.Entities;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.Utility.Class;
using System.Text.RegularExpressions;
using System.ComponentModel.Composition;
using System.IO;
using Whiskey.Utility.Logging;
using XKMath36;
using System.Diagnostics;
using Whiskey.Utility.Helper;


namespace Whiskey.ZeroStore.ERP.Services.Implements
{

    [Export(typeof(IFactorysContract))]
    public class FactorysService : ServiceBase, IFactorysContract
    {
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(FactorysService));

        private readonly IRepository<Factorys, int> _factorysRepository;

        public FactorysService(IRepository<Factorys, int> factorysRepository) : base(factorysRepository.UnitOfWork)
        {
            _factorysRepository = factorysRepository;
        }


        public OperationResult Insert(params FactorysDto[] dtos)
        {
            try
            {
                OperationResult result = _factorysRepository.Insert(dtos,
                dto =>
                {

                },
                (dto, entity) =>
                {
                    entity.CreatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;

                    return entity;
                });
                return result;
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "添加失败！错误如下：" + ex.Message, ex.ToString());
            }
        }

        public Factorys View(int Id)
        {
            var entity = _factorysRepository.GetByKey(Id);
            return entity;
        }

        public OperationResult Update(params FactorysDto[] dtos)
        {
            try
            {

                OperationResult result = _factorysRepository.Update(dtos,
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
            catch(Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "更新失败！错误如下：" + ex.Message, ex.ToString());
            }        
        }

        public OperationResult Remove(params int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _factorysRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _factorysRepository.Update(entity);

                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "移除成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "移除失败！错误如下：" + ex.Message, ex.ToString());
            }
        }

        public OperationResult Delete(params int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _factorysRepository.Entities.Where(m => ids.Contains(m.Id));
                var images = new List<ProductImage>();
                foreach (var entity in entities)
                {
                    _factorysRepository.Delete(entity);
                }

                foreach (var image in images)
                {
                    FileHelper.Delete(image.OriginalPath);
                    FileHelper.Delete(image.ThumbnailLargePath);
                    FileHelper.Delete(image.ThumbnailSmallPath);
                }

                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "删除成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "删除失败！错误如下：" + ex.Message, ex.ToString());
            }

        }

        public OperationResult Recovery(params int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _factorysRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _factorysRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "恢复成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "恢复失败！错误如下：" + ex.Message, ex.ToString());
            }
        }

        public OperationResult Enable(params int[] ids)
        {

            try
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _factorysRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _factorysRepository.Update(entity);
                }

                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "启用成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "启用失败！错误如下：" + ex.Message, ex.ToString());
            }
        }

        public OperationResult Disable(params int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _factorysRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;

                    _factorysRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "禁用成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "禁用失败！错误如下：" + ex.Message, ex.ToString());
            }
        }

        public IQueryable<Factorys> SelectFactorys
        {
            get
            {
                return _factorysRepository.Entities;
            }
        }
    }
}
