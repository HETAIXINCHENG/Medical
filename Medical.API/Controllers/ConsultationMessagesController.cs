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
/// 咨询消息控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class ConsultationMessagesController : ControllerBase
{
    private readonly MedicalDbContext _context;
    private readonly ILogger<ConsultationMessagesController> _logger;

    public ConsultationMessagesController(MedicalDbContext context, ILogger<ConsultationMessagesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// 获取咨询消息列表
    /// </summary>
    /// <param name="consultationId">咨询ID（可选，用于筛选特定咨询的消息）</param>
    /// <param name="page">页码</param>
    /// <param name="pageSize">每页数量</param>
    /// <returns>消息列表</returns>
    [HttpGet]
    [RequirePermission("consultationmessage.view")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetConsultationMessages(
        [FromQuery] Guid? consultationId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userRole = User.FindFirstValue(ClaimTypes.Role);
        
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userIdGuid))
        {
            return Unauthorized();
        }

        var query = _context.ConsultationMessages
            .Include(m => m.Consultation)
                .ThenInclude(c => c.ConsultationPatients)
                    .ThenInclude(cp => cp.Patient)
            .Include(m => m.Consultation)
                .ThenInclude(c => c.Doctor)
            .AsQueryable();

        // 如果不是管理员，只能查看自己相关的消息
        if (userRole != "Admin" && userRole != "SuperAdmin")
        {
            // 获取当前用户对应的患者信息
            var currentPatient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == userIdGuid);
            
            if (currentPatient != null)
            {
                // 获取当前患者关联的医生ID列表（通过 DoctorPatient 中间表）
                var doctorIds = await _context.DoctorPatients
                    .Where(dp => dp.PatientId == currentPatient.Id)
                    .Select(dp => dp.DoctorId)
                    .ToListAsync();
                
                query = query.Where(m => 
                    m.Consultation.ConsultationPatients.Any(cp => cp.PatientId == currentPatient.Id) || 
                    doctorIds.Contains(m.Consultation.DoctorId));
            }
            else
            {
                // 如果没有患者信息，返回空列表
                return Ok(new { items = new List<object>(), total = 0, page, pageSize });
            }
        }

        // 如果指定了咨询ID，筛选该咨询的消息
        if (consultationId.HasValue)
        {
            query = query.Where(m => m.ConsultationId == consultationId.Value);
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new
            {
                m.Id,
                m.ConsultationId,
                Consultation = new
                {
                    m.Consultation.Id,
                    Patients = m.Consultation.ConsultationPatients.Select(cp => new { cp.Patient.RealName }).ToList(),
                    Doctor = new { m.Consultation.Doctor.Name }
                },
                m.SenderId,
                m.IsFromDoctor,
                m.Content,
                m.MessageType,
                m.AttachmentUrl,
                m.IsRead,
                m.Status,
                m.CreatedAt
            })
            .ToListAsync();

        return Ok(new { items, total, page, pageSize });
    }

    /// <summary>
    /// 根据ID获取咨询消息详情
    /// </summary>
    /// <param name="id">消息ID</param>
    /// <returns>消息详情</returns>
    [HttpGet("{id}")]
    [RequirePermission("consultationmessage.view")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConsultationMessage>> GetConsultationMessageById(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userRole = User.FindFirstValue(ClaimTypes.Role);
        
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userIdGuid))
        {
            return Unauthorized();
        }

        var message = await _context.ConsultationMessages
            .Include(m => m.Consultation)
                .ThenInclude(c => c.ConsultationPatients)
                    .ThenInclude(cp => cp.Patient)
            .Include(m => m.Consultation)
                .ThenInclude(c => c.Doctor)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (message == null)
        {
            return NotFound(new { message = "消息不存在" });
        }

        // 权限检查：只有管理员或消息相关的患者/医生可以查看
        if (userRole != "Admin" && userRole != "SuperAdmin")
        {
            // 获取当前用户对应的患者信息
            var currentPatient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == userIdGuid);
            
            if (currentPatient == null)
            {
                return Forbid();
            }

            // 获取医生对应的患者信息（通过 DoctorPatient 中间表查询）
            var doctorPatient = await _context.DoctorPatients
                .Where(dp => dp.DoctorId == message.Consultation.DoctorId && dp.Patient.UserId == userIdGuid)
                .Select(dp => dp.Patient)
                .FirstOrDefaultAsync();

            var isPatientOfConsultation = message.Consultation.ConsultationPatients
                .Any(cp => cp.PatientId == currentPatient.Id);
            
            if (!isPatientOfConsultation && doctorPatient == null)
            {
                return Forbid();
            }
        }

        return Ok(message);
    }

    /// <summary>
    /// 创建咨询消息
    /// </summary>
    /// <param name="dto">消息信息</param>
    /// <returns>创建的消息</returns>
    [HttpPost]
    [RequirePermission("consultationmessage.create")]
    [ProducesResponseType(typeof(ConsultationMessage), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ConsultationMessage>> CreateConsultationMessage([FromBody] CreateConsultationMessageDto dto)
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

            if (string.IsNullOrWhiteSpace(dto.Content))
            {
                return BadRequest(new { message = "消息内容不能为空" });
            }

            if (string.IsNullOrWhiteSpace(dto.MessageType))
            {
                return BadRequest(new { message = "消息类型不能为空" });
            }

            // 验证咨询是否存在
            var consultation = await _context.Consultations
                .Include(c => c.Doctor)
                .FirstOrDefaultAsync(c => c.Id == dto.ConsultationId);

            if (consultation == null)
            {
                return BadRequest(new { message = "咨询不存在" });
            }

            // 获取当前用户对应的患者信息（用于权限判断）
            var currentPatient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == currentUserIdGuid);

            // 获取医生对应的患者信息（通过 DoctorPatient 中间表查询）
            var doctorPatient = await _context.DoctorPatients
                .Where(dp => dp.DoctorId == consultation.DoctorId && dp.Patient.UserId == currentUserIdGuid)
                .Select(dp => dp.Patient)
                .FirstOrDefaultAsync();

            // 权限检查：只有咨询的患者或医生可以发送消息
            if (userRole != "Admin" && userRole != "SuperAdmin")
            {
                var isPatientOfConsultation = currentPatient != null
                    ? await _context.ConsultationPatients
                        .AnyAsync(cp => cp.ConsultationId == consultation.Id && cp.PatientId == currentPatient.Id)
                    : false;

                var isConsultDoctor = doctorPatient != null && doctorPatient.UserId == currentUserIdGuid;

                if (!isPatientOfConsultation && !isConsultDoctor)
                {
                    return Forbid();
                }
            }

            // 判断是否为医生发送（当前用户是否为该咨询的医生）
            var isFromDoctor = doctorPatient != null && doctorPatient.UserId == currentUserIdGuid;

            // 创建新消息（SenderId 始终为当前登录用户的 UserId）
            var newMessage = new ConsultationMessage
            {
                Id = Guid.NewGuid(),
                ConsultationId = dto.ConsultationId,
                SenderId = currentUserIdGuid,
                IsFromDoctor = isFromDoctor,
                Content = dto.Content,
                MessageType = dto.MessageType,
                AttachmentUrl = string.IsNullOrWhiteSpace(dto.AttachmentUrl) ? null : dto.AttachmentUrl,
                IsRead = dto.IsRead,
                Status = dto.Status ?? "Pending",
                CreatedAt = DateTime.UtcNow
            };

            // 如果咨询没有开始时间，设置开始时间
            if (consultation.StartTime == null)
            {
                consultation.StartTime = DateTime.UtcNow;
            }
            consultation.UpdatedAt = DateTime.UtcNow;

            _context.ConsultationMessages.Add(newMessage);
            await _context.SaveChangesAsync();

            // 重新加载包含咨询信息的消息
            var createdMessage = await _context.ConsultationMessages
                .Include(m => m.Consultation)
                    .ThenInclude(c => c.ConsultationPatients)
                        .ThenInclude(cp => cp.Patient)
                .Include(m => m.Consultation)
                    .ThenInclude(c => c.Doctor)
                .FirstOrDefaultAsync(m => m.Id == newMessage.Id);

            _logger.LogInformation("咨询消息创建成功: Id={Id}, ConsultationId={ConsultationId}, MessageType={MessageType}", 
                newMessage.Id, newMessage.ConsultationId, newMessage.MessageType);

            return CreatedAtAction(nameof(GetConsultationMessageById), new { id = newMessage.Id }, createdMessage);
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "数据库更新失败");
            return BadRequest(new { message = "数据库操作失败", error = dbEx.InnerException?.Message ?? dbEx.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建咨询消息失败");
            return BadRequest(new { message = "创建咨询消息失败", error = ex.Message });
        }
    }

    /// <summary>
    /// 更新咨询消息
    /// </summary>
    /// <param name="id">消息ID</param>
    /// <param name="dto">消息信息</param>
    /// <returns>更新后的消息</returns>
    [HttpPut("{id}")]
    [RequirePermission("consultationmessage.update")]
    [ProducesResponseType(typeof(ConsultationMessage), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConsultationMessage>> UpdateConsultationMessage(
        Guid id, 
        [FromBody] CreateConsultationMessageDto dto)
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

            var message = await _context.ConsultationMessages
                .Include(m => m.Consultation)
                    .ThenInclude(c => c.Doctor)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (message == null)
            {
                return NotFound(new { message = "消息不存在" });
            }

            // 权限检查：只有管理员或消息发送者可以更新
            if (userRole != "Admin" && userRole != "SuperAdmin")
            {
                // 仅允许消息发送者本人修改消息
                if (message.SenderId != currentUserIdGuid)
                {
                    return Forbid();
                }
            }

            // 验证必填字段
            if (dto == null)
            {
                return BadRequest(new { message = "请求数据不能为空" });
            }

            if (string.IsNullOrWhiteSpace(dto.Content))
            {
                return BadRequest(new { message = "消息内容不能为空" });
            }

            // 更新消息
            message.Content = dto.Content;
            message.MessageType = dto.MessageType;
            message.AttachmentUrl = string.IsNullOrWhiteSpace(dto.AttachmentUrl) ? null : dto.AttachmentUrl;
            message.IsRead = dto.IsRead;
            if (!string.IsNullOrWhiteSpace(dto.Status))
            {
                message.Status = dto.Status;
            }

            await _context.SaveChangesAsync();

            // 重新加载包含咨询信息的消息
            var updatedMessage = await _context.ConsultationMessages
                .Include(m => m.Consultation)
                    .ThenInclude(c => c.ConsultationPatients)
                        .ThenInclude(cp => cp.Patient)
                .Include(m => m.Consultation)
                    .ThenInclude(c => c.Doctor)
                .FirstOrDefaultAsync(m => m.Id == id);

            _logger.LogInformation("咨询消息更新成功: Id={Id}", id);

            return Ok(updatedMessage);
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "数据库更新失败");
            return BadRequest(new { message = "数据库操作失败", error = dbEx.InnerException?.Message ?? dbEx.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新咨询消息失败");
            return BadRequest(new { message = "更新咨询消息失败", error = ex.Message });
        }
    }

    /// <summary>
    /// 删除咨询消息
    /// </summary>
    /// <param name="id">消息ID</param>
    /// <returns>删除结果</returns>
    [HttpDelete("{id}")]
    [RequirePermission("consultationmessage.delete")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteConsultationMessage(Guid id)
    {
        var message = await _context.ConsultationMessages.FindAsync(id);
        if (message == null)
        {
            return NotFound(new { message = "消息不存在" });
        }

        _context.ConsultationMessages.Remove(message);
        await _context.SaveChangesAsync();

        _logger.LogInformation("咨询消息删除成功: Id={Id}", id);

        return NoContent();
    }
}

