using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.Web.Helper
{
    public class GaoDeCoordinateHelper
    {
        /// <summary>
        /// 高德地图坐标转地理位置
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static PositionInfo GaoDeAnalysis(double Longitude, double Latitude)
        {
            string url = string.Format("http://restapi.amap.com/v3/geocode/regeo?key=d8440a22fb3fc04b72a61aa6b51902a2&location={0},{1}", Longitude, Latitude);
            return HttpRequestHelper.Get<PositionInfo>(url);
        }

    }
    public class PositionInfo
    {
        public string status;
        public string info;
        public string infocode;
        public regeocode regeocode;
    }
    public class regeocode
    {
        public string formatted_address;
        public addressComponent addressComponent;
    }
    public class addressComponent
    {
        public string country;
        public string province;
        public string[] city;
        public string citycode;
        public string district;
        public string adcode;
        public string township;
        public string towncode;
        public streetNumber streetNumber;
    }
    public class streetNumber
    {
        public string street;
        public string number;
        public string location;
        public string direction;
        public string distance;
    }
}
