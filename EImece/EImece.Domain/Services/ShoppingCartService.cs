using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using NLog;

namespace EImece.Domain.Services
{
    public class ShoppingCartService : BaseEntityService<ShoppingCart>, IShoppingCartService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private IShoppingCartRepository ShoppingCartRepository { get; set; }

        public ShoppingCartService(IShoppingCartRepository repository) : base(repository)
        {
            ShoppingCartRepository = repository;
        }

       
    }
}