using EImece.Domain.Entities;
using EImece.Domain.Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EImece.Domain.Repositories.IRepositories;

namespace EImece.Domain.Services
{
    public class SubsciberService : BaseEntityService<Subscriber>, ISubsciberService
    {

        private ISubscriberRepository SubscriberRepository { get; set; }
        public SubsciberService(ISubscriberRepository
            repository) : base(repository)
        {
            SubscriberRepository = repository;
        }
    }
}
