using Microsoft.EntityFrameworkCore;

namespace Article.Models
{
    public class ArticleContext : DbContext
    {
        public DbSet<Article> Article { get; set; }

        public ArticleContext(DbContextOptions<ArticleContext> options): base(options)
        {

        }
    }
}
