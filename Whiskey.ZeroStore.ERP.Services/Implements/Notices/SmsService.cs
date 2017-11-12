using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Utility;
using Whiskey.Utility.Data;
using Whiskey.Utility.Extensions;
using Whiskey.Utility.Helper;
using Whiskey.Utility.Secutiry;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Template;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    public class SmsService : ServiceBase, ISmsContract
    {
        private readonly IRepository<Template, int> _TemplateRepository;
        private readonly IRepository<Sms, int> _SmsRepository;
        private readonly IRepository<Store, int> _StoreRepository;
        private readonly IRepository<Member, int> _MemberRepository;

        public SmsService(
            IRepository<Template, int> _TemplateRepository,
            IRepository<Sms, int> _SmsRepository,
            IRepository<Store, int> _StoreRepository,
            IRepository<Member, int> _MemberRepository
            ) : base(_SmsRepository.UnitOfWork)
        {
            this._TemplateRepository = _TemplateRepository;
            this._SmsRepository = _SmsRepository;
            this._StoreRepository = _StoreRepository;
            this._MemberRepository = _MemberRepository;
        }

        #region 纯发送短信

        public bool SendSms(string phone, string content)
        {
            try
            {
                if (content.IsNullOrEmpty() || content.Length > 250)
                    return false;
                //短信接口账号
                string strAdminNum = "xiaoruis";
                //短信接口密码32位小写加密
                string strPassWord = HashHelper.GetMd5("0fashioncom");
                string url = "http://sms.dtcms.net/httpapi/?cmd=tx&pass=1&uid=" + strAdminNum + "&pwd=" + strPassWord + "&mobile=" + phone + "&content=" + content;
                Uri mUri = new Uri(url);
                HttpWebRequest mRequest = (HttpWebRequest)WebRequest.Create(mUri);
                mRequest.Method = "GET";
                mRequest.ContentType = "application/x-www-form-urlencoded"; ;
                HttpWebResponse response = (HttpWebResponse)mRequest.GetResponse();
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private int? _GetSmsCount(string url)
        {
            try
            {
                //短信接口账号
                string strAdminNum = "xiaoruis";
                //短信接口密码32位小写加密
                string strPassWord = HashHelper.GetMd5("0fashioncom");
                url = $"{url}&uid={strAdminNum}&pwd={strPassWord}";
                var resp = HttpRequestHelper.Get(url);
                var arr = resp.Split("||", true).Select(s => Convert.ToInt32(s)).ToArray();
                if (arr[0] == 100)
                {
                    return arr[1];
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public int? GetRemainSmsCount()
        {
            return _GetSmsCount("http://sms.dtcms.net/httpapi/?cmd=mm");
        }

        public int? GetSendSmsCount()
        {
            return _GetSmsCount("http://sms.dtcms.net/httpapi/?cmd=se");
        }

        /// <summary>
        /// 发送指定模板内容的短信
        /// </summary>
        /// <param name="phone"></param>
        /// <param name="flag">只识别>=1000的flag</param>
        /// <param name="data">字典数据 查看/Templates/SmsTemplate/Index</param>
        /// <returns></returns>
        public bool SendSms(string phone, TemplateNotificationType flag, Dictionary<string, object> data)
        {
            if (!phone.IsMobileNumber())
                return false;
            var modtemp = _TemplateRepository.Entities.Where(w => w.TemplateType == (int)TemplateFlag.SMS && w.IsEnabled && !w.IsDeleted && w.templateNotification.NotifciationType == flag).OrderByDescending(o => o.IsDefault).OrderByDescending(o => o.IsDefaultPhone).FirstOrDefault();
            if (modtemp.IsNull() || modtemp.TemplateHtml.IsNullOrEmpty())
            {
                return false;
            }
            var strcontent = NVelocityHelper.Generate(modtemp.TemplateHtml, data, "_log_sms_template_");
            return SendSms(phone, strcontent);
        }

        #endregion

        public IQueryable<Sms> Entities
        {
            get
            {
                return _SmsRepository.Entities;
            }
        }

        public OperationResult DeleteOrRecovery(bool delete, params int[] ids)
        {
            return OperationHelper.Try((opera) =>
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = delete;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                }
                return OperationHelper.ReturnOperationResult(UnitOfWork.SaveChanges() > 0, opera);
            }, delete ? Operation.Delete : Operation.Recovery);
        }

        public SmsDto Edit(int Id)
        {
            var entity = View(Id);
            var dto = entity.MapperTo<SmsDto>();
            if (dto.IsNotNull())
            {
                dto.StoreIds = entity.Stores.Where(w => w.IsEnabled && !w.IsDeleted).Select(s => s.Id).ToList();
            }
            return dto;
        }

        public OperationResult EnableOrDisable(bool enable, params int[] ids)
        {
            return OperationHelper.Try((opera) =>
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = enable;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                }
                return OperationHelper.ReturnOperationResult(UnitOfWork.SaveChanges() > 0, opera);
            }, enable ? Operation.Enable : Operation.Disable);
        }

        public OperationResult Insert(params Sms[] entities)
        {
            return OperationHelper.Try((opera) =>
            {
                entities.CheckNotNull("entities");
                UnitOfWork.TransactionEnabled = true;
                OperationResult result = _SmsRepository.Insert(entities,
                entity =>
                {
                    entity.CreatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                });
                int count = UnitOfWork.SaveChanges();
                return OperationHelper.ReturnOperationResult(count > 0, opera);
            }, Operation.Add);
        }

        public OperationResult Insert(params SmsDto[] dtos)
        {
            return OperationHelper.Try((opera) =>
            {
                dtos.CheckNotNull("dtos");
                UnitOfWork.TransactionEnabled = true;
                OperationResult result = _SmsRepository.Insert(dtos, a => { },
                    (dto, entity) =>
                    {
                        if (dto.StoreIds.IsNotNullOrEmptyThis())
                        {
                            entity.Stores = _StoreRepository.Entities.Where(w => w.IsEnabled && !w.IsDeleted).Where(w => dto.StoreIds.Contains(w.Id)).ToList();
                            entity.Members = entity.Stores.SelectMany(s => s.Members).Where(w => w.IsEnabled && !w.IsDeleted && w.MobilePhone != null).ToList();
                        }
                        entity.CreatedTime = DateTime.Now;
                        entity.OperatorId = AuthorityHelper.OperatorId;
                        return entity;
                    });
                int count = UnitOfWork.SaveChanges();
                return OperationHelper.ReturnOperationResult(count > 0, opera);
            }, Operation.Add);
        }

        public OperationResult Update(params Sms[] entities)
        {
            return OperationHelper.Try((opera) =>
            {
                entities.CheckNotNull("entities");
                UnitOfWork.TransactionEnabled = true;
                OperationResult result = _SmsRepository.Update(entities,
                entity =>
                {
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                });
                int count = UnitOfWork.SaveChanges();
                return OperationHelper.ReturnOperationResult(count > 0, opera);
            }, Operation.Update);
        }

        public OperationResult Update(params SmsDto[] dtos)
        {
            return OperationHelper.Try((opera) =>
            {
                dtos.CheckNotNull("dtos");
                UnitOfWork.TransactionEnabled = true;
                OperationResult result = _SmsRepository.Update(dtos, a => { },
                    (dto, entity) =>
                    {
                        entity.Stores.Clear();
                        entity.Members.Clear();
                        if (dto.StoreIds.IsNotNullOrEmptyThis())
                        {
                            entity.Stores = _StoreRepository.Entities.Where(w => w.IsEnabled && !w.IsDeleted).Where(w => dto.StoreIds.Contains(w.Id)).ToList();
                            entity.Members = entity.Stores.SelectMany(s => s.Members).Where(w => w.IsEnabled && !w.IsDeleted && w.MobilePhone != null).ToList();
                        }
                        entity.UpdatedTime = DateTime.Now;
                        entity.OperatorId = AuthorityHelper.OperatorId;
                        return entity;
                    });
                int count = UnitOfWork.SaveChanges();
                return OperationHelper.ReturnOperationResult(count > 0, opera);
            }, Operation.Update);
        }

        public Sms View(int Id)
        {
            return _SmsRepository.GetByKey(Id);
        }

        public OperationResult ConfirmSend(int Id)
        {
            var mod = Entities.FirstOrDefault(w => w.IsEnabled && !w.IsDeleted && w.Id == Id);
            if (mod.IsNotNull())
            {
                if (mod.IsSend)
                {
                    return OperationHelper.ReturnOperationResultDIY(OperationResultType.QueryNull, "已发送过了");
                }
                if (mod.Description.IsNullOrEmpty())
                {
                    return OperationHelper.ReturnOperationResultDIY(OperationResultType.Error, "发送内容不能为空");
                }

                var listphones = mod.Members.Where(w => w.IsEnabled && !w.IsDeleted && w.MobilePhone != null)
                                .Select(s => s.MobilePhone).ToList().Where(w => !string.IsNullOrEmpty(w)).Distinct().ToList();
                if (listphones.IsNotNullOrEmptyThis())
                {
                    mod.IsSend = true;
                    mod.SendTime = DateTime.Now;

                    var res = _SmsRepository.Update(new Sms[] { mod });
                    if (res.ResultType == OperationResultType.Success)
                    {
                        ThreadPool.QueueUserWorkItem((obj) =>
                        {
                            foreach (var phone in listphones as List<string>)
                            {
                                SendSms(phone, mod.Description);
                            }
                        }, listphones);
                        return OperationHelper.ReturnOperationResultDIY(OperationResultType.Success, "后台发送中...（可以关闭）");
                    }
                    return res;
                }
                return OperationHelper.ReturnOperationResultDIY(OperationResultType.Error, "没有可发送的会员");
            }
            else
            {
                return OperationHelper.ReturnOperationResultDIY(OperationResultType.QueryNull, "数据不存在");
            }
        }
    }
}
