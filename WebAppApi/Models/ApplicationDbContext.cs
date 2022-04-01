using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAppApi.Models
{
    public partial class ApplicationDbContext : IdentityDbContext
    {
        // Constractor
        public ApplicationDbContext() { }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }


        // Tables In Database
        public virtual DbSet<Employee> Employees { get; set; }


    }
}
