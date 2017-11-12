using Autofac;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Models.Entities;
using Whiskey.ZeroStore.ERP.Services;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Services.Implements.Notice;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Xunit;

namespace UnitTest
{
    public class ExamUnitTest : IClassFixture<AutofacFixture>
    {
        private AutofacFixture _fixture;

        public ExamUnitTest(AutofacFixture fixture)
        {
            _fixture = fixture;
        }

        //[Fact]
        public void TestStat()
        {

            var questionSvc = _fixture.Container.Resolve<IExamQuestionContract>();
            var res = questionSvc.Insert(new ExamQuestionEntity[] {
                new ExamQuestionEntity
                {
                   Title = "以行政执法的内容和性质为标准，下列不属于行政执法种类的选项是",
                   IsMulti = false,
                   Score =10,
                   OperatorId = 9,
                   RightAnswer = "A",
                    AnswerOptions =JsonConvert.SerializeObject(
                         new AnswerOptionEntry[]
                        {
                             new AnswerOptionEntry{Title="行政处罚",Value ="A"},
                             new AnswerOptionEntry{Title="行政许可",Value ="B"},
                             new AnswerOptionEntry{Title="行政强制",Value ="C"},
                             new AnswerOptionEntry{Title="行政调解",Value ="D"}
                        }),
                }
                ,new ExamQuestionEntity
                {
                   Title = "对与人民群众日常生活、生产直接相关的行政执法活动，主要由（  ）行政执法机关实施",
                   IsMulti = true,
                   Score =10,
                   OperatorId = 9,
                   RightAnswer = "A,C",
                    AnswerOptions =JsonConvert.SerializeObject(
                         new AnswerOptionEntry[]
                        {
                             new AnswerOptionEntry{Title="市、县两级",Value ="A"},
                             new AnswerOptionEntry{Title="省、县两级",Value ="B"},
                             new AnswerOptionEntry{Title="区、县两级",Value ="C"},
                             new AnswerOptionEntry{Title="省市区县两级",Value ="D"},
                        }),
                }
            });

            Assert.True(res.ResultType == OperationResultType.Success);
        }



        public void TestGetAllItem()
        {
            var AppointmentService = _fixture.Container.Resolve<IAppointmentContract>();
            var memberId = 5275;

            var items = AppointmentService.GetItems(memberId);

        }


        [Fact]
        public void TestSMS()
        {
            var contract = _fixture.Container.Resolve<ISmsContract>();
            var res = contract.SendSms("18513691544", "hello,world");
            Assert.True(res);
        }


    }
}