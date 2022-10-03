namespace Article.Models
{
    public class CreateArticleDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public IFormFile FilePath { get; set; }
    }
}
