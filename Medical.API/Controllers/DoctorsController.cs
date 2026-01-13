using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Medical.API.Data;
using Medical.API.Models.DTOs;
using Medical.API.Models.Entities;
using Medical.API.Attributes;

namespace Medical.API.Controllers;

/// <summary>
/// 医生控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DoctorsController : ControllerBase
{
    private readonly MedicalDbContext _context;
    private readonly ILogger<DoctorsController> _logger;

    public DoctorsController(MedicalDbContext context, ILogger<DoctorsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// 获取医生列表
    /// </summary>
    /// <param name="departmentId">科室ID（可选）</param>
    /// <param name="isRecommended">是否推荐（可选）</param>
    /// <param name="keyword">关键词搜索（姓名、医院、擅长）</param>
    /// <param name="page">页码（默认1）</param>
    /// <param name="pageSize">每页数量（默认10）</param>
    /// <returns>医生列表</returns>
    [HttpGet]
    [AllowAnonymous] // 允许匿名访问，首页需要显示推荐医生
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetDoctors(
        [FromQuery] Guid? departmentId = null,
        [FromQuery] bool? isRecommended = null,
        [FromQuery] string? keyword = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = _context.Doctors
            .Include(d => d.Department)
            .AsQueryable();

        if (departmentId.HasValue)
        {
            query = query.Where(d => d.DepartmentId == departmentId.Value);
        }

        if (isRecommended.HasValue)
        {
            query = query.Where(d => d.IsRecommended == isRecommended.Value);
        }

        // 关键词搜索
        if (!string.IsNullOrEmpty(keyword))
        {
            query = query.Where(d => 
                d.Name.Contains(keyword) ||
                (d.Hospital != null && d.Hospital.Contains(keyword)) ||
                (d.Specialty != null && d.Specialty.Contains(keyword)));
        }

        // 获取总数
        var total = await query.CountAsync();

        // 获取分页数据
        var doctors = await query
            .OrderByDescending(d => d.Rating)
            .ThenByDescending(d => d.ConsultationCount)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(d => new DoctorDto
            {
                Id = d.Id,
                Name = d.Name,
                Title = d.Title,
                Specialty = d.Specialty,
                Hospital = d.Hospital,
                Introduction = d.Introduction,
                AvatarUrl = d.AvatarUrl,
                Rating = d.Rating,
                ConsultationCount = d.ConsultationCount,
                IsOnline = d.IsOnline,
                IsRecommended = d.IsRecommended,
                DepartmentId = d.DepartmentId,
                DepartmentName = d.Department.Name,
                FollowerCount = d.FollowerCount,
                TotalReadCount = d.TotalReadCount
            })
            .ToListAsync();

        // 返回包含总数和数据的对象
        return Ok(new { items = doctors, total, page, pageSize });
    }

    /// <summary>
    /// 根据ID获取医生详情
    /// </summary>
    /// <param name="id">医生ID</param>
    /// <returns>医生详情</returns>
    [HttpGet("{id}")]
    [AllowAnonymous] // 允许匿名访问，患者端需要查看医生详情
    [ProducesResponseType(typeof(DoctorDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DoctorDetailDto>> GetDoctor(Guid id)
    {
        var doctor = await _context.Doctors
            .Include(d => d.Department)
            .Include(d => d.Schedules)
            .Include(d => d.Reviews)
                .ThenInclude(r => r.Patient)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (doctor == null)
        {
            return NotFound(new { message = "医生不存在" });
        }

        // 获取该医生的咨询价格（按类型）
        var phoneConsultation = await _context.Consultations
            .Where(c => c.DoctorId == doctor.Id && c.ConsultationType == "Phone")
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync();
        
        var textConsultation = await _context.Consultations
            .Where(c => c.DoctorId == doctor.Id && c.ConsultationType == "Text")
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync();

        var doctorDto = new DoctorDetailDto
        {
            Id = doctor.Id,
            Name = doctor.Name,
            Title = doctor.Title,
            Specialty = doctor.Specialty,
            Hospital = doctor.Hospital,
            Introduction = doctor.Introduction,
            AvatarUrl = doctor.AvatarUrl,
            Rating = doctor.Rating,
            ConsultationCount = doctor.ConsultationCount,
            IsOnline = doctor.IsOnline,
            IsRecommended = doctor.IsRecommended,
            DepartmentId = doctor.DepartmentId,
            DepartmentName = doctor.Department.Name,
            PhonePrice = phoneConsultation?.Price,
            TextPrice = textConsultation?.Price,
            Schedules = doctor.Schedules.Select(s => new DoctorScheduleDto
            {
                Id = s.Id,
                DayOfWeek = s.DayOfWeek,
                TimeSlot = s.TimeSlot,
                IsAvailable = s.IsAvailable && s.CurrentAppointments < s.MaxAppointments
            }).ToList(),
            RecentReviews = doctor.Reviews
                .OrderByDescending(r => r.CreatedAt)
                .Take(10)
                .Select(r => new DoctorReviewDto
                {
                    Id = r.Id,
                    Rating = r.Rating,
                    Content = r.Content,
                    ReviewerName = r.Patient.RealName ?? "匿名",
                    CreatedAt = r.CreatedAt
                }).ToList()
        };

        return Ok(doctorDto);
    }

    /// <summary>
    /// 搜索医生
    /// </summary>
    /// <param name="keyword">关键词（姓名、医院、擅长）</param>
    /// <param name="page">页码</param>
    /// <param name="pageSize">每页数量</param>
    /// <returns>医生列表</returns>
    [HttpGet("search")]
    [AllowAnonymous] // 允许匿名访问，患者端需要搜索医生
    [ProducesResponseType(typeof(List<DoctorDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<DoctorDto>>> SearchDoctors(
        [FromQuery] string keyword,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return BadRequest(new { message = "搜索关键词不能为空" });
        }

        var doctors = await _context.Doctors
            .Include(d => d.Department)
            .Where(d => d.Name.Contains(keyword) || 
                       (d.Hospital != null && d.Hospital.Contains(keyword)) ||
                       (d.Specialty != null && d.Specialty.Contains(keyword)))
            .OrderByDescending(d => d.Rating)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(d => new DoctorDto
            {
                Id = d.Id,
                Name = d.Name,
                Title = d.Title,
                Specialty = d.Specialty,
                Hospital = d.Hospital,
                Introduction = d.Introduction,
                AvatarUrl = d.AvatarUrl,
                Rating = d.Rating,
                ConsultationCount = d.ConsultationCount,
                IsOnline = d.IsOnline,
                IsRecommended = d.IsRecommended,
                DepartmentId = d.DepartmentId,
                DepartmentName = d.Department.Name
            })
            .ToListAsync();

        return Ok(doctors);
    }

    /// <summary>
    /// 创建医生
    /// </summary>
    /// <param name="doctor">医生信息</param>
    /// <returns>创建的医生</returns>
    [HttpPost]
    [RequirePermission("doctor.create")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(Doctor), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Doctor>> CreateDoctor([FromBody] CreateDoctorDto dto)
    {
        _logger.LogInformation("创建医生请求: Name={Name}, Title={Title}, DepartmentId={DepartmentId}",
            dto?.Name, dto?.Title, dto?.DepartmentId);

        // 验证模型状态
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .SelectMany(x => x.Value!.Errors.Select(e => $"{x.Key}: {e.ErrorMessage}"));
            _logger.LogWarning("模型验证失败: {Errors}", string.Join(", ", errors));
            return BadRequest(new { message = "数据验证失败", errors });
        }

        if (dto == null)
        {
            _logger.LogWarning("创建医生失败: 请求数据为空");
            return BadRequest(new { message = "请求数据不能为空" });
        }

        // 验证科室是否存在
        if (dto.DepartmentId == Guid.Empty)
        {
            return BadRequest(new { message = "科室不能为空" });
        }

        var department = await _context.Departments.FindAsync(dto.DepartmentId);
        if (department == null)
        {
            _logger.LogWarning("科室不存在: {DepartmentId}", dto.DepartmentId);
            return BadRequest(new { message = $"科室不存在 (ID: {dto.DepartmentId})" });
        }

        // 创建医生实体（Doctor 和 Patient 现在是多对多关系，不再直接绑定）
        var doctor = new Doctor
        {
            Id = Guid.NewGuid(),
            DepartmentId = dto.DepartmentId,
            Name = dto.Name,
            Title = dto.Title,
            Hospital = dto.Hospital,
            Specialty = dto.Specialty,
            Introduction = dto.Introduction,
            AvatarUrl = dto.AvatarUrl,
            IsRecommended = dto.IsRecommended,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Doctors.Add(doctor);
        await _context.SaveChangesAsync();

        // 自动创建该医生的患友会
        var supportGroup = new PatientSupportGroup
        {
            Id = Guid.NewGuid(),
            DoctorId = doctor.Id,
            Name = $"{doctor.Name}医生的患友会",
            Description = $"欢迎加入{doctor.Name}医生的患友会，与医生和其他患者交流。",
            PostCount = 0,
            TotalReadCount = 0,
            IsEnabled = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.PatientSupportGroups.Add(supportGroup);

        // 创建默认会规
        var rules = new GroupRules
        {
            Id = Guid.NewGuid(),
            PatientSupportGroupId = supportGroup.Id,
            Content = GetDefaultRulesContent(doctor.Name),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.GroupRules.Add(rules);

        await _context.SaveChangesAsync();

        // 重新加载医生数据（包含导航属性）
        var createdDoctor = await _context.Doctors
            .Include(d => d.Department)
            .FirstOrDefaultAsync(d => d.Id == doctor.Id);

        _logger.LogInformation("医生创建成功: Id={Id}, Name={Name}, 已自动创建患友会", doctor.Id, doctor.Name);

        // 返回 DTO 而不是实体，避免循环引用
        var doctorDto = new DoctorDto
        {
            Id = createdDoctor!.Id,
            Name = createdDoctor.Name,
            Title = createdDoctor.Title,
            Specialty = createdDoctor.Specialty,
            Hospital = createdDoctor.Hospital,
            Introduction = createdDoctor.Introduction,
            AvatarUrl = createdDoctor.AvatarUrl,
            Rating = createdDoctor.Rating,
            ConsultationCount = createdDoctor.ConsultationCount,
            IsOnline = createdDoctor.IsOnline,
            IsRecommended = createdDoctor.IsRecommended,
            DepartmentId = createdDoctor.DepartmentId,
            DepartmentName = createdDoctor.Department?.Name
        };

        return CreatedAtAction(nameof(GetDoctor), new { id = doctor.Id }, doctorDto);
    }

    /// <summary>
    /// 获取默认会规内容（与 PatientSupportGroupsController 保持一致）
    /// </summary>
    private string GetDefaultRulesContent(string doctorName)
    {
        return $@"{doctorName}医生患友会会规

1、严格按照版块分区要求发帖，乱占用版块发帖将会被删除。严重乱水帖者直接封号处理。没意义且影响阅读性，任由超水帖杂草般在患友会里丛生，不利于患友会整体发展。

2、违反患友会协议，包括但不限于色情帖、暴力帖、辱骂吵架帖、广告帖、毫无节操的猥琐帖，地域攻击帖，各种骗子帖等各种不利于患友会发展的不良信息帖将一律删除，严重者将予以封号处理。双方互怼互骂、恶意滋扰患友会秩序、重复发布攻击帖将进行拉黑处理。

3、咨询看病问题，请购买医生的服务。";
    }

    /// <summary>
    /// 更新医生信息
    /// </summary>
    /// <param name="id">医生ID</param>
    /// <param name="dto">医生信息</param>
    /// <returns>更新后的医生</returns>
    [HttpPut("{id}")]
    [RequirePermission("doctor.update")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(DoctorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DoctorDto>> UpdateDoctor(Guid id, [FromBody] CreateDoctorDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var existingDoctor = await _context.Doctors
            .Include(d => d.Department)
            .FirstOrDefaultAsync(d => d.Id == id);
        
        if (existingDoctor == null)
        {
            return NotFound(new { message = "医生不存在" });
        }

        // Doctor 和 Patient 现在是多对多关系，不再直接绑定 PatientId

        // 验证科室是否存在
        if (dto.DepartmentId != Guid.Empty && dto.DepartmentId != existingDoctor.DepartmentId)
        {
            var department = await _context.Departments.FindAsync(dto.DepartmentId);
            if (department == null)
            {
                return BadRequest(new { message = "科室不存在" });
            }
        }

        existingDoctor.Name = dto.Name;
        existingDoctor.Title = dto.Title;
        existingDoctor.Specialty = dto.Specialty;
        existingDoctor.Hospital = dto.Hospital;
        existingDoctor.Introduction = dto.Introduction;
        existingDoctor.AvatarUrl = dto.AvatarUrl;
        existingDoctor.DepartmentId = dto.DepartmentId;
        existingDoctor.IsRecommended = dto.IsRecommended;
        existingDoctor.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // 重新加载以获取最新的导航属性
        await _context.Entry(existingDoctor).Reference(d => d.Department).LoadAsync();

        // 返回 DTO 而不是实体
        var doctorDto = new DoctorDto
        {
            Id = existingDoctor.Id,
            Name = existingDoctor.Name,
            Title = existingDoctor.Title,
            Specialty = existingDoctor.Specialty,
            Hospital = existingDoctor.Hospital,
            Introduction = existingDoctor.Introduction,
            AvatarUrl = existingDoctor.AvatarUrl,
            Rating = existingDoctor.Rating,
            ConsultationCount = existingDoctor.ConsultationCount,
            IsOnline = existingDoctor.IsOnline,
            IsRecommended = existingDoctor.IsRecommended,
            DepartmentId = existingDoctor.DepartmentId,
            DepartmentName = existingDoctor.Department?.Name
        };

        return Ok(doctorDto);
    }

    /// <summary>
    /// 删除医生
    /// </summary>
    /// <param name="id">医生ID</param>
    /// <returns>删除结果</returns>
    [HttpDelete("{id}")]
    [RequirePermission("doctor.delete")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteDoctor(Guid id)
    {
        var doctor = await _context.Doctors.FindAsync(id);
        if (doctor == null)
        {
            return NotFound(new { message = "医生不存在" });
        }

        _context.Doctors.Remove(doctor);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// 获取当前患者咨询过的医生列表
    /// </summary>
    [HttpGet("my-consulted-doctors")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetMyConsultedDoctors()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            return Unauthorized(new { message = "用户未登录或Token无效" });
        }

        // 获取当前用户对应的患者信息
        var currentPatient = await _context.Patients
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (currentPatient == null)
        {
            return Ok(new List<object>());
        }

        // 通过 ConsultationPatients 找到该患者的所有咨询，然后获取医生信息
        var consultedDoctors = await _context.ConsultationPatients
            .Where(cp => cp.PatientId == currentPatient.Id)
            .Include(cp => cp.Consultation)
                .ThenInclude(c => c.Doctor)
                    .ThenInclude(d => d.Department)
            .Include(cp => cp.Consultation)
                .ThenInclude(c => c.Doctor)
                    .ThenInclude(d => d.Reviews)
            .Select(cp => cp.Consultation.Doctor)
            .Where(d => d != null && !d.IsDeleted)
            .Distinct()
            .Select(d => new
            {
                d.Id,
                d.Name,
                d.Title,
                d.Hospital,
                d.Specialty,
                d.AvatarUrl,
                d.Rating,
                ReviewCount = d.Reviews.Count, // 从导航属性计算评价数量
                Department = new
                {
                    d.Department.Id,
                    d.Department.Name
                },
                // 统计该患者与该医生的咨询次数
                ConsultationCount = _context.ConsultationPatients
                    .Count(cp => cp.PatientId == currentPatient.Id && 
                                 cp.Consultation.DoctorId == d.Id),
                // 最后一次咨询时间
                LastConsultationTime = _context.ConsultationPatients
                    .Where(cp => cp.PatientId == currentPatient.Id && 
                                 cp.Consultation.DoctorId == d.Id)
                    .OrderByDescending(cp => cp.Consultation.CreatedAt)
                    .Select(cp => cp.Consultation.CreatedAt)
                    .FirstOrDefault()
            })
            .OrderByDescending(d => d.LastConsultationTime)
            .ToListAsync();

        return Ok(consultedDoctors);
    }
}

