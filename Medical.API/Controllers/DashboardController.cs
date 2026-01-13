using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Medical.API.Data;
using Medical.API.Attributes;

namespace Medical.API.Controllers;

/// <summary>
/// 仪表盘统计控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class DashboardController : ControllerBase
{
    private readonly MedicalDbContext _context;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(MedicalDbContext context, ILogger<DashboardController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// 获取患者总数
    /// </summary>
    [HttpGet("patient-count")]
    [RequirePermission("dashboard.view")]
    public async Task<ActionResult<int>> GetPatientCount()
    {
        var count = await _context.Patients.CountAsync();
        return Ok(count);
    }

    /// <summary>
    /// 获取医生总数
    /// </summary>
    [HttpGet("doctor-count")]
    [RequirePermission("dashboard.view")]
    public async Task<ActionResult<int>> GetDoctorCount()
    {
        var count = await _context.Doctors
            .Where(d => !d.IsDeleted)
            .CountAsync();
        return Ok(count);
    }

    /// <summary>
    /// 获取咨询消息总数
    /// </summary>
    [HttpGet("consultation-message-count")]
    [RequirePermission("dashboard.view")]
    public async Task<ActionResult<int>> GetConsultationMessageCount()
    {
        var count = await _context.ConsultationMessages.CountAsync();
        return Ok(count);
    }

    /// <summary>
    /// 获取科室的患者人数就诊占比（饼状图数据）
    /// </summary>
    [HttpGet("department-patient-distribution")]
    [RequirePermission("dashboard.view")]
    public async Task<ActionResult> GetDepartmentPatientDistribution()
    {
        var distribution = await _context.Consultations
            .Include(c => c.Doctor)
                .ThenInclude(d => d.Department)
            .GroupBy(c => new { 
                DepartmentId = c.Doctor.DepartmentId, 
                DepartmentName = c.Doctor.Department.Name 
            })
            .Select(g => new
            {
                name = g.Key.DepartmentName,
                value = g.SelectMany(c => c.ConsultationPatients.Select(cp => cp.PatientId)).Distinct().Count() // 去重患者数
            })
            .OrderByDescending(x => x.value)
            .ToListAsync();

        return Ok(distribution);
    }

    /// <summary>
    /// 获取医生的就诊次数排名（柱状图数据）
    /// </summary>
    [HttpGet("doctor-consultation-ranking")]
    [RequirePermission("dashboard.view")]
    public async Task<ActionResult> GetDoctorConsultationRanking()
    {
        var ranking = await _context.Consultations
            .Include(c => c.Doctor)
            .GroupBy(c => new { 
                DoctorId = c.DoctorId, 
                DoctorName = c.Doctor.Name 
            })
            .Select(g => new
            {
                name = g.Key.DoctorName,
                value = g.Count()
            })
            .OrderByDescending(x => x.value)
            .ToListAsync();

        return Ok(ranking);
    }

    /// <summary>
    /// 获取医生的活跃度排名（二维图表，只显示前十名）
    /// 活跃度 = 咨询次数 + 咨询消息数
    /// </summary>
    [HttpGet("doctor-activity-ranking")]
    [RequirePermission("dashboard.view")]
    public async Task<ActionResult> GetDoctorActivityRanking()
    {
        var doctors = await _context.Doctors
            .Include(d => d.Department)
            .ToListAsync();

        var activityDataList = new List<(string Name, string Department, int ConsultationCount, int MessageCount, int Activity)>();

        foreach (var doctor in doctors)
        {
            // 咨询次数
            var consultationCount = await _context.Consultations
                .Where(c => c.DoctorId == doctor.Id)
                .CountAsync();

            // 咨询消息数
            var messageCount = await _context.ConsultationMessages
                .Where(m => m.Consultation.DoctorId == doctor.Id)
                .CountAsync();

            // 活跃度 = 咨询次数 + 咨询消息数
            var activity = consultationCount + messageCount;

            activityDataList.Add((
                doctor.Name,
                doctor.Department?.Name ?? "未知科室",
                consultationCount,
                messageCount,
                activity
            ));
        }

        // 按活跃度排序，取前10名
        var top10 = activityDataList
            .OrderByDescending(x => x.Activity)
            .Take(10)
            .Select(x => new
            {
                name = x.Name,
                department = x.Department,
                consultationCount = x.ConsultationCount,
                messageCount = x.MessageCount,
                activity = x.Activity
            })
            .ToList();

        return Ok(top10);
    }
}

