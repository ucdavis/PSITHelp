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
        public virtual DbSet<WorkOrders> WorkOrders { get; set; }
        public virtual DbSet<Actions> Actions { get; set; }
        public virtual DbSet<ActiveStatus> ActiveStatus { get; set; }
        public virtual DbSet<AssignScheme> AssignSchemes { get; set; }
        public virtual DbSet<Files> Files { get; set; }
        public virtual DbSet<ManualEmployees> ManEmployees { get; set; }
        public virtual DbSet<PIGroups> PIGroups { get; set; }   
        public virtual DbSet<Status> Status { get; set; }
        public virtual DbSet<UserReequestPermissions> UserReequestPermissions { get; set; }
        public virtual DbSet<UserRequest> UserRequests { get; set; }
        public virtual DbSet<MigratedGroups> MigratedGroups { get; set; }


       

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

            modelBuilder.Entity<Status>(entity =>
            {
                entity.ToTable("status_trans");
                entity.Property(e => e.Id).HasColumnName("status");
                entity.Property(e => e.StatusTranslated).HasColumnName("status_trans");

            });

            modelBuilder.Entity<WorkOrders>(entity =>
            {
                entity.ToTable("work_orders");
                entity.Property(e => e.Id).HasColumnName("wo_id");
                entity.Property(e =>e.SubmittedBy).HasColumnName("submitted_by");
                entity.Property(e => e.RequestDate).HasColumnName("request_date");
                entity.Property(e => e.CloseDate).HasColumnName("close_date");
                entity.Property(e => e.DueDate).HasColumnName("due_date");
                entity.Property(e => e.FullText).HasColumnName("full_text");
                entity.Property(e => e.TechComments).HasColumnName("tech_comments");
                entity.Property(e => e.ComputerTag).HasColumnName("comp_tag");
                entity.Property(e => e.RateComment).HasColumnName("rate_comment");
                entity.Property(e => e.CreatedBy).HasColumnName("created_by");
                entity.Property(e => e.Difficulty).HasColumnName("difficulty_rating");
                entity.Property(e => e.Building).HasColumnName("bldg");
            });

            modelBuilder.Entity<AssignScheme>(entity =>
            {
                entity.HasNoKey();                
            });

            
        }
          
    


        
    }
}