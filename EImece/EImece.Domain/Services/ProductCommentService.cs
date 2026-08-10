using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using NLog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Services
{
    public class ProductCommentService : BaseEntityService<ProductComment>, IProductCommentService
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private IProductCommentRepository ProductCommentRepository { get; set; }

        public ProductCommentService(IProductCommentRepository repository) : base(repository)
        {
            ProductCommentRepository = repository;
        }

        public List<ProductComment> GetAdminPageList(int productId, string search, int lang)
        {
            return ProductCommentRepository.GetAdminPageList(productId, search, lang);
        }

        public async Task<List<ProductComment>> GetAdminPageListAsync(int productId, string search, int lang, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await ProductCommentRepository.GetAdminPageListAsync(productId, search, lang, cancellationToken).ConfigureAwait(false);
        }
    }
}