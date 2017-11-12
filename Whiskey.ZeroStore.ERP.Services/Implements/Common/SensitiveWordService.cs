using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.Utility;
using Whiskey.Web.Helper;
using System.Text.RegularExpressions;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Transfers;
using AutoMapper;

namespace Whiskey.ZeroStore.ERP.Services.Implements.Common
{
    /// <summary>
    /// 敏感词业务层
    /// </summary>
    public class SensitiveWordService : ServiceBase, ISensitiveWordContract
    {
        #region 声明数据层操作对象

        private readonly IRepository<SensitiveWord, int> _sensitiveWordRepository;

        protected readonly ILogger _Logger = LogManager.GetLogger(typeof(SensitiveWordService));

        public SensitiveWordService(IRepository<SensitiveWord, int> sensitiveWordRepository)
            : base(sensitiveWordRepository.UnitOfWork)
		{
            _sensitiveWordRepository = sensitiveWordRepository;
		}
        #endregion

        #region 添加数据
                
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dtos"></param>
        /// <returns></returns>
        public OperationResult Insert(params Transfers.SensitiveWordDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                for (int i = 0; i < dtos.Length; i++)
                {
                    string word = dtos[i].WordPattern;
                    var list = _sensitiveWordRepository.Entities.Where(x => x.WordPattern == word);
                    if (list.Count()>0)
                    {
                        return new OperationResult(OperationResultType.Error, "添加失败！添加内容已经存在！" );
                    }
                }
                OperationResult result = _sensitiveWordRepository.Insert(dtos,
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
                return new OperationResult(OperationResultType.Error, "添加失败！错误如下：" + ex.Message);
            }
        }
        #endregion

        #region 更新数据
        
        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="dtos"></param>
        /// <returns></returns>
        public OperationResult Update(params  SensitiveWordDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                for (int i = 0; i < dtos.Length; i++)
                {
                    string word = dtos[i].WordPattern;
                    int id=dtos[i].Id;
                    var entity = _sensitiveWordRepository.Entities.Where(x => x.WordPattern == word).FirstOrDefault();
                    if (entity!=null && entity.Id != id)
                    {
                        return new OperationResult(OperationResultType.Error, "更新失败！添加内容已经存在！" );
                    }
                }
                OperationResult result = _sensitiveWordRepository.Update(dtos,
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

        #region 删除数据
                
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public  OperationResult Delete(params int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                OperationResult result = _sensitiveWordRepository.Delete(ids);
                return result;
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "删除失败！错误如下：" + ex.Message);
            }
        }
        #endregion

        #region 获取数据列表

        public IQueryable<SensitiveWord> SensitiveWords { get { return _sensitiveWordRepository.Entities; } }
        #endregion

        #region 校验输入文本
        /// <summary>
        /// 校验输入文本
        /// </summary>
        /// <param name="strComment"></param>
        /// <returns></returns>
        public bool CheckComment(string strComment)
        {
            try
            {
                IQueryable<string> listWords = SensitiveWords.Select(x=>x.WordPattern);
                string strWord = string.Join("|", listWords);
                bool isMatch = Regex.IsMatch(strComment, strWord);
                return isMatch;
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                throw;
            }
        }
        #endregion

        #region 获取编辑对象
        public SensitiveWordDto Edit(int Id)
        {
            var entity = _sensitiveWordRepository.GetByKey(Id);
            Mapper.CreateMap<SensitiveWord, SensitiveWordDto>();
            var dto = Mapper.Map<SensitiveWord, SensitiveWordDto>(entity);
            return dto;
        }
        #endregion
    }
}
