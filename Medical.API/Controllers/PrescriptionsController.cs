using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Medical.API.Data;
using Medical.API.Models.Entities;
using Medical.API.Models.DTOs;
using Medical.API.Attributes;

namespace Medical.API.Controllers;

/// <summary>
/// 处方控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class PrescriptionsController : ControllerBase
{
    private readonly MedicalDbContext _context;
    private readonly ILogger<PrescriptionsController> _logger;

    public PrescriptionsController(MedicalDbContext context, ILogger<PrescriptionsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// 获取处方列表（处方生成 - 管理员和医生可查看所有，患者只能查看自己的）
    /// </summary>
    /// <param name="patientId">患者ID（可选）</param>
    /// <param name="doctorId">医生ID（可选）</param>
    /// <param name="status">状态（可选）</param>
    /// <param name="page">页码</param>
    /// <param name="pageSize">每页数量</param>
    /// <returns>处方列表</returns>
    [HttpGet]
    [RequirePermission("prescription.view")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetPrescriptions(
        [FromQuery] Guid? patientId = null,
        [FromQuery] Guid? doctorId = null,
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userRole = User.FindFirstValue(ClaimTypes.Role);
        
        if (string.IsNullOrEmpty(currentUserId) || !Guid.TryParse(currentUserId, out var currentUserIdGuid))
        {
            return Unauthorized();
        }

        var query = _context.Prescriptions
            .Include(p => p.Patient)
            .Include(p => p.Doctor)
            .AsQueryable();

        // 如果不是管理员和医生，只能查看自己的处方
        if (userRole != "Admin" && userRole != "SuperAdmin" && userRole != "Doctor")
        {
            // 获取当前用户对应的患者信息
            var currentPatient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == currentUserIdGuid);
            
            if (currentPatient != null)
            {
                query = query.Where(p => p.PatientId == currentPatient.Id);
            }
            else
            {
                // 如果没有患者信息，返回空列表
                return Ok(new { items = new List<object>(), total = 0, page, pageSize });
            }
        }

        // 筛选条件
        if (patientId.HasValue)
        {
            query = query.Where(p => p.PatientId == patientId.Value);
        }

        if (doctorId.HasValue)
        {
            query = query.Where(p => p.DoctorId == doctorId.Value);
        }

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(p => p.Status == status);
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new
            {
                p.Id,
                p.PatientId,
                Patient = new { p.Patient.RealName },
                p.DoctorId,
                Doctor = new { p.Doctor.Name },
                p.ConsultationId,
                p.PrescriptionNumber,
                p.Diagnosis,
                p.Status,
                p.CreatedAt,
                p.UpdatedAt
            })
            .ToListAsync();

        return Ok(new { items, total, page, pageSize });
    }

    /// <summary>
    /// 根据ID获取处方详情
    /// </summary>
    /// <param name="id">处方ID</param>
    /// <returns>处方详情</returns>
    [HttpGet("{id}")]
    [RequirePermission("prescription.view")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Prescription>> GetPrescriptionById(Guid id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userRole = User.FindFirstValue(ClaimTypes.Role);
        
        if (string.IsNullOrEmpty(currentUserId) || !Guid.TryParse(currentUserId, out var currentUserIdGuid))
        {
            return Unauthorized();
        }

        var prescription = await _context.Prescriptions
                .Include(p => p.Patient)
            .Include(p => p.Doctor)
            .Include(p => p.PrescriptionMedicines)
                .ThenInclude(pm => pm.Medicine)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (prescription == null)
        {
            return NotFound(new { message = "处方不存在" });
        }

        // 权限检查
        if (userRole != "Admin" && userRole != "SuperAdmin" && userRole != "Doctor")
        {
            // 获取当前用户对应的患者信息
            var currentPatient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == currentUserIdGuid);
            
            if (currentPatient == null || prescription.PatientId != currentPatient.Id)
            {
                return Forbid();
            }
        }

        return Ok(prescription);
    }

    /// <summary>
    /// 创建处方（处方生成）
    /// </summary>
    /// <param name="dto">处方信息</param>
    /// <returns>创建的处方</returns>
    [HttpPost]
    [RequirePermission("prescription.create")]
    [Authorize(Roles = "Admin,SuperAdmin,Doctor")]
    [ProducesResponseType(typeof(Prescription), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Prescription>> CreatePrescription([FromBody] CreatePrescriptionDto dto)
    {
        try
        {
            ModelState.Clear();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            
            if (string.IsNullOrEmpty(currentUserId) || !Guid.TryParse(currentUserId, out var currentUserIdGuid))
            {
                return Unauthorized();
            }

            // 验证必填字段
            if (dto == null)
            {
                return BadRequest(new { message = "请求数据不能为空" });
            }

            if (string.IsNullOrWhiteSpace(dto.PrescriptionNumber))
            {
                return BadRequest(new { message = "处方编号不能为空" });
            }

            if (string.IsNullOrWhiteSpace(dto.PrescriptionContent))
            {
                return BadRequest(new { message = "处方内容不能为空" });
            }

            // 验证患者是否存在
            var patientExists = await _context.Patients.AnyAsync(p => p.Id == dto.PatientId);
            if (!patientExists)
            {
                return BadRequest(new { message = "患者不存在" });
            }

            // 验证医生是否存在
            var doctorExists = await _context.Doctors.AnyAsync(d => d.Id == dto.DoctorId);
            if (!doctorExists)
            {
                return BadRequest(new { message = "医生不存在" });
            }

            // 如果不是管理员，医生只能为自己创建处方
            if (userRole != "Admin" && userRole != "SuperAdmin")
            {
                // 获取当前用户对应的患者信息（医生也是患者）
                var currentPatient = await _context.Patients
                    .FirstOrDefaultAsync(p => p.UserId == currentUserIdGuid);
                
                if (currentPatient == null)
                {
                    return BadRequest(new { message = "未找到对应的患者信息" });
                }

                // 获取医生信息（通过 DoctorPatient 中间表查询）
                var doctor = await _context.DoctorPatients
                    .Where(dp => dp.PatientId == currentPatient.Id)
                    .Select(dp => dp.Doctor)
                    .FirstOrDefaultAsync();
                
                if (doctor == null || doctor.Id != dto.DoctorId)
                {
                    return Forbid();
                }
            }

            // 创建处方
            var newPrescription = new Prescription
            {
                Id = Guid.NewGuid(),
                PatientId = dto.PatientId,
                DoctorId = dto.DoctorId,
                ConsultationId = dto.ConsultationId,
                PrescriptionNumber = dto.PrescriptionNumber,
                Diagnosis = string.IsNullOrWhiteSpace(dto.Diagnosis) ? null : dto.Diagnosis,
                PrescriptionContent = dto.PrescriptionContent,
                Status = dto.Status,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Prescriptions.Add(newPrescription);
            await _context.SaveChangesAsync();

            // 重新加载包含关联信息的处方
            var createdPrescription = await _context.Prescriptions
                .Include(p => p.Patient)
            .Include(p => p.Doctor)
                .FirstOrDefaultAsync(p => p.Id == newPrescription.Id);

            _logger.LogInformation("处方创建成功: Id={Id}, PrescriptionNumber={PrescriptionNumber}", 
                newPrescription.Id, newPrescription.PrescriptionNumber);

            return CreatedAtAction(nameof(GetPrescriptionById), new { id = newPrescription.Id }, createdPrescription);
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "数据库更新失败");
            return BadRequest(new { message = "数据库操作失败", error = dbEx.InnerException?.Message ?? dbEx.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建处方失败");
            return BadRequest(new { message = "创建处方失败", error = ex.Message });
        }
    }

    /// <summary>
    /// 更新处方
    /// </summary>
    /// <param name="id">处方ID</param>
    /// <param name="dto">处方信息</param>
    /// <returns>更新后的处方</returns>
    [HttpPut("{id}")]
    [RequirePermission("prescription.update")]
    [Authorize(Roles = "Admin,SuperAdmin,Doctor")]
    [ProducesResponseType(typeof(Prescription), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Prescription>> UpdatePrescription(
        Guid id, 
        [FromBody] CreatePrescriptionDto dto)
    {
        try
        {
            ModelState.Clear();

            var prescription = await _context.Prescriptions.FindAsync(id);
            if (prescription == null)
            {
                return NotFound(new { message = "处方不存在" });
            }

            // 验证必填字段
            if (dto == null)
            {
                return BadRequest(new { message = "请求数据不能为空" });
            }

            // 更新处方
            prescription.PatientId = dto.PatientId;
            prescription.DoctorId = dto.DoctorId;
            prescription.ConsultationId = dto.ConsultationId;
            prescription.PrescriptionNumber = dto.PrescriptionNumber;
            prescription.Diagnosis = string.IsNullOrWhiteSpace(dto.Diagnosis) ? null : dto.Diagnosis;
            prescription.PrescriptionContent = dto.PrescriptionContent;
            prescription.Status = dto.Status;
            prescription.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // 重新加载包含关联信息的处方
            var updatedPrescription = await _context.Prescriptions
                .Include(p => p.Patient)
            .Include(p => p.Doctor)
                .FirstOrDefaultAsync(p => p.Id == id);

            _logger.LogInformation("处方更新成功: Id={Id}", id);

            return Ok(updatedPrescription);
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "数据库更新失败");
            return BadRequest(new { message = "数据库操作失败", error = dbEx.InnerException?.Message ?? dbEx.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新处方失败");
            return BadRequest(new { message = "更新处方失败", error = ex.Message });
        }
    }

    /// <summary>
    /// 删除处方
    /// </summary>
    /// <param name="id">处方ID</param>
    /// <returns>删除结果</returns>
    [HttpDelete("{id}")]
    [RequirePermission("prescription.delete")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePrescription(Guid id)
    {
        var prescription = await _context.Prescriptions.FindAsync(id);
        if (prescription == null)
        {
            return NotFound(new { message = "处方不存在" });
        }

        _context.Prescriptions.Remove(prescription);
        await _context.SaveChangesAsync();

        _logger.LogInformation("处方删除成功: Id={Id}", id);

        return NoContent();
    }
}

