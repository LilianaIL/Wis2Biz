using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Wis2Biz.Models
{
    public class ClientsContext : DbContext
    {
        public DbSet<ClientDetails> ClientDetails { get; set; }
        public DbSet<UserDetails> UserDetails { get; set; }
        public DbSet<ClientMeasures> ClientMeasures { get; set; }
        public ClientsContext(DbContextOptions<ClientsContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }
    }
}
