using light_brary.Application.DTOs.BorrowRecords;
using light_brary.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace light_brary.Controllers
{
    [ApiController]
    [Authorize(Policy = "LibrarianOrAdmin")]
    public class BorrowRecordsController : ControllerBase
    {
        private readonly IBorrowRecordService _borrowRecordService;

        public BorrowRecordsController(IBorrowRecordService borrowRecordService)
        {
            _borrowRecordService = borrowRecordService;
        }

        /// <summary>
        /// Tạo phiếu mượn sách mới (Librarian hoặc Admin)
        /// </summary>
        [HttpPost("api/borrow-records")]
        [ProducesResponseType(typeof(BorrowRecordResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<BorrowRecordResponse>> Create([FromBody] CreateBorrowRecordRequest request)
        {
            var created = await _borrowRecordService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>
        /// Lấy danh sách phiếu mượn, hỗ trợ lọc theo userId, status, overdue (Librarian hoặc Admin)
        /// </summary>
        [HttpGet("api/borrow-records")]
        [ProducesResponseType(typeof(IEnumerable<BorrowRecordSummaryResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<BorrowRecordSummaryResponse>>> GetAll(
            [FromQuery] int? userId = null,
            [FromQuery] string? status = null,
            [FromQuery] bool? overdue = null)
        {
            var records = await _borrowRecordService.GetAllAsync(userId, status, overdue);
            return Ok(records);
        }

        /// <summary>
        /// Xem chi tiết một phiếu mượn theo ID (Librarian hoặc Admin)
        /// </summary>
        [HttpGet("api/borrow-records/{id:int}")]
        [ProducesResponseType(typeof(BorrowRecordResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BorrowRecordResponse>> GetById(int id)
        {
            var record = await _borrowRecordService.GetByIdAsync(id);
            return Ok(record);
        }

        /// <summary>
        /// Trả một cuốn sách theo ID chi tiết mượn (Librarian hoặc Admin)
        /// </summary>
        [HttpPost("api/borrow-details/{id:int}/return")]
        [ProducesResponseType(typeof(BorrowDetailActionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<BorrowDetailActionResponse>> ReturnSingleBook(int id)
        {
            var result = await _borrowRecordService.ReturnSingleBookAsync(id);
            return Ok(result);
        }

        /// <summary>
        /// Trả nhiều cuốn sách trong một lượt (Librarian hoặc Admin)
        /// </summary>
        [HttpPost("api/returns")]
        [ProducesResponseType(typeof(ReturnBooksResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ReturnBooksResponse>> ReturnMultipleBooks([FromBody] ReturnBooksRequest request)
        {
            var result = await _borrowRecordService.ReturnMultipleBooksAsync(request);
            return Ok(result);
        }

        /// <summary>
        /// Báo mất một cuốn sách đang mượn theo ID chi tiết mượn (Librarian hoặc Admin)
        /// </summary>
        [HttpPost("api/borrow-details/{id:int}/mark-lost")]
        [ProducesResponseType(typeof(BorrowDetailActionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<BorrowDetailActionResponse>> MarkLostBook(int id)
        {
            var result = await _borrowRecordService.MarkLostBookAsync(id);
            return Ok(result);
        }
    }
}

