
using Application.DTOs;
using Infrastructure.Persistence.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/Consultations")]
    [Authorize(Roles = "Admin,Receptionist,Doctor,Supervisor,Diwan")]

    public class ConsultationsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ConsultationsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET: api/Doctorsk
        [HttpGet]
        public async Task<IActionResult> Get(
     [FromQuery] string? filter = null,
     [FromQuery] string? sortBy = null,
     [FromQuery] bool sortDesc = false,
     [FromQuery] int page = 1,

                 [FromQuery] Dictionary<string, string>? filterDict = null,
     [FromQuery] int pageSize = 20)
        {
            Expression<Func<Consultation, bool>>? filterExpr = null;

            if (!string.IsNullOrWhiteSpace(filter))
            {
                filterExpr = a => a.Result.Contains(filter!) || a.ApplicantFileNumber.Contains(filter!);
            }
            if (filterDict != null && filterDict.Any())
            {
                var dictExpr = Repository<Consultation>.BuildFilter(filterDict);
                if (dictExpr != null)
                    filterExpr = filterExpr != null ? Repository<Consultation>.CombineFilters(filterExpr, dictExpr) : dictExpr;
            }

            var query = new GetEntitiesQuery<Consultation, ConsultationDto>(
                filterExpr,
                null,
                sortBy,
                sortDesc,
                page,
                pageSize
                ,
                    new Expression<Func<Consultation, object>>[] { c => c.Doctor }

            );

            var result = await _mediator.Send(query);
            return Ok(ApiResult.Ok(result, "Fetched all data!", 200, HttpContext.TraceIdentifier));
        }

        // GET: api/EyeExams/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var query = new GetEntityByIdQuery<Consultation, ConsultationDto>(id);
            var result = await _mediator.Send(query);

            if (result == null)
                return NotFound(ApiResult.Fail("Doctor not found", 404, traceId: HttpContext.TraceIdentifier));

            return Ok(ApiResult.Ok(result, "Fetched all data!", 200, HttpContext.TraceIdentifier));
        }


        // POST: api/Doctors
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ConsultationRequest dto)
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
            var command = new CreateEntityCommand<Consultation, ConsultationRequest>(dto);
            var result = await _mediator.Send(command);
            return Ok(ApiResult.Ok(result, "Entity created successfully!", 200, HttpContext.TraceIdentifier));
        }

        // PUT: api/Doctors/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] ConsultationRequest dto)
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
            var command = new UpdateEntityCommand<Consultation, ConsultationRequest>(id, dto);
            var result = await _mediator.Send(command);

            return Ok(ApiResult.Ok(result, "Entity updated successfully!", 200, HttpContext.TraceIdentifier));
        }

        // DELETE: api/Doctors/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {

            var command = new DeleteEntityCommand<Consultation>(id);
            var success = await _mediator.Send(command);
            if (!success)
                return NotFound(ApiResult.Fail("Entity not found", 404, null, HttpContext.TraceIdentifier));

            return Ok(ApiResult.Ok(null, "Entity deleted successfully", 200, HttpContext.TraceIdentifier));
        }
    }
}
