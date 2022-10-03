using Article.Data.Services;
using Article.Models;
using Microsoft.AspNetCore.Mvc;

namespace Article.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArticlesController : ControllerBase
    {
        private readonly IArticleService _articleService;

        public ArticlesController(IArticleService articleService) 
            => _articleService = articleService;

        // GET: api/Articles
        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            return Ok(await _articleService.GetAllAsync());
        }

        // GET: api/Articles/1
        [HttpGet("{id}")]
        public async Task<ActionResult<GetSingleArticleDTO>> GetSingleAsync(int id)
        {
            return await _articleService.GetSingleAsync(id);
        }

        // POST: api/Articles
        [HttpPost]
        public async Task<ActionResult<CreateArticleDTO>> CreateAsync([FromForm] CreateArticleDTO createArticleDTO)
        {
            var article = await _articleService.CreateAsync(createArticleDTO);

            return CreatedAtAction(
                nameof(GetSingleAsync),
                new { id = article.Id },
                article
                );
        }

        // PUT: api/Articles/1
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id, [FromForm]UpdateArticleDTO updateArticleDTO)
        {
            if( await _articleService.UpdateAsync(id, updateArticleDTO) != 1)
                return BadRequest();

            return Ok();
        }

        // DELETE: api/Articles/1
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            if (await _articleService.DeleteAsync(id))
                return Ok();

            return BadRequest();
        }
    }
}
