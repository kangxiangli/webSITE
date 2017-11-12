using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.Web.Helper
{
    
   public static class OrderHelper
    {

       public enum FormType
       {
           CaiGou,
           ChaiXie,
           ChuKu,
           LingLiao,
           PeiChang,
           RuKu,
           ShengChan,
           SunYi,
           TuiHuo,
           XiaoShou,
           PeiSong,
           ZengPin,
           FuKuan,
           ShouKuan
       }

        public static string GetFormCode(FormType pFromType)
        {
            string formcode = "";
            switch (pFromType)
            {
                case FormType.CaiGou:
                    {
                        formcode = "CG";
                        break;
                    }
                case FormType.ChaiXie:
                    {
                        formcode = "CX";
                        break;
                    }
                case FormType.ChuKu:
                    {
                        formcode = "CK";
                        break;
                    }
                case FormType.LingLiao:
                    {
                        formcode = "LL";
                        break;
                    }
                case FormType.PeiChang:
                    {
                        formcode = "PC";
                        break;
                    }
                case FormType.RuKu:
                    {
                        formcode = "RK";
                        break;
                    }
                case FormType.ShengChan:
                    {
                        formcode = "SC";
                        break;
                    }
                case FormType.SunYi:
                    {
                        formcode = "SY";
                        break;
                    }
                case FormType.TuiHuo:
                    {
                        formcode = "TH";
                        break;
                    }
                case FormType.XiaoShou:
                    {
                        formcode = "XS";
                        break;
                    }
                case FormType.PeiSong:
                    {
                        formcode = "PS";
                        break;
                    }
                case FormType.ZengPin:
                    {
                        formcode = "ZP";
                        break;
                    }
                case FormType.FuKuan:
                    {
                        formcode = "FK";
                        break;
                    }
                case FormType.ShouKuan:
                    {
                        formcode = "SK";
                        break;
                    }
            }
            formcode += DateTime.Now.Year.ToString();
            formcode += DateTime.Now.Month.ToString().Length == 1 ? "0" + DateTime.Now.Month.ToString() : DateTime.Now.Month.ToString();
            formcode += DateTime.Now.Day.ToString().Length == 1 ? "0" + DateTime.Now.Day.ToString() : DateTime.Now.Day.ToString();
            formcode += DateTime.Now.Hour.ToString().Length == 1 ? "0" + DateTime.Now.Hour.ToString() : DateTime.Now.Hour.ToString();
            formcode += DateTime.Now.Minute.ToString().Length == 1 ? "0" + DateTime.Now.Minute.ToString() : DateTime.Now.Minute.ToString();
            formcode += DateTime.Now.Second.ToString().Length == 1 ? "0" + DateTime.Now.Second.ToString() : DateTime.Now.Second.ToString();
            if (DateTime.Now.Millisecond.ToString().Length == 1)
            {
                formcode += "00" + DateTime.Now.Millisecond.ToString();
            }
            else if (DateTime.Now.Millisecond.ToString().Length == 2)
            {
                formcode += "0" + DateTime.Now.Millisecond.ToString();
            }
            else
            {
                formcode += DateTime.Now.Millisecond.ToString();
            }
            return formcode;
        }

    }
}
