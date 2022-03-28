﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Bit.Core.Entities;
using Bit.Core.Enums;
using Bit.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Bit.Infrastructure.EntityFramework.Repositories
{
    public class OrganizationConnectionRepository : Repository<OrganizationConnection, Models.OrganizationConnection, Guid>, IOrganizationConnectionRepository
    {
        public OrganizationConnectionRepository(IServiceScopeFactory serviceScopeFactory,
            IMapper mapper)
            : base(serviceScopeFactory, mapper, context => context.OrganizationConnections)
        {
        }

        public async Task<ICollection<OrganizationConnection>> GetEnabledByOrganizationIdTypeAsync(Guid organizationId, OrganizationConnectionType type)
        {
            using (var scope = ServiceScopeFactory.CreateScope())
            {
                var dbContext = GetDatabaseContext(scope);
                var connections = await dbContext.OrganizationConnections
                    .Where(oc => oc.OrganizationId == organizationId && oc.Type == type && oc.Enabled)
                    .ToListAsync();
                return Mapper.Map<List<OrganizationConnection>>(connections);
            }
        }
    }
}
