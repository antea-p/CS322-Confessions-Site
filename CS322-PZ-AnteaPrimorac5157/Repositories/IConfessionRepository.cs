﻿using CS322_PZ_AnteaPrimorac5157.Models;

namespace CS322_PZ_AnteaPrimorac5157.Repositories
{
    public interface IConfessionRepository : IRepository<Confession>
    {
        Task<Confession?> GetByIdAsync(int id, bool includeComments = false);
        Task<IEnumerable<Confession>> GetTopConfessionsAsync(int count);
        Task<IEnumerable<Confession>> SearchConfessionsAsync(string searchTerm);
        Task<Comment?> GetCommentAsync(int commentId);
        Task AddCommentAsync(Comment comment);
        Task UpdateCommentAsync(Comment comment);
        Task DeleteCommentAsync(int confessionId, int commentId);
        Task IncrementLikesAsync(int id);
        Task DecrementLikesAsync(int id);
        Task IncrementCommentLikesAsync(int commentId);
        Task DecrementCommentLikesAsync(int commentId);
    }
}