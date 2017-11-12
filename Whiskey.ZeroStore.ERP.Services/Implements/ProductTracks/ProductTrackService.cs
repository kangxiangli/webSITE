using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Utility;
using Whiskey.Utility.Data;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{
  public class ProductTrackService : ServiceBase, IProductTrackContract
    {
        private readonly IRepository<ProductTrack, int> _productTrack;


        public ProductTrackService(
            IRepository<ProductTrack, int> productTrack
        )
            : base(productTrack.UnitOfWork)
        {
            _productTrack = productTrack;
        }


        public IQueryable<ProductTrack> Tracks
        {
            get { return _productTrack.Entities; }
        }

        /// <summary>
        /// 添加商品操作信息
        /// </summary>
        /// <param name="dtos">要添加的DTO数据</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Insert(params ProductTrackDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                OperationResult result = _productTrack.Insert(dtos,
                dto =>
                {

                },
                (dto, entity) =>
                {
                    entity.CreatedTime = dto.CreatedTime ?? DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
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
        /// 添加商品操作信息,禁止使用导航属性
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public OperationResult BulkInsert(params ProductTrack[] entities)
        {
            try
            {
                entities.CheckNotNull("entities");
                OperationResult result = _productTrack.InsertBulk(entities, entity =>
                {
                    entity.CreatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                });
                return result;
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "添加失败！错误如下：" + ex.Message);
            }
        }
        /// <summary>
        /// 添加商品操作信息,禁止使用导航属性
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public OperationResult BulkInsert(IEnumerable<ProductTrack> entities)
        {
            try
            {
                entities.CheckNotNull("entities");
                OperationResult result = _productTrack.InsertBulk(entities, entity =>
                {
                    entity.CreatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                });
                return result;
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "添加失败！错误如下：" + ex.Message);
            }
        }
    }
}
