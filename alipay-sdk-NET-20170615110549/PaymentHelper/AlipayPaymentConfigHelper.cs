using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Whiskey.Utility.Helper;

namespace Aop.Api.PaymentHelper
{
    public class AlipayPaymentConfigHelper
    {
        /// <summary>
        /// 支付宝分配给开发者的应用ID
        /// </summary>
        private static string app_Id = "2017071207728694";            //正式
        //private static string app_Id = "2016080600178672";          //沙箱
        /// <summary>
        /// 支付宝分配给开发者的应用ID
        /// </summary>
        private static string seller_Id = "2088311831911938";            //正式
        //private static string seller_Id = "2088102170169393";          //沙箱
        /// <summary>
        /// 仅支持JSON
        /// </summary>
        private static string format = "json";
        /// <summary>
        /// 接口名称
        /// </summary>
        private static string method = "";
        /// <summary>
        /// 调用的接口版本，固定为：1.0
        /// </summary>
        private static string version = "1.0";
        /// <summary>
        /// 商户生成签名字符串所使用的签名算法类型，目前支持RSA2和RSA，推荐使用RSA2
        /// </summary>
        private static string sign_Type = "RSA2";
        /// <summary>
        /// 针对用户授权接口，获取用户相关数据时，用于标识用户授权关系,即auth_token
        /// </summary>
        private static string auth_Token = "";
        /// <summary>
        /// 商户请求参数的签名串，详见签名
        /// </summary>
        private static string sign = "";
        private static string terminal_Type = "";
        private static string terminal_Info = "";
        /// <summary>
        /// 销售产品码，与支付宝签约的产品码名称。 注：目前仅支持FAST_INSTANT_TRADE_PAY
        /// </summary>
        private static string prod_Code = "";
        /// <summary>
        /// 支付宝服务器主动通知商户服务器里指定的页面http/https路径。建议商户使用https
        /// </summary>
        private static string notify_Url = ConfigurationHelper.ApiUrl + "/Recharge/ResultNotifyZFBHandler";
        /// <summary>
        /// 请求使用的编码格式，如utf-8,gbk,gb2312等
        /// </summary>
        private static string charset = "utf-8";
        private static string encrypt_Type = "AES";
        /// <summary>
        /// 业务请求参数的集合，最大长度不限，除公共参数外所有请求参数都必须放在这个参数中传递，具体参照各产品快速接入文档
        /// </summary>
        private static string biz_Content = "";
        private static string app_Auth_Token = "";
        /// <summary>
        /// 同步返回地址，HTTP/HTTPS开头字符串
        /// </summary>
        private static string return_Url = ConfigurationHelper.ApiUrl + "/Recharge/ResultNotifyZFBHandler";

        public static string server_Url = "https://openapi.alipay.com/gateway.do";     //正式地址
        //private static string server_Url = "https://openapi.alipaydev.com/gateway.do";        //沙箱地址

        /// <summary>
        /// 商户公钥(RSA2)
        /// </summary>
       private static string publicKeyPem ="MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAyxFSN+BPxfULgvwk7+N1HPjEV4IjD9hg/1CKfP9mNilC3IDBBQiD5xIn20xDFmc2vOqokTcsHcDjDRbmVXk2ykrT32xWPfAj/yNrimVjZPzc9MBm7EpeB96mzk/NdQRTvKX60+jmdO2zQJ4P/lhrtoiyLfBIVjHBeXrmJ1eEqnThUZ+QBRrKr9NnshqzQJCxXrVscqeLmut4El+UUrKFeTX3RmES+vMYlCLdJsnyaOOaJzHtHNcp/dXc694ANSLLyMQmAhrHA76SkUo0vqwth3QJciTTwm4zQDG7qCv4eMLRCg6x7sVM8/KHp7t5gTvseuAErjf2ubT0uyopALEnRwIDAQAB" ;               //正式
        //private static string publicKeyPem = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAyIKYGfhuLf2aaJbtTXwx9VVTsOw1slPu1R6GgNt2WnxOpt10VAdfcVhQ0LknfpywYc+3Vfl3nHUyrdxiYLdbAR15bmrD28NY3RkZbRt+PI4NF97Fad0akIjbNIRgCO+3hq60ByM7bRvrpWfYK4H7vT7tQFGz0p8TQ22KQAx+Sqgu81GCAtqQflUU9ZdGVAzwtN0vdSXqTzQGb/ooa/+zjwUN1xbLuDRAaFYPhn5zMHxd37/AE6aWku0t712nmAbPDazZTLWVn+4aYkHeWFFpHHCJPIMt3Z6uDzBuc9Y7aQTqlQMeATSvUmsOTZtpO2hjuasTOGCnWTsMJDl2NOSwjQIDAQAB";               //沙箱

        /// <summary>
        /// 支付宝公钥
        /// </summary>
        private static string publicKeyPemAlipay = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA5M9Ku8tUYH9dSGkAp4wfKZYPPktjjX63BSpnxSRZjHgan75KBhg5OtTGfTlIlJyFy6T+1p11jqf5fqr8Usgyo5n+buqzP3fSz1L8DejU3Nwwui9GpIM/bo+KL6SL7nLmXr2r8M8QV+fYGcgVTtGWkwiJST9kDA7v0p1woMIeU77E0H7ik1cjh1zPULkwZyGXMIiegQMRim8PouMwxmpnQuQB86JgvcLYbf+LPFMWcuaXgls4P8CQVg9tFJxWnGvVffKtMvbzPPHWtzca9LNOedtTeLla0+vrD6ZtgWDtNoKp1wMlA0yjO7FFWXu46Anf2bmQD9Nne6BxRCSJc7PTZQIDAQAB";              //正式
        //private static string publicKeyPemAlipay = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAtg/0DufbPq2w2KfjCiYm3RL5WteG0Y/kAgoZJCaeNY23b7A2pNyS6nYMq/+Jk1Nx27SNt3+3LJwRkHxkMl4lMHCgpoM0KyfrI/rZ0WRF4Wq8hDi0zW6IbUTkRKazro6DisrI9bFo7cr8TkdziHVfzKmWzzdV9n0Sr4Jlh2qxS5vm5EJ+0BEFC5RFfocsd4K0d/ZcQULmtgLAn41ftZNgjEucSaqCav0s9YgFCt+MKYCNBZ/UR8gX9obK6+l573pprXOcUUAzCXh6ZLT0eNZC5W+giQVIMux8CLUoml1ldKV292Q199zAKDewA+KcdGa74Id4kalgjNf6Zw0f5HC4NQIDAQAB";              //沙箱

        /// <summary>
        /// 商户私钥(RSA2)
        /// </summary>
        private static string privateKeyPem = "MIIEpAIBAAKCAQEAyxFSN+BPxfULgvwk7+N1HPjEV4IjD9hg/1CKfP9mNilC3IDBBQiD5xIn20xDFmc2vOqokTcsHcDjDRbmVXk2ykrT32xWPfAj/yNrimVjZPzc9MBm7EpeB96mzk/NdQRTvKX60+jmdO2zQJ4P/lhrtoiyLfBIVjHBeXrmJ1eEqnThUZ+QBRrKr9NnshqzQJCxXrVscqeLmut4El+UUrKFeTX3RmES+vMYlCLdJsnyaOOaJzHtHNcp/dXc694ANSLLyMQmAhrHA76SkUo0vqwth3QJciTTwm4zQDG7qCv4eMLRCg6x7sVM8/KHp7t5gTvseuAErjf2ubT0uyopALEnRwIDAQABAoIBAQCqFXvz6SFoAPL/ZwnZE37IIWTylsGfR/EWZ/NW9uQ5gR3LatxAxv0T0ZUojRuz7Adg/HrsBnYhBaonEIMkHD7T+RC1Fhy2DClaTeSJKpqxv0mihnSufSt9E/RK4XSJCnLIk40faPd+AIofGUT0GEM/xAbZi2RLLamLgnC7iwZHbdei8a8pWWG4KvOp+zl4gk6AYjEid3bToVRy7ySEKKK5tcs3nBDD98tcGnX5NuZpE+ZyUYZ/FhvSvu3DUxiCZ0taD9uxsHjhlCtmMskqy6qotz/KY2zdjH0JBSGW4JDZEW/ih3WY2pxVK3K6k2moTmMtz4u3MrG+7VgUH1qZRMnBAoGBAPG4DEw/y/GwA0FsTFewik48bf80Ehj7JUcu2Z7dGQ3p/K8Tn2u1i9hiysYOzzipejPB1a6ug20f/Y1e0DXzCgm+1Pgr33vfVCGUt2MZR1WHEqs1QTG7X35Qg7r7et53kO9RF3CF43MCyTPaW6T+CVuSg9/NLyydGvOv15ZW80BZAoGBANcQrhlkOc8ywZl6/wMQvoPN+YRkW2NQD7pWENPr2NLwPrmTX4I49TxU6QwNMV4Fp6wGJXNtKu0V1XkMJSWMQCFNd+kfuxXt3T0PKYP1xgwVfRr/jkpmwBVWWH7WSDAteIEgyG5sG8mjhWhxlSnELAoNFBxXS2uK/IGRrVpqd7CfAoGAGSwqk7I9IfFLlX+av7MR5C3k4qPgkpts+WnGngW2ez135B+uBYCFjMZVCaU3LVZw9HzmLOfdpfqj5BoQot2pmPQ3p5SiFHtgfpLHOlnClQSZDB1iqbQysKvf3BQeapaXEGUohL5cvnh2zaCa1KbkAJZUh3UNyZ6e1OvmDrEFahkCgYB3bujwaut7uHkxLc/uuN7EPZ/CuSTTS0PgvCkON1yrEAYVctVIcS8neRQOwEjZKRTLgIJNzqNXFJf/aBI8/t0iYW4lBZ0U+YqyfPgWM2fvkANw+djgUp17e/8bOQ7PyoCwXS9RuzHXUbslnGY00p+OWWMAti3JJF9D6U2294dOiwKBgQCYOSTkIZfWVMRWX0YzOTWT/qlqyvJWcAq87/TQEWq7AQ1ubnKzeNDa3xQJ5kZ5MvzqyFYdV10wI4jTSdDxPZ8ofeC4czdsz+or7cRxFdcp3yLJPuQRKbw3QajPJti+/ceoGlUMKOoiH5FUv+iZ1faxScEO71Lkw2EChdPpGIly2Q==";            //正式
        //private static string privateKeyPem = "MIIEpQIBAAKCAQEAyIKYGfhuLf2aaJbtTXwx9VVTsOw1slPu1R6GgNt2WnxOpt10VAdfcVhQ0LknfpywYc+3Vfl3nHUyrdxiYLdbAR15bmrD28NY3RkZbRt+PI4NF97Fad0akIjbNIRgCO+3hq60ByM7bRvrpWfYK4H7vT7tQFGz0p8TQ22KQAx+Sqgu81GCAtqQflUU9ZdGVAzwtN0vdSXqTzQGb/ooa/+zjwUN1xbLuDRAaFYPhn5zMHxd37/AE6aWku0t712nmAbPDazZTLWVn+4aYkHeWFFpHHCJPIMt3Z6uDzBuc9Y7aQTqlQMeATSvUmsOTZtpO2hjuasTOGCnWTsMJDl2NOSwjQIDAQABAoIBAQDIbKnnQ8m2lQoUQ9Eeo+c4KTuH3QTrpTRVubaO9VA/sIPaDgDqwZfor3PQv1M4Hx28F6pV+RBTx16KJYH81SrVWYX5FiWC9ahNEXq18kZj90YlNxuz7zxPf01GGI/6Psv/h4ASpmgPb1pEMyIEk2B0UYNLyJ9sCHBz9Pm9ff97VQ1FQ4+2W7fDOKde1adTdA7F/6ImvGfEiEZGhOq198SfRJkLYWEMOb2k9OF6vx+bx9xHLggTh6fOvG6LCBMLDrzWspyKYNNufYB0mtqQWlqhg6sVEERxy/aoMRH/bAFBpYZl8QjHDBZFxuTbwiGI7m46fuGtspDWE/y7sc7YQ3OhAoGBAPbg5HmiTqv+w1nVBqFZBKbxM95ZJ+QyGyiob4aNan/CJFJ9qpMpL1dlC++h9mYoDpakknCZRLlKc+fiwFVgM9SocVwZlM3N2ChSzJLzuxiuxMN5fP0dulKorIMJAfToZu04SVWsIPo70KLem+CfcRF9hzyA2ExvLhZhcMlf48AJAoGBAM/rIA5IP+Zc06Z85lvcNqdte9IpO90h1lL0GyPhaQWM9sVcgB86vjuwT9kPbvkIak7Ob4g0zctUMEUSDEbYfiSNPkJA695uJ3j2lFyAL06Um5R1F58Udt5mLSgTvBZF4QNi5QAr4n+UGH0YNWP/Cs4hR+VYfxZnLlV4MkRt68VlAoGADxB0AJQN1uLpyq58BBZpeUJYluW70GnaTGXSwFQavob7LmlqoiYuNFf/HXU1ktA466pJIUPI9MF9RxYIBG2lXAGXQTZyZQh9eyBqSUFEFmJ7sS3VmaBZwTd3p1tCWk5gEXOQzgB91qcVuQp6Wn+AJ8AQI3n9ONTD7Cuv9Sm7vPkCgYEAlfJcC/DPJLONcGRjPZxzTgwHYGmcYr4kc27Yo+fMddRTo8IyJFOZHjLpYwFeZvtKr8rxZKmwQRNWPHnnoLBBSNsUK3Pmp2OJ1BMArDVF6MYD7e0EPBXmJD5MErsoymda/7YysQz1LE0B3DW5S4SDpmUaFKzd+qovIDSvC+q6PoECgYEAwA1nNIrT8w62b49aYu+CoJGGHwcBSyz39WMqwf4HV72gTri1nBmvG2tKHgXfWN/eY37rBJhDKTqfw7nVEvzwWtDMKJahrzxmPmHSa/YkXqYuh0cQl6gj66EF9Znw4f6KLbF+b6tpI7CDMMjXqPwMukproVhIJTtLaF8UKmGLXdk=";            //沙箱

        /// <summary>
        /// 
        /// </summary>
        private static string aesKey = "Et2JjiPkzAJIKaBmFZ5r1w==";      //正式
        //private static string aesKey = "Qa6mwf4pP/KoVCLUhWORnQ==";      //沙箱

        /// <summary>
        /// 支付宝分配给开发者的应用ID
        /// </summary>
        public static string App_Id
        {
            get
            {
                return app_Id;
            }
        }

        /// <summary>
        /// 仅支持JSON
        /// </summary>
        public static string Format
        {
            get
            {
                return format;
            }
        }

        /// <summary>
        /// 接口名称
        /// </summary>
        public static string Method
        {
            get
            {
                return method;
            }
        }

        /// <summary>
        /// 调用的接口版本，固定为：1.0
        /// </summary>
        public static string Version
        {
            get
            {
                return version;
            }
        }

        /// <summary>
        /// 商户生成签名字符串所使用的签名算法类型，目前支持RSA2和RSA，推荐使用RSA2
        /// </summary>
        public static string Sign_Type
        {
            get
            {
                return sign_Type;
            }
        }

        /// <summary>
        /// 针对用户授权接口，获取用户相关数据时，用于标识用户授权关系,即auth_token
        /// </summary>
        public static string Auth_Token
        {
            get
            {
                return auth_Token;
            }
        }

        /// <summary>
        /// 商户请求参数的签名串，详见签名
        /// </summary>
        public static string Sign
        {
            get
            {
                return sign;
            }
        }

        public static string Terminal_Type
        {
            get
            {
                return terminal_Type;
            }
        }

        public static string Terminal_Info
        {
            get
            {
                return terminal_Info;
            }
        }

        /// <summary>
        /// 销售产品码，与支付宝签约的产品码名称。 注：目前仅支持FAST_INSTANT_TRADE_PAY
        /// </summary>
        public static string Prod_Code
        {
            get
            {
                return prod_Code;
            }
        }

        /// <summary>
        /// 支付宝服务器主动通知商户服务器里指定的页面http/https路径。建议商户使用https
        /// </summary>
        public static string Notify_Url
        {
            get
            {
                return notify_Url;
            }
        }

        /// <summary>
        /// 请求使用的编码格式，如utf-8,gbk,gb2312等
        /// </summary>
        public static string Charset
        {
            get
            {
                return charset;
            }
        }

        public static string Encrypt_Type
        {
            get
            {
                return encrypt_Type;
            }
        }

        /// <summary>
        /// 业务请求参数的集合，最大长度不限，除公共参数外所有请求参数都必须放在这个参数中传递，具体参照各产品快速接入文档
        /// </summary>
        public static string Biz_Content
        {
            get
            {
                return biz_Content;
            }
        }

        public static string App_Auth_Token
        {
            get
            {
                return app_Auth_Token;
            }
        }

        /// <summary>
        /// 同步返回地址，HTTP/HTTPS开头字符串
        /// </summary>
        public static string Return_Url
        {
            get
            {
                return return_Url;
            }
        }
        /// <summary>
        /// 接入地址
        /// </summary>
        public static string Server_Url
        {
            get
            {
                return server_Url;
            }
        }
        /// <summary>
        /// 商户私钥
        /// </summary>
        public static string PrivateKeyPem
        {
            get
            {
                //privateKeyPem = GetPrivateKeyPem();
                return privateKeyPem;
            }
        }
        /// <summary>
        /// 商户公钥
        /// </summary>
        public static string PublicKeyPem
        {
            get
            {
                //privateKeyPem = GetPublicKeyPem();
                return publicKeyPem;
            }
        }
        /// <summary>
        /// AES密钥
        /// </summary>
        public static string AesKey
        {
            get
            {
                return aesKey;
            }
        }
        /// <summary>
        /// 商户UID
        /// </summary>
        public static string Seller_Id
        {
            get
            {
                return seller_Id;
            }
        }

        public static string PublicKeyPemAlipay
        {
            get
            {
                return publicKeyPemAlipay;
            }
        }


        /// <summary>
        /// 商户私钥
        /// </summary>
        /// <returns></returns>
        private static string GetPrivateKeyPem()
        {
            return GetCurrentPath() + "aop-sandbox-RSA-private-c#.pem";
        }

        /// <summary>
        /// 商户公钥
        /// </summary>
        /// <returns></returns>
        private static string GetPublicKeyPem()
        {
            return GetCurrentPath() + "public-key.pem";
        }

        private static string GetCurrentPath()
        {
            string basePath = System.IO.Directory.GetParent(System.Environment.CurrentDirectory).Parent.FullName;
            return basePath + "Test\\";
        }
    }
}
