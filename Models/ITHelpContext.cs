using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ITHelp.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ITHelp.Models
{
    public class ITHelpContext : DbContext
    {
        public ITHelpContext (DbContextOptions<ITHelpContext> options)
            : base(options)
        {
        }

        
        public virtual DbSet<Employee> Employees { get; set; }
       

        public static ILoggerFactory GetLoggerFactory()
        {
            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging(builder =>
                   builder.AddConsole()
                          .AddFilter(DbLoggerCategory.Database.Command.Name,
                                     LogLevel.Information));
            return serviceCollection.BuildServiceProvider()
                    .GetService<ILoggerFactory>();
        }
          
    


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.ToTable("employees");

                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ucpath_id");
                entity.Property(e => e.FirstName).HasColumnName("first_name");
                entity.Property(e => e.LastName).HasColumnName("last_name");                
                entity.Property(e => e.KerberosId).HasColumnName("kerberos_id");
                entity.Property(e => e.Role).HasColumnName("cats_role");
            });

            
        }
    }
}