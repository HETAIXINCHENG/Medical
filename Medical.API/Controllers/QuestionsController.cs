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
/// 问题控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class QuestionsController : ControllerBase
{
    private readonly MedicalDbContext _context;
    private readonly ILogger<QuestionsController> _logger;

    public QuestionsController(MedicalDbContext context, ILogger<QuestionsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// 获取问题列表
    /// </summary>
    /// <param name="category">分类</param>
    /// <param name="isHot">是否热门</param>
    /// <param name="keyword">关键词搜索（标题、内容）</param>
    /// <param name="page">页码</param>
    /// <param name="pageSize">每页数量</param>
    /// <returns>问题列表</returns>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetQuestions(
        [FromQuery] string? category = null,
        [FromQuery] bool? isHot = null,
        [FromQuery] string? keyword = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = _context.Questions
            .Include(q => q.Patient)
            .AsQueryable();

        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(q => q.Category == category);
        }

        if (isHot.HasValue)
        {
            query = query.Where(q => q.IsHot == isHot.Value);
        }

        // 关键词搜索
        if (!string.IsNullOrEmpty(keyword))
        {
            query = query.Where(q => 
                q.Title.Contains(keyword) ||
                q.Content.Contains(keyword));
        }

        // 获取总数
        var total = await query.CountAsync();

        var questions = await query
            .OrderByDescending(q => q.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // 返回分页格式，兼容前端
        return Ok(new { items = questions, total, page, pageSize });
    }

    /// <summary>
    /// 根据ID获取问题详情
    /// </summary>
    /// <param name="id">问题ID</param>
    /// <returns>问题详情（包含回答）</returns>
    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(Question), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Question>> GetQuestionById(Guid id)
    {
        var question = await _context.Questions
            .Include(q => q.Patient)
            .Include(q => q.Answers)
                .ThenInclude(a => a.Patient)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (question == null)
        {
            return NotFound(new { message = "问题不存在" });
        }

        // 增加查看数
        question.ViewCount++;
        await _context.SaveChangesAsync();

        return Ok(question);
    }

    /// <summary>
    /// 创建问题
    /// </summary>
    /// <param name="dto">问题信息</param>
    /// <returns>创建的问题</returns>
    [HttpPost]
    [RequirePermission("question.create")]
    [ProducesResponseType(typeof(Question), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Question>> CreateQuestion([FromBody] CreateQuestionDto dto)
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

            _logger.LogInformation("收到创建问题请求: PatientId={PatientId}, Title={Title}", 
                dto?.PatientId, dto?.Title);
            
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

            // 获取当前用户对应的患者信息
            Guid finalPatientId;
            if (userRole == "Admin" || userRole == "SuperAdmin")
            {
                // 管理员可以指定 patientId，但如果未指定或为空，使用当前用户的患者 ID
                if (dto.PatientId == null || dto.PatientId == Guid.Empty)
                {
                    var currentPatient = await _context.Patients
                        .FirstOrDefaultAsync(p => p.UserId == currentUserIdGuid);
                    if (currentPatient == null)
                    {
                        return BadRequest(new { message = "未找到对应的患者信息，请先完善患者信息" });
                    }
                    finalPatientId = currentPatient.Id;
                }
                else
                {
                    // 验证 patientId 是否存在
                    var patientExists = await _context.Patients.AnyAsync(p => p.Id == dto.PatientId.Value);
                    if (!patientExists)
                    {
                        return BadRequest(new { message = $"患者ID {dto.PatientId} 不存在" });
                    }
                    finalPatientId = dto.PatientId.Value;
                }
            }
            else
            {
                // 普通用户只能为自己创建问题
                var currentPatient = await _context.Patients
                    .FirstOrDefaultAsync(p => p.UserId == currentUserIdGuid);
                if (currentPatient == null)
                {
                    return BadRequest(new { message = "未找到对应的患者信息，请先完善患者信息" });
                }
                finalPatientId = currentPatient.Id;
            }

            // 创建新的问题对象
            var newQuestion = new Question
            {
                Id = Guid.NewGuid(),
                PatientId = finalPatientId,
                Title = dto.Title,
                Content = dto.Content,
                Category = string.IsNullOrWhiteSpace(dto.Category) ? null : dto.Category,
                Tags = string.IsNullOrWhiteSpace(dto.Tags) ? null : dto.Tags,
                FavoriteCount = dto.FavoriteCount,
                AnswerCount = dto.AnswerCount,
                ViewCount = dto.ViewCount,
                IsHot = dto.IsHot,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Questions.Add(newQuestion);
            await _context.SaveChangesAsync();

            // 重新加载包含患者信息的问题
            var createdQuestion = await _context.Questions
                .Include(q => q.Patient)
                .FirstOrDefaultAsync(q => q.Id == newQuestion.Id);

            _logger.LogInformation("问题创建成功: Id={Id}, Title={Title}, PatientId={PatientId}", 
                newQuestion.Id, newQuestion.Title, newQuestion.PatientId);

            return CreatedAtAction(nameof(GetQuestionById), new { id = newQuestion.Id }, createdQuestion);
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "数据库更新失败");
            return BadRequest(new { message = "数据库操作失败", error = dbEx.InnerException?.Message ?? dbEx.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建问题失败");
            return BadRequest(new { message = "创建问题失败", error = ex.Message });
        }
    }

    /// <summary>
    /// 回答问题
    /// </summary>
    /// <param name="questionId">问题ID</param>
    /// <param name="answer">回答信息</param>
    /// <returns>创建的回答</returns>
    [HttpPost("{questionId}/answers")]
    [ProducesResponseType(typeof(Answer), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Answer>> CreateAnswer(Guid questionId, [FromBody] Answer answer)
    {
        var question = await _context.Questions.FindAsync(questionId);
        if (question == null)
        {
            return NotFound(new { message = "问题不存在" });
        }

        var currentUserIdentifier = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(currentUserIdentifier) || !Guid.TryParse(currentUserIdentifier, out var currentUserId))
        {
            return Unauthorized();
        }

        // 获取当前用户对应的患者信息
        var currentPatient = await _context.Patients
            .FirstOrDefaultAsync(p => p.UserId == currentUserId);
        
        if (currentPatient == null)
        {
            return BadRequest(new { message = "未找到对应的患者信息，请先完善患者信息" });
        }

        answer.Id = Guid.NewGuid();
        answer.QuestionId = questionId;
        answer.PatientId = currentPatient.Id;
        answer.CreatedAt = DateTime.UtcNow;
        answer.UpdatedAt = DateTime.UtcNow;

        _context.Answers.Add(answer);

        // 更新问题的回答数
        question.AnswerCount++;
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetQuestionById), new { id = questionId }, answer);
    }

    /// <summary>
    /// 更新问题
    /// </summary>
    /// <param name="id">问题ID</param>
    /// <param name="question">问题信息</param>
    /// <returns>更新后的问题</returns>
    [HttpPut("{id}")]
    [RequirePermission("question.update")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(Question), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Question>> UpdateQuestion(Guid id, [FromBody] Question question)
    {
        var existingQuestion = await _context.Questions.FindAsync(id);
        if (existingQuestion == null)
        {
            return NotFound(new { message = "问题不存在" });
        }

        existingQuestion.Title = question.Title;
        existingQuestion.Category = string.IsNullOrWhiteSpace(question.Category) ? null : question.Category;
        existingQuestion.Content = question.Content;
        existingQuestion.Tags = string.IsNullOrWhiteSpace(question.Tags) ? null : question.Tags;
        existingQuestion.FavoriteCount = question.FavoriteCount;
        existingQuestion.AnswerCount = question.AnswerCount;
        existingQuestion.ViewCount = question.ViewCount;
        existingQuestion.IsHot = question.IsHot;
        existingQuestion.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(existingQuestion);
    }

    /// <summary>
    /// 删除问题
    /// </summary>
    /// <param name="id">问题ID</param>
    /// <returns>删除结果</returns>
    [HttpDelete("{id}")]
    [RequirePermission("question.delete")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteQuestion(Guid id)
    {
        var question = await _context.Questions.FindAsync(id);
        if (question == null)
        {
            return NotFound(new { message = "问题不存在" });
        }

        _context.Questions.Remove(question);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

