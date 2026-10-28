using light_brary.Constants;
using light_brary.Services;
using System.Security.Claims;
using light_brary.DTOs.Requests;
using light_brary.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace light_brary.Controllers
{
    [ApiController]
    public class BorrowRecordsController(BorrowRecordService borrowRecordService) : ControllerBase
    {
        [HttpPost("api/borrow-records")]
        [Authorize(Policy = "LibrarianOrAdmin")]
        public async Task<ActionResult<BorrowRecordResponse>> Create(CreateBorrowRecordRequest request)
        {
            var created = await borrowRecordService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpGet("api/borrow-records")]
        [Authorize(Policy = "LibrarianOrAdmin")]
        public async Task<ActionResult<IEnumerable<BorrowRecordSummaryResponse>>> GetAll(
            [FromQuery] Guid? userId = null,
            [FromQuery] string? status = null,
            [FromQuery] bool? overdue = null)
        {
            var records = await borrowRecordService.GetAllAsync(userId, status, overdue);
            return Ok(records);
        }

        [HttpGet("api/borrow-records/{id:guid}")]
        [Authorize]
        public async Task<ActionResult<BorrowRecordResponse>> GetById(Guid id)
        {
            var record = await borrowRecordService.GetByIdAsync(id);
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var isStaffOrAdmin = User.IsInRole(RoleNames.ADMIN) || User.IsInRole(RoleNames.LIBRARIAN);
            if (!isStaffOrAdmin && record.User.Id != userId)
            {
                return Forbid();
            }
            return Ok(record);
        } 

        [HttpPost("api/borrow-records/{borrowRecordId:guid}/returns")]
        [Authorize(Policy = "LibrarianOrAdmin")]
        public async Task<ActionResult<ReturnBooksResponse>> ReturnMultipleBooks(Guid borrowRecordId, ReturnBooksRequest request)
        {
            var result = await borrowRecordService.ReturnMultipleBooksAsync(borrowRecordId, request);
            return Ok(result);
        }

        [HttpPost("api/borrow-details/{id:guid}/mark-lost")]
        [Authorize(Policy = "LibrarianOrAdmin")]
        public async Task<ActionResult<BorrowDetailActionResponse>> MarkLostBook(Guid id)
        {
            var result = await borrowRecordService.MarkLostBookAsync(id);
            return Ok(result);
        }
    }
}
