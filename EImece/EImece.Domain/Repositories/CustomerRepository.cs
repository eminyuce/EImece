using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Models.DTOs;
using EImece.Domain.Repositories.IRepositories;
using NLog;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories
{
    public class CustomerRepository : BaseEntityRepository<Customer>, ICustomerRepository
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public CustomerRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }

        public Customer GetUserId(string userId)
        {
            return EImeceDbContext.Customers.AsNoTracking().FirstOrDefault(r => r.UserId == userId);
        }

        public async Task<Customer> GetUserIdAsync(string userId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await EImeceDbContext.Customers.AsNoTracking().FirstOrDefaultAsync(r => r.UserId == userId, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Targeted promotion of a guest customer to Normal type.
        /// Reads only Id+GsmNumber, then updates exactly those two columns — no full-entity load/save.
        /// </summary>
        public async Task<bool> PromoteCustomerToNormalTypeAsync(string userId, int normalCustomerType)
        {
            var info = await EImeceDbContext.Customers.AsNoTracking()
                .Where(c => c.UserId == userId)
                .Select(c => new { c.Id, c.GsmNumber })
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);
            if (info == null)
            {
                return false;
            }

            var entity = new Customer { Id = info.Id };
            var entry = EImeceDbContext.Entry(entity);
            entity.GsmNumber = GeneralHelper.CheckGsmNumber(info.GsmNumber);
            entity.CustomerType = normalCustomerType;
            entry.Property(c => c.GsmNumber).IsModified = true;
            entry.Property(c => c.CustomerType).IsModified = true;
            await EImeceDbContext.SaveChangesAsync().ConfigureAwait(false);
            return true;
        }

        /// <summary>
        /// 5-column projection (Id, Name, Surname, Email, CreatedDate) feeding CustomerSummaryDto.
        /// </summary>
        public async Task<Models.DTOs.Storefront.CustomerSummaryDto> GetStorefrontCustomerSummaryByUserIdAsync(string userId, CancellationToken cancellationToken = default(CancellationToken))
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
        public async Task<CustomerDto> GetStorefrontCustomerProfileByUserIdAsync(string userId, CancellationToken cancellationToken = default(CancellationToken))
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