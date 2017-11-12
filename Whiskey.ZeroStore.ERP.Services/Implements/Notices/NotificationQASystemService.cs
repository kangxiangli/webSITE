
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
        #region 初始化操作对象
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

        #region 获取数据列表
        #region 问题列表
        /// <summary>
        /// 问题列表
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

        #region 答案列表
        /// <summary>
        /// 答案列表
        /// </summary>
        public IQueryable<NotificationAnswering> AnsweringEntities
        {
            get
            {
                return _notificationAnsweringRepository.Entities;
            }
        }
        #endregion

        #region 答题者列表
        /// <summary>
        /// 答题者列表
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

        #region 启用或禁用数据
        #region 启用或禁用问题
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

        #region 启用或禁用答案
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

        #region 添加数据
        #region 添加问题
        /// <summary>
        /// 添加问题
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

        #region 添加答案
        /// <summary>
        /// 添加答案
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

        #region 添加答题者答题信息
        /// <summary>
        /// 添加答题者答题信息
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

        #region 删除或恢复数据
        #region 删除或恢复问题
        /// <summary>
        /// 删除或恢复问题
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

        #region 删除或恢复答案
        /// <summary>
        /// 删除或恢复答案
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

        #region 删除或恢复答题者答题信息
        /// <summary>
        /// 删除或恢复答题者答题信息
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

        #region 更新数据
        #region 更新问题
        /// <summary>
        /// 更新问题
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

        #region 更新答案
        /// <summary>
        /// 更新答案
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

        #region 更新答题者答题信息
        /// <summary>
        /// 更新答题者答题信息
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

        #region 查看数据
        #region 查看问题
        /// <summary>
        /// 查看问题
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

        #region 查看答案
        /// <summary>
        /// 查看答案
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public NotificationAnswering ViewByAnsweringId(int Id)
        {
            return _notificationAnsweringRepository.GetByKey(Id);
        }
        #endregion

        #region 查看答题者答题信息
        /// <summary>
        /// 查看答题者答题信息
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public NotificationAnswerers ViewByAnswerersId(int Id)
        {
            return _notificationAnswerersRepository.GetByKey(Id);
        }
        #endregion
        #endregion

        #region 添加数据
        #region 添加问题
        /// <summary>
        /// 添加问题
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

        #region 添加答案
        /// <summary>
        /// 添加答案
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

        #region 添加答题者信息
        /// <summary>
        /// 添加答题者信息
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

        #region 更新数据
        #region 更新问题
        /// <summary>
        /// 更新问题
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

        #region 更新答案
        /// <summary>
        /// 更新答案
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

        #region 更新答题者信息
        /// <summary>
        /// 更新答题者信息
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

        #region 获取编辑对象
        #region 根据Id获取问题
        /// <summary>
        /// 根据Id获取问题
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

        #region 根据Id获取答案
        /// <summary>
        /// 根据Id获取答案
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

        #region 根据Id获取答题者信息
        /// <summary>
        /// 根据Id获取答题者信息
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

        #region 根据消息Id随机抽取问题
        /// <summary>
        /// 根据消息Id随机抽取问题
        /// </summary>
        /// <param name="notificationId">消息Id</param>
        /// <param name="num">要获取的问题数量</param>
        /// <returns></returns>
        public List<NotificationQuestion> GetQuestionRandomly(int notificationId)
        {
            try
            {
                //获取单次需要答题的数量
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

        #region 检查答案是否正确
        /// <summary>
        /// 检查答案是否正确
        /// </summary>
        /// <param name="questionId">问题Id</param>
        /// <param name="content">用户回答内容（选择题传用户选择的答案Id，填空题直接传入用户输入的内容，判断题传0（×）或者1（√））</param>
        /// <returns></returns>
        public OperationResult CheckAnswer(int questionId, string content)
        {
            try
            {
                var question = View(questionId);
                if (question == null)
                {
                    return new OperationResult(OperationResultType.Error, "问题不存在");
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
                    return new OperationResult(OperationResultType.Success, "回答正确");
                }
                return new OperationResult(OperationResultType.Error, "回答错误");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "系统异常");
            }
        }
        #endregion

        #region 检查答案格式是否正确
        /// <summary>
        /// 检查答案格式是否正确
        /// </summary>
        /// <param name="questionId">问题Id</param>
        /// <param name="content">用户回答内容（选择题传用户选择的答案Id，填空题直接传入用户输入的内容，判断题传0（×）或者1（√））</param>
        /// <returns></returns>
        public OperationResult CheckAnswerFormat(int questionId, string content)
        {
            try
            {
                var question = View(questionId);
                if (question == null)
                {
                    return new OperationResult(OperationResultType.Error, "问题不存在");
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
                    return new OperationResult(OperationResultType.Success, "格式正确");
                }
                return new OperationResult(OperationResultType.Error, "请正确填写答案");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "系统异常");
            }
        }
        #endregion

        #region 检查是否可以回答问题
        /// <summary>
        /// 检查是否可以回答问题
        /// </summary>
        /// <param name="questionId">问题Id</param>
        /// <param name="msgNotificationId">用户消息Id（MsgNotification表Id）</param>
        /// <param name="adminId">用户Id</param>
        /// <returns></returns>
        public OperationResult CheckIsAnswer(int notificationReadId, int adminId)
        {
            try
            {
                if (!_administratorRepository.ExistsCheck(a => a.Id == adminId && a.IsEnabled && !a.IsDeleted))
                {
                    return new OperationResult(OperationResultType.Error, "该用户不存在");
                }
                if (!_msgNotificationReaderRepository.ExistsCheck(n => n.Id == notificationReadId && n.IsEnabled && !n.IsDeleted && n.AdministratorId == adminId))
                {
                    return new OperationResult(OperationResultType.Error, "该消息不存在");
                }
                if (_msgNotificationReaderRepository.ExistsCheck(n => n.Id == notificationReadId && n.AdministratorId == adminId && n.IsRead))
                {
                    return new OperationResult(OperationResultType.Error, "该消息已阅读");
                }

                //获取答题间隔冷却时间
                int answerTime = 0;
                //int.TryParse(XmlStaticHelper.GetXmlNodeByXpath("QASystem", "QASystemConfiguration", "answerTime"), out answerTime);
                int.TryParse(_configureContract.GetConfigureValue("QASystem", "QASystemConfiguration", "answerTime"), out answerTime);

                int notificationId = _msgNotificationReaderRepository.GetByKey(notificationReadId).NotificationId;
                if (_notificationAnswerersRepository.Entities.Where(a => a.AdministratorId == adminId && a.MsgNotificationId == notificationReadId).ToList().Exists(a => a.CreatedTime.AddSeconds(answerTime + 1) > DateTime.Now))
                {
                    var time = _notificationAnswerersRepository.Entities.Where(a => a.AdministratorId == adminId && a.MsgNotificationId == notificationReadId).ToList().Where(a => a.CreatedTime.AddSeconds(answerTime + 1) > DateTime.Now).Select(a => DateTime.Now - a.CreatedTime.AddSeconds(answerTime + 1)).FirstOrDefault();
                    return new OperationResult(OperationResultType.Error, "请等待" + time.Hours + "时" + time.Minutes + "分" + time.Seconds + "秒后再次回答");
                }
                return new OperationResult(OperationResultType.Success);
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "系统异常");
            }
        }
        #endregion

        #region 获取答题
        /// <summary>
        /// 获取答题
        /// </summary>
        /// <param name="msgNotificationId">用户消息Id（MsgNotification表Id）</param>
        /// <param name="adminId">用户Id</param>
        /// <returns></returns>
        public OperationResult GetQuestion(int notificationReadId, int adminId)
        {
            try
            {
                if (!_administratorRepository.ExistsCheck(a => a.Id == adminId && a.IsEnabled && !a.IsDeleted))
                {
                    return new OperationResult(OperationResultType.Error, "该用户不存在");
                }
                if (!_msgNotificationReaderRepository.ExistsCheck(n => n.Id == notificationReadId && n.IsEnabled && !n.IsDeleted && n.AdministratorId == adminId))
                {
                    return new OperationResult(OperationResultType.Error, "该消息不存在");
                }
                if (_msgNotificationReaderRepository.ExistsCheck(n => n.Id == notificationReadId && n.AdministratorId == adminId && n.IsRead))
                {
                    return new OperationResult(OperationResultType.Error, "该消息已阅读");
                }

                //获取答题间隔冷却时间
                int answerTime = 0;
                //int.TryParse(XmlStaticHelper.GetXmlNodeByXpath("QASystem", "QASystemConfiguration", "answerTime"), out answerTime);
                int.TryParse(_configureContract.GetConfigureValue("QASystem","QASystemConfiguration", "answerTime"), out answerTime);

                if (_notificationAnswerersRepository.Entities.Where(a => a.AdministratorId == adminId && a.MsgNotificationId == notificationReadId).ToList().Exists(a => a.CreatedTime.AddSeconds(answerTime + 1) > DateTime.Now))
                {
                    var time = _notificationAnswerersRepository.Entities.Where(a => a.AdministratorId == adminId && a.MsgNotificationId == notificationReadId).ToList().Where(a => a.CreatedTime.AddSeconds(answerTime + 1) > DateTime.Now).Select(a => DateTime.Now - a.CreatedTime.AddSeconds(answerTime + 1)).FirstOrDefault();
                    return new OperationResult(OperationResultType.Error, "请等待" + time.Hours + "时" + time.Minutes + "分" + time.Seconds + "秒后再次回答");
                }
                OperationResult opera = new OperationResult(OperationResultType.Success, "获取成功");

                opera.Data = GetQuestionRandomly(_msgNotificationReaderRepository.GetByKey(notificationReadId).NotificationId);
                return opera;
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "系统异常");
            }
        }
        #endregion

        #region 答题
        /// <summary>
        /// 答题
        /// </summary>
        /// <param name="adminId">答题者Id</param>
        /// <param name="msgNotificationId">用户消息Id（MsgNotification表Id）</param>
        /// <param name="questionId">问题Id</param>
        /// <param name="content">回答内容</param>
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
                    opera.Message = "回答错误";
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
                    opera.Message = "答题成功";
                    return opera;
                }
                opera.ResultType = OperationResultType.Error;
                opera.Message = "答题失败";
                return opera;
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "系统异常");
            }
        }
        #endregion

        #region 添加问题
        /// <summary>
        /// 添加问题
        /// </summary>
        /// <param name="questions">问题</param>
        /// <returns></returns>
        public OperationResult InsertQuestions(NotificationQuestion question, int notificationId)
        {
            try
            {
                if (question == null)
                {
                    return new OperationResult(OperationResultType.Error, "数据不可为空");
                }
                if (!_notificationRepository.ExistsCheck(n => n.Id == notificationId && !n.IsDeleted && n.IsEnabled))
                {
                    return new OperationResult(OperationResultType.Error, "消息不存在");
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
                return new OperationResult(OperationResultType.Error, "系统异常");
            }
        }

        public OperationResult InsertQuestions(NotificationQuestion[] questions, int notificationId)
        {
            try
            {
                if (questions == null)
                {
                    return new OperationResult(OperationResultType.Error, "数据不可为空");
                }
                if (!_notificationRepository.ExistsCheck(n => n.Id == notificationId && !n.IsDeleted && n.IsEnabled))
                {
                    return new OperationResult(OperationResultType.Error, "消息不存在");
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
                return new OperationResult(OperationResultType.Error, "系统异常");
            }
        }
        #endregion

        #region 更新问题
        /// <summary>
        /// 更新问题
        /// </summary>
        /// <param name="questions">问题</param>
        /// <returns></returns>
        public OperationResult UpdateQuestions(NotificationQuestion[] questions, int notificationId)
        {
            try
            {
                if (questions == null)
                {
                    return new OperationResult(OperationResultType.Error, "数据不可为空");
                }
                if (!_notificationRepository.ExistsCheck(n => n.Id == notificationId && !n.IsDeleted && n.IsEnabled))
                {
                    return new OperationResult(OperationResultType.Error, "消息不存在");
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
                return new OperationResult(OperationResultType.Error, "系统异常");
            }
        }
        #endregion

        #region 获取用户回答该答案的数量
        /// <summary>
        /// 获取用户回答该答案的数量
        /// </summary>
        /// <param name="answeringId">问题Id</param>
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

