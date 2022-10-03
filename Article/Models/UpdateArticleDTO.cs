namespace Article.Models
{
    public class UpdateArticleDTO
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public IFormFile FilePath { get; set; }
    }
}
