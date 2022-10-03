using Article.Models;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.EntityFrameworkCore;

namespace Article.Data.Services
{
    public class ArticleService : IArticleService
    {
        private readonly ArticleContext _articleContext;
        private string blobStorageCS = "";
        private string blobStorageContainerName = "";

        public ArticleService(ArticleContext articleContext) => _articleContext = articleContext;

        public async Task<List<GetSingleArticleDTO>> GetAllAsync()
        {
            var container = new BlobContainerClient(blobStorageCS, blobStorageContainerName);
            var containerUri = container.Uri.AbsoluteUri;

            BlobSasBuilder blobSasBuilder = new BlobSasBuilder()
            {
                BlobContainerName = container.Name,
                // "c" oznacza dostęp do całego kontenera ...
                Resource = "c",
                StartsOn = DateTimeOffset.UtcNow,
                ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(1),
            };

            blobSasBuilder.SetPermissions(BlobSasPermissions.Read);

            var sasToken = blobSasBuilder.ToSasQueryParameters(
                new StorageSharedKeyCredential(
                    "", 
                    ""))
                .ToString();

            return await _articleContext.Article
                .Select(x => new GetSingleArticleDTO
                {
                    Id = x.Id,
                    Title = x.Title,
                    Description = x.Description,
                    ImageURL = containerUri + "/" + x.ImageURL + "?" + sasToken
                }).ToListAsync();
        }

        public async Task<GetSingleArticleDTO> GetSingleAsync(int id)
        {
            var article = await _articleContext.Article.Where(x => x.Id == id).FirstOrDefaultAsync();

            if (article == null)
                throw new Exception("Panie, nie mamy takich rzeczy");

            var container = new BlobContainerClient(blobStorageCS, blobStorageContainerName);
            var containerUri = container.Uri.AbsoluteUri;

            BlobSasBuilder blobSasBuilder = new BlobSasBuilder()
            {
                BlobContainerName = container.Name,
                // "b" oznacza dostęp do konkretnego pliku w kontenerze 
                Resource = "b",
                StartsOn = DateTimeOffset.UtcNow,
                ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(1),
            };

            blobSasBuilder.SetPermissions(BlobSasPermissions.Read);

            var sasToken = blobSasBuilder.ToSasQueryParameters(
                new StorageSharedKeyCredential(
                    "", 
                    ""))
                .ToString();

            return new GetSingleArticleDTO 
            {
                Id = article.Id,
                Title = article.Title,
                Description = article.Description,
                ImageURL = containerUri + "/" + article.ImageURL + "?" + sasToken
            };
        }

        public async Task<CreateArticleDTO> CreateAsync(CreateArticleDTO createArticleDTO)
        {
            var article = new Models.Article
            {
                Title = createArticleDTO.Title,
                Description = createArticleDTO.Description,
                ImageURL = createArticleDTO.FilePath.FileName
            };

            // wrzucamy image do azure storage account 
            var container = new BlobContainerClient(blobStorageCS, blobStorageContainerName);
            var blob = container.GetBlobClient(createArticleDTO.FilePath.FileName);
            var stream = createArticleDTO.FilePath.OpenReadStream();
            await blob.UploadAsync(stream);

            _articleContext.Article.Add(article);
            if (await _articleContext.SaveChangesAsync() != 1)
                throw new Exception("Cos sie zepsulo przy zapisie do bazy chłopaku");

            return CreateArticleToDTO(article);
        }

        public async Task<long> UpdateAsync(int id, UpdateArticleDTO updateArticleDTO)
        {
            var article = await _articleContext.Article.Where(x => x.Id == id).FirstOrDefaultAsync();

            if (article == null)
                throw new Exception("Panie, nie mamy takich rzeczy");

            // usuwamy stary image z konkretnego kontenera w storage account
            var container = new BlobContainerClient(blobStorageCS, blobStorageContainerName);
            var blob = container.GetBlobClient(article.ImageURL);
            await blob.DeleteAsync();

            article.Title = updateArticleDTO.Title;
            article.Description = updateArticleDTO.Description;
            article.ImageURL = updateArticleDTO.FilePath.FileName;

            // wrzucamy nowy image do konkretnego kontenera w storage account
            blob = container.GetBlobClient(article.ImageURL);
            var stream = updateArticleDTO.FilePath.OpenReadStream();
            await blob.UploadAsync(stream);

            var result = await _articleContext.SaveChangesAsync();

            if (result != 1)
                throw new Exception("Cos sie zepsulo przy zapisie do bazy chłopaku");

            return result;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var article = await _articleContext.Article.Where(x => x.Id == id).FirstOrDefaultAsync();

            if (article == null)
                throw new Exception("Panie, nie mamy takich rzeczy");

            _articleContext.Article.Remove(article);

            var container = new BlobContainerClient(blobStorageCS, blobStorageContainerName);
            var blob = container.GetBlobClient(article.ImageURL);
            await blob.DeleteAsync();

            if (await _articleContext.SaveChangesAsync() != 1)
                throw new Exception("Cos sie zepsulo przy zapisie do bazy chłopaku");

            return true;
        }

        private static CreateArticleDTO CreateArticleToDTO(Models.Article article) =>
            new CreateArticleDTO
            {
                Id = article.Id,
                Title = article.Title,
                Description = article.Description,
            };
    }
}
