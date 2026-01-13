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
/// 咨询控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class ConsultationsController : ControllerBase
{
    private readonly MedicalDbContext _context;
    private readonly ILogger<ConsultationsController> _logger;

    public ConsultationsController(MedicalDbContext context, ILogger<ConsultationsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// 获取咨询列表（管理员可查看所有，普通用户只能查看自己的）
    /// </summary>
    /// <param name="keyword">关键词搜索（用户名、医生名）</param>
    /// <param name="page">页码</param>
    /// <param name="pageSize">每页数量</param>
    /// <returns>咨询列表</returns>
    [HttpGet]
    [RequirePermission("consultation.view")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetConsultations(
        [FromQuery] string? keyword = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userRole = User.FindFirstValue(ClaimTypes.Role);
        
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userIdGuid))
        {
            return Unauthorized();
        }

        var query = _context.Consultations
            .Include(c => c.Doctor)
                .ThenInclude(d => d.Department)
            .Include(c => c.ConsultationPatients)
                .ThenInclude(cp => cp.Patient)
            .AsQueryable();

        // 如果不是管理员，只返回当前用户的咨询
        if (userRole != "Admin" && userRole != "SuperAdmin")
        {
            // 获取当前用户对应的患者信息
            var currentPatient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == userIdGuid);
            
            if (currentPatient != null)
            {
                query = query.Where(c => c.ConsultationPatients.Any(cp => cp.PatientId == currentPatient.Id));
            }
            else
            {
                // 如果没有患者信息，返回空列表
                return Ok(new { items = new List<object>(), total = 0, page, pageSize });
            }
        }

        // 关键词搜索
        if (!string.IsNullOrEmpty(keyword))
        {
            query = query.Where(c => 
                (c.ConsultationPatients.Any(cp => cp.Patient.RealName.Contains(keyword))) ||
                (c.Doctor != null && c.Doctor.Name.Contains(keyword)));
        }

        // 获取总数
        var total = await query.CountAsync();

        var consultations = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // 返回分页格式，兼容前端
        return Ok(new { items = consultations, total, page, pageSize });
    }

    /// <summary>
    /// 根据ID获取咨询详情
    /// </summary>
    /// <param name="id">咨询ID</param>
    /// <returns>咨询详情（包含消息）</returns>
    [HttpGet("{id}")]
    [RequirePermission("consultation.view")]
    [ProducesResponseType(typeof(Consultation), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Consultation>> GetConsultationById(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userRole = User.FindFirstValue(ClaimTypes.Role);
        
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userIdGuid))
        {
            return Unauthorized();
        }

        var query = _context.Consultations
            .Include(c => c.ConsultationPatients)
                .ThenInclude(cp => cp.Patient)
            .Include(c => c.Doctor)
                .ThenInclude(d => d.Department)
            .Include(c => c.Messages)
            .AsQueryable();

        // 如果不是管理员，只能查看自己的咨询
        if (userRole != "Admin" && userRole != "SuperAdmin")
        {
            // 获取当前用户对应的患者信息
            var currentPatient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == userIdGuid);
            
            if (currentPatient != null)
            {
                query = query.Where(c => c.ConsultationPatients.Any(cp => cp.PatientId == currentPatient.Id));
            }
            else
            {
                return NotFound(new { message = "未找到对应的患者信息" });
            }
        }

        var consultation = await query.FirstOrDefaultAsync(c => c.Id == id);

        if (consultation == null)
        {
            return NotFound(new { message = "咨询不存在" });
        }

        return Ok(consultation);
    }

    /// <summary>
    /// 创建咨询
    /// </summary>
    /// <param name="dto">咨询信息</param>
    /// <returns>创建的咨询</returns>
    [HttpPost]
    [RequirePermission("consultation.create")]
    [ProducesResponseType(typeof(Consultation), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Consultation>> CreateConsultation([FromBody] CreateConsultationDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .SelectMany(x => x.Value!.Errors.Select(e => $"{x.Key}: {e.ErrorMessage}"));
                _logger.LogWarning("模型验证失败: {Errors}", string.Join(", ", errors));
                return BadRequest(new { message = "数据验证失败", errors });
            }

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

            // 验证医生是否存在
            var doctor = await _context.Doctors.FindAsync(dto.DoctorId);
            if (doctor == null)
            {
                return BadRequest(new { message = "医生不存在" });
            }

            // 创建新的咨询对象
            var newConsultation = new Consultation
            {
                Id = Guid.NewGuid(),
                DoctorId = dto.DoctorId,
                ConsultationType = dto.ConsultationType,
                Price = dto.Price,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Consultations.Add(newConsultation);
            await _context.SaveChangesAsync();

            // 如果提供了 PatientId，创建咨询-患者关联
            if (dto.PatientId != null && dto.PatientId != Guid.Empty)
            {
                // 验证 patientId 是否存在
                var patientExists = await _context.Patients.AnyAsync(p => p.Id == dto.PatientId.Value);
                if (!patientExists)
                {
                    return BadRequest(new { message = $"患者ID {dto.PatientId} 不存在" });
                }

                var consultationPatient = new ConsultationPatient
                {
                    Id = Guid.NewGuid(),
                    ConsultationId = newConsultation.Id,
                    PatientId = dto.PatientId.Value,
                    CreatedAt = DateTime.UtcNow
                };
                _context.ConsultationPatients.Add(consultationPatient);
                await _context.SaveChangesAsync();
            }
            // 如果没有提供 PatientId，尝试使用当前用户的患者信息（如果存在）
            else
            {
                var currentPatient = await _context.Patients
                    .FirstOrDefaultAsync(p => p.UserId == currentUserIdGuid);
                
                if (currentPatient != null)
                {
                    var consultationPatient = new ConsultationPatient
                    {
                        Id = Guid.NewGuid(),
                        ConsultationId = newConsultation.Id,
                        PatientId = currentPatient.Id,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.ConsultationPatients.Add(consultationPatient);
                    await _context.SaveChangesAsync();
                }
                // 如果当前用户也没有患者信息，允许创建咨询但不创建患者关联（管理员场景）
            }

            // 重新加载包含患者和医生信息的咨询
            var createdConsultation = await _context.Consultations
                .Include(c => c.ConsultationPatients)
                    .ThenInclude(cp => cp.Patient)
                .Include(c => c.Doctor)
                    .ThenInclude(d => d.Department)
                .FirstOrDefaultAsync(c => c.Id == newConsultation.Id);

            _logger.LogInformation("咨询创建成功: Id={Id}, DoctorId={DoctorId}", 
                newConsultation.Id, newConsultation.DoctorId);

            return CreatedAtAction(nameof(GetConsultationById), new { id = newConsultation.Id }, createdConsultation);
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "数据库更新失败");
            return BadRequest(new { message = "数据库操作失败", error = dbEx.InnerException?.Message ?? dbEx.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建咨询失败");
            return BadRequest(new { message = "创建咨询失败", error = ex.Message });
        }
    }

    /// <summary>
    /// 发送咨询消息
    /// </summary>
    /// <param name="consultationId">咨询ID</param>
    /// <param name="message">消息内容</param>
    /// <returns>创建的消息</returns>
    [HttpPost("{consultationId}/messages")]
    [ProducesResponseType(typeof(ConsultationMessage), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConsultationMessage>> SendMessage(Guid consultationId, [FromBody] ConsultationMessage message)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userIdGuid))
        {
            return Unauthorized();
        }

        var consultation = await _context.Consultations
            .Include(c => c.Doctor)
            .FirstOrDefaultAsync(c => c.Id == consultationId);

        if (consultation == null)
        {
            return NotFound(new { message = "咨询不存在" });
        }

        // 获取当前用户对应的患者信息
        var currentPatient = await _context.Patients
            .FirstOrDefaultAsync(p => p.UserId == userIdGuid);
        
        if (currentPatient == null)
        {
            return BadRequest(new { message = "未找到对应的患者信息" });
        }

        // 获取医生对应的患者信息（通过 DoctorPatient 中间表查询）
        var doctorPatient = await _context.DoctorPatients
            .Where(dp => dp.DoctorId == consultation.DoctorId && dp.Patient.UserId == userIdGuid)
            .Select(dp => dp.Patient)
            .FirstOrDefaultAsync();

        // 检查权限：只有咨询的患者或医生可以发送消息
        var isPatientOfConsultation = await _context.ConsultationPatients
            .AnyAsync(cp => cp.ConsultationId == consultation.Id && cp.PatientId == currentPatient.Id);
        
        if (!isPatientOfConsultation && doctorPatient == null)
        {
            return Forbid();
        }

        var isFromDoctor = doctorPatient != null && doctorPatient.UserId == userIdGuid;
        
        message.Id = Guid.NewGuid();
        message.ConsultationId = consultationId;
        // 使用当前登录用户的 UserId 作为发送者ID（无论是医生还是患者）
        message.SenderId = userIdGuid;
        message.IsFromDoctor = isFromDoctor;
        message.Status = "Pending"; // 默认状态
        message.CreatedAt = DateTime.UtcNow;

        // 如果咨询没有开始时间，设置开始时间
        if (consultation.StartTime == null)
        {
            consultation.StartTime = DateTime.UtcNow;
        }
        consultation.UpdatedAt = DateTime.UtcNow;

        _context.ConsultationMessages.Add(message);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetConsultationById), new { id = consultationId }, message);
    }

    /// <summary>
    /// 更新咨询
    /// </summary>
    /// <param name="id">咨询ID</param>
    /// <param name="dto">咨询信息</param>
    /// <returns>更新后的咨询</returns>
    [HttpPut("{id}")]
    [RequirePermission("consultation.update")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(Consultation), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Consultation>> UpdateConsultation(Guid id, [FromBody] CreateConsultationDto dto)
    {
        try
        {
            var existingConsultation = await _context.Consultations.FindAsync(id);
            if (existingConsultation == null)
            {
                return NotFound(new { message = "咨询不存在" });
            }

            // 如果提供了 patientId，更新咨询-患者关联
            if (dto.PatientId.HasValue && dto.PatientId != Guid.Empty)
            {
                var patientExists = await _context.Patients.AnyAsync(p => p.Id == dto.PatientId.Value);
                if (!patientExists)
                {
                    return BadRequest(new { message = $"患者ID {dto.PatientId} 不存在" });
                }
                
                // 删除旧的关联
                var oldAssociations = await _context.ConsultationPatients
                    .Where(cp => cp.ConsultationId == id)
                    .ToListAsync();
                _context.ConsultationPatients.RemoveRange(oldAssociations);
                
                // 创建新的关联
                var newAssociation = new ConsultationPatient
                {
                    Id = Guid.NewGuid(),
                    ConsultationId = id,
                    PatientId = dto.PatientId.Value,
                    CreatedAt = DateTime.UtcNow
                };
                _context.ConsultationPatients.Add(newAssociation);
            }

            // 如果提供了 doctorId，验证医生是否存在
            if (dto.DoctorId != Guid.Empty)
            {
                var doctorExists = await _context.Doctors.AnyAsync(d => d.Id == dto.DoctorId);
                if (!doctorExists)
                {
                    return BadRequest(new { message = $"医生ID {dto.DoctorId} 不存在" });
                }
                existingConsultation.DoctorId = dto.DoctorId;
            }

            existingConsultation.ConsultationType = dto.ConsultationType;
            existingConsultation.Price = dto.Price;
            existingConsultation.StartTime = dto.StartTime;
            existingConsultation.EndTime = dto.EndTime;
            existingConsultation.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // 重新加载包含患者和医生信息
            var updatedConsultation = await _context.Consultations
                .Include(c => c.ConsultationPatients)
                    .ThenInclude(cp => cp.Patient)
                .Include(c => c.Doctor)
                    .ThenInclude(d => d.Department)
                .FirstOrDefaultAsync(c => c.Id == id);

            _logger.LogInformation("咨询更新成功: Id={Id}", id);

            return Ok(updatedConsultation);
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "数据库更新失败");
            return BadRequest(new { message = "数据库操作失败", error = dbEx.InnerException?.Message ?? dbEx.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新咨询失败");
            return BadRequest(new { message = "更新咨询失败", error = ex.Message });
        }
    }

    /// <summary>
    /// 删除咨询
    /// </summary>
    /// <param name="id">咨询ID</param>
    /// <returns>删除结果</returns>
    [HttpDelete("{id}")]
    [RequirePermission("consultation.delete")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteConsultation(Guid id)
    {
        var consultation = await _context.Consultations.FindAsync(id);
        if (consultation == null)
        {
            return NotFound(new { message = "咨询不存在" });
        }

        _context.Consultations.Remove(consultation);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

