using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Repositories.IRepositories;
using NLog;
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
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public ShoppingCartRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }

        public List<ShoppingCart> GetAdminPageList(string search, int currentLanguage)
        {
            var items = GetAll().Where(r => r.Lang == currentLanguage);
            if (!string.IsNullOrEmpty(search))
            {
                items = items.Where(r => r.Name.Contains(search));
            }
            items = items.OrderBy(r => r.Position).ThenByDescending(r => r.UpdatedDate);

            return items.ToList();
        }

        public async Task<List<ShoppingCart>> GetAdminPageListAsync(string search, int currentLanguage, CancellationToken cancellationToken = default(CancellationToken))
        {
            var items = GetAll().Where(r => r.Lang == currentLanguage);
            if (!string.IsNullOrEmpty(search))
            {
                items = items.Where(r => r.Name.Contains(search));
            }
            items = items.OrderBy(r => r.Position).ThenByDescending(r => r.UpdatedDate);

            return await items.ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public ShoppingCart GetShoppingCartByOrderGuid(string orderGuid)
        {
            return EImeceDbContext.ShoppingCarts.AsNoTracking().FirstOrDefault(r => r.OrderGuid == orderGuid);
        }

        public async Task<ShoppingCart> GetShoppingCartByOrderGuidAsync(string orderGuid, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await EImeceDbContext.ShoppingCarts.AsNoTracking().FirstOrDefaultAsync(r => r.OrderGuid == orderGuid, cancellationToken).ConfigureAwait(false);
        }

        public int DeleteExpiredShoppingCarts(DateTime cutoffDate, int batchSize = 500)
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

        public async Task<int> DeleteExpiredShoppingCartsAsync(DateTime cutoffDate, int batchSize = 500, CancellationToken cancellationToken = default(CancellationToken))
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