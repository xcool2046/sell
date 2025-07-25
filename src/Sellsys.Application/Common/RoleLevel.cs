namespace Sellsys.Application.Common
{
    /// <summary>
    /// 角色级别枚举
    /// </summary>
    public enum RoleLevel
    {
        /// <summary>
        /// 普通员工（销售、客服）
        /// </summary>
        Staff = 1,

        /// <summary>
        /// 主管级别（销售主管、客服主管）
        /// </summary>
        Supervisor = 2,

        /// <summary>
        /// 经理级别（销售经理、客服经理）
        /// </summary>
        Manager = 3,

        /// <summary>
        /// 管理员
        /// </summary>
        Admin = 4
    }
}
