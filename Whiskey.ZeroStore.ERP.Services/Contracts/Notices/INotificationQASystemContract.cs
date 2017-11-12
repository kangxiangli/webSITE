
using System;
using System.Collections.Generic;
using System.Linq;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    public interface INotificationQASystemContract : IBaseContract<NotificationQuestion, NotificationQuestionDto>
    {
        /// <summary>
        /// 答案列表
        /// </summary>
        IQueryable<NotificationAnswering> AnsweringEntities { get; }

        /// <summary>
        /// 答题者列表
        /// </summary>
        IQueryable<NotificationAnswerers> AnswererEntities { get; }

        /// <summary>
        /// 启用或禁用答案
        /// </summary>
        /// <param name="enable"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        OperationResult EnableOrDisableByAnsweringId(bool enable, params int[] ids);

        /// <summary>
        /// 删除或恢复答案
        /// </summary>
        /// <param name="delete"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        OperationResult DeleteOrRecoveryByAnswering(bool delete, params int[] ids);

        /// <summary>
        /// 删除或恢复答题者
        /// </summary>
        /// <param name="delete"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        OperationResult DeleteOrRecoveryByAnswerers(bool delete, params int[] ids);

        /// <summary>
        /// 添加答案
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        OperationResult Insert(params NotificationAnswering[] entities);

        /// <summary>
        /// 添加答题者答题信息
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        OperationResult Insert(params NotificationAnswerers[] entities);

        /// <summary>
        /// 查看答案
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        NotificationAnswering ViewByAnsweringId(int Id);

        /// <summary>
        /// 查看答题者答题信息
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        NotificationAnswerers ViewByAnswerersId(int Id);

        /// <summary>
        /// 添加答案
        /// </summary>
        /// <param name="dtos"></param>
        /// <returns></returns>
        OperationResult Insert(params NotificationAnsweringDto[] dtos);

        /// <summary>
        /// 添加答题者信息
        /// </summary>
        /// <param name="dtos"></param>
        /// <returns></returns>
        OperationResult Insert(params NotificationAnswerersDto[] dtos);

        /// <summary>
        /// 更新答案
        /// </summary>
        /// <param name="dtos"></param>
        /// <returns></returns>
        OperationResult Update(params NotificationAnsweringDto[] dtos);

        /// <summary>
        /// 更新答题者信息
        /// </summary>
        /// <param name="dtos"></param>
        /// <returns></returns>
        OperationResult Update(params NotificationAnswerersDto[] dtos);

        /// <summary>
        /// 根据Id获取答案
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        NotificationAnsweringDto EditByAnsweringId(int Id);

        /// <summary>
        /// 根据Id获取答题者信息
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        NotificationAnswerersDto EditByAnswerersId(int Id);

        /// <summary>
        /// 根据消息Id随机抽取问题
        /// </summary>
        /// <param name="notificationId">消息Id</param>
        /// <returns></returns>
        List<NotificationQuestion> GetQuestionRandomly(int notificationId);

        /// <summary>
        /// 检查答案是否正确
        /// </summary>
        /// <param name="questionId">问题Id</param>
        /// <param name="content">用户回答内容（选择题传用户选择的答案Id，填空题直接传入用户输入的内容，判断题传0（×）或者1（√））</param>
        /// <returns></returns>
        OperationResult CheckAnswer(int questionId, string content);

        /// <summary>
        /// 检查是否可以回答问题
        /// </summary>
        /// <param name="questionId">问题Id</param>
        /// <param name="msgNotificationId">用户消息Id（MsgNotification表Id）</param>
        /// <param name="adminId">用户Id</param>
        /// <returns></returns>
        OperationResult CheckIsAnswer(int notificationReadId, int adminId);

        /// <summary>
        /// 获取答题
        /// </summary>
        /// <param name="msgNotificationId">用户消息Id（MsgNotification表Id）</param>
        /// <param name="adminId">用户Id</param>
        /// <returns></returns>
        OperationResult GetQuestion(int notificationReadId, int adminId);

        /// <summary>
        /// 答题
        /// </summary>
        /// <param name="adminId">答题者Id</param>
        /// <param name="msgNotificationId">用户消息Id（MsgNotification表Id）</param>
        /// <param name="questionId">问题Id</param>
        /// <param name="content">回答内容</param>
        /// <returns></returns>
        OperationResult Answer(int adminId, int notificationReadId, IDictionary<Guid, string> dic);

        /// <summary>
        /// 添加问题
        /// </summary>
        /// <param name="questions">问题</param>
        /// <param name="notificationId">消息Id</param>
        /// <returns></returns>
        OperationResult InsertQuestions(NotificationQuestion questions, int notificationId);
        OperationResult InsertQuestions(NotificationQuestion[] questions, int notificationId);

        /// <summary>
        /// 更新问题
        /// </summary>
        /// <param name="questions">问题</param>
        /// <returns></returns>
        OperationResult UpdateQuestions(NotificationQuestion[] questions, int notificationId);

        /// <summary>
        /// 获取用户回答该答案的数量
        /// </summary>
        /// <param name="answeringId"></param>
        /// <returns></returns>
        int GetAnsweringsCheckedCount(int answeringId);
    }
}

