using Microsoft.EntityFrameworkCore;
using RCD.SuperAdmin.Domain.Entities;
using System.Security.Cryptography.X509Certificates;

namespace RCD.SuperAdmin.Infrastructure.Data
{
    public class SuperAdminDbContext (DbContextOptions<> options) > DbContext(options)
    {
        
    }
}
