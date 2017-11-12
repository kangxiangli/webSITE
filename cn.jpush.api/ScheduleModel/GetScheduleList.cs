using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.jpush.api.common;
using Whiskey.jpush.api.push.mode;
using Whiskey.jpush.api.common.resp;
using Whiskey.jpush.api.schedule;
namespace Whiskey.jpush.api.ScheduleModel
{
    public class GetScheduleList : BaseExample
    {
        public static void Main(string[] args)
        {
            //init a pushpayload
            PushPayload pushPayload = new PushPayload();
            pushPayload.platform = Platform.all();
            pushPayload.audience = Audience.all();
            pushPayload.notification = new Notification().setAlert(ALERT);

            ScheduleClient scheduleclient = new ScheduleClient(app_key, master_secret);


            //get schedule
            try
            {
                var result = scheduleclient.getSchedule(PAGEID);
                Console.WriteLine(result.schedules[0].name);

                Console.WriteLine(result.schedules);
                Console.WriteLine(result);
            }
            catch (APIRequestException e)
            {
                Console.WriteLine("Error response from JPush server. Should review and fix it. ");
                Console.WriteLine("HTTP Status: " + e.Status);
                Console.WriteLine("Error Code: " + e.ErrorCode);
                Console.WriteLine("Error Message: " + e.ErrorCode);
            }
            catch (APIConnectionException e)
            {
                Console.WriteLine(e.Message);
            }


        }
    }
}
