using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    /// <summary>
    /// 业务层操作接口
    /// </summary>
    public interface ISensitiveWordContract : IDependency
    {
        #region ISensitiveWordContract
        
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dtos"></param>
        /// <returns></returns>
        OperationResult Insert(params SensitiveWordDto[] dtos);

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="dtos"></param>
        /// <returns></returns>
        OperationResult Update(params SensitiveWordDto[] dtos);

        /// <summary>
        /// 删除数据（物理删除）
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        OperationResult Delete(params int[] ids);

        /// <summary>
        /// 校验文本
        /// </summary>
        /// <param name="strComment"></param>
        /// <returns></returns>
        bool CheckComment(string strComment);

        /// <summary>
        /// 获取数据集
        /// </summary>
        IQueryable<SensitiveWord> SensitiveWords { get; }

        /// <summary>
        /// 获取编辑对象
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        SensitiveWordDto Edit(int Id);



        #endregion




    }
}
