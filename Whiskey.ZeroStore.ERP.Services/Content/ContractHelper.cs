using Autofac;
using Autofac.Integration.Mvc;

namespace Whiskey
{
    /// <summary>
    /// Contract解析器，MobileApi禁止使用
    /// </summary>
    public class ContractHelper
    {
        private static IContainer Container;
        /// <summary>
        /// 设置容器，在Global已设置过
        /// </summary>
        /// <param name="container"></param>
        public static void SetContainer(IContainer container)
        {
            Container = container;
        }

        /// <summary>
        /// 基于 HTTP Request生命周期，异步禁止使用
        /// </summary>
        /// <typeparam name="IDependency"></typeparam>
        /// <returns></returns>
        public static IDependency Resolve<IDependency>()
        {
            return AutofacDependencyResolver.Current.RequestLifetimeScope.Resolve<IDependency>();
        }
        /// <summary>
        ///  和当前Http Request 不是同一个 DbConext
        /// </summary>
        /// <typeparam name="IDependency"></typeparam>
        /// <returns></returns>
        public static IDependency ResolveDiff<IDependency>()
        {
            return Container.Resolve<IDependency>();
        }
    }
}
