using CS322_PZ_AnteaPrimorac5157.Data;
using CS322_PZ_AnteaPrimorac5157.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CS322_PZ_AnteaPrimorac5157.Repositories
{
    public class ConfessionRepository : IConfessionRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ConfessionRepository> _logger;

        public ConfessionRepository(ApplicationDbContext context, ILogger<ConfessionRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Confession>> GetAllAsync()
        {
            try
            {
                return await _context.Confessions
                    .Include(c => c.Comments)
                    .OrderByDescending(c => c.DateCreated)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all confessions");
                throw;
            }
        }

        async Task<Confession?> IRepository<Confession>.GetByIdAsync(int id)
        {
            return await GetByIdAsync(id, false);
        }


        public async Task<Confession?> GetByIdAsync(int id, bool includeComments = false)
        {
            try
            {
                var query = _context.Confessions.AsQueryable();

                if (includeComments)
                {
                    query = query.Include(c => c.Comments);
                    return await query.FirstOrDefaultAsync(c => c.Id == id);
                }
                else
                {
                    return await query
                        .Select(c => new Confession
                        {
                            Id = c.Id,
                            Title = c.Title,
                            Content = c.Content,
                            DateCreated = c.DateCreated,
                            Likes = c.Likes,
                            Comments = new List<Comment>()
                        })
                        .FirstOrDefaultAsync(c => c.Id == id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting confession with ID: {Id}", id);
                throw;
            }
        }

        public async Task<Confession> AddAsync(Confession confession)
        {
            try
            {
                _context.Confessions.Add(confession);
                await _context.SaveChangesAsync();
                return confession;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding confession");
                throw;
            }
        }

        public async Task UpdateAsync(Confession confession)
        {
            try
            {
                var entry = _context.Entry(confession);
                if (entry.State == EntityState.Detached)
                {
                    _context.Confessions.Attach(confession);
                }
                entry.State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating confession with ID: {Id}", confession.Id);
                throw;
            }
        }

        public async Task DeleteAsync(Confession confession)
        {
            try
            {
                _context.Confessions.Remove(confession);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting confession with ID: {Id}", confession.Id);
                throw;
            }
        }

        public async Task<Comment?> GetCommentAsync(int commentId)
        {
            try
            {
                return await _context.Comments
                    .FirstOrDefaultAsync(c => c.Id == commentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting comment {CommentId}", commentId);
                throw;
            }
        }

        public async Task AddCommentAsync(Comment comment)
        {
            try
            {
                var confession = await _context.Confessions
                    .FirstOrDefaultAsync(c => c.Id == comment.ConfessionId);

                if (confession == null)
                    throw new DbUpdateException($"Confession with ID {comment.ConfessionId} not found");

                _context.Comments.Add(comment);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving comment to database");
                throw;
            }
        }

        public async Task UpdateCommentAsync(Comment comment)
        {
            try
            {
                var entry = _context.Entry(comment);
                if (entry.State == EntityState.Detached)
                {
                    _context.Comments.Attach(comment);
                }
                entry.State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating comment {CommentId}", comment.Id);
                throw;
            }
        }

        public async Task DeleteCommentAsync(int confessionId, int commentId)
        {
            try
            {
                var comment = await _context.Comments
                    .FirstOrDefaultAsync(c => c.ConfessionId == confessionId && c.Id == commentId);

                if (comment != null)
                {
                    _context.Comments.Remove(comment);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting comment {CommentId} from confession {ConfessionId}", commentId, confessionId);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            try
            {
                return await _context.Confessions.AnyAsync(c => c.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking existence of confession with ID: {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<Confession>> FindAsync(Expression<Func<Confession, bool>> predicate)
        {
            try
            {
                return await _context.Confessions.Where(predicate).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while finding confessions with predicate");
                throw;
            }
        }

        public async Task<IEnumerable<Confession>> GetTopConfessionsAsync(int count)
        {
            try
            {
                return await _context.Confessions
                    .OrderByDescending(c => c.Likes)
                    .Take(count)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting top confessions");
                throw;
            }
        }

        public async Task<IEnumerable<Confession>> SearchConfessionsAsync(string searchTerm)
        {

            try
            {
                // Pretraga nije osjetljiva na mala i velika slova
                return await _context.Confessions
                    .Where(c =>
                        EF.Functions.Like(c.Title, $"%{searchTerm}%") ||
                        EF.Functions.Like(c.Content, $"%{searchTerm}%"))
                    .OrderByDescending(c => c.DateCreated)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching confessions");
                throw;
            }
        }

        public async Task IncrementLikesAsync(int id)
        {
            try
            {
                var confession = await GetByIdAsync(id);
                if (confession != null)
                {
                    confession.Likes++;
                    await UpdateAsync(confession);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while incrementing likes for confession ID: {Id}", id);
                throw;
            }
        }

        public async Task DecrementLikesAsync(int id)
        {
            try
            {
                var confession = await GetByIdAsync(id);
                if (confession != null && confession.Likes > 0)
                {
                    confession.Likes--;
                    await UpdateAsync(confession);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while decrementing likes for confession ID: {Id}", id);
                throw;
            }
        }
    }
}