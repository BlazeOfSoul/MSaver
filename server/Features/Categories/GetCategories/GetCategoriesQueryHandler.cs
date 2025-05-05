using MediatR;
using Microsoft.EntityFrameworkCore;
using server.Data;

namespace server.Features.Categories.GetCategories;

public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, List<CategoryResponse>>
{
    private readonly ApplicationDbContext _context;

    public GetCategoriesQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CategoryResponse>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        return await _context.Categories
            .Where(c => c.UserId == request.UserId && !c.IsDeleted)
            .Select(c => new CategoryResponse(
                c.Id,
                c.Name,
                c.Type,
                c.Color
            ))
            .ToListAsync(cancellationToken);
    }
}
