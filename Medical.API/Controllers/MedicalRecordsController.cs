using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Medical.API.Attributes;
using Medical.API.Data;
using Medical.API.Models.DTOs;
using Medical.API.Models.Entities;
using System.Security.Claims;

namespace Medical.API.Controllers;

/// <summary>
/// 病历信息管理
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class MedicalRecordsController : ControllerBase
{
    private readonly MedicalDbContext _context;

    public MedicalRecordsController(MedicalDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// 获取病历列表
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetMedicalRecords(
        [FromQuery] Guid? patientId = null,
        [FromQuery] Guid? doctorId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userRole = User.FindFirstValue(ClaimTypes.Role);

        var query = _context.MedicalRecords
            .Include(m => m.Patient)
            .Include(m => m.Doctor)
            .AsQueryable();

        // 如果是患者角色，只允许查看自己的病历，忽略传入的 patientId
        if (!string.IsNullOrEmpty(userRole) && userRole == "Patient" && Guid.TryParse(userId, out var userIdGuid))
        {
            var currentPatient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == userIdGuid);
            if (currentPatient == null)
            {
                return Forbid();
            }

            query = query.Where(m => m.PatientId == currentPatient.Id);
        }
        else
        {
            // 管理员 / 医生 等角色可以按条件过滤
            if (patientId.HasValue && patientId.Value != Guid.Empty)
            {
                query = query.Where(m => m.PatientId == patientId.Value);
            }

            if (doctorId.HasValue && doctorId.Value != Guid.Empty)
            {
                query = query.Where(m => m.DoctorId == doctorId.Value);
            }
        }

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new
            {
                m.Id,
                m.PatientId,
                Patient = new { m.Patient.RealName },
                m.DoctorId,
                Doctor = m.Doctor == null ? null : new { m.Doctor.Name },
                m.ConsultationId,
                m.CreatedAt,
                m.DiseaseName,
                m.DiseaseDescription
            })
            .ToListAsync();

        return Ok(new { items, total, page, pageSize });
    }

    /// <summary>
    /// 根据ID获取病历详情
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetMedicalRecord(Guid id)
    {
        var record = await _context.MedicalRecords
            .Include(m => m.Patient)
            .Include(m => m.Doctor)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (record == null)
        {
            return NotFound(new { message = "病历不存在" });
        }

        // 手动构造返回对象，包含 Patient 信息（因为导航属性有 JsonIgnore）
        var result = new
        {
            record.Id,
            record.PatientId,
            Patient = record.Patient != null ? new
            {
                record.Patient.Id,
                record.Patient.RealName,
                record.Patient.Gender,
                record.Patient.DateOfBirth,
                record.Patient.Address
            } : null,
            record.DoctorId,
            Doctor = record.Doctor != null ? new
            {
                record.Doctor.Id,
                record.Doctor.Name
            } : null,
            record.ConsultationId,
            record.CreatedAt,
            record.Height,
            record.Weight,
            record.DiseaseDuration,
            record.DiseaseName,
            record.HasVisitedHospital,
            record.CurrentMedications,
            record.IsPregnant,
            record.MajorTreatmentHistory,
            record.AllergyHistory,
            record.DiseaseDescription,
            record.PastMedicalHistory,
            record.AdditionalNotes
        };

        return Ok(result);
    }

    /// <summary>
    /// 创建病历
    /// </summary>
    [HttpPost]
    [Authorize] // 需要登录
    [ProducesResponseType(typeof(MedicalRecord), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MedicalRecord>> CreateMedicalRecord([FromBody] CreateMedicalRecordDto dto)
    {
        if (dto == null)
        {
            return BadRequest(new { message = "请求数据不能为空" });
        }

        // 从当前登录用户解析患者ID（始终绑定到当前用户的患者信息）
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            return Unauthorized(new { message = "用户未登录或Token无效" });
        }

        var currentPatient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == userId);
        if (currentPatient == null)
        {
            return BadRequest(new { message = "未找到对应的患者信息，请先完善患者信息" });
        }

        if (dto.DoctorId.HasValue)
        {
            var doctorExists = await _context.Doctors.AnyAsync(d => d.Id == dto.DoctorId.Value);
            if (!doctorExists)
            {
                return BadRequest(new { message = "医生不存在" });
            }
        }

        var record = new MedicalRecord
        {
            Id = Guid.NewGuid(),
            PatientId = currentPatient.Id,
            DoctorId = dto.DoctorId,
            ConsultationId = dto.ConsultationId,
            CreatedAt = DateTime.UtcNow,
            Height = dto.Height,
            Weight = dto.Weight,
            DiseaseDuration = dto.DiseaseDuration,
            DiseaseName = dto.DiseaseName,
            HasVisitedHospital = dto.HasVisitedHospital,
            CurrentMedications = dto.CurrentMedications,
            IsPregnant = dto.IsPregnant,
            MajorTreatmentHistory = dto.MajorTreatmentHistory,
            AllergyHistory = dto.AllergyHistory,
            DiseaseDescription = dto.DiseaseDescription,
            PastMedicalHistory = dto.PastMedicalHistory,
            AdditionalNotes = dto.AdditionalNotes
        };

        _context.MedicalRecords.Add(record);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMedicalRecord), new { id = record.Id }, record);
    }

    /// <summary>
    /// 更新病历（允许患者追加补充）
    /// </summary>
    [HttpPut("{id}")]
    [Authorize] // 需要登录，但允许患者自己更新
    [ProducesResponseType(typeof(MedicalRecord), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MedicalRecord>> UpdateMedicalRecord(Guid id, [FromBody] UpdateMedicalRecordDto dto)
    {
        var existing = await _context.MedicalRecords.FindAsync(id);
        if (existing == null)
        {
            return NotFound(new { message = "病历不存在" });
        }

        // 权限检查：只有管理员或病历所属患者可以更新
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            return Unauthorized(new { message = "用户未登录或Token无效" });
        }

        var userRole = User.FindFirstValue(ClaimTypes.Role);
        if (userRole != "Admin" && userRole != "SuperAdmin")
        {
            // 检查当前用户是否是病历所属患者
            var currentPatient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == userId);
            if (currentPatient == null || existing.PatientId != currentPatient.Id)
            {
                return Forbid();
            }
        }

        // 只允许更新业务字段，不修改主键
        // 如果字段有值则更新，支持追加补充（将新内容追加到原有内容）
        if (dto.Height.HasValue) existing.Height = dto.Height;
        if (dto.Weight.HasValue) existing.Weight = dto.Weight;
        if (!string.IsNullOrWhiteSpace(dto.DiseaseDuration)) existing.DiseaseDuration = dto.DiseaseDuration;
        if (!string.IsNullOrWhiteSpace(dto.DiseaseName))
        {
            // 追加疾病名称：如果原有内容不为空，用逗号分隔追加
            existing.DiseaseName = string.IsNullOrWhiteSpace(existing.DiseaseName)
                ? dto.DiseaseName
                : $"{existing.DiseaseName}, {dto.DiseaseName}";
        }
        if (dto.HasVisitedHospital.HasValue) existing.HasVisitedHospital = dto.HasVisitedHospital;
        if (!string.IsNullOrWhiteSpace(dto.CurrentMedications)) existing.CurrentMedications = dto.CurrentMedications;
        if (dto.IsPregnant.HasValue) existing.IsPregnant = dto.IsPregnant;
        if (!string.IsNullOrWhiteSpace(dto.MajorTreatmentHistory)) existing.MajorTreatmentHistory = dto.MajorTreatmentHistory;
        if (!string.IsNullOrWhiteSpace(dto.AllergyHistory)) existing.AllergyHistory = dto.AllergyHistory;
        if (!string.IsNullOrWhiteSpace(dto.DiseaseDescription))
        {
            // 追加病情描述：如果原有内容不为空，用换行分隔追加
            existing.DiseaseDescription = string.IsNullOrWhiteSpace(existing.DiseaseDescription)
                ? dto.DiseaseDescription
                : $"{existing.DiseaseDescription}\n{dto.DiseaseDescription}";
        }
        if (!string.IsNullOrWhiteSpace(dto.PastMedicalHistory)) existing.PastMedicalHistory = dto.PastMedicalHistory;
        if (!string.IsNullOrWhiteSpace(dto.AdditionalNotes)) existing.AdditionalNotes = dto.AdditionalNotes;

        await _context.SaveChangesAsync();

        // 重新加载包含导航属性的完整数据
        var updated = await _context.MedicalRecords
            .Include(m => m.Patient)
            .Include(m => m.Doctor)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (updated == null)
        {
            return NotFound(new { message = "病历不存在" });
        }

        // 返回和 GetMedicalRecord 相同格式的数据
        var result = new
        {
            updated.Id,
            updated.PatientId,
            Patient = updated.Patient != null ? new
            {
                updated.Patient.Id,
                updated.Patient.RealName,
                updated.Patient.Gender,
                updated.Patient.DateOfBirth,
                updated.Patient.Address
            } : null,
            updated.DoctorId,
            Doctor = updated.Doctor != null ? new
            {
                updated.Doctor.Id,
                updated.Doctor.Name
            } : null,
            updated.ConsultationId,
            updated.CreatedAt,
            updated.Height,
            updated.Weight,
            updated.DiseaseDuration,
            updated.DiseaseName,
            updated.HasVisitedHospital,
            updated.CurrentMedications,
            updated.IsPregnant,
            updated.MajorTreatmentHistory,
            updated.AllergyHistory,
            updated.DiseaseDescription,
            updated.PastMedicalHistory,
            updated.AdditionalNotes
        };

        return Ok(result);
    }

    /// <summary>
    /// 删除病历
    /// </summary>
    [HttpDelete("{id}")]
    [RequirePermission("medicalrecord.delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMedicalRecord(Guid id)
    {
        var record = await _context.MedicalRecords.FindAsync(id);
        if (record == null)
        {
            return NotFound(new { message = "病历不存在" });
        }

        _context.MedicalRecords.Remove(record);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}


