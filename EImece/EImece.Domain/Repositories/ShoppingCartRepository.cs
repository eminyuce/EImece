using Microsoft.Extensions.Logging;
using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Observability.Telemetry;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories
{
    public class ShoppingCartRepository : BaseEntityRepository<ShoppingCart>, IShoppingCartRepository
    {
        public ShoppingCartRepository(IEImeceContext dbContext, ILogger<ShoppingCartRepository> logger) : base(dbContext, logger) {
        }

        [Timed("repo.shopping_cart.get_admin_page_list_sync")]
        public virtual List<ShoppingCart> GetAdminPageList(string search, int currentLanguage)
        {
            var items = GetAll().Where(r => r.Lang == currentLanguage);
            if (!string.IsNullOrEmpty(search))
            {
                items = items.Where(r => r.Name.Contains(search));
            }
            items = items.OrderByDescending(r => r.UpdatedDate).ThenByDescending(r => r.Id);

            return items.ToList();
        }

        [Timed("repo.shopping_cart.get_admin_page_list")]
        public virtual async Task<List<ShoppingCart>> GetAdminPageListAsync(string search, int currentLanguage, CancellationToken cancellationToken = default(CancellationToken))
        {
            var items = GetAll().Where(r => r.Lang == currentLanguage);
            if (!string.IsNullOrEmpty(search))
            {
                items = items.Where(r => r.Name.Contains(search));
            }
            items = items.OrderByDescending(r => r.UpdatedDate).ThenByDescending(r => r.Id);

            return await items.ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        [Timed("repo.shopping_cart.get_by_order_guid_sync")]
        public virtual ShoppingCart GetShoppingCartByOrderGuid(string orderGuid)
        {
            return EImeceDbContext.ShoppingCarts.AsNoTracking().FirstOrDefault(r => r.OrderGuid == orderGuid);
        }

        [Timed("repo.shopping_cart.get_by_order_guid")]
        public virtual async Task<ShoppingCart> GetShoppingCartByOrderGuidAsync(string orderGuid, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await EImeceDbContext.ShoppingCarts.AsNoTracking().FirstOrDefaultAsync(r => r.OrderGuid == orderGuid, cancellationToken).ConfigureAwait(false);
        }

        [Timed("repo.shopping_cart.delete_expired_sync")]
        public virtual int DeleteExpiredShoppingCarts(DateTime cutoffDate, int batchSize = 500)
        {
            string connectionString = ConnectionStringProvider.GetConnectionString();
            int totalDeleted = 0;
            string commandText = @"DELETE TOP (@BatchSize) FROM dbo.ShoppingCarts WHERE CreatedDate < @CutoffDate";

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                while (true)
                {
                    using (var command = new SqlCommand(commandText, connection))
                    {
                        command.CommandType = CommandType.Text;
                        command.Parameters.Add(DatabaseUtility.GetSqlParameter("BatchSize", batchSize, SqlDbType.Int));
                        command.Parameters.Add(DatabaseUtility.GetSqlParameter("CutoffDate", cutoffDate, SqlDbType.DateTime));
                        int rows = command.ExecuteNonQuery();
                        totalDeleted += rows;
                        if (rows < batchSize)
                        {
                            break;
                        }
                    }
                }
            }
            return totalDeleted;
        }

        [Timed("repo.shopping_cart.delete_expired")]
        public virtual async Task<int> DeleteExpiredShoppingCartsAsync(DateTime cutoffDate, int batchSize = 500, CancellationToken cancellationToken = default(CancellationToken))
        {
            string connectionString = ConnectionStringProvider.GetConnectionString();
            int totalDeleted = 0;
            string commandText = @"DELETE TOP (@BatchSize) FROM dbo.ShoppingCarts WHERE CreatedDate < @CutoffDate";

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                while (!cancellationToken.IsCancellationRequested)
                {
                    using (var command = new SqlCommand(commandText, connection))
                    {
                        command.CommandType = CommandType.Text;
                        command.Parameters.Add(DatabaseUtility.GetSqlParameter("BatchSize", batchSize, SqlDbType.Int));
                        command.Parameters.Add(DatabaseUtility.GetSqlParameter("CutoffDate", cutoffDate, SqlDbType.DateTime));
                        int rows = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                        totalDeleted += rows;
                        if (rows < batchSize)
                        {
                            break;
                        }
                    }
                }
            }
            return totalDeleted;
        }
    }
}