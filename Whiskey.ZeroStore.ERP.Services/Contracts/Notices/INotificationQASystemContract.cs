
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
        /// ���б�
        /// </summary>
        IQueryable<NotificationAnswering> AnsweringEntities { get; }

        /// <summary>
        /// �������б�
        /// </summary>
        IQueryable<NotificationAnswerers> AnswererEntities { get; }

        /// <summary>
        /// ���û���ô�
        /// </summary>
        /// <param name="enable"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        OperationResult EnableOrDisableByAnsweringId(bool enable, params int[] ids);

        /// <summary>
        /// ɾ����ָ���
        /// </summary>
        /// <param name="delete"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        OperationResult DeleteOrRecoveryByAnswering(bool delete, params int[] ids);

        /// <summary>
        /// ɾ����ָ�������
        /// </summary>
        /// <param name="delete"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        OperationResult DeleteOrRecoveryByAnswerers(bool delete, params int[] ids);

        /// <summary>
        /// ��Ӵ�
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        OperationResult Insert(params NotificationAnswering[] entities);

        /// <summary>
        /// ��Ӵ����ߴ�����Ϣ
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        OperationResult Insert(params NotificationAnswerers[] entities);

        /// <summary>
        /// �鿴��
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        NotificationAnswering ViewByAnsweringId(int Id);

        /// <summary>
        /// �鿴�����ߴ�����Ϣ
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        NotificationAnswerers ViewByAnswerersId(int Id);

        /// <summary>
        /// ��Ӵ�
        /// </summary>
        /// <param name="dtos"></param>
        /// <returns></returns>
        OperationResult Insert(params NotificationAnsweringDto[] dtos);

        /// <summary>
        /// ��Ӵ�������Ϣ
        /// </summary>
        /// <param name="dtos"></param>
        /// <returns></returns>
        OperationResult Insert(params NotificationAnswerersDto[] dtos);

        /// <summary>
        /// ���´�
        /// </summary>
        /// <param name="dtos"></param>
        /// <returns></returns>
        OperationResult Update(params NotificationAnsweringDto[] dtos);

        /// <summary>
        /// ���´�������Ϣ
        /// </summary>
        /// <param name="dtos"></param>
        /// <returns></returns>
        OperationResult Update(params NotificationAnswerersDto[] dtos);

        /// <summary>
        /// ����Id��ȡ��
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        NotificationAnsweringDto EditByAnsweringId(int Id);

        /// <summary>
        /// ����Id��ȡ��������Ϣ
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        NotificationAnswerersDto EditByAnswerersId(int Id);

        /// <summary>
        /// ������ϢId�����ȡ����
        /// </summary>
        /// <param name="notificationId">��ϢId</param>
        /// <returns></returns>
        List<NotificationQuestion> GetQuestionRandomly(int notificationId);

        /// <summary>
        /// �����Ƿ���ȷ
        /// </summary>
        /// <param name="questionId">����Id</param>
        /// <param name="content">�û��ش����ݣ�ѡ���⴫�û�ѡ��Ĵ�Id�������ֱ�Ӵ����û���������ݣ��ж��⴫0����������1���̣���</param>
        /// <returns></returns>
        OperationResult CheckAnswer(int questionId, string content);

        /// <summary>
        /// ����Ƿ���Իش�����
        /// </summary>
        /// <param name="questionId">����Id</param>
        /// <param name="msgNotificationId">�û���ϢId��MsgNotification��Id��</param>
        /// <param name="adminId">�û�Id</param>
        /// <returns></returns>
        OperationResult CheckIsAnswer(int notificationReadId, int adminId);

        /// <summary>
        /// ��ȡ����
        /// </summary>
        /// <param name="msgNotificationId">�û���ϢId��MsgNotification��Id��</param>
        /// <param name="adminId">�û�Id</param>
        /// <returns></returns>
        OperationResult GetQuestion(int notificationReadId, int adminId);

        /// <summary>
        /// ����
        /// </summary>
        /// <param name="adminId">������Id</param>
        /// <param name="msgNotificationId">�û���ϢId��MsgNotification��Id��</param>
        /// <param name="questionId">����Id</param>
        /// <param name="content">�ش�����</param>
        /// <returns></returns>
        OperationResult Answer(int adminId, int notificationReadId, IDictionary<Guid, string> dic);

        /// <summary>
        /// �������
        /// </summary>
        /// <param name="questions">����</param>
        /// <param name="notificationId">��ϢId</param>
        /// <returns></returns>
        OperationResult InsertQuestions(NotificationQuestion questions, int notificationId);
        OperationResult InsertQuestions(NotificationQuestion[] questions, int notificationId);

        /// <summary>
        /// ��������
        /// </summary>
        /// <param name="questions">����</param>
        /// <returns></returns>
        OperationResult UpdateQuestions(NotificationQuestion[] questions, int notificationId);

        /// <summary>
        /// ��ȡ�û��ش�ô𰸵�����
        /// </summary>
        /// <param name="answeringId"></param>
        /// <returns></returns>
        int GetAnsweringsCheckedCount(int answeringId);
    }
}

