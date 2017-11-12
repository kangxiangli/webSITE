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

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    public interface IExamContract:IBaseContract<ExamEntity>
    {
       
        OperationResult Delete(params ExamEntity[] entities);

        OperationResult Update(ICollection<ExamEntity> entities);

        ExamEntity Edit(int id);

    }

    public interface IExamQuestionContract : IDependency
    {
        OperationResult Insert(params ExamQuestionEntity[] entites);
        OperationResult Delete(params ExamQuestionEntity[] entities);

        OperationResult Update(ICollection<ExamQuestionEntity> entities);

    
        IQueryable<ExamQuestionEntity> Entities { get; }
        ExamQuestionEntity Edit(int id);
        OperationResult DeleteOrRecovery(bool delete, params int[] ids);

        OperationResult EnableOrDisable(bool enable, params int[] ids);

    }


    public interface IExamRecordContract : IBaseContract<ExamRecordEntity>
    {

        ExamRecordEntity Edit(int id);

        Tuple<bool,decimal,decimal> IsRestartExam(int examRecordId);
        OperationResult StartOrRestartExam(int examRecordId);

        OperationResult SubmitExam(SubmitExamDTO dto);

        OperationResult GetExamRecordDetail(int examRecordId);
    }


}
