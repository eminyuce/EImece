using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Models.Enums;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using EImece.Models;
using Ninject;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;


namespace EImece.Domain.Services
{
    public class CouponService : BaseEntityService<Coupon>, ICouponService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private ICouponRepository CouponRepository { get; set; }

        public CouponService(ICouponRepository repository) : base(repository)
        {
            CouponRepository = repository;
        }
    }
}
