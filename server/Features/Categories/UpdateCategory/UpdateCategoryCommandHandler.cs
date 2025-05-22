using MediatR;
using Microsoft.EntityFrameworkCore;
using server.Data;

namespace server.Features.Categories.UpdateCategory;

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, bool>
{
    private readonly ApplicationDbContext _context;

    public UpdateCategoryCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == request.CategoryId && c.UserId == request.UserId && !c.IsDeleted, cancellationToken);

        if (category == null) return false;

        category.Name = request.Name;
        category.Color = request.Color;
        category.Type = request.Type;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}