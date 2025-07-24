namespace Sellsys.WpfClient.Models
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

    /// <summary>
    /// 操作类型枚举
    /// </summary>
    public enum OperationType
    {
        /// <summary>
        /// 查看
        /// </summary>
        View = 1,

        /// <summary>
        /// 创建
        /// </summary>
        Create = 2,

        /// <summary>
        /// 编辑
        /// </summary>
        Edit = 3,

        /// <summary>
        /// 删除
        /// </summary>
        Delete = 4
    }
}
