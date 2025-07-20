namespace Sellsys.Application.DTOs.Finance
{
    /// <summary>
    /// 财务管理筛选条件DTO
    /// </summary>
    public class FinanceFilterDto
    {
        /// <summary>
        /// 客户ID
        /// </summary>
        public int? CustomerId { get; set; }

        /// <summary>
        /// 产品ID
        /// </summary>
        public int? ProductId { get; set; }

        /// <summary>
        /// 生效日期开始
        /// </summary>
        public DateTime? EffectiveDateStart { get; set; }

        /// <summary>
        /// 生效日期结束
        /// </summary>
        public DateTime? EffectiveDateEnd { get; set; }

        /// <summary>
        /// 到期日期开始
        /// </summary>
        public DateTime? ExpiryDateStart { get; set; }

        /// <summary>
        /// 到期日期结束
        /// </summary>
        public DateTime? ExpiryDateEnd { get; set; }

        /// <summary>
        /// 支付日期开始
        /// </summary>
        public DateTime? PaymentDateStart { get; set; }

        /// <summary>
        /// 支付日期结束
        /// </summary>
        public DateTime? PaymentDateEnd { get; set; }

        /// <summary>
        /// 负责人ID（销售人员）
        /// </summary>
        public int? SalesPersonId { get; set; }

        /// <summary>
        /// 订单状态
        /// </summary>
        public string? OrderStatus { get; set; }

        /// <summary>
        /// 搜索关键词（客户名称、订单号等）
        /// </summary>
        public string? SearchKeyword { get; set; }

        /// <summary>
        /// 页码
        /// </summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// 每页大小
        /// </summary>
        public int PageSize { get; set; } = 50;
    }

    /// <summary>
    /// 筛选数据源DTO
    /// </summary>
    public class FinanceFilterOptionsDto
    {
        /// <summary>
        /// 客户列表
        /// </summary>
        public List<FilterOptionDto> Customers { get; set; } = new List<FilterOptionDto>();

        /// <summary>
        /// 产品列表
        /// </summary>
        public List<FilterOptionDto> Products { get; set; } = new List<FilterOptionDto>();

        /// <summary>
        /// 负责人列表
        /// </summary>
        public List<FilterOptionDto> SalesPersons { get; set; } = new List<FilterOptionDto>();

        /// <summary>
        /// 订单状态列表
        /// </summary>
        public List<FilterOptionDto> OrderStatuses { get; set; } = new List<FilterOptionDto>();

        /// <summary>
        /// 生效日期选项（年月）
        /// </summary>
        public List<FilterOptionDto> EffectiveDateOptions { get; set; } = new List<FilterOptionDto>();

        /// <summary>
        /// 到期日期选项（年月）
        /// </summary>
        public List<FilterOptionDto> ExpiryDateOptions { get; set; } = new List<FilterOptionDto>();

        /// <summary>
        /// 支付日期选项（年月）
        /// </summary>
        public List<FilterOptionDto> PaymentDateOptions { get; set; } = new List<FilterOptionDto>();
    }

    /// <summary>
    /// 筛选选项DTO
    /// </summary>
    public class FilterOptionDto
    {
        /// <summary>
        /// 值
        /// </summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// 显示文本
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// 是否选中
        /// </summary>
        public bool IsSelected { get; set; }
    }
}
