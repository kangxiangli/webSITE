namespace Whiskey.ZeroStore.ERP.Models.Enums
{
    /// <summary>
    /// 会员充值上下文,充值类型
    /// </summary>
    public enum MemberDepositContextEnum
    {
        /// <summary>
        /// 用于会员管理，店铺管理对会员账户充值
        /// </summary>
        线下充值 = 0,

        /// <summary>
        /// 包含微信充值等线上充值
        /// </summary>
        线上充值 = 1,

        /// <summary>
        /// 用于零售单退货
        /// </summary>
        退货充值 = 2,

        /// <summary>
        /// 用于商品零售消费后获得赠送积分
        /// </summary>
        购物奖励 = 3,

        /// <summary>
        /// 用于后台[储值积分维护]功能
        /// </summary>
        系统调整 = 4,
        /// <summary>
        /// 通过游戏获取
        /// </summary>
        游戏获取 = 5,

        /// <summary>
        /// 用于加班奖励积分
        /// </summary>
        加班奖励 = 6,

        /// <summary>
        /// 考试通过奖励积分
        /// </summary>
        考试奖励 = 7
    }
}
