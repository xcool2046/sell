namespace Sellsys.Application.DTOs.Departments
{
    /// <summary>
    /// 部门删除结果
    /// </summary>
    public class DepartmentDeleteResultDto
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public int DeletedGroupCount { get; set; }
        public List<string> DeletedGroupNames { get; set; } = new List<string>();
        public int DeletedEmployeeCount { get; set; }
        public List<string> DeletedEmployeeNames { get; set; } = new List<string>();
    }
}
