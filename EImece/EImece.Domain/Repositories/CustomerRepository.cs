using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Models.DTOs;
using EImece.Domain.Observability.Telemetry;
using EImece.Domain.Repositories.IRepositories;
using Microsoft.Extensions.Logging;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories
{
    public class CustomerRepository : BaseEntityRepository<Customer>, ICustomerRepository
    {
        public CustomerRepository(IEImeceContext dbContext, ILogger<CustomerRepository> logger) : base(dbContext, logger)
        {
        }

        [Timed("repo.customers.get_by_user_sync")]
        public virtual Customer GetUserId(string userId)
        {
            return EImeceDbContext.Customers.AsNoTracking().FirstOrDefault(r => r.UserId == userId);
        }

        [Timed("repo.customers.get_by_user", "Time taken to get customer by user")]
        public virtual async Task<Customer> GetUserIdAsync(string userId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await EImeceDbContext.Customers.AsNoTracking().FirstOrDefaultAsync(r => r.UserId == userId, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Promotes a customer to Normal type and normalizes GsmNumber.
        /// Loads a tracked entity so EF can persist the two fields; a detached stub
        /// cannot have IsModified set (that threw after successful Iyzico callbacks).
        /// </summary>
        public async Task<bool> PromoteCustomerToNormalTypeAsync(string userId, int normalCustomerType)
        {
            var entity = await EImeceDbContext.Customers
                .FirstOrDefaultAsync(c => c.UserId == userId)
                .ConfigureAwait(false);
            if (entity == null)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(entity.GsmNumber) && GeneralHelper.IsGsmNumberValid(entity.GsmNumber))
            {
                entity.GsmNumber = GeneralHelper.CheckGsmNumber(entity.GsmNumber);
            }

            entity.CustomerType = normalCustomerType;
            await EImeceDbContext.SaveChangesAsync().ConfigureAwait(false);
            return true;
        }

        /// <summary>
        /// 5-column projection (Id, Name, Surname, Email, CreatedDate) feeding CustomerSummaryDto.
        /// </summary>
        [Timed("repo.customers.get_summary_by_user")]
        public virtual async Task<Models.DTOs.Storefront.CustomerSummaryDto> GetStorefrontCustomerSummaryByUserIdAsync(string userId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await EImeceDbContext.Customers.AsNoTracking()
                .Where(r => r.UserId == userId)
                .Select(r => new Models.DTOs.Storefront.CustomerSummaryDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Surname = r.Surname,
                    Email = r.Email,
                    GsmNumber = r.GsmNumber,
                    CreatedDate = r.CreatedDate,
                    UserId = r.UserId
                })
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Profile-form projection: only the columns the account form binds/displays, straight into CustomerDto.
        /// </summary>
        [Timed("repo.customers.get_profile_by_user")]
        public virtual async Task<CustomerDto> GetStorefrontCustomerProfileByUserIdAsync(string userId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await EImeceDbContext.Customers.AsNoTracking()
                .Where(r => r.UserId == userId)
                .Select(r => new CustomerDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Surname = r.Surname,
                    Email = r.Email,
                    IdentityNumber = r.IdentityNumber,
                    GsmNumber = r.GsmNumber,
                    Gender = r.Gender,
                    City = r.City,
                    Town = r.Town,
                    District = r.District,
                    Street = r.Street,
                    ZipCode = r.ZipCode,
                    Country = r.Country,
                    Description = r.Description,
                    Company = r.Company,
                    Ip = r.Ip,
                    UserId = r.UserId,
                    IsPermissionGranted = r.IsPermissionGranted,
                    IsActive = r.IsActive,
                    Lang = r.Lang,
                    Position = r.Position,
                    CustomerType = r.CustomerType,
                    CreatedDate = r.CreatedDate,
                    UpdatedDate = r.UpdatedDate
                })
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
        }
    }
}