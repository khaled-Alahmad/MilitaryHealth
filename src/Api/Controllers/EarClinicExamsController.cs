using Application.DTOs;
using Infrastructure.Persistence.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/EarClinicExams")]
    [Authorize(Roles = "Admin,Receptionist,Doctor,Supervisor,Diwan")]
    public class EarClinicExamsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public EarClinicExamsController(IMediator mediator)
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

                     [FromQuery] Dictionary<string, string>? filterDict = null,
         [FromQuery] int pageSize = 20)
        {
            Expression<Func<EarClinicExam, bool>>? filterExpr = null;

            if (!string.IsNullOrWhiteSpace(filter))
            {
                filterExpr = a => a.Reason.Contains(filter!) || a.ApplicantFileNumber.Contains(filter!);
            }
            if (filterDict != null && filterDict.Any())
            {
                var dictExpr = Repository<EarClinicExam>.BuildFilter(filterDict);
                if (dictExpr != null)
                    filterExpr = filterExpr != null ? Repository<EarClinicExam>.CombineFilters(filterExpr, dictExpr) : dictExpr;
            }

            var query = new GetEntitiesQuery<EarClinicExam, EarClinicExamDto>(
                filterExpr,
                null,
                sortBy,
                sortDesc,
                page,
                pageSize
                ,
                    new Expression<Func<EarClinicExam, object>>[] { c => c.Doctor,e=>e.Result }

            );

            var result = await _mediator.Send(query);
            return Ok(ApiResult.Ok(result, "Fetched all data!", 200, HttpContext.TraceIdentifier));
        }

        // GET: api/EyeExams/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var query = new GetEntityByIdQuery<EarClinicExam, EarClinicExamDto>(
                id,
                new Expression<Func<EarClinicExam, object>>[]
                {
                    a => a.Result,
                    a => a.Doctor
                }
            );
            var result = await _mediator.Send(query);

            if (result == null)
                return NotFound(ApiResult.Fail("EyeExam not found", 404, traceId: HttpContext.TraceIdentifier));

            return Ok(ApiResult.Ok(result, "Fetched all data!", 200, HttpContext.TraceIdentifier));
        }


        // POST: api/Doctors
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] EarClinicExamRequest dto)
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
            var command = new CreateEntityCommand<EarClinicExam, EarClinicExamRequest>(dto);
            var result = await _mediator.Send(command);
            return Ok(ApiResult.Ok(result, "Entity created successfully!", 200, HttpContext.TraceIdentifier));
        }

        // PUT: api/Doctors/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] EarClinicExamRequest dto)
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
            var command = new UpdateEntityCommand<EarClinicExam, EarClinicExamRequest>(id, dto);
            var result = await _mediator.Send(command);

            return Ok(ApiResult.Ok(result, "Entity updated successfully!", 200, HttpContext.TraceIdentifier));
        }

        // DELETE: api/Doctors/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {

            var command = new DeleteEntityCommand<EarClinicExam>(id);
            var success = await _mediator.Send(command);
            if (!success)
                return NotFound(ApiResult.Fail("Entity not found", 404, null, HttpContext.TraceIdentifier));

            return Ok(ApiResult.Ok(null, "Entity deleted successfully", 200, HttpContext.TraceIdentifier));
        }
    }
}    