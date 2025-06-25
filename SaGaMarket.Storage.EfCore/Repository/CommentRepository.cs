using Microsoft.EntityFrameworkCore;
using SaGaMarket.Core.Entities;
using SaGaMarket.Infrastructure.Data;
using TourGuide.Core.Storage.Repositories;

namespace SaGaMarket.Storage.EfCore.Repository
{
    public class CommentRepository : ICommentRepository
    {
        private SaGaMarketDbContext _context;


        public CommentRepository(SaGaMarketDbContext context)
        {
            _context = context;
        }
        public async Task<Guid> Create(Comment comment)
        {
            _context.Add(comment);
            await _context.SaveChangesAsync();
            return comment.CommentId;
        }

        public async Task Delete(Guid commentId)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment != null) 
            {
                _context.Comments.Remove(comment);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Comment?> Get(Guid commentId)
        {
            return await _context.Comments.FindAsync(commentId);
        }

        public async Task<List<Comment>?> GetByAuthor(Guid AuthorId)
        {            
            return await _context.Comments
                .Where(c => c.AuthorId == AuthorId) 
                .ToListAsync();
        }
        public async Task<List<Comment>?> GetByTourRoute(Guid reviewId)
        {
            return await _context.Comments
                .Where(r => r.ReviewId == reviewId)
                .ToListAsync();
        }

        public async Task<bool> Update(Comment comment)
        {
            if (comment == null)
            {
                throw new ArgumentNullException(nameof(comment), "Comment cannot be null.");
            }
            // Проверяем, существует ли комментарий в базе данных
            var existingComment = await _context.Comments.FindAsync(comment.CommentId);
            if (existingComment == null)
            {
                return false;
            }
            // Обновляем свойства существующего комментария
            existingComment.CommentText = comment.CommentText;
            existingComment.TimeLastUpdate = comment.TimeLastUpdate;

            //var r = await _context.Comments.Where(...).ExecuteUpdateAsync(sp =>
            //    sp.SetProperty(c => c.Body, c => comment.Body )
            //    .SetProperty(c => c.TimeCreated, c => comment.TimeCreated)
            //    );

            try
            {
                await _context.SaveChangesAsync();
                return true; // Обновление успешно
            }
            catch (DbUpdateException ex)
            {
                // Логирование ошибки или обработка исключения
                // Например, можно записать в журнал или выбросить новое исключение
                // ArgumentException
                // InvalidOperationException
                throw new Exception("An error occurred while updating the comment.", ex);
            }
        }
    }
}
