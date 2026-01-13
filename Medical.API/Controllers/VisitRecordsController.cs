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
/// 就诊记录控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class VisitRecordsController : ControllerBase
{
    private readonly MedicalDbContext _context;
    private readonly ILogger<VisitRecordsController> _logger;

    public VisitRecordsController(MedicalDbContext context, ILogger<VisitRecordsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// 获取就诊记录列表
    /// </summary>
    /// <param name="patientId">患者ID（可选）</param>
    /// <param name="doctorId">医生ID（可选）</param>
    /// <param name="page">页码</param>
    /// <param name="pageSize">每页数量</param>
    /// <returns>就诊记录列表</returns>
    [HttpGet]
    [RequirePermission("visitrecord.view")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetVisitRecords(
        [FromQuery] Guid? patientId = null,
        [FromQuery] Guid? doctorId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userRole = User.FindFirstValue(ClaimTypes.Role);
        
        if (string.IsNullOrEmpty(currentUserId) || !Guid.TryParse(currentUserId, out var currentUserIdGuid))
        {
            return Unauthorized();
        }

        var query = _context.VisitRecords
            .Include(v => v.Patient)
            .Include(v => v.Doctor)
            .AsQueryable();

        // 如果不是管理员，只能查看自己的就诊记录
        if (userRole != "Admin" && userRole != "SuperAdmin")
        {
            // 获取当前用户对应的患者信息
            var currentPatient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == currentUserIdGuid);
            
            if (currentPatient != null)
            {
                query = query.Where(v => v.PatientId == currentPatient.Id);
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
            query = query.Where(v => v.PatientId == patientId.Value);
        }

        if (doctorId.HasValue)
        {
            query = query.Where(v => v.DoctorId == doctorId.Value);
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(v => v.VisitDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(v => new
            {
                v.Id,
                v.PatientId,
                Patient = new { v.Patient.RealName },
                v.DoctorId,
                Doctor = new { v.Doctor.Name },
                v.ConsultationId,
                v.VisitDate,
                v.ChiefComplaint,
                v.Diagnosis,
                v.VisitType,
                v.Status,
                v.CreatedAt,
                v.UpdatedAt
            })
            .ToListAsync();

        return Ok(new { items, total, page, pageSize });
    }

    /// <summary>
    /// 根据ID获取就诊记录详情
    /// </summary>
    /// <param name="id">就诊记录ID</param>
    /// <returns>就诊记录详情</returns>
    [HttpGet("{id}")]
    [RequirePermission("visitrecord.view")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VisitRecord>> GetVisitRecordById(Guid id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userRole = User.FindFirstValue(ClaimTypes.Role);
        
        if (string.IsNullOrEmpty(currentUserId) || !Guid.TryParse(currentUserId, out var currentUserIdGuid))
        {
            return Unauthorized();
        }

        var visitRecord = await _context.VisitRecords
                .Include(v => v.Patient)
            .Include(v => v.Doctor)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (visitRecord == null)
        {
            return NotFound(new { message = "就诊记录不存在" });
        }

        // 权限检查
        if (userRole != "Admin" && userRole != "SuperAdmin")
        {
            // 获取当前用户对应的患者信息
            var currentPatient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == currentUserIdGuid);
            
            if (currentPatient == null || visitRecord.PatientId != currentPatient.Id)
            {
                return Forbid();
            }
        }

        return Ok(visitRecord);
    }

    /// <summary>
    /// 创建就诊记录
    /// </summary>
    /// <param name="dto">就诊记录信息</param>
    /// <returns>创建的就诊记录</returns>
    [HttpPost]
    [RequirePermission("visitrecord.create")]
    [Authorize(Roles = "Admin,SuperAdmin,Doctor")]
    [ProducesResponseType(typeof(VisitRecord), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<VisitRecord>> CreateVisitRecord([FromBody] CreateVisitRecordDto dto)
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

            // 如果不是管理员，医生只能为自己创建就诊记录
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

            // 创建就诊记录
            var newVisitRecord = new VisitRecord
            {
                Id = Guid.NewGuid(),
                PatientId = dto.PatientId,
                DoctorId = dto.DoctorId,
                ConsultationId = dto.ConsultationId,
                VisitDate = dto.VisitDate,
                ChiefComplaint = string.IsNullOrWhiteSpace(dto.ChiefComplaint) ? null : dto.ChiefComplaint,
                PresentIllness = string.IsNullOrWhiteSpace(dto.PresentIllness) ? null : dto.PresentIllness,
                Diagnosis = string.IsNullOrWhiteSpace(dto.Diagnosis) ? null : dto.Diagnosis,
                TreatmentPlan = string.IsNullOrWhiteSpace(dto.TreatmentPlan) ? null : dto.TreatmentPlan,
                MedicalAdvice = string.IsNullOrWhiteSpace(dto.MedicalAdvice) ? null : dto.MedicalAdvice,
                VisitType = dto.VisitType,
                Status = dto.Status,
                Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.VisitRecords.Add(newVisitRecord);
            await _context.SaveChangesAsync();

            // 重新加载包含关联信息的就诊记录
            var createdRecord = await _context.VisitRecords
                .Include(v => v.Patient)
            .Include(v => v.Doctor)
                .FirstOrDefaultAsync(v => v.Id == newVisitRecord.Id);

            _logger.LogInformation("就诊记录创建成功: Id={Id}, PatientId={PatientId}, DoctorId={DoctorId}", 
                newVisitRecord.Id, newVisitRecord.PatientId, newVisitRecord.DoctorId);

            return CreatedAtAction(nameof(GetVisitRecordById), new { id = newVisitRecord.Id }, createdRecord);
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "数据库更新失败");
            return BadRequest(new { message = "数据库操作失败", error = dbEx.InnerException?.Message ?? dbEx.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建就诊记录失败");
            return BadRequest(new { message = "创建就诊记录失败", error = ex.Message });
        }
    }

    /// <summary>
    /// 更新就诊记录
    /// </summary>
    /// <param name="id">就诊记录ID</param>
    /// <param name="dto">就诊记录信息</param>
    /// <returns>更新后的就诊记录</returns>
    [HttpPut("{id}")]
    [RequirePermission("visitrecord.update")]
    [Authorize(Roles = "Admin,SuperAdmin,Doctor")]
    [ProducesResponseType(typeof(VisitRecord), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VisitRecord>> UpdateVisitRecord(
        Guid id, 
        [FromBody] CreateVisitRecordDto dto)
    {
        try
        {
            ModelState.Clear();

            var visitRecord = await _context.VisitRecords.FindAsync(id);
            if (visitRecord == null)
            {
                return NotFound(new { message = "就诊记录不存在" });
            }

            // 验证必填字段
            if (dto == null)
            {
                return BadRequest(new { message = "请求数据不能为空" });
            }

            // 更新就诊记录
            visitRecord.PatientId = dto.PatientId;
            visitRecord.DoctorId = dto.DoctorId;
            visitRecord.ConsultationId = dto.ConsultationId;
            visitRecord.VisitDate = dto.VisitDate;
            visitRecord.ChiefComplaint = string.IsNullOrWhiteSpace(dto.ChiefComplaint) ? null : dto.ChiefComplaint;
            visitRecord.PresentIllness = string.IsNullOrWhiteSpace(dto.PresentIllness) ? null : dto.PresentIllness;
            visitRecord.Diagnosis = string.IsNullOrWhiteSpace(dto.Diagnosis) ? null : dto.Diagnosis;
            visitRecord.TreatmentPlan = string.IsNullOrWhiteSpace(dto.TreatmentPlan) ? null : dto.TreatmentPlan;
            visitRecord.MedicalAdvice = string.IsNullOrWhiteSpace(dto.MedicalAdvice) ? null : dto.MedicalAdvice;
            visitRecord.VisitType = dto.VisitType;
            visitRecord.Status = dto.Status;
            visitRecord.Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes;
            visitRecord.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // 重新加载包含关联信息的就诊记录
            var updatedRecord = await _context.VisitRecords
                .Include(v => v.Patient)
            .Include(v => v.Doctor)
                .FirstOrDefaultAsync(v => v.Id == id);

            _logger.LogInformation("就诊记录更新成功: Id={Id}", id);

            return Ok(updatedRecord);
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "数据库更新失败");
            return BadRequest(new { message = "数据库操作失败", error = dbEx.InnerException?.Message ?? dbEx.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新就诊记录失败");
            return BadRequest(new { message = "更新就诊记录失败", error = ex.Message });
        }
    }

    /// <summary>
    /// 删除就诊记录
    /// </summary>
    /// <param name="id">就诊记录ID</param>
    /// <returns>删除结果</returns>
    [HttpDelete("{id}")]
    [RequirePermission("visitrecord.delete")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteVisitRecord(Guid id)
    {
        var visitRecord = await _context.VisitRecords.FindAsync(id);
        if (visitRecord == null)
        {
            return NotFound(new { message = "就诊记录不存在" });
        }

        _context.VisitRecords.Remove(visitRecord);
        await _context.SaveChangesAsync();

        _logger.LogInformation("就诊记录删除成功: Id={Id}", id);

        return NoContent();
    }
}

