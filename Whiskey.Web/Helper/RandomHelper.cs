using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.Web.Helper
{
    /// <summary>
    /// 生成随机数工具
    /// </summary>
    public static class RandomHelper
    {
        /// <summary>
        /// 生成随机密码
        /// </summary>
        /// <param name="length">密码长度</param>
        /// <returns></returns>
        public static string GetRandomPassword(int length)
        {
            string randomChars = "BCDFGHJKMPQRTVWXY2346789";
            string password = string.Empty;
            int randomNum;
            Random random = new Random();
            for (int i = 0; i < length; i++)
            {
                randomNum = random.Next(randomChars.Length);
                password += randomChars[randomNum];
            }
            return password;
        }

        public static Random random = new Random();
        /// <summary>
        /// 生成随机数字
        /// </summary>
        /// <param name="length">随机数长度</param>
        /// <returns></returns>
        public static string GetRandomNum(int length)
        {
            int minNum = (int)Math.Pow(10, length - 1);
            int maxNum = (int)Math.Pow(10, length);

            int num = random.Next(minNum, maxNum);
            return num.ToString();
        }

        public static string GetRandomCode(int length)
        {
            string randomChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ12346789";
            string password = string.Empty;
            int randomNum;
            Random random = new Random();
            for (int i = 0; i < length; i++)
            {
                randomNum = random.Next(randomChars.Length);
                password += randomChars[randomNum];
            }
            return password;
        }
        /// <summary>
        /// 常用姓名-汉字92个
        /// </summary>
        static string[] arrayLastName = new string[] { "英", "梅", "华", "兰", "珍", "芳", "伟", "军", "丽", "敏", "荣", "勇", "静", "燕", "娟", "婷", "强", "云", "杰", "平", "超", "红", "艳", "磊", "丹", "萍", "霞", "斌", "波", "玲", "涛", "明", "峰", "浩", "飞", "辉", "俊", "鑫", "鹏", "林", "慧", "颖", "洋", "国", "刚", "莉", "倩", "娜", "龙", "亮", "琴", "凯", "帅", "雪", "琳", "晶", "阳", "博", "兵", "洁", "芝", "宇", "宁", "健", "建", "岩", "帆", "瑞", "花", "佳", "香", "欢", "欣", "莹", "坤", "利", "旭", "文", "雷", "彬", "柳", "晨", "凤", "璐", "瑜", "畅", "玉", "想", "楠", "东", "杨", "成" };
        /// <summary>
        /// 常用姓氏19个
        /// </summary>
        static string[] arrayFirstName = new string[] { "李", "王", "张", "刘", "陈", "杨", "赵", "黄", "周", "吴", "徐", "孙", "胡", "朱", "高", "林", "何", "郭", "马" };

        /// <summary>
        /// 随机指定个数的汉字
        /// </summary>
        /// <param name="count">个数</param>
        /// <param name="prefix">前缀</param>
        /// <returns></returns>
        public static string RandomChinese(uint count = 1, string prefix = null)
        {
            StringBuilder strChinese = new StringBuilder();
            if (!string.IsNullOrEmpty(prefix)) strChinese.Append(prefix);
            for (int i = 0; i < count; i++)
            {
                var ind = random.Next(arrayLastName.Length);
                strChinese.Append(arrayLastName[ind]);
            }
            return strChinese.ToString();
        }
        /// <summary>
        /// 随机指定个数范围的汉字
        /// </summary>
        /// <param name="minCount">最小个数</param>
        /// <param name="maxCount">最大个数</param>
        /// <param name="prefix">前缀</param>
        /// <returns></returns>
        public static string RandomChinese(uint minCount = 1, uint maxCount = 2, string prefix = null)
        {
            var count = random.Next((int)minCount, (int)maxCount + 1);
            return RandomChinese((uint)count, prefix);
        }
        /// <summary>
        /// 随机生成姓名,如：刘* 、 张*英
        /// </summary>
        /// <returns></returns>
        public static string RandomFullName()
        {
            var firstname = arrayFirstName[random.Next(arrayFirstName.Length)];
            var lastname = RandomChinese(1, 2);
            lastname = lastname.Length > 1 ? "*" + lastname.Substring(1) : "*";
            return firstname + lastname;
        }


        private static string[] telStarts = "134,135,136,137,138,139,150,151,152,157,158,159,130,131,132,155,156,133,153,180,181,182,183,185,186,176,187,188,189,177,178".Split(',');

        /// <summary>
        /// 随机生成电话号码,中间4位为*
        /// </summary>
        /// <returns></returns>
        public static string RandomTel()
        {
            int index = random.Next(0, telStarts.Length - 1);
            string first = telStarts[index];
            string second = "****";
            string thrid = (random.Next(1, 9100) + 10000).ToString().Substring(1);
            return first + second + thrid;
        }

    }
}
