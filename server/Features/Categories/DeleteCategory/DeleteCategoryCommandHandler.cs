using MediatR;
using Microsoft.EntityFrameworkCore;
using server.Data;

namespace server.Features.Categories.DeleteCategory;

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, bool>
{
    private readonly ApplicationDbContext _context;

    public DeleteCategoryCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == request.CategoryId && c.UserId == request.UserId, cancellationToken);

        if (category == null) return false;

        category.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}