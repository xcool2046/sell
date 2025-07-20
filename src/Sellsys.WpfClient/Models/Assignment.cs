using System;

namespace Sellsys.WpfClient.Models
{
    /// <summary>
    /// 客户分配信息
    /// </summary>
    public class Assignment
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int? SalesPersonId { get; set; }
        public int? SupportPersonId { get; set; }
        public string? SalesPersonName { get; set; }
        public string? SupportPersonName { get; set; }
        public string? DepartmentName { get; set; }
        public string? GroupName { get; set; }
        public DateTime AssignedAt { get; set; }
        public string? AssignedBy { get; set; }
        public string? Remarks { get; set; }
    }

    /// <summary>
    /// 销售人员信息
    /// </summary>
    public class SalesPerson
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public int DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public int GroupId { get; set; }
        public string? GroupName { get; set; }
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// 客服人员信息
    /// </summary>
    public class SupportPerson
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public int DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public int GroupId { get; set; }
        public string? GroupName { get; set; }
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// 组别信息
    /// </summary>
    public class Group
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
