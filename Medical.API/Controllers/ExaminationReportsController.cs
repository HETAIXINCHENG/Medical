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
/// 检查报告控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class ExaminationReportsController : ControllerBase
{
    private readonly MedicalDbContext _context;
    private readonly ILogger<ExaminationReportsController> _logger;

    public ExaminationReportsController(MedicalDbContext context, ILogger<ExaminationReportsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// 获取检查报告列表
    /// </summary>
    /// <param name="patientId">患者ID（可选）</param>
    /// <param name="doctorId">医生ID（可选）</param>
    /// <param name="examinationType">检查类型（可选）</param>
    /// <param name="page">页码</param>
    /// <param name="pageSize">每页数量</param>
    /// <returns>检查报告列表</returns>
    [HttpGet]
    [RequirePermission("examinationreport.view")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetExaminationReports(
        [FromQuery] Guid? patientId = null,
        [FromQuery] Guid? doctorId = null,
        [FromQuery] string? examinationType = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userRole = User.FindFirstValue(ClaimTypes.Role);
        
        if (string.IsNullOrEmpty(currentUserId) || !Guid.TryParse(currentUserId, out var currentUserIdGuid))
        {
            return Unauthorized();
        }

        var query = _context.ExaminationReports
            .Include(e => e.Patient)
            .Include(e => e.Doctor)
            .AsQueryable();

        // 如果不是管理员，只能查看自己的检查报告
        if (userRole != "Admin" && userRole != "SuperAdmin")
        {
            // 获取当前用户对应的患者信息
            var currentPatient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == currentUserIdGuid);
            
            if (currentPatient != null)
            {
                query = query.Where(e => e.PatientId == currentPatient.Id);
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
            query = query.Where(e => e.PatientId == patientId.Value);
        }

        if (doctorId.HasValue)
        {
            query = query.Where(e => e.DoctorId == doctorId.Value);
        }

        if (!string.IsNullOrEmpty(examinationType))
        {
            query = query.Where(e => e.ExaminationType == examinationType);
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(e => e.ExaminationDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new
            {
                e.Id,
                e.PatientId,
                Patient = new { e.Patient.RealName },
                e.DoctorId,
                Doctor = new { e.Doctor.Name },
                e.ReportNumber,
                e.ExaminationType,
                e.ExaminationName,
                e.ExaminationDate,
                e.ReportDate,
                e.Conclusion,
                e.Status,
                e.ReportFileUrl,
                e.CreatedAt
            })
            .ToListAsync();

        return Ok(new { items, total, page, pageSize });
    }

    /// <summary>
    /// 根据ID获取检查报告详情
    /// </summary>
    /// <param name="id">检查报告ID</param>
    /// <returns>检查报告详情</returns>
    [HttpGet("{id}")]
    [RequirePermission("examinationreport.view")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ExaminationReport>> GetExaminationReportById(Guid id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userRole = User.FindFirstValue(ClaimTypes.Role);
        
        if (string.IsNullOrEmpty(currentUserId) || !Guid.TryParse(currentUserId, out var currentUserIdGuid))
        {
            return Unauthorized();
        }

        var report = await _context.ExaminationReports
                .Include(e => e.Patient)
            .Include(e => e.Doctor)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (report == null)
        {
            return NotFound(new { message = "检查报告不存在" });
        }

        // 权限检查
        if (userRole != "Admin" && userRole != "SuperAdmin")
        {
            // 获取当前用户对应的患者信息
            var currentPatient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == currentUserIdGuid);
            
            if (currentPatient == null || report.PatientId != currentPatient.Id)
            {
                return Forbid();
            }
        }

        return Ok(report);
    }

    /// <summary>
    /// 创建检查报告
    /// </summary>
    /// <param name="dto">检查报告信息</param>
    /// <returns>创建的检查报告</returns>
    [HttpPost]
    [RequirePermission("examinationreport.create")]
    [Authorize(Roles = "Admin,SuperAdmin,Doctor")]
    [ProducesResponseType(typeof(ExaminationReport), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ExaminationReport>> CreateExaminationReport([FromBody] CreateExaminationReportDto dto)
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

            if (string.IsNullOrWhiteSpace(dto.ReportNumber))
            {
                return BadRequest(new { message = "报告编号不能为空" });
            }

            if (string.IsNullOrWhiteSpace(dto.ExaminationType))
            {
                return BadRequest(new { message = "检查类型不能为空" });
            }

            if (string.IsNullOrWhiteSpace(dto.ExaminationName))
            {
                return BadRequest(new { message = "检查项目名称不能为空" });
            }

            if (string.IsNullOrWhiteSpace(dto.Results))
            {
                return BadRequest(new { message = "检查结果不能为空" });
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

            // 如果不是管理员，医生只能为自己创建检查报告
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

            // 创建检查报告
            var newReport = new ExaminationReport
            {
                Id = Guid.NewGuid(),
                PatientId = dto.PatientId,
                DoctorId = dto.DoctorId,
                VisitRecordId = dto.VisitRecordId,
                ConsultationId = dto.ConsultationId,
                ReportNumber = dto.ReportNumber,
                ExaminationType = dto.ExaminationType,
                ExaminationName = dto.ExaminationName,
                ExaminationDate = dto.ExaminationDate,
                ReportDate = dto.ReportDate,
                Results = dto.Results,
                Conclusion = string.IsNullOrWhiteSpace(dto.Conclusion) ? null : dto.Conclusion,
                Recommendations = string.IsNullOrWhiteSpace(dto.Recommendations) ? null : dto.Recommendations,
                ReportFileUrl = string.IsNullOrWhiteSpace(dto.ReportFileUrl) ? null : dto.ReportFileUrl,
                Status = dto.Status,
                Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.ExaminationReports.Add(newReport);
            await _context.SaveChangesAsync();

            // 重新加载包含关联信息的检查报告
            var createdReport = await _context.ExaminationReports
                .Include(e => e.Patient)
            .Include(e => e.Doctor)
                .FirstOrDefaultAsync(e => e.Id == newReport.Id);

            _logger.LogInformation("检查报告创建成功: Id={Id}, ReportNumber={ReportNumber}", 
                newReport.Id, newReport.ReportNumber);

            return CreatedAtAction(nameof(GetExaminationReportById), new { id = newReport.Id }, createdReport);
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "数据库更新失败");
            return BadRequest(new { message = "数据库操作失败", error = dbEx.InnerException?.Message ?? dbEx.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建检查报告失败");
            return BadRequest(new { message = "创建检查报告失败", error = ex.Message });
        }
    }

    /// <summary>
    /// 更新检查报告
    /// </summary>
    /// <param name="id">检查报告ID</param>
    /// <param name="dto">检查报告信息</param>
    /// <returns>更新后的检查报告</returns>
    [HttpPut("{id}")]
    [RequirePermission("examinationreport.update")]
    [Authorize(Roles = "Admin,SuperAdmin,Doctor")]
    [ProducesResponseType(typeof(ExaminationReport), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ExaminationReport>> UpdateExaminationReport(
        Guid id, 
        [FromBody] CreateExaminationReportDto dto)
    {
        try
        {
            ModelState.Clear();

            var report = await _context.ExaminationReports.FindAsync(id);
            if (report == null)
            {
                return NotFound(new { message = "检查报告不存在" });
            }

            // 验证必填字段
            if (dto == null)
            {
                return BadRequest(new { message = "请求数据不能为空" });
            }

            // 更新检查报告
            report.PatientId = dto.PatientId;
            report.DoctorId = dto.DoctorId;
            report.VisitRecordId = dto.VisitRecordId;
            report.ConsultationId = dto.ConsultationId;
            report.ReportNumber = dto.ReportNumber;
            report.ExaminationType = dto.ExaminationType;
            report.ExaminationName = dto.ExaminationName;
            report.ExaminationDate = dto.ExaminationDate;
            report.ReportDate = dto.ReportDate;
            report.Results = dto.Results;
            report.Conclusion = string.IsNullOrWhiteSpace(dto.Conclusion) ? null : dto.Conclusion;
            report.Recommendations = string.IsNullOrWhiteSpace(dto.Recommendations) ? null : dto.Recommendations;
            report.ReportFileUrl = string.IsNullOrWhiteSpace(dto.ReportFileUrl) ? null : dto.ReportFileUrl;
            report.Status = dto.Status;
            report.Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes;
            report.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // 重新加载包含关联信息的检查报告
            var updatedReport = await _context.ExaminationReports
                .Include(e => e.Patient)
            .Include(e => e.Doctor)
                .FirstOrDefaultAsync(e => e.Id == id);

            _logger.LogInformation("检查报告更新成功: Id={Id}", id);

            return Ok(updatedReport);
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "数据库更新失败");
            return BadRequest(new { message = "数据库操作失败", error = dbEx.InnerException?.Message ?? dbEx.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新检查报告失败");
            return BadRequest(new { message = "更新检查报告失败", error = ex.Message });
        }
    }

    /// <summary>
    /// 删除检查报告
    /// </summary>
    /// <param name="id">检查报告ID</param>
    /// <returns>删除结果</returns>
    [HttpDelete("{id}")]
    [RequirePermission("examinationreport.delete")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteExaminationReport(Guid id)
    {
        var report = await _context.ExaminationReports.FindAsync(id);
        if (report == null)
        {
            return NotFound(new { message = "检查报告不存在" });
        }

        _context.ExaminationReports.Remove(report);
        await _context.SaveChangesAsync();

        _logger.LogInformation("检查报告删除成功: Id={Id}", id);

        return NoContent();
    }
}

