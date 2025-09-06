using Application.DTOs;
using Infrastructure.Persistence.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/Archives")]
    [Authorize(Roles = "Admin,Receptionist,Doctor,Supervisor,Diwan")]
    public class ArchivesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ArchivesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET: api/Doctors
        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] string? filter = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] bool sortDesc = false,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] Dictionary<string, string>? filterDict = null // إضافة خيار الفلترة بالدكشنري
        )
        {
            Expression<Func<Archive, bool>>? filterExpr = null;

            if (!string.IsNullOrWhiteSpace(filter))
            {
                filterExpr = a => a.ApplicantFileNumber.Contains(filter!);
            }

            // إذا تم تمرير فلتر دكشنري، استخدم BuildFilter وادمجه مع الفلتر العادي
            if (filterDict != null && filterDict.Any())
            {
                var dictExpr = Repository<Archive>.BuildFilter(filterDict);
                if (dictExpr != null)
                    filterExpr = filterExpr != null ? Repository<Archive>.CombineFilters(filterExpr, dictExpr) : dictExpr;
            }

            var query = new GetEntitiesQuery<Archive, ArchiveDto>(
                filterExpr,
                null,
                sortBy,
                sortDesc,
                page,
                pageSize,
                new Expression<Func<Archive, object>>[] { b => b.Applicant }
            );

            var result = await _mediator.Send(query);
            return Ok(ApiResult.Ok(result, "Fetched all data!", 200, HttpContext.TraceIdentifier));
        }
        // GET: api/EyeExams/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var query = new GetEntityByIdQuery<Archive, ArchiveDto>(
                id,
                new Expression<Func<Archive, object>>[]
                {
                    a => a.Applicant,
                   
                }
            );
            var result = await _mediator.Send(query);

            if (result == null)
                return NotFound(ApiResult.Fail("EyeExam not found", 404, traceId: HttpContext.TraceIdentifier));

            return Ok(ApiResult.Ok(result, "Fetched all data!", 200, HttpContext.TraceIdentifier));
        }




        // PUT: api/Doctors/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] ArchiveRequest dto)
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
            var command = new UpdateEntityCommand<Archive, ArchiveRequest>(id, dto);
            var result = await _mediator.Send(command);

            return Ok(ApiResult.Ok(result, "Entity updated successfully!", 200, HttpContext.TraceIdentifier));
        }

      
    }
}
