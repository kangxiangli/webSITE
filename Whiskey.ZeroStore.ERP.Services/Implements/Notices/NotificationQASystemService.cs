
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Utility;
using Whiskey.Utility.Data;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Notices;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    public class NotificationQASystemService : ServiceBase, INotificationQASystemContract
    {
        #region ��ʼ����������
        private readonly IRepository<NotificationQuestion, int> _notificationQuestionRepository;
        private readonly IRepository<NotificationAnswering, int> _notificationAnsweringRepository;
        private readonly IRepository<NotificationAnswerers, int> _notificationAnswerersRepository;
        private readonly IRepository<Administrator, int> _administratorRepository;
        private readonly IRepository<MsgNotificationReader, int> _msgNotificationReaderRepository;
        private readonly IRepository<Notification, int> _notificationRepository;
        private readonly IConfigureContract _configureContract;
        public NotificationQASystemService(
            IRepository<NotificationQuestion, int> notificationQuestionRepository,
            IRepository<NotificationAnswering, int> notificationAnsweringRepository,
            IRepository<NotificationAnswerers, int> notificationAnswerersRepository,
            IRepository<Administrator, int> administratorRepository,
            IRepository<MsgNotificationReader, int> msgNotificationReaderRepository,
            IRepository<Notification, int> notificationRepository,
            IConfigureContract configureContract
            ) : base(notificationQuestionRepository.UnitOfWork)
        {
            this._notificationQuestionRepository = notificationQuestionRepository;
            this._notificationAnsweringRepository = notificationAnsweringRepository;
            this._notificationAnswerersRepository = notificationAnswerersRepository;
            this._administratorRepository = administratorRepository;
            this._msgNotificationReaderRepository = msgNotificationReaderRepository;
            this._notificationRepository = notificationRepository;
            this._configureContract = configureContract;
        }
        #endregion

        #region ��ȡ�����б�
        #region �����б�
        /// <summary>
        /// �����б�
        /// </summary>
        public IQueryable<NotificationQuestion> Entities
        {
            get
            {
                var entities = _notificationQuestionRepository.Entities;
                entities.Each(q =>
                {
                    q.AnswerersList = AnswererEntities.Where(a => a.QuestionGuidId == q.GuidId && a.IsEnabled && !a.IsDeleted).ToList();
                    q.AnsweringsList = AnsweringEntities.Where(a => a.QuestionGuidId == q.GuidId && !a.IsDeleted && a.IsEnabled).ToList();
                });

                return entities;
            }
        }
        #endregion

        #region ���б�
        /// <summary>
        /// ���б�
        /// </summary>
        public IQueryable<NotificationAnswering> AnsweringEntities
        {
            get
            {
                return _notificationAnsweringRepository.Entities;
            }
        }
        #endregion

        #region �������б�
        /// <summary>
        /// �������б�
        /// </summary>
        public IQueryable<NotificationAnswerers> AnswererEntities
        {
            get
            {
                return _notificationAnswerersRepository.Entities;
            }
        }
        #endregion
        #endregion

        #region ���û��������
        #region ���û��������
        public OperationResult EnableOrDisable(bool enable, params int[] ids)
        {
            return OperationHelper.Try((opera) =>
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _notificationQuestionRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = enable;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = entity.OperatorId ?? AuthorityHelper.OperatorId;
                }
                return OperationHelper.ReturnOperationResult(UnitOfWork.SaveChanges() > 0, opera);
            }, enable ? Operation.Enable : Operation.Disable);
        }
        #endregion

        #region ���û���ô�
        public OperationResult EnableOrDisableByAnsweringId(bool enable, params int[] ids)
        {
            return OperationHelper.Try((opera) =>
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _notificationAnsweringRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = enable;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = entity.OperatorId ?? AuthorityHelper.OperatorId;
                }
                return OperationHelper.ReturnOperationResult(UnitOfWork.SaveChanges() > 0, opera);
            }, enable ? Operation.Enable : Operation.Disable);
        }
        #endregion
        #endregion

        #region �������
        #region �������
        /// <summary>
        /// �������
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public OperationResult Insert(params NotificationQuestion[] entities)
        {
            return OperationHelper.Try((opera) =>
            {
                entities.CheckNotNull("entities");
                OperationResult result = _notificationQuestionRepository.Insert(entities,
                entity =>
                {
                    entity.IsDeleted = false;
                    entity.IsEnabled = true;
                    entity.GuidId = entity.GuidId == Guid.Empty ? Guid.NewGuid() : entity.GuidId;
                    entity.AnswerersCount = 0;
                    entity.CreatedTime = DateTime.Now;
                    entity.OperatorId = entity.OperatorId ?? AuthorityHelper.OperatorId;
                });
                return result;
            }, Operation.Add);
        }
        #endregion

        #region ��Ӵ�
        /// <summary>
        /// ��Ӵ�
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public OperationResult Insert(params NotificationAnswering[] entities)
        {
            return OperationHelper.Try((opera) =>
            {
                entities.CheckNotNull("entities");
                OperationResult result = _notificationAnsweringRepository.Insert(entities,
                entity =>
                {
                    entity.IsDeleted = false;
                    entity.IsEnabled = true;
                    entity.GuidId = entity.GuidId == Guid.Empty ? Guid.NewGuid() : entity.GuidId;
                    entity.CreatedTime = DateTime.Now;
                    entity.OperatorId = entity.OperatorId ?? AuthorityHelper.OperatorId;
                });
                return result;
            }, Operation.Add);
        }
        #endregion

        #region ��Ӵ����ߴ�����Ϣ
        /// <summary>
        /// ��Ӵ����ߴ�����Ϣ
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public OperationResult Insert(params NotificationAnswerers[] entities)
        {
            return OperationHelper.Try((opera) =>
            {
                entities.CheckNotNull("entities");
                OperationResult result = _notificationAnswerersRepository.Insert(entities,
                entity =>
                {
                    entity.IsDeleted = false;
                    entity.IsEnabled = true;
                    entity.GuidId = entity.GuidId == Guid.Empty ? Guid.NewGuid() : entity.GuidId;
                    entity.CreatedTime = DateTime.Now;
                    entity.OperatorId = entity.OperatorId ?? AuthorityHelper.OperatorId;
                });
                return result;
            }, Operation.Add);
        }
        #endregion
        #endregion

        #region ɾ����ָ�����
        #region ɾ����ָ�����
        /// <summary>
        /// ɾ����ָ�����
        /// </summary>
        /// <param name="delete"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        public OperationResult DeleteOrRecovery(bool delete, params int[] ids)
        {
            return OperationHelper.Try((opera) =>
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _notificationQuestionRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = delete;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = entity.OperatorId ?? AuthorityHelper.OperatorId;
                }
                return OperationHelper.ReturnOperationResult(UnitOfWork.SaveChanges() > 0, opera);
            }, delete ? Operation.Delete : Operation.Recovery);
        }
        #endregion

        #region ɾ����ָ���
        /// <summary>
        /// ɾ����ָ���
        /// </summary>
        /// <param name="delete"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        public OperationResult DeleteOrRecoveryByAnswering(bool delete, params int[] ids)
        {
            return OperationHelper.Try((opera) =>
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _notificationAnsweringRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = delete;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = entity.OperatorId ?? AuthorityHelper.OperatorId;
                }
                return OperationHelper.ReturnOperationResult(UnitOfWork.SaveChanges() > 0, opera);
            }, delete ? Operation.Delete : Operation.Recovery);
        }
        #endregion

        #region ɾ����ָ������ߴ�����Ϣ
        /// <summary>
        /// ɾ����ָ������ߴ�����Ϣ
        /// </summary>
        /// <param name="delete"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        public OperationResult DeleteOrRecoveryByAnswerers(bool delete, params int[] ids)
        {
            return OperationHelper.Try((opera) =>
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _notificationAnswerersRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = delete;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = entity.OperatorId ?? AuthorityHelper.OperatorId;
                }
                return OperationHelper.ReturnOperationResult(UnitOfWork.SaveChanges() > 0, opera);
            }, delete ? Operation.Delete : Operation.Recovery);
        }
        #endregion
        #endregion

        #region ��������
        #region ��������
        /// <summary>
        /// ��������
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public OperationResult Update(params NotificationQuestion[] entities)
        {
            return OperationHelper.Try((opera) =>
            {
                entities.CheckNotNull("entities");
                //UnitOfWork.TransactionEnabled = true;
                OperationResult result = _notificationQuestionRepository.Update(entities,
                entity =>
                {
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = entity.OperatorId ?? AuthorityHelper.OperatorId;
                });
                //int count = UnitOfWork.SaveChanges();
                //return OperationHelper.ReturnOperationResult(count > 0, opera);
                return result;
            }, Operation.Update);
        }
        #endregion

        #region ���´�
        /// <summary>
        /// ���´�
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public OperationResult Update(params NotificationAnswering[] entities)
        {
            return OperationHelper.Try((opera) =>
            {
                entities.CheckNotNull("entities");
                //UnitOfWork.TransactionEnabled = true;
                OperationResult result = _notificationAnsweringRepository.Update(entities,
                entity =>
                {
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = entity.OperatorId ?? AuthorityHelper.OperatorId;
                });
                //int count = UnitOfWork.SaveChanges();
                //return OperationHelper.ReturnOperationResult(count > 0, opera);
                return result;
            }, Operation.Update);
        }
        #endregion

        #region ���´����ߴ�����Ϣ
        /// <summary>
        /// ���´����ߴ�����Ϣ
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public OperationResult Update(params NotificationAnswerers[] entities)
        {
            return OperationHelper.Try((opera) =>
            {
                entities.CheckNotNull("entities");
                //UnitOfWork.TransactionEnabled = true;
                OperationResult result = _notificationAnswerersRepository.Update(entities,
                entity =>
                {
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = entity.OperatorId ?? AuthorityHelper.OperatorId;
                });
                //int count = UnitOfWork.SaveChanges();
                //return OperationHelper.ReturnOperationResult(count > 0, opera);
                return result;
            }, Operation.Update);
        }
        #endregion
        #endregion

        #region �鿴����
        #region �鿴����
        /// <summary>
        /// �鿴����
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public NotificationQuestion View(int Id)
        {
            var model = _notificationQuestionRepository.GetByKey(Id);
            model.AnswerersList = AnswererEntities.Where(a => a.IsEnabled && !a.IsDeleted && a.QuestionGuidId == model.GuidId).ToList();
            model.AnsweringsList = AnsweringEntities.Where(a => a.IsEnabled && !a.IsDeleted && a.QuestionGuidId == model.GuidId).ToList();
            return model;
        }
        #endregion

        #region �鿴��
        /// <summary>
        /// �鿴��
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public NotificationAnswering ViewByAnsweringId(int Id)
        {
            return _notificationAnsweringRepository.GetByKey(Id);
        }
        #endregion

        #region �鿴�����ߴ�����Ϣ
        /// <summary>
        /// �鿴�����ߴ�����Ϣ
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public NotificationAnswerers ViewByAnswerersId(int Id)
        {
            return _notificationAnswerersRepository.GetByKey(Id);
        }
        #endregion
        #endregion

        #region �������
        #region �������
        /// <summary>
        /// �������
        /// </summary>
        /// <param name="dtos"></param>
        /// <returns></returns>
        public OperationResult Insert(params NotificationQuestionDto[] dtos)
        {
            return OperationHelper.Try((opera) =>
            {
                dtos.CheckNotNull("dtos");
                UnitOfWork.TransactionEnabled = true;
                OperationResult result = _notificationQuestionRepository.Insert(dtos, a => { },
                    (dto, entity) =>
                    {
                        entity.IsDeleted = false;
                        entity.IsEnabled = true;
                        entity.GuidId = entity.GuidId == Guid.Empty ? Guid.NewGuid() : entity.GuidId;
                        entity.AnswerersCount = 0;
                        entity.CreatedTime = DateTime.Now;
                        entity.OperatorId = entity.OperatorId ?? AuthorityHelper.OperatorId;
                        return entity;
                    });
                int count = UnitOfWork.SaveChanges();
                return OperationHelper.ReturnOperationResult(count > 0, opera);
            }, Operation.Add);
        }
        #endregion

        #region ��Ӵ�
        /// <summary>
        /// ��Ӵ�
        /// </summary>
        /// <param name="dtos"></param>
        /// <returns></returns>
        public OperationResult Insert(params NotificationAnsweringDto[] dtos)
        {
            return OperationHelper.Try((opera) =>
            {
                dtos.CheckNotNull("dtos");
                UnitOfWork.TransactionEnabled = true;
                OperationResult result = _notificationAnsweringRepository.Insert(dtos, a => { },
                    (dto, entity) =>
                    {
                        entity.IsDeleted = false;
                        entity.IsEnabled = true;
                        entity.GuidId = entity.GuidId == Guid.Empty ? Guid.NewGuid() : entity.GuidId;
                        entity.CreatedTime = DateTime.Now;
                        entity.OperatorId = entity.OperatorId ?? AuthorityHelper.OperatorId;
                        return entity;
                    });
                int count = UnitOfWork.SaveChanges();
                return OperationHelper.ReturnOperationResult(count > 0, opera);
            }, Operation.Add);
        }
        #endregion

        #region ��Ӵ�������Ϣ
        /// <summary>
        /// ��Ӵ�������Ϣ
        /// </summary>
        /// <param name="dtos"></param>
        /// <returns></returns>
        public OperationResult Insert(params NotificationAnswerersDto[] dtos)
        {
            return OperationHelper.Try((opera) =>
            {
                dtos.CheckNotNull("dtos");
                UnitOfWork.TransactionEnabled = true;
                OperationResult result = _notificationAnswerersRepository.Insert(dtos, a => { },
                    (dto, entity) =>
                    {
                        entity.IsDeleted = false;
                        entity.IsEnabled = true;
                        entity.GuidId = entity.GuidId == Guid.Empty ? Guid.NewGuid() : entity.GuidId;
                        entity.CreatedTime = DateTime.Now;
                        entity.OperatorId = entity.OperatorId ?? AuthorityHelper.OperatorId;
                        return entity;
                    });
                int count = UnitOfWork.SaveChanges();
                return OperationHelper.ReturnOperationResult(count > 0, opera);
            }, Operation.Add);
        }
        #endregion
        #endregion

        #region ��������
        #region ��������
        /// <summary>
        /// ��������
        /// </summary>
        /// <param name="dtos"></param>
        /// <returns></returns>
        public OperationResult Update(params NotificationQuestionDto[] dtos)
        {
            return OperationHelper.Try((opera) =>
            {
                dtos.CheckNotNull("dtos");
                UnitOfWork.TransactionEnabled = true;
                OperationResult result = _notificationQuestionRepository.Update(dtos, a => { },
                    (dto, entity) =>
                    {
                        entity.UpdatedTime = DateTime.Now;
                        entity.OperatorId = entity.OperatorId ?? AuthorityHelper.OperatorId;
                        return entity;
                    });
                int count = UnitOfWork.SaveChanges();
                return OperationHelper.ReturnOperationResult(count > 0, opera);
            }, Operation.Update);
        }
        #endregion

        #region ���´�
        /// <summary>
        /// ���´�
        /// </summary>
        /// <param name="dtos"></param>
        /// <returns></returns>
        public OperationResult Update(params NotificationAnsweringDto[] dtos)
        {
            return OperationHelper.Try((opera) =>
            {
                dtos.CheckNotNull("dtos");
                UnitOfWork.TransactionEnabled = true;
                OperationResult result = _notificationAnsweringRepository.Update(dtos, a => { },
                    (dto, entity) =>
                    {
                        entity.UpdatedTime = DateTime.Now;
                        entity.OperatorId = entity.OperatorId ?? AuthorityHelper.OperatorId;
                        return entity;
                    });
                int count = UnitOfWork.SaveChanges();
                return OperationHelper.ReturnOperationResult(count > 0, opera);
            }, Operation.Update);
        }
        #endregion

        #region ���´�������Ϣ
        /// <summary>
        /// ���´�������Ϣ
        /// </summary>
        /// <param name="dtos"></param>
        /// <returns></returns>
        public OperationResult Update(params NotificationAnswerersDto[] dtos)
        {
            return OperationHelper.Try((opera) =>
            {
                dtos.CheckNotNull("dtos");
                UnitOfWork.TransactionEnabled = true;
                OperationResult result = _notificationAnswerersRepository.Update(dtos, a => { },
                    (dto, entity) =>
                    {
                        entity.UpdatedTime = DateTime.Now;
                        entity.OperatorId = entity.OperatorId ?? AuthorityHelper.OperatorId;
                        return entity;
                    });
                int count = UnitOfWork.SaveChanges();
                return OperationHelper.ReturnOperationResult(count > 0, opera);
            }, Operation.Update);
        }
        #endregion
        #endregion

        #region ��ȡ�༭����
        #region ����Id��ȡ����
        /// <summary>
        /// ����Id��ȡ����
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public NotificationQuestionDto Edit(int Id)
        {
            var entity = View(Id);
            Mapper.CreateMap<NotificationQuestion, NotificationQuestionDto>();
            var dto = Mapper.Map<NotificationQuestion, NotificationQuestionDto>(entity);
            return dto;
        }
        #endregion

        #region ����Id��ȡ��
        /// <summary>
        /// ����Id��ȡ��
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public NotificationAnsweringDto EditByAnsweringId(int Id)
        {
            var entity = _notificationAnsweringRepository.GetByKey(Id);
            Mapper.CreateMap<NotificationAnswering, NotificationAnsweringDto>();
            var dto = Mapper.Map<NotificationAnswering, NotificationAnsweringDto>(entity);
            return dto;
        }
        #endregion

        #region ����Id��ȡ��������Ϣ
        /// <summary>
        /// ����Id��ȡ��������Ϣ
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public NotificationAnswerersDto EditByAnswerersId(int Id)
        {
            var entity = _notificationAnswerersRepository.GetByKey(Id);
            Mapper.CreateMap<NotificationAnswerers, NotificationAnswerersDto>();
            var dto = Mapper.Map<NotificationAnswerers, NotificationAnswerersDto>(entity);
            return dto;
        }
        #endregion
        #endregion

        #region ������ϢId�����ȡ����
        /// <summary>
        /// ������ϢId�����ȡ����
        /// </summary>
        /// <param name="notificationId">��ϢId</param>
        /// <param name="num">Ҫ��ȡ����������</param>
        /// <returns></returns>
        public List<NotificationQuestion> GetQuestionRandomly(int notificationId)
        {
            try
            {
                //��ȡ������Ҫ���������
                int SingleAnswerQuantity = 1;
                //int.TryParse(XmlStaticHelper.GetXmlNodeByXpath("QASystem", "QASystemConfiguration", "SingleAnswerQuantity", "1"), out SingleAnswerQuantity);
                int.TryParse(_configureContract.GetConfigureValue("QASystem", "QASystemConfiguration", "SingleAnswerQuantity", "1"), out SingleAnswerQuantity);

                var list = (from q in Entities
                            where q.IsEnabled && !q.IsDeleted && q.NotificationId == notificationId
                            orderby Guid.NewGuid()
                            select q).Take(SingleAnswerQuantity < 1 ? 1 : SingleAnswerQuantity).ToList();
                list.Each(l =>
                {
                    l.AnswerersList = AnswererEntities.Where(a => a.IsEnabled && !a.IsDeleted && a.QuestionGuidId == l.GuidId).ToList();
                    l.AnsweringsList = AnsweringEntities.Where(a => a.IsEnabled && !a.IsDeleted && a.QuestionGuidId == l.GuidId).ToList();
                });
                return list;
            }
            catch (Exception ex)
            {
                return new List<NotificationQuestion>();
            }
        }
        #endregion

        #region �����Ƿ���ȷ
        /// <summary>
        /// �����Ƿ���ȷ
        /// </summary>
        /// <param name="questionId">����Id</param>
        /// <param name="content">�û��ش����ݣ�ѡ���⴫�û�ѡ��Ĵ�Id�������ֱ�Ӵ����û���������ݣ��ж��⴫0����������1���̣���</param>
        /// <returns></returns>
        public OperationResult CheckAnswer(int questionId, string content)
        {
            try
            {
                var question = View(questionId);
                if (question == null)
                {
                    return new OperationResult(OperationResultType.Error, "���ⲻ����");
                }
                int count = 0;
                switch (question.QuestionType)
                {
                    case (int)QuestionTypeFlag.Choice:
                        count = question.AnsweringsList.Count(a => a.IsRight && !a.IsDeleted && a.IsEnabled && a.GuidId.ToString() == content);
                        break;
                    case (int)QuestionTypeFlag.FillIn:
                        count = question.AnsweringsList.ToList().Count(a => a.IsRight && !a.IsDeleted && a.IsEnabled && a.Content.Trim().Equals(content.Trim()));
                        break;
                    case (int)QuestionTypeFlag.Judgment:
                        count = question.AnsweringsList.ToList().Count(a => a.IsRight && !a.IsDeleted && a.IsEnabled && a.Content.Trim().Equals(content.Trim()));
                        break;
                    default:
                        break;
                }

                if (count > 0)
                {
                    return new OperationResult(OperationResultType.Success, "�ش���ȷ");
                }
                return new OperationResult(OperationResultType.Error, "�ش����");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "ϵͳ�쳣");
            }
        }
        #endregion

        #region ���𰸸�ʽ�Ƿ���ȷ
        /// <summary>
        /// ���𰸸�ʽ�Ƿ���ȷ
        /// </summary>
        /// <param name="questionId">����Id</param>
        /// <param name="content">�û��ش����ݣ�ѡ���⴫�û�ѡ��Ĵ�Id�������ֱ�Ӵ����û���������ݣ��ж��⴫0����������1���̣���</param>
        /// <returns></returns>
        public OperationResult CheckAnswerFormat(int questionId, string content)
        {
            try
            {
                var question = View(questionId);
                if (question == null)
                {
                    return new OperationResult(OperationResultType.Error, "���ⲻ����");
                }
                int count = 0;
                switch (question.QuestionType)
                {
                    case (int)QuestionTypeFlag.Choice:
                        count = question.AnsweringsList.Count(a => !a.IsDeleted && a.IsEnabled && a.GuidId.ToString() == content);
                        break;
                    case (int)QuestionTypeFlag.FillIn:
                        count = 1;
                        break;
                    case (int)QuestionTypeFlag.Judgment:
                        count = content.Trim() != "0" && content.Trim() != "1" ? 0 : 1;
                        break;
                    default:
                        break;
                }

                if (count > 0)
                {
                    return new OperationResult(OperationResultType.Success, "��ʽ��ȷ");
                }
                return new OperationResult(OperationResultType.Error, "����ȷ��д��");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "ϵͳ�쳣");
            }
        }
        #endregion

        #region ����Ƿ���Իش�����
        /// <summary>
        /// ����Ƿ���Իش�����
        /// </summary>
        /// <param name="questionId">����Id</param>
        /// <param name="msgNotificationId">�û���ϢId��MsgNotification��Id��</param>
        /// <param name="adminId">�û�Id</param>
        /// <returns></returns>
        public OperationResult CheckIsAnswer(int notificationReadId, int adminId)
        {
            try
            {
                if (!_administratorRepository.ExistsCheck(a => a.Id == adminId && a.IsEnabled && !a.IsDeleted))
                {
                    return new OperationResult(OperationResultType.Error, "���û�������");
                }
                if (!_msgNotificationReaderRepository.ExistsCheck(n => n.Id == notificationReadId && n.IsEnabled && !n.IsDeleted && n.AdministratorId == adminId))
                {
                    return new OperationResult(OperationResultType.Error, "����Ϣ������");
                }
                if (_msgNotificationReaderRepository.ExistsCheck(n => n.Id == notificationReadId && n.AdministratorId == adminId && n.IsRead))
                {
                    return new OperationResult(OperationResultType.Error, "����Ϣ���Ķ�");
                }

                //��ȡ��������ȴʱ��
                int answerTime = 0;
                //int.TryParse(XmlStaticHelper.GetXmlNodeByXpath("QASystem", "QASystemConfiguration", "answerTime"), out answerTime);
                int.TryParse(_configureContract.GetConfigureValue("QASystem", "QASystemConfiguration", "answerTime"), out answerTime);

                int notificationId = _msgNotificationReaderRepository.GetByKey(notificationReadId).NotificationId;
                if (_notificationAnswerersRepository.Entities.Where(a => a.AdministratorId == adminId && a.MsgNotificationId == notificationReadId).ToList().Exists(a => a.CreatedTime.AddSeconds(answerTime + 1) > DateTime.Now))
                {
                    var time = _notificationAnswerersRepository.Entities.Where(a => a.AdministratorId == adminId && a.MsgNotificationId == notificationReadId).ToList().Where(a => a.CreatedTime.AddSeconds(answerTime + 1) > DateTime.Now).Select(a => DateTime.Now - a.CreatedTime.AddSeconds(answerTime + 1)).FirstOrDefault();
                    return new OperationResult(OperationResultType.Error, "��ȴ�" + time.Hours + "ʱ" + time.Minutes + "��" + time.Seconds + "����ٴλش�");
                }
                return new OperationResult(OperationResultType.Success);
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "ϵͳ�쳣");
            }
        }
        #endregion

        #region ��ȡ����
        /// <summary>
        /// ��ȡ����
        /// </summary>
        /// <param name="msgNotificationId">�û���ϢId��MsgNotification��Id��</param>
        /// <param name="adminId">�û�Id</param>
        /// <returns></returns>
        public OperationResult GetQuestion(int notificationReadId, int adminId)
        {
            try
            {
                if (!_administratorRepository.ExistsCheck(a => a.Id == adminId && a.IsEnabled && !a.IsDeleted))
                {
                    return new OperationResult(OperationResultType.Error, "���û�������");
                }
                if (!_msgNotificationReaderRepository.ExistsCheck(n => n.Id == notificationReadId && n.IsEnabled && !n.IsDeleted && n.AdministratorId == adminId))
                {
                    return new OperationResult(OperationResultType.Error, "����Ϣ������");
                }
                if (_msgNotificationReaderRepository.ExistsCheck(n => n.Id == notificationReadId && n.AdministratorId == adminId && n.IsRead))
                {
                    return new OperationResult(OperationResultType.Error, "����Ϣ���Ķ�");
                }

                //��ȡ��������ȴʱ��
                int answerTime = 0;
                //int.TryParse(XmlStaticHelper.GetXmlNodeByXpath("QASystem", "QASystemConfiguration", "answerTime"), out answerTime);
                int.TryParse(_configureContract.GetConfigureValue("QASystem","QASystemConfiguration", "answerTime"), out answerTime);

                if (_notificationAnswerersRepository.Entities.Where(a => a.AdministratorId == adminId && a.MsgNotificationId == notificationReadId).ToList().Exists(a => a.CreatedTime.AddSeconds(answerTime + 1) > DateTime.Now))
                {
                    var time = _notificationAnswerersRepository.Entities.Where(a => a.AdministratorId == adminId && a.MsgNotificationId == notificationReadId).ToList().Where(a => a.CreatedTime.AddSeconds(answerTime + 1) > DateTime.Now).Select(a => DateTime.Now - a.CreatedTime.AddSeconds(answerTime + 1)).FirstOrDefault();
                    return new OperationResult(OperationResultType.Error, "��ȴ�" + time.Hours + "ʱ" + time.Minutes + "��" + time.Seconds + "����ٴλش�");
                }
                OperationResult opera = new OperationResult(OperationResultType.Success, "��ȡ�ɹ�");

                opera.Data = GetQuestionRandomly(_msgNotificationReaderRepository.GetByKey(notificationReadId).NotificationId);
                return opera;
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "ϵͳ�쳣");
            }
        }
        #endregion

        #region ����
        /// <summary>
        /// ����
        /// </summary>
        /// <param name="adminId">������Id</param>
        /// <param name="msgNotificationId">�û���ϢId��MsgNotification��Id��</param>
        /// <param name="questionId">����Id</param>
        /// <param name="content">�ش�����</param>
        /// <returns></returns>
        public OperationResult Answer(int adminId, int notificationReadId, IDictionary<Guid, string> dic)
        {
            try
            {
                OperationResult opera = CheckIsAnswer(notificationReadId, adminId);
                if (opera.ResultType != OperationResultType.Success)
                {
                    return opera;
                }

                UnitOfWork.TransactionEnabled = true;

                bool beFlag = true;

                List<NotificationAnswerers> list = new List<NotificationAnswerers>();
                foreach (var item in dic)
                {
                    var question = _notificationQuestionRepository.Entities.FirstOrDefault(q => q.GuidId == item.Key);

                    opera = CheckAnswerFormat(question.Id, item.Value);
                    if (opera.ResultType != OperationResultType.Success)
                    {
                        return opera;
                    }


                    NotificationAnswerers answerer = new NotificationAnswerers();
                    answerer.AdministratorId = adminId;
                    answerer.Content = item.Value;
                    answerer.QuestionGuidId = item.Key;
                    answerer.MsgNotificationId = notificationReadId;

                    answerer.NotificationId = question.NotificationId;
                    answerer.CreatedTime = DateTime.Now;
                    answerer.OperatorId = adminId;
                    answerer.QuestionType = question.QuestionType;
                    answerer.UpdatedTime = DateTime.Now;
                    answerer.GuidId = Guid.NewGuid();
                    answerer.QuestionGuidId = question.GuidId;

                    opera = CheckAnswer(question.Id, item.Value);
                    if (opera.ResultType != OperationResultType.Success)
                    {
                        beFlag = false;
                        answerer.IsRight = false;
                    }
                    else
                    {
                        answerer.IsRight = true;
                    }
                    list.Add(answerer);
                }

                Insert(list.ToArray());

                if (!beFlag)
                {
                    UnitOfWork.SaveChanges();

                    opera.ResultType = OperationResultType.Error;
                    opera.Message = "�ش����";
                    return opera;
                }

                //Update(answerer);

                var entity = _msgNotificationReaderRepository.GetByKey(notificationReadId);
                entity.IsRead = true;
                entity.UpdatedTime = DateTime.Now;
                entity.OperatorId = adminId;
                _msgNotificationReaderRepository.Update(entity);

                int count = UnitOfWork.SaveChanges();
                if (count > 0)
                {
                    opera.ResultType = OperationResultType.Success;
                    opera.Message = "����ɹ�";
                    return opera;
                }
                opera.ResultType = OperationResultType.Error;
                opera.Message = "����ʧ��";
                return opera;
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "ϵͳ�쳣");
            }
        }
        #endregion

        #region �������
        /// <summary>
        /// �������
        /// </summary>
        /// <param name="questions">����</param>
        /// <returns></returns>
        public OperationResult InsertQuestions(NotificationQuestion question, int notificationId)
        {
            try
            {
                if (question == null)
                {
                    return new OperationResult(OperationResultType.Error, "���ݲ���Ϊ��");
                }
                if (!_notificationRepository.ExistsCheck(n => n.Id == notificationId && !n.IsDeleted && n.IsEnabled))
                {
                    return new OperationResult(OperationResultType.Error, "��Ϣ������");
                }

                question.OperatorId = question.OperatorId != null && question.OperatorId > 0 ? question.OperatorId : AuthorityHelper.OperatorId;

                question.NotificationId = notificationId;
                question.AnsweringsList.Each(a =>
                    {
                        a.QuestionGuidId = question.GuidId;
                        a.OperatorId = question.OperatorId;
                    });

                var opera = Insert(question);
                return opera;
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "ϵͳ�쳣");
            }
        }

        public OperationResult InsertQuestions(NotificationQuestion[] questions, int notificationId)
        {
            try
            {
                if (questions == null)
                {
                    return new OperationResult(OperationResultType.Error, "���ݲ���Ϊ��");
                }
                if (!_notificationRepository.ExistsCheck(n => n.Id == notificationId && !n.IsDeleted && n.IsEnabled))
                {
                    return new OperationResult(OperationResultType.Error, "��Ϣ������");
                }

                questions.Each(q =>
                {
                    q.NotificationId = notificationId;
                    int? operationId = q.OperatorId != null && q.OperatorId > 0 ? q.OperatorId : AuthorityHelper.OperatorId;
                    q.AnsweringsList.Each(a =>
                    {
                        a.QuestionGuidId = q.GuidId;
                        a.OperatorId = operationId;
                    });
                });

                var opera = Insert(questions);
                return opera;
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "ϵͳ�쳣");
            }
        }
        #endregion

        #region ��������
        /// <summary>
        /// ��������
        /// </summary>
        /// <param name="questions">����</param>
        /// <returns></returns>
        public OperationResult UpdateQuestions(NotificationQuestion[] questions, int notificationId)
        {
            try
            {
                if (questions == null)
                {
                    return new OperationResult(OperationResultType.Error, "���ݲ���Ϊ��");
                }
                if (!_notificationRepository.ExistsCheck(n => n.Id == notificationId && !n.IsDeleted && n.IsEnabled))
                {
                    return new OperationResult(OperationResultType.Error, "��Ϣ������");
                }

                var qls = _notificationQuestionRepository.Entities.Where(q => q.NotificationId == notificationId).ToList();
                qls.Each(q =>
                {
                    if (!questions.ToList().Exists(ql => ql.GuidId == q.GuidId))
                    {
                        DeleteOrRecovery(true, q.Id);

                        int[] ids = q.AnsweringsList.Select(a => a.Id).ToArray();
                        DeleteOrRecoveryByAnswering(true, ids);
                    }
                });

                questions.Each(q =>
                {
                    if (!_notificationQuestionRepository.ExistsCheck(qs => qs.GuidId == q.GuidId))
                    {
                        InsertQuestions(q, notificationId);
                    }

                    var answerings = _notificationAnsweringRepository.Entities.Where(a => a.QuestionGuidId == q.GuidId).ToList();
                    answerings.Each(a =>
                    {
                        if (!q.AnsweringsList.Exists(al => al.GuidId == a.GuidId))
                        {
                            DeleteOrRecoveryByAnswering(true, a.Id);
                        }
                    });

                    q.AnsweringsList.Each(a =>
                    {
                        if (!answerings.Exists(al => al.GuidId == a.GuidId))
                        {
                            a.QuestionGuidId = q.GuidId;
                            Insert(a);
                        }
                    });
                });

                var opera = Update(questions);
                return opera;
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "ϵͳ�쳣");
            }
        }
        #endregion

        #region ��ȡ�û��ش�ô𰸵�����
        /// <summary>
        /// ��ȡ�û��ش�ô𰸵�����
        /// </summary>
        /// <param name="answeringId">����Id</param>
        /// <returns></returns>
        public int GetAnsweringsCheckedCount(int answeringId)
        {
            try
            {
                int count = 0;
                var model = ViewByAnsweringId(answeringId);
                var question = _notificationQuestionRepository.Entities.FirstOrDefault(q => q.GuidId == model.QuestionGuidId);
                if (question.QuestionType == (int)QuestionTypeFlag.Choice)
                {
                    count = _notificationAnswerersRepository.Entities.Count(a => a.QuestionGuidId == question.GuidId && model.GuidId.ToString() == a.Content);
                }
                else
                {
                    count = _notificationAnswerersRepository.Entities.Count(a => a.QuestionGuidId == question.GuidId && model.Content.Trim() == a.Content.Trim());
                }

                return count;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        #endregion
    }
}

