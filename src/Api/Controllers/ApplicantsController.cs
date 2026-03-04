
using Application.DTOs;
using Infrastructure.Persistence.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/Applicants")]
    [Authorize(Roles = "Admin,Receptionist,Doctor,Supervisor,Diwan")]
    public class ApplicantsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ApplicantsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET: api/Applicants
        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] string? filter = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] bool sortDesc = false,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            Expression<Func<Applicant, bool>>? filterExpr = null;

            if (!string.IsNullOrWhiteSpace(filter))
            {
                filterExpr = a => a.FullName.Contains(filter!) || a.FileNumber.Contains(filter!);
            }

            if (fromDate.HasValue)
            {
                var from = fromDate.Value.Date;
                var dateFromExpr = (Expression<Func<Applicant, bool>>)(a => a.CreatedAt >= from);
                filterExpr = filterExpr == null ? dateFromExpr : Repository<Applicant>.CombineFilters(filterExpr, dateFromExpr);
            }

            if (toDate.HasValue)
            {
                var to = toDate.Value.Date.AddDays(1).AddTicks(-1);
                var dateToExpr = (Expression<Func<Applicant, bool>>)(a => a.CreatedAt <= to);
                filterExpr = filterExpr == null ? dateToExpr : Repository<Applicant>.CombineFilters(filterExpr, dateToExpr);
            }

            var query = new GetEntitiesQuery<Applicant, ApplicantDto>(
                filterExpr,
                null,
                sortBy,
                sortDesc,
                page,
                pageSize
                ,
                    new Expression<Func<Applicant, object>>[] { a => a.MaritalStatus }

            );

            var result = await _mediator.Send(query);
            return Ok(ApiResult.Ok(result, "Fetched all data!", 200, HttpContext.TraceIdentifier));
        }
        [HttpGet("GetApplicantsStatistics")]
        public async Task<IActionResult> GetApplicantsStatistics(CancellationToken ct)
        {
            var stats = await _mediator.Send(new GetApplicantsStatisticsQuery(), ct);

            return Ok(new
            {
                succeeded = true,
                status = 200,
                message = "Applicants statistics fetched successfully",
                data = stats,
                traceId = HttpContext.TraceIdentifier
            });
        }
        [HttpGet("get/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var query = new GetEntityByIdQuery<Applicant, ApplicantDto>(id);
            var result = await _mediator.Send(query);

            if (result == null)
                return NotFound(ApiResult.Fail("Applicant not found", 404, traceId: HttpContext.TraceIdentifier));

            return Ok(ApiResult.Ok(result, "Fetched all data!", 200, HttpContext.TraceIdentifier));
        }
        // GET: api/Applicants/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var query = new GetApplicantQuery(id);
            var result = await _mediator.Send(query);

            if (result == null)
                return NotFound(ApiResult.Fail("Applicant not found", 404, traceId: HttpContext.TraceIdentifier));

            return Ok(ApiResult.Ok(result, "Fetched all data!", 200, HttpContext.TraceIdentifier));
        }
        [HttpGet("Details/{id}")]
        public async Task<IActionResult> GetDetails(string id)
        {
            var query = new GetApplicantDetailsQuery(id);
            var result = await _mediator.Send(query);

            if (result == null)
                return NotFound(ApiResult.Fail("Applicant not found", 404, traceId: HttpContext.TraceIdentifier));

            return Ok(ApiResult.Ok(result, "Fetched all data!", 200, HttpContext.TraceIdentifier));
        }

        // POST: api/Applicants
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ApplicantRequest dto)
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
            var command = new CreateEntityCommand<Applicant, ApplicantRequest>(dto);
            var result = await _mediator.Send(command);
            return Ok(ApiResult.Ok(result, "Entity created successfully!", 200, HttpContext.TraceIdentifier));
        }

        // PUT: api/Applicants/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] ApplicantRequest dto)
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
            var command = new UpdateEntityCommand<Applicant, ApplicantRequest>(id, dto);
            var result = await _mediator.Send(command);

            return Ok(ApiResult.Ok(result, "Entity updated successfully!", 200, HttpContext.TraceIdentifier));
        }

        // DELETE: api/Applicants/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {

            var command = new DeleteEntityCommand<Applicant>(id);
            var success = await _mediator.Send(command);
            if (!success)
                return NotFound(ApiResult.Fail("Entity not found", 404, null, HttpContext.TraceIdentifier));

            return Ok(ApiResult.Ok(null, "Entity deleted successfully", 200, HttpContext.TraceIdentifier));
        }
    }
}
