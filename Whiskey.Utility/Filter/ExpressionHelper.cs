using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;


namespace ExpressionParser
{
    /// <summary>
    /// 提供Expression的常规操作 yxk 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ExpressionAccessHelp<T> where T : class
    {

        private ParameterExpression param;

        private BinaryExpression filter;

        public ExpressionAccessHelp()
        {

            param = Expression.Parameter(typeof(T), "c");

            //1==1

            Expression left = Expression.Constant(1);

            filter = Expression.Equal(left, left);

        }

        public Expression<Func<T, bool>> GetExpression()
        {

            return Expression.Lambda<Func<T, bool>>(filter, param);

        }

        public void Equal(string propertyName, object value)
        {

            try
            {
                Expression left = Expression.Property(param, typeof(T).GetProperty(propertyName));

                Expression right = Expression.Constant(value, value.GetType());

                Expression result = Expression.Equal(left, right);

                filter = Expression.And(filter, result);
            }
            catch (Exception)
            {

                throw;
            }

        }

        public void Contains(string propertyName, string value)
        {

            Expression left = Expression.Property(param, typeof(T).GetProperty(propertyName));

            Expression right = Expression.Constant(value, value.GetType());

            Expression result = Expression.Call(left, typeof(string).GetMethod("Contains"), right);

            filter = Expression.And(filter, result);

        }

        public void Contains(string propertyName, int value)
        {

            Expression left = Expression.Property(param, typeof(T).GetProperty(propertyName));

            Expression right = Expression.Constant(value.ToString(), typeof(string));

            Expression result = Expression.Call(left, typeof(string).GetMethod("Contains"), right);

            filter = Expression.And(filter, result);

        }
        /// <summary>
        /// 大于或者等于
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        public void Greater(string propertyName, object value)
        {
            Expression left = Expression.Property(param, typeof(T).GetProperty(propertyName));
            Expression right = Expression.Constant(value, value.GetType());
            Expression result = Expression.GreaterThanOrEqual(left, right);
            filter = Expression.And(filter, result);
        }
        /// <summary>
        /// 小于或等于
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        public void Less(string propertyName, object value)
        {
            Expression left = Expression.Property(param, typeof(T).GetProperty(propertyName));
            Expression right = Expression.Constant(value, value.GetType());
            Expression result = Expression.LessThanOrEqual(left, right);
            filter = Expression.And(filter, result);
        }


        public void OrWithContains(params Tuple<string, object>[] conditionPair)
        {
            if (conditionPair.Length == 0)
                return;
            var expList = new List<Expression>();
            foreach (var item in conditionPair)
            {
                Expression left = Expression.Property(param, typeof(T).GetProperty(item.Item1));
                Expression right = Expression.Constant(item.Item2, item.Item2.GetType());
                expList.Add(Expression.Call(left, typeof(string).GetMethod("Contains"), right));
            }

            if (expList.Count > 1)
            {
                Expression ex = expList.First();
                for (var i = 1; i < expList.Count; i++)
                {
                    ex = Expression.Or(ex, expList[i]);
                }
                filter = Expression.And(filter, ex);
            }
            else
                filter = Expression.And(filter, expList[0]);
        }

        public void IsNotNull(string propertyName)
        {
            Expression exp = Expression.Property(param, typeof(T).GetProperty(propertyName));
            Expression result = Expression.NotEqual(exp, Expression.Constant(""));
            filter = Expression.And(filter, result);
        }
        /// <summary>
        /// 判断一个值是否包含在一个集合中
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="values"></param>
        public void In(string propertyName, object[] values)
        {
            Expression left = Expression.Constant(1);
            BinaryExpression bexp = null;

           
            for (int i = 0; i < values.Length; i++)
            {
                object obj = values[i];
                string ty = typeof(T).GetProperty(propertyName).ToString();
                string te = "System." + ty.Substring(0, ty.Length - propertyName.Length - 1);
                Type ty1 = Type.GetType(te);
                var _obj = Convert.ChangeType(obj, ty1);
                Expression exp = Expression.Property(param, typeof(T).GetProperty(propertyName));
                if (i == 0)
                {
                    bexp = Expression.Equal(exp, Expression.Constant(_obj));
                    continue;
                }
                else
                {
                    Expression result = Expression.Equal(exp, Expression.Constant(_obj));
                    bexp = Expression.Or(bexp, result);
                }
            }
            filter = Expression.And(filter, bexp);
        }
    }
}
