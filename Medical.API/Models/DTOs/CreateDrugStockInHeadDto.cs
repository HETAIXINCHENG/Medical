using System.ComponentModel.DataAnnotations;

namespace Medical.API.Models.DTOs;

public class CreateDrugStockInHeadDto
{
    [Required(ErrorMessage = "入库单号不能为空")]
    [MaxLength(50, ErrorMessage = "入库单号长度不能超过50个字符")]
    public string InvoiceNo { get; set; } = string.Empty;

    [Required(ErrorMessage = "供应商名称不能为空")]
    [MaxLength(100, ErrorMessage = "供应商名称长度不能超过100个字符")]
    public string SupplierName { get; set; } = string.Empty;

    // OperatorId 由后端自动从当前登录用户获取

    [Required(ErrorMessage = "入库操作时间不能为空")]
    public DateTime OperationTime { get; set; } = DateTime.UtcNow;

    public decimal TotalAmount { get; set; } = 0.00m;

    public string? Remarks { get; set; }

    public int Status { get; set; } = 1; // 1-已入库，0-已取消

    // 入库单明细行
    [Required(ErrorMessage = "入库明细不能为空")]
    public List<CreateDrugStockInLineDto> Lines { get; set; } = new List<CreateDrugStockInLineDto>();
}

public class CreateDrugStockInLineDto
{
    [Required(ErrorMessage = "药品ID不能为空")]
    public Guid DrugId { get; set; }

    [Required(ErrorMessage = "生产批号不能为空")]
    [MaxLength(50, ErrorMessage = "生产批号长度不能超过50个字符")]
    public string BatchNumber { get; set; } = string.Empty;

    public DateTime? ProductionDate { get; set; }

    [Required(ErrorMessage = "有效期至不能为空")]
    public DateTime ExpiryDate { get; set; }

    [Required(ErrorMessage = "入库数量不能为空")]
    [Range(1, int.MaxValue, ErrorMessage = "入库数量必须大于0")]
    public int Quantity { get; set; }

    [Required(ErrorMessage = "单价不能为空")]
    [Range(0.01, double.MaxValue, ErrorMessage = "单价必须大于0")]
    public decimal PurchasePrice { get; set; }

    [Required(ErrorMessage = "库位不能为空")]
    [MaxLength(50, ErrorMessage = "库位长度不能超过50个字符")]
    public string WarehouseLocation { get; set; } = "主仓库";
}

