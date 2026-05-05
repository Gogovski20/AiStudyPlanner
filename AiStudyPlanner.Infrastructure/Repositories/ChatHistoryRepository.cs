using AiStudyPlanner.Application.Repositories;
using AiStudyPlanner.Domain.Models;
using AiStudyPlanner.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AiStudyPlanner.Infrastructure.Repositories
{
    public class ChatHistoryRepository : IChatHistoryRepository
    {
        private readonly ApplicationDbContext _context;

        public ChatHistoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(ChatHistory chat)
        {
            await _context.ChatHistories.AddAsync(chat);
        }

        public async Task<ChatHistory?> GetByIdAsync(int id)
        {
            return await _context.ChatHistories.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<ChatHistory>> GetByUserIdAsync(int userId)
        {
            return await _context.ChatHistories
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
