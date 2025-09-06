using MapsterMapper;
using MediatR;
using System.Linq.Expressions;

public class GenericQueryHandler<TEntity, TDto> :
    IRequestHandler<GetEntityByIdQuery<TEntity, TDto>, TDto?>,
    IRequestHandler<GetEntitiesQuery<TEntity, TDto>, PagedResult<TDto>>
    where TEntity : class
{
    private readonly IPagedRepository<TEntity> _repo;
    private readonly IMapper _mapper;

    public GenericQueryHandler(IPagedRepository<TEntity> repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<TDto?> Handle(GetEntityByIdQuery<TEntity, TDto> request, CancellationToken ct)
    {
        var includes = request.Includes ?? Array.Empty<Expression<Func<TEntity, object>>>();
        var entity = await _repo.GetByIdAsync(request.Id, ct, includes);
        return entity == null ? default : _mapper.Map<TDto>(entity);
    }

    public async Task<PagedResult<TDto>> Handle(GetEntitiesQuery<TEntity, TDto> request, CancellationToken ct)
    {
        var includes = request.Includes ?? Array.Empty<Expression<Func<TEntity, object>>>();

        var pagedEntities = await _repo.GetPagedAsync(
            request.Filter,
            request.SortBy,
            request.SortDesc,
            request.Page,
            request.PageSize,
            ct,
            includes
        );

      
        var entities = pagedEntities.Items.ToList();

        var dtoItems = entities
            .Select(e => _mapper.Map<TDto>(e))
            .ToList();

        return new PagedResult<TDto>
        {
            Items = dtoItems,
            TotalCount = pagedEntities.TotalCount,
            Page = pagedEntities.Page,
            PageSize = pagedEntities.PageSize
        };
    }



}
