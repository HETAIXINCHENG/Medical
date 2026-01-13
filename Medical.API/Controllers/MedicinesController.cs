using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Medical.API.Data;
using Medical.API.Models.Entities;
using Medical.API.Attributes;

namespace Medical.API.Controllers;

/// <summary>
/// 药品控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class MedicinesController : ControllerBase
{
    private readonly MedicalDbContext _context;
    private readonly ILogger<MedicinesController> _logger;

    public MedicinesController(MedicalDbContext context, ILogger<MedicinesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// 获取药品列表
    /// </summary>
    /// <param name="page">页码</param>
    /// <param name="pageSize">每页数量</param>
    /// <returns>药品列表</returns>
    [HttpGet]
    [RequirePermission("medicine.view")]
    [ProducesResponseType(typeof(List<Medicine>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<Medicine>>> GetMedicines(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var medicines = await _context.Medicines
            .OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(medicines);
    }

    /// <summary>
    /// 根据ID获取药品详情
    /// </summary>
    /// <param name="id">药品ID</param>
    /// <returns>药品详情</returns>
    [HttpGet("{id}")]
    [RequirePermission("medicine.view")]
    [ProducesResponseType(typeof(Medicine), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Medicine>> GetMedicineById(Guid id)
    {
        var medicine = await _context.Medicines.FindAsync(id);

        if (medicine == null)
        {
            return NotFound(new { message = "药品不存在" });
        }

        return Ok(medicine);
    }

    /// <summary>
    /// 创建药品
    /// </summary>
    /// <param name="medicine">药品信息</param>
    /// <returns>创建的药品</returns>
    [HttpPost]
    [RequirePermission("medicine.create")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(Medicine), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Medicine>> CreateMedicine([FromBody] Medicine medicine)
    {
        if (string.IsNullOrWhiteSpace(medicine.Name))
        {
            return BadRequest(new { message = "药品名称不能为空" });
        }

        medicine.Id = Guid.NewGuid();
        medicine.CreatedAt = DateTime.UtcNow;
        medicine.UpdatedAt = DateTime.UtcNow;

        _context.Medicines.Add(medicine);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMedicineById), new { id = medicine.Id }, medicine);
    }

    /// <summary>
    /// 更新药品
    /// </summary>
    /// <param name="id">药品ID</param>
    /// <param name="medicine">药品信息</param>
    /// <returns>更新后的药品</returns>
    [HttpPut("{id}")]
    [RequirePermission("medicine.update")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(Medicine), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Medicine>> UpdateMedicine(Guid id, [FromBody] Medicine medicine)
    {
        var existingMedicine = await _context.Medicines.FindAsync(id);
        if (existingMedicine == null)
        {
            return NotFound(new { message = "药品不存在" });
        }

        existingMedicine.Name = medicine.Name;
        existingMedicine.Specification = medicine.Specification;
        existingMedicine.Manufacturer = medicine.Manufacturer;
        existingMedicine.Price = medicine.Price;
        existingMedicine.Stock = medicine.Stock;
        existingMedicine.ImageUrl = medicine.ImageUrl;
        existingMedicine.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(existingMedicine);
    }

    /// <summary>
    /// 删除药品
    /// </summary>
    /// <param name="id">药品ID</param>
    /// <returns>删除结果</returns>
    [HttpDelete("{id}")]
    [RequirePermission("medicine.delete")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMedicine(Guid id)
    {
        var medicine = await _context.Medicines.FindAsync(id);
        if (medicine == null)
        {
            return NotFound(new { message = "药品不存在" });
        }

        _context.Medicines.Remove(medicine);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

