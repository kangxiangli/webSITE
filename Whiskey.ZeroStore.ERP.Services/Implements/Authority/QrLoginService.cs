using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Utility;
using Whiskey.Utility.Data;
using Whiskey.Utility.Logging;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.Utility.Extensions;
using Whiskey.Utility.Helper;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    public class QrLoginService : ServiceBase, IQrLoginContract
    {
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(QrLoginService));

        private readonly IRepository<QrLogin, int> _qrLoginRepository;
        public QrLoginService(
            IRepository<QrLogin, int> _qrLoginRepository)
            : base(_qrLoginRepository.UnitOfWork)
        {
            this._qrLoginRepository = _qrLoginRepository;
        }

        public IQueryable<QrLogin> QrLogins
        {
            get
            {
                return _qrLoginRepository.Entities;
            }
        }

        public OperationResult Delete(Expression<Func<QrLogin, bool>> predicate)
        {
            try
            {
                predicate.CheckNotNull("predicate");
                return _qrLoginRepository.Delete(predicate) > 0 ? new OperationResult(OperationResultType.Success) : new OperationResult(OperationResultType.NoChanged);
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "删除失败！错误如下：" + ex.Message, ex.ToString());
            }
        }

        public OperationResult Delete(params int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                return _qrLoginRepository.Delete(ids);
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "删除失败！错误如下：" + ex.Message, ex.ToString());
            }
        }

        public Task<int> DeleteAsync(Expression<Func<QrLogin, bool>> predicate)
        {
            return _qrLoginRepository.DeleteAsync(predicate);
        }

        public QrLoginDto Edit(int Id)
        {
            var entity = _qrLoginRepository.GetByKey(Id);
            return AutoMapper.Mapper.Map<QrLogin, QrLoginDto>(entity);
        }

        public OperationResult Insert(params QrLogin[] qrlogin)
        {
            try
            {
                qrlogin.CheckNotNull("qrlogin");

                DeleteDataAndFile();

                var result = _qrLoginRepository.Insert(qrlogin.AsEnumerable()) > 0 ? new OperationResult(OperationResultType.Success) : new OperationResult(OperationResultType.NoChanged);

                return result;
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "添加失败！错误如下：" + ex.Message, ex.ToString());
            }
        }

        public OperationResult Insert(params QrLoginDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");

                DeleteDataAndFile();

                OperationResult result = _qrLoginRepository.Insert(dtos,
                dto =>
                {

                },
                (dto, entity) =>
                {
                    entity.CreatedTime = DateTime.Now;
                    return entity;
                });

                return result;
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "添加失败！错误如下：" + ex.Message, ex.ToString());
            }
        }

        public OperationResult Update(params QrLogin[] qrlogin)
        {
            try
            {
                qrlogin.CheckNotNull("qrlogin");
                return _qrLoginRepository.Update(qrlogin);
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "更新失败！错误如下：" + ex.Message, ex.ToString());
            }
        }

        public OperationResult Update(params QrLoginDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                OperationResult result = _qrLoginRepository.Update(dtos,
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
                return new OperationResult(OperationResultType.Error, "更新失败！错误如下：" + ex.Message, ex.ToString());
            }
        }

        public QrLogin View(int Id)
        {
            return _qrLoginRepository.GetByKey(Id);
        }
        /// <summary>
        /// 删除过期的数据和图片，默认5分钟
        /// </summary>
        /// <param name="timeout">几分钟前的数据</param>
        private void DeleteDataAndFile(int timeout = 5)
        {
            try
            {
                var outTime = DateTime.Now.AddMinutes(-Math.Abs(timeout));
                //DeleteAsync(entity => entity.CreatedTime < DateTime.Now.AddMinutes(-Math.Abs(timeout)));
                var listDel = _qrLoginRepository.Entities.Where(w => w.CreatedTime < outTime).Select(s => new { s.Id, s.QrImgPath }).ToList();
                if (listDel.IsNotNullOrEmptyThis())
                {
                    OperationResult result = Delete(listDel.Select(s => s.Id).ToArray());
                    if (result.ResultType == OperationResultType.Success)
                    {
                        #region 改用下边的删除

                        //listDel.ForEach(f =>
                        //{
                        //    FileHelper.Delete(f.QrImgPath);
                        //});

                        #endregion
                    }
                }
                FileHelper.Delete(ConfigurationHelper.GetAppSetting("QrLoginPath"), outTime);
            }
            catch (Exception ex)
            {
                _Logger.Error(string.Format("删除过期的数据和图片，({0}分钟前)"), ex, timeout);
            }
        }
    }
}
