using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Medical.API.Data;
using Medical.API.Models.Entities;
using Medical.API.Models.DTOs;
using Medical.API.Attributes;
using Medical.API.Services;
using Medical.API.Models.Enums;

namespace Medical.API.Controllers;

/// <summary>
/// 患者信息控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PatientsController : ControllerBase
{
    private readonly MedicalDbContext _context;
    private readonly ILogger<PatientsController> _logger;

    public PatientsController(MedicalDbContext context, ILogger<PatientsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// 获取患者信息列表
    /// </summary>
    /// <param name="keyword">关键词搜索（真实姓名、电话、身份证号）</param>
    /// <param name="page">页码</param>
    /// <param name="pageSize">每页数量</param>
    /// <returns>患者信息列表</returns>
    [HttpGet]
    [RequirePermission("patient.view")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetPatients(
        [FromQuery] string? keyword = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = _context.Patients
            .Include(p => p.User)
            .Include(p => p.FamilyMembers)
            .AsQueryable();

        // 关键词搜索
        if (!string.IsNullOrEmpty(keyword))
        {
            query = query.Where(p =>
                (p.RealName != null && p.RealName.Contains(keyword)) ||
                (p.PhoneNumber != null && p.PhoneNumber.Contains(keyword)) ||
                (p.IdCardNumber != null && p.IdCardNumber.Contains(keyword)) ||
                (p.User != null && p.User.Username.Contains(keyword))
            );
        }

        var total = await query.CountAsync();
        var allPatients = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // 返回数据
        var items = allPatients.Select(p => new
        {
            p.Id,
            PatientId = p.UserId,
            User = new { p.User?.Username },
            Patient = new { RealName = p.RealName },
            RealName = p.RealName,
            p.Gender,
            p.DateOfBirth,
            IdCardNumber = p.IdCardNumber,
            PhoneNumber = p.PhoneNumber,
            Email = p.Email,
            p.Address,
            p.BloodType,
            p.Height,
            p.Weight,
            p.BMI,
            p.AllergyHistory,
            p.MedicalHistory,
            p.FamilyHistory,
            // 从 FamilyMembers 表读取紧急联系人信息
            EmergencyContactName = p.FamilyMembers != null && p.FamilyMembers.Any() 
                ? p.FamilyMembers.First().Name 
                : null,
            EmergencyContactPhone = p.FamilyMembers != null && p.FamilyMembers.Any() && !string.IsNullOrEmpty(p.FamilyMembers.First().PhoneNumber)
                ? p.FamilyMembers.First().PhoneNumber 
                : null,
            EmergencyContactRelation = p.FamilyMembers != null && p.FamilyMembers.Any()
                ? p.FamilyMembers.First().Relationship.ToString()
                : null,
            EmergencyContactGender = p.FamilyMembers != null && p.FamilyMembers.Any()
                ? p.FamilyMembers.First().Gender
                : null,
            EmergencyContactDateOfBirth = p.FamilyMembers != null && p.FamilyMembers.Any()
                ? p.FamilyMembers.First().DateOfBirth
                : null,
            EmergencyContactIdCardNumber = p.FamilyMembers != null && p.FamilyMembers.Any() && !string.IsNullOrEmpty(p.FamilyMembers.First().IdCardNumber)
                ? p.FamilyMembers.First().IdCardNumber
                : null,
            p.Notes,
            p.CreatedAt,
            p.UpdatedAt
        }).ToList();

        return Ok(new { items, total, page, pageSize });
    }

    /// <summary>
    /// 根据ID获取患者信息详情
    /// </summary>
    /// <param name="id">患者ID</param>
    /// <returns>患者信息详情</returns>
    [HttpGet("{id}")]
    [RequirePermission("patient.view")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetPatientById(Guid id)
    {
        var patient = await _context.Patients
            .Include(p => p.User)
            .Include(p => p.FamilyMembers)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (patient == null)
        {
            return NotFound(new { message = "患者信息不存在" });
        }

        // 返回数据
        var patientDto = new
        {
            patient.Id,
            PatientId = patient.UserId,
            User = patient.User != null ? new { patient.User.Username } : null,
            Patient = new { RealName = patient.RealName },
            RealName = patient.RealName,
            patient.Gender,
            patient.DateOfBirth,
            IdCardNumber = patient.IdCardNumber,
            PhoneNumber = patient.PhoneNumber,
            Email = patient.Email,
            patient.Address,
            patient.BloodType,
            patient.Height,
            patient.Weight,
            patient.BMI,
            patient.AllergyHistory,
            patient.MedicalHistory,
            patient.FamilyHistory,
            // 从 FamilyMembers 表读取紧急联系人信息
            EmergencyContactName = patient.FamilyMembers != null && patient.FamilyMembers.Any()
                ? patient.FamilyMembers.First().Name
                : null,
            EmergencyContactPhone = patient.FamilyMembers != null && patient.FamilyMembers.Any() && !string.IsNullOrEmpty(patient.FamilyMembers.First().PhoneNumber)
                ? patient.FamilyMembers.First().PhoneNumber
                : null,
            EmergencyContactRelation = patient.FamilyMembers != null && patient.FamilyMembers.Any()
                ? patient.FamilyMembers.First().Relationship.ToString()
                : null,
            EmergencyContactGender = patient.FamilyMembers != null && patient.FamilyMembers.Any()
                ? patient.FamilyMembers.First().Gender
                : null,
            EmergencyContactDateOfBirth = patient.FamilyMembers != null && patient.FamilyMembers.Any()
                ? patient.FamilyMembers.First().DateOfBirth
                : null,
            EmergencyContactIdCardNumber = patient.FamilyMembers != null && patient.FamilyMembers.Any() && !string.IsNullOrEmpty(patient.FamilyMembers.First().IdCardNumber)
                ? patient.FamilyMembers.First().IdCardNumber
                : null,
            patient.Notes,
            patient.CreatedAt,
            patient.UpdatedAt
        };

        return Ok(patientDto);
    }

    /// <summary>
    /// 根据用户ID获取患者信息
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>患者信息</returns>
    [HttpGet("by-user/{userId}")]
    [Authorize] // 需要登录，但不需要特定权限
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetPatientByUserId(Guid userId)
    {
        var patient = await _context.Patients
            .Include(p => p.User)
            .Include(p => p.FamilyMembers)
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (patient == null)
        {
            return NotFound(new { message = "患者信息不存在" });
        }

        // 返回数据
        var patientDto = new
        {
            patient.Id,
            PatientId = patient.UserId,
            User = patient.User != null ? new { patient.User.Username } : null,
            Patient = new { RealName = patient.RealName },
            RealName = patient.RealName,
            patient.Gender,
            patient.DateOfBirth,
            IdCardNumber = patient.IdCardNumber,
            PhoneNumber = patient.PhoneNumber,
            Email = patient.Email,
            patient.Address,
            patient.BloodType,
            patient.Height,
            patient.Weight,
            patient.BMI,
            patient.AllergyHistory,
            patient.MedicalHistory,
            patient.FamilyHistory,
            // 从 FamilyMembers 表读取紧急联系人信息
            EmergencyContactName = patient.FamilyMembers != null && patient.FamilyMembers.Any()
                ? patient.FamilyMembers.First().Name
                : null,
            EmergencyContactPhone = patient.FamilyMembers != null && patient.FamilyMembers.Any() && !string.IsNullOrEmpty(patient.FamilyMembers.First().PhoneNumber)
                ? patient.FamilyMembers.First().PhoneNumber
                : null,
            EmergencyContactRelation = patient.FamilyMembers != null && patient.FamilyMembers.Any()
                ? patient.FamilyMembers.First().Relationship.ToString()
                : null,
            EmergencyContactGender = patient.FamilyMembers != null && patient.FamilyMembers.Any()
                ? patient.FamilyMembers.First().Gender
                : null,
            EmergencyContactDateOfBirth = patient.FamilyMembers != null && patient.FamilyMembers.Any()
                ? patient.FamilyMembers.First().DateOfBirth
                : null,
            EmergencyContactIdCardNumber = patient.FamilyMembers != null && patient.FamilyMembers.Any() && !string.IsNullOrEmpty(patient.FamilyMembers.First().IdCardNumber)
                ? patient.FamilyMembers.First().IdCardNumber
                : null,
            patient.Notes,
            patient.CreatedAt,
            patient.UpdatedAt
        };

        return Ok(patientDto);
    }

    /// <summary>
    /// 创建患者信息
    /// </summary>
    /// <param name="dto">患者信息</param>
    /// <returns>创建的患者信息</returns>
    [HttpPost]
    [Authorize] // 需要登录，允许创建Patient记录
    [ProducesResponseType(typeof(Patient), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Patient>> CreatePatient([FromBody] CreatePatientDto dto)
    {
        try
        {
            ModelState.Clear();

            // 验证必填字段
            if (dto == null)
            {
                return BadRequest(new { message = "请求数据不能为空" });
            }

            if (string.IsNullOrWhiteSpace(dto.RealName))
            {
                return BadRequest(new { message = "真实姓名不能为空" });
            }

            // 验证患者是否存在
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == dto.PatientId);
            if (user == null)
            {
                return BadRequest(new { message = "用户不存在" });
            }

            if (user.Role != "Patient")
            {
                return BadRequest(new { message = "该用户不是患者角色" });
            }

            // 确保用户的UserTypeId为3（Patient类型）
            if (user.UserTypeId != 3)
            {
                user.UserTypeId = 3; // 更新为Patient类型
                await _context.SaveChangesAsync();
            }

            // 检查该用户是否已有患者信息
            var existingPatient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == dto.PatientId);
            if (existingPatient != null)
            {
                return BadRequest(new { message = "该用户已有患者信息，请使用更新接口" });
            }

            // 创建患者信息
            var newPatient = new Patient
            {
                Id = Guid.NewGuid(),
                UserId = dto.PatientId,
                RealName = dto.RealName,
                Gender = string.IsNullOrWhiteSpace(dto.Gender) ? null : dto.Gender,
                DateOfBirth = dto.DateOfBirth,
                IdCardNumber = string.IsNullOrWhiteSpace(dto.IdCardNumber) ? null : dto.IdCardNumber,
                PhoneNumber = string.IsNullOrWhiteSpace(dto.PhoneNumber) ? null : dto.PhoneNumber,
                Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email,
                Address = string.IsNullOrWhiteSpace(dto.Address) ? null : dto.Address,
                // 不再保存紧急联系人信息到 Patient 表
                BloodType = string.IsNullOrWhiteSpace(dto.BloodType) ? null : dto.BloodType,
                Height = dto.Height,
                Weight = dto.Weight,
                AllergyHistory = string.IsNullOrWhiteSpace(dto.AllergyHistory) ? null : dto.AllergyHistory,
                MedicalHistory = string.IsNullOrWhiteSpace(dto.MedicalHistory) ? null : dto.MedicalHistory,
                FamilyHistory = string.IsNullOrWhiteSpace(dto.FamilyHistory) ? null : dto.FamilyHistory,
                Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Patients.Add(newPatient);
            await _context.SaveChangesAsync();

            // 如果有紧急联系人信息，保存到 FamilyMembers 表
            if (!string.IsNullOrWhiteSpace(dto.EmergencyContactName))
            {
                var relationship = RelationshipType.Other;
                if (!string.IsNullOrWhiteSpace(dto.EmergencyContactRelation) &&
                    Enum.TryParse<RelationshipType>(dto.EmergencyContactRelation, true, out var parsed))
                {
                    relationship = parsed;
                }

                var emergencyContact = new FamilyMember
                {
                    Id = Guid.NewGuid(),
                    PatientId = newPatient.Id,
                    Name = dto.EmergencyContactName.Trim(),
                    Relationship = relationship,
                    Gender = string.IsNullOrWhiteSpace(dto.EmergencyContactGender) ? null : dto.EmergencyContactGender,
                    DateOfBirth = dto.EmergencyContactDateOfBirth,
                    IdCardNumber = string.IsNullOrWhiteSpace(dto.EmergencyContactIdCardNumber) ? null : dto.EmergencyContactIdCardNumber,
                    PhoneNumber = string.IsNullOrWhiteSpace(dto.EmergencyContactPhone) ? null : dto.EmergencyContactPhone,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.FamilyMembers.Add(emergencyContact);
                await _context.SaveChangesAsync();
            }

            // 重新加载包含用户信息的患者
            var createdPatient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == newPatient.Id);

            _logger.LogInformation("患者信息创建成功: Id={Id}, UserId={UserId}, RealName={RealName}", 
                newPatient.Id, newPatient.UserId, newPatient.RealName);

            return CreatedAtAction(nameof(GetPatientById), new { id = newPatient.Id }, createdPatient);
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "数据库更新失败");
            return BadRequest(new { message = "数据库操作失败", error = dbEx.InnerException?.Message ?? dbEx.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建患者信息失败");
            return BadRequest(new { message = "创建患者信息失败", error = ex.Message });
        }
    }

    /// <summary>
    /// 更新患者信息
    /// </summary>
    /// <param name="id">患者ID</param>
    /// <param name="dto">患者信息</param>
    /// <returns>更新后的患者信息</returns>
    [HttpPut("{id}")]
    [RequirePermission("patient.update")]
    [ProducesResponseType(typeof(Patient), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Patient>> UpdatePatient(
        Guid id, 
        [FromBody] CreatePatientDto dto)
    {
        try
        {
            ModelState.Clear();

            var patient = await _context.Patients.FindAsync(id);
            if (patient == null)
            {
                return NotFound(new { message = "患者信息不存在" });
            }

            // 验证必填字段
            if (dto == null)
            {
                return BadRequest(new { message = "请求数据不能为空" });
            }

            if (string.IsNullOrWhiteSpace(dto.RealName))
            {
                return BadRequest(new { message = "真实姓名不能为空" });
            }

            // 如果更改了PatientId，验证新用户是否存在且为患者角色
            // 只有当 dto.PatientId 不为空且与当前 patient.UserId 不同时才验证
            if (dto.PatientId != Guid.Empty && dto.PatientId != patient.UserId)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == dto.PatientId);
                if (user == null)
                {
                    return BadRequest(new { message = "用户不存在" });
                }

                if (user.Role != "Patient")
                {
                    return BadRequest(new { message = "该用户不是患者角色" });
                }

                // 检查新用户是否已有患者信息
                var existingPatient = await _context.Patients
                    .FirstOrDefaultAsync(p => p.UserId == dto.PatientId && p.Id != id);
                if (existingPatient != null)
                {
                    return BadRequest(new { message = "该用户已有患者信息" });
                }
            }
            
            // 如果 dto.PatientId 为空或为 Guid.Empty，保持原有的 UserId 不变
            if (dto.PatientId == Guid.Empty)
            {
                dto.PatientId = patient.UserId;
            }

            // 更新患者信息
            patient.UserId = dto.PatientId;
            patient.RealName = dto.RealName;
            patient.Gender = string.IsNullOrWhiteSpace(dto.Gender) ? null : dto.Gender;
            patient.DateOfBirth = dto.DateOfBirth;
            patient.IdCardNumber = string.IsNullOrWhiteSpace(dto.IdCardNumber) ? null : dto.IdCardNumber;
            patient.PhoneNumber = string.IsNullOrWhiteSpace(dto.PhoneNumber) ? null : dto.PhoneNumber;
            patient.Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email;
            patient.Address = string.IsNullOrWhiteSpace(dto.Address) ? null : dto.Address;
            // 不再更新紧急联系人信息到 Patient 表
            patient.BloodType = string.IsNullOrWhiteSpace(dto.BloodType) ? null : dto.BloodType;
            patient.Height = dto.Height;
            patient.Weight = dto.Weight;
            patient.AllergyHistory = string.IsNullOrWhiteSpace(dto.AllergyHistory) ? null : dto.AllergyHistory;
            patient.MedicalHistory = string.IsNullOrWhiteSpace(dto.MedicalHistory) ? null : dto.MedicalHistory;
            patient.FamilyHistory = string.IsNullOrWhiteSpace(dto.FamilyHistory) ? null : dto.FamilyHistory;
            patient.Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes;
            patient.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // 处理紧急联系人信息（保存到 FamilyMembers 表）
            var existingEmergencyContact = await _context.FamilyMembers
                .FirstOrDefaultAsync(fm => fm.PatientId == patient.Id);

            var hasEmergencyContact = !string.IsNullOrWhiteSpace(dto.EmergencyContactName);
            if (hasEmergencyContact)
            {
                var relationship = RelationshipType.Other;
                if (!string.IsNullOrWhiteSpace(dto.EmergencyContactRelation) &&
                    Enum.TryParse<RelationshipType>(dto.EmergencyContactRelation, true, out var parsed))
                {
                    relationship = parsed;
                }

                if (existingEmergencyContact != null)
                {
                    existingEmergencyContact.Name = dto.EmergencyContactName.Trim();
                    existingEmergencyContact.Relationship = relationship;
                    existingEmergencyContact.Gender = string.IsNullOrWhiteSpace(dto.EmergencyContactGender) ? null : dto.EmergencyContactGender;
                    existingEmergencyContact.DateOfBirth = dto.EmergencyContactDateOfBirth;
                    existingEmergencyContact.IdCardNumber = string.IsNullOrWhiteSpace(dto.EmergencyContactIdCardNumber) ? null : dto.EmergencyContactIdCardNumber;
                    existingEmergencyContact.PhoneNumber = string.IsNullOrWhiteSpace(dto.EmergencyContactPhone) ? null : dto.EmergencyContactPhone;
                    existingEmergencyContact.UpdatedAt = DateTime.UtcNow;
                    _context.FamilyMembers.Update(existingEmergencyContact);
                }
                else
                {
                    var emergencyContact = new FamilyMember
                    {
                        Id = Guid.NewGuid(),
                        PatientId = patient.Id,
                        Name = dto.EmergencyContactName.Trim(),
                        Relationship = relationship,
                        Gender = string.IsNullOrWhiteSpace(dto.EmergencyContactGender) ? null : dto.EmergencyContactGender,
                        DateOfBirth = dto.EmergencyContactDateOfBirth,
                        IdCardNumber = string.IsNullOrWhiteSpace(dto.EmergencyContactIdCardNumber) ? null : dto.EmergencyContactIdCardNumber,
                        PhoneNumber = string.IsNullOrWhiteSpace(dto.EmergencyContactPhone) ? null : dto.EmergencyContactPhone,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.FamilyMembers.Add(emergencyContact);
                }
            }
            else if (existingEmergencyContact != null)
            {
                _context.FamilyMembers.Remove(existingEmergencyContact);
            }

            await _context.SaveChangesAsync();

            // 重新加载包含用户信息的患者
            var updatedPatient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            _logger.LogInformation("患者信息更新成功: Id={Id}", id);

            return Ok(updatedPatient);
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "数据库更新失败");
            return BadRequest(new { message = "数据库操作失败", error = dbEx.InnerException?.Message ?? dbEx.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新患者信息失败");
            return BadRequest(new { message = "更新患者信息失败", error = ex.Message });
        }
    }

    /// <summary>
    /// 删除患者信息
    /// </summary>
    /// <param name="id">患者ID</param>
    /// <returns>删除结果</returns>
    [HttpDelete("{id}")]
    [RequirePermission("patient.delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePatient(Guid id)
    {
        var patient = await _context.Patients.FindAsync(id);
        if (patient == null)
        {
            return NotFound(new { message = "患者信息不存在" });
        }

        _context.Patients.Remove(patient);
        await _context.SaveChangesAsync();

        _logger.LogInformation("患者信息删除成功: Id={Id}", id);

        return NoContent();
    }

    /// <summary>
    /// 获取当前登录用户的患者和家庭成员列表（用于诊前信息收集）
    /// </summary>
    /// <returns>患者和家庭成员列表</returns>
    [HttpGet("my-patients")]
    [Authorize] // 需要登录
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetMyPatients()
    {
        var currentUserIdentifier = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(currentUserIdentifier) || !Guid.TryParse(currentUserIdentifier, out var currentUserId))
        {
            return Unauthorized(new { message = "用户未登录或Token无效" });
        }

        var result = new List<object>();

        // 获取当前登录用户对应的患者信息
        var currentPatient = await _context.Patients
            .FirstOrDefaultAsync(p => p.UserId == currentUserId);
        
        if (currentPatient != null)
        {
            // 计算年龄
            int? patientAge = null;
            if (currentPatient.DateOfBirth.HasValue)
            {
                var today = DateTime.UtcNow;
                patientAge = today.Year - currentPatient.DateOfBirth.Value.Year;
                if (today.Month < currentPatient.DateOfBirth.Value.Month || 
                    (today.Month == currentPatient.DateOfBirth.Value.Month && today.Day < currentPatient.DateOfBirth.Value.Day))
                {
                    patientAge--;
                }
            }

            result.Add(new
            {
                id = currentPatient.Id,
                name = currentPatient.RealName,
                gender = currentPatient.Gender == "Male" ? "男" : currentPatient.Gender == "Female" ? "女" : currentPatient.Gender,
                age = patientAge,
                relation = "本人"
            });

            // 获取该患者的家庭成员
            var familyMembers = await _context.FamilyMembers
                .Where(fm => fm.PatientId == currentPatient.Id)
                .ToListAsync();

            foreach (var familyMember in familyMembers)
            {
                // 计算年龄
                int? familyMemberAge = null;
                if (familyMember.DateOfBirth.HasValue)
                {
                    var today = DateTime.UtcNow;
                    familyMemberAge = today.Year - familyMember.DateOfBirth.Value.Year;
                    if (today.Month < familyMember.DateOfBirth.Value.Month || 
                        (today.Month == familyMember.DateOfBirth.Value.Month && today.Day < familyMember.DateOfBirth.Value.Day))
                    {
                        familyMemberAge--;
                    }
                }

                // 将枚举转换为中文显示
                string relationshipText = familyMember.Relationship switch
                {
                    RelationshipType.Spouse => "配偶",
                    RelationshipType.Child => "子女",
                    RelationshipType.Parent => "父母",
                    RelationshipType.Other => "其他",
                    _ => "其他"
                };

                result.Add(new
                {
                    id = familyMember.Id,
                    name = familyMember.Name,
                    gender = familyMember.Gender == "Male" ? "男" : familyMember.Gender == "Female" ? "女" : familyMember.Gender,
                    age = familyMemberAge,
                    relation = relationshipText
                });
            }
        }

        return Ok(result);
    }

    /// <summary>
    /// 创建或更新当前登录用户的患者信息
    /// </summary>
    /// <param name="dto">患者信息</param>
    /// <returns>创建或更新后的患者信息</returns>
    [HttpPost("me")]
    [Authorize] // 需要登录
    [ProducesResponseType(typeof(Patient), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Patient>> CreateOrUpdateMyPatient([FromBody] CreateMyPatientDto dto)
    {
        try
        {
            var currentUserIdentifier = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserIdentifier) || !Guid.TryParse(currentUserIdentifier, out var currentUserId))
            {
                return Unauthorized(new { message = "用户未登录或Token无效" });
            }

            // 验证必填字段
            if (dto == null)
            {
                return BadRequest(new { message = "请求数据不能为空" });
            }

            if (string.IsNullOrWhiteSpace(dto.RealName))
            {
                return BadRequest(new { message = "真实姓名不能为空" });
            }

            // 检查当前登录用户是否已有患者信息
            var existingPatient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == currentUserId);

            if (existingPatient != null)
            {
                // 更新现有患者信息
                existingPatient.RealName = dto.RealName;
                existingPatient.Gender = string.IsNullOrWhiteSpace(dto.Gender) ? null : dto.Gender;
                existingPatient.DateOfBirth = dto.DateOfBirth;
                existingPatient.PhoneNumber = string.IsNullOrWhiteSpace(dto.PhoneNumber) ? null : dto.PhoneNumber;
                existingPatient.Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email;
                existingPatient.Address = string.IsNullOrWhiteSpace(dto.Address) ? null : dto.Address;
                existingPatient.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("患者信息更新成功: PatientId={PatientId}, RealName={RealName}", existingPatient.Id, existingPatient.RealName);
                return Ok(existingPatient);
            }
            else
            {
                // 创建新患者信息
                var newPatient = new Patient
                {
                    Id = Guid.NewGuid(),
                    UserId = currentUserId,
                    RealName = dto.RealName,
                    Gender = string.IsNullOrWhiteSpace(dto.Gender) ? null : dto.Gender,
                    DateOfBirth = dto.DateOfBirth,
                    PhoneNumber = string.IsNullOrWhiteSpace(dto.PhoneNumber) ? null : dto.PhoneNumber,
                    Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email,
                    Address = string.IsNullOrWhiteSpace(dto.Address) ? null : dto.Address,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Patients.Add(newPatient);
                await _context.SaveChangesAsync();

                _logger.LogInformation("患者信息创建成功: PatientId={PatientId}, RealName={RealName}", 
                    newPatient.Id, newPatient.RealName);

                return CreatedAtAction(nameof(GetPatientById), new { id = newPatient.Id }, newPatient);
            }
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "数据库更新失败");
            return BadRequest(new { message = "数据库操作失败", error = dbEx.InnerException?.Message ?? dbEx.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建或更新患者信息失败");
            return BadRequest(new { message = "创建或更新患者信息失败", error = ex.Message });
        }
    }
}

