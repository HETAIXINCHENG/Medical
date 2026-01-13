using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Medical.API.Data;
using Medical.API.Models.DTOs;
using Medical.API.Models.Entities;
using Medical.API.Attributes;
using System.Security.Claims;

namespace Medical.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FeedbacksController : ControllerBase
    {
        private readonly MedicalDbContext _context;
        private readonly ILogger<FeedbacksController> _logger;

        public FeedbacksController(MedicalDbContext context, ILogger<FeedbacksController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// 提交反馈（无需登录）
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<FeedbackDto>> CreateFeedback([FromBody] CreateFeedbackDto dto)
        {
            try
            {
                var feedback = new Feedback
                {
                    Id = Guid.NewGuid(),
                    Title = dto.Title,
                    Content = dto.Content,
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Feedbacks.Add(feedback);
                await _context.SaveChangesAsync();

                var feedbackDto = MapToDto(feedback);
                return Ok(feedbackDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建反馈失败");
                return StatusCode(500, new { message = "创建反馈失败", error = ex.Message });
            }
        }

        /// <summary>
        /// 获取反馈列表（管理员）
        /// </summary>
        [HttpGet]
        [Authorize]
        [RequirePermission("feedbacks.view")]
        public async Task<ActionResult<object>> GetFeedbacks(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? status = null,
            [FromQuery] string? keyword = null)
        {
            try
            {
                var query = _context.Feedbacks.AsQueryable();

                // 状态筛选
                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(f => f.Status == status);
                }

                // 关键词搜索（标题或内容）
                if (!string.IsNullOrEmpty(keyword))
                {
                    query = query.Where(f => f.Title.Contains(keyword) || f.Content.Contains(keyword));
                }

                // 排序
                query = query.OrderByDescending(f => f.CreatedAt);

                var total = await query.CountAsync();
                var items = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var feedbackDtos = items.Select(f => MapToDto(f)).ToList();

                return Ok(new
                {
                    items = feedbackDtos,
                    total,
                    page,
                    pageSize,
                    totalPages = (int)Math.Ceiling(total / (double)pageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取反馈列表失败");
                return StatusCode(500, new { message = "获取反馈列表失败", error = ex.Message });
            }
        }

        /// <summary>
        /// 获取反馈详情（管理员）
        /// </summary>
        [HttpGet("{id}")]
        [Authorize]
        [RequirePermission("feedbacks.view")]
        public async Task<ActionResult<FeedbackDto>> GetFeedbackById(Guid id)
        {
            try
            {
                var feedback = await _context.Feedbacks
                    .FirstOrDefaultAsync(f => f.Id == id);

                if (feedback == null)
                {
                    return NotFound(new { message = "反馈不存在" });
                }

                var feedbackDto = MapToDto(feedback);
                return Ok(feedbackDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取反馈详情失败");
                return StatusCode(500, new { message = "获取反馈详情失败", error = ex.Message });
            }
        }

        /// <summary>
        /// 更新反馈（管理员）
        /// </summary>
        [HttpPut("{id}")]
        [Authorize]
        [RequirePermission("feedbacks.update")]
        public async Task<ActionResult<FeedbackDto>> UpdateFeedback(Guid id, [FromBody] UpdateFeedbackDto dto)
        {
            try
            {
                var feedback = await _context.Feedbacks
                    .FirstOrDefaultAsync(f => f.Id == id);

                if (feedback == null)
                {
                    return NotFound(new { message = "反馈不存在" });
                }

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { message = "未授权" });
                }

                // 更新字段
                if (!string.IsNullOrEmpty(dto.Title))
                {
                    feedback.Title = dto.Title;
                }
                if (!string.IsNullOrEmpty(dto.Content))
                {
                    feedback.Content = dto.Content;
                }
                if (!string.IsNullOrEmpty(dto.Status))
                {
                    feedback.Status = dto.Status;
                    if (dto.Status != "Pending" && feedback.ProcessedAt == null)
                    {
                        feedback.ProcessedBy = userId;
                        feedback.ProcessedAt = DateTime.UtcNow;
                    }
                }
                if (!string.IsNullOrEmpty(dto.ProcessNote))
                {
                    feedback.ProcessNote = dto.ProcessNote;
                }

                feedback.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var feedbackDto = MapToDto(feedback);
                return Ok(feedbackDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新反馈失败");
                return StatusCode(500, new { message = "更新反馈失败", error = ex.Message });
            }
        }

        /// <summary>
        /// 删除反馈（管理员）
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize]
        [RequirePermission("feedbacks.delete")]
        public async Task<IActionResult> DeleteFeedback(Guid id)
        {
            try
            {
                var feedback = await _context.Feedbacks.FindAsync(id);
                if (feedback == null)
                {
                    return NotFound(new { message = "反馈不存在" });
                }

                _context.Feedbacks.Remove(feedback);
                await _context.SaveChangesAsync();

                return Ok(new { message = "删除成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除反馈失败");
                return StatusCode(500, new { message = "删除反馈失败", error = ex.Message });
            }
        }

        private FeedbackDto MapToDto(Feedback feedback)
        {
            var processor = feedback.ProcessedBy.HasValue
                ? _context.Users.Find(feedback.ProcessedBy.Value)
                : null;

            return new FeedbackDto
            {
                Id = feedback.Id,
                Title = feedback.Title,
                Content = feedback.Content,
                Status = feedback.Status,
                ProcessNote = feedback.ProcessNote,
                ProcessedBy = feedback.ProcessedBy,
                ProcessorName = processor?.Username,
                ProcessedAt = feedback.ProcessedAt,
                CreatedAt = feedback.CreatedAt,
                UpdatedAt = feedback.UpdatedAt
            };
        }
    }
}

