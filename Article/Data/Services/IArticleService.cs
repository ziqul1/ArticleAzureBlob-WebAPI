using Article.Models;

namespace Article.Data.Services
{
    public interface IArticleService
    {
        public Task<List<GetSingleArticleDTO>> GetAllAsync();
        public Task<GetSingleArticleDTO> GetSingleAsync(int id);
        public Task<CreateArticleDTO> CreateAsync(CreateArticleDTO createArticleDTO);
        public Task<long> UpdateAsync(int id, UpdateArticleDTO updateArticleDTO);
        public Task<bool> DeleteAsync(int id);
    }
}
