using Application.Abstractions;
using Application.DTOs;
using Application.DTOs.FinalDecisions;
using Infrastructure.Persistence.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/FinalDecisions")]
    [Authorize(Roles = "Admin,Receptionist,Doctor,Supervisor,Diwan")]
    public class FinalDecisionsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IFinalDecisionHistoryQuery _historyQuery;

        public FinalDecisionsController(IMediator mediator, IFinalDecisionHistoryQuery historyQuery)
        {
            _mediator = mediator;
            _historyQuery = historyQuery;
        }

        // GET: api/Doctors
        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] string? filter = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] bool sortDesc = false,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            Expression<Func<FinalDecision, bool>>? filterExpr = null;

            if (!string.IsNullOrWhiteSpace(filter))
            {
                filterExpr = a => a.Reason.Contains(filter!) || a.ApplicantFileNumber.Contains(filter!);
            }

            var query = new GetEntitiesQuery<FinalDecision, FinalDecisionDto>(
                filterExpr,
                null,
                sortBy,
                sortDesc,
                page,
                pageSize
                ,
                    new Expression<Func<FinalDecision, object>>[] { b => b.Result, c => c.OrthopedicExam,a=>a.InternalExam,f=>f.EyeExam,m=>m.InternalExam }

            );

            var result = await _mediator.Send(query);
            return Ok(ApiResult.Ok(result, "Fetched all data!", 200, HttpContext.TraceIdentifier));
        }

        /// <summary>
        /// سجل تغيير النتيجة النهائية لرقم ملف منتسب (كان مرفوض → صار مقبول).
        /// </summary>
        [HttpGet("History")]
        public async Task<IActionResult> GetHistory([FromQuery] string applicantFileNumber)
        {
            if (string.IsNullOrWhiteSpace(applicantFileNumber))
                return BadRequest(ApiResult.Fail("applicantFileNumber is required", 400, null, HttpContext.TraceIdentifier));
            var list = await _historyQuery.GetByApplicantFileNumberAsync(applicantFileNumber, HttpContext.RequestAborted);
            return Ok(ApiResult.Ok(list, "Fetched.", 200, HttpContext.TraceIdentifier));
        }

        /// <summary>
        /// Get final decisions by applicant file numbers (e.g. for supervisor list - النتيجة النهائية).
        /// </summary>
        [HttpGet("ByFileNumbers")]
        public async Task<IActionResult> GetByFileNumbers([FromQuery] List<string> fileNumbers)
        {
            if (fileNumbers == null || !fileNumbers.Any())
            {
                return Ok(ApiResult.Ok(new PagedResult<FinalDecisionDto> { Items = new List<FinalDecisionDto>(), TotalCount = 0, Page = 1, PageSize = 0 }, "Fetched.", 200, HttpContext.TraceIdentifier));
            }
            Expression<Func<FinalDecision, bool>> filterExpr = fd => fileNumbers.Contains(fd.ApplicantFileNumber);
            var query = new GetEntitiesQuery<FinalDecision, FinalDecisionDto>(
                filterExpr,
                null,
                null,
                false,
                1,
                fileNumbers.Count + 100,
                new Expression<Func<FinalDecision, object>>[] { b => b.Result! });
            var result = await _mediator.Send(query);
            return Ok(ApiResult.Ok(result, "Fetched.", 200, HttpContext.TraceIdentifier));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var query = new GetEntityByIdQuery<FinalDecision, FinalDecisionDto>(id);
            var result = await _mediator.Send(query);

            if (result == null)
                return NotFound(ApiResult.Fail("Doctor not found", 404, traceId: HttpContext.TraceIdentifier));

            return Ok(ApiResult.Ok(result, "Fetched all data!", 200, HttpContext.TraceIdentifier));
        }


        [HttpPost]
        public async Task<IActionResult> Post([FromBody] FinalDecisionRequest dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                    );

                return BadRequest(ApiResult.Fail("Validation errors", 400, errors, HttpContext.TraceIdentifier));
            }
            var command = new CreateEntityCommand<FinalDecision, FinalDecisionRequest>(dto);
            var result = await _mediator.Send(command);
            return Ok(ApiResult.Ok(result, "Entity created successfully!", 200, HttpContext.TraceIdentifier));
        }

        // PUT: api/Doctors/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] FinalDecisionRequest dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                    );

                return BadRequest(ApiResult.Fail("Validation errors", 400, errors, HttpContext.TraceIdentifier));
            }
            var command = new UpdateEntityCommand<FinalDecision, FinalDecisionRequest>(id, dto);
            var result = await _mediator.Send(command);

            return Ok(ApiResult.Ok(result, "Entity updated successfully!", 200, HttpContext.TraceIdentifier));
        }

        // DELETE: api/Doctors/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {

            var command = new DeleteEntityCommand<FinalDecision>(id);
            var success = await _mediator.Send(command);
            if (!success)
                return NotFound(ApiResult.Fail("Entity not found", 404, null, HttpContext.TraceIdentifier));

            return Ok(ApiResult.Ok(null, "Entity deleted successfully", 200, HttpContext.TraceIdentifier));
        }
    }
}
