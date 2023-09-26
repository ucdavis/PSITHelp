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
        public virtual DbSet<Buildings> Buildings { get; set; }
        public virtual DbSet<WorkOrders> WorkOrders { get; set; }
        public virtual DbSet<Actions> Actions { get; set; }
        public virtual DbSet<ActiveStatus> ActiveStatus { get; set; }
        public virtual DbSet<AssignScheme> AssignSchemes { get; set; }
        public virtual DbSet<AssignmentSchemeTable> AssignmentSchemes { get; set; }
        public virtual DbSet<Files> Files { get; set; }
        public virtual DbSet<ManualEmployees> ManEmployees { get; set; }
        public virtual DbSet<PIGroups> PIGroups { get; set; }   
        public virtual DbSet<Status> Status { get; set; }
        public virtual DbSet<UserRequestPermissions> UserRequestPermissions { get; set; }
        public virtual DbSet<UserRequest> UserRequests { get; set; }
        public virtual DbSet<MigratedGroups> MigratedGroups { get; set; }
        public virtual DbSet<EmployeePreferences> Preferences { get; set; }
        public virtual DbSet<Notifications> Notifications { get; set; }
        public virtual DbSet<AssignmentStats> AssignmentStats { get; set; }
        public virtual DbSet<UserReequestPermissionsSummary> UserReequestPermissionsSummary { get; set; }





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
                entity.ToTable("cats_employees");

                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("employee_id");
                entity.Property(e => e.FirstName).HasColumnName("first_name");
                entity.Property(e => e.LastName).HasColumnName("last_name");                
                entity.Property(e => e.KerberosId).HasColumnName("kerberos_id");
                entity.Property(e => e.Role).HasColumnName("cats_role");
                entity.Property(e => e.Phone).HasColumnName("campus_phone");
                entity.Property(e => e.Building).HasColumnName("campus_bldg");
                entity.Property(e => e.Room).HasColumnName("campus_room");
                entity.Property(e => e.Email).HasColumnName("ucd_mailid");
            });

			modelBuilder.Entity<ManualEmployees>(entity =>
			{
				entity.ToTable("cats_manual_employees");

				entity.HasKey(e => e.Id);
				entity.Property(e => e.Id).HasColumnName("member_id");
				entity.Property(e => e.FirstName).HasColumnName("first_name");
				entity.Property(e => e.LastName).HasColumnName("last_name");
				entity.Property(e => e.KerberosId).HasColumnName("kerberos_id");
				entity.Property(e => e.Role).HasColumnName("cats_role");
				entity.Property(e => e.Phone).HasColumnName("campus_phone");
				entity.Property(e => e.Email).HasColumnName("ucd_mailid");
                entity.Property(e => e.Current).HasColumnName("cats_access");
			});

			modelBuilder.Entity<Status>(entity =>
            {
                entity.ToTable("status_trans");
                entity.Property(e => e.Id).HasColumnName("status");
                entity.Property(e => e.StatusTranslated).HasColumnName("status_trans");

            });

            modelBuilder.Entity<UserRequestPermissions>(entity =>
            {
                entity.ToTable("user_request_permissions");
                entity.Property(e => e.Id).HasColumnName("delegate_id");
				entity.Property(e => e.PIEmployeeId).HasColumnName("pi_employee_id");
                entity.Property(e => e.DelegateId).HasColumnName("delegate_employee_id");
                entity.Property(e => e.SDrive).HasColumnName("s_drive");
                entity.Property(e => e.ADGroup).HasColumnName("ad_group");
                entity.Property(e => e.BaseGroup).HasColumnName("base_ad_group");
			});

			modelBuilder.Entity<Buildings>(entity =>
			{
				entity.ToView("buildings");
			});

			modelBuilder.Entity<MigratedGroups>(entity =>
			{
				entity.Property(e => e.Id).HasColumnName("groupId");
			});

            modelBuilder.Entity<UserReequestPermissionsSummary>(entity =>
            {
                entity.HasNoKey();
            });

			modelBuilder.Entity<Actions>(entity =>
			{
				entity.ToTable("wo_actions");
                entity.Property(e =>e.Id).HasColumnName("action_id");
				entity.Property(e => e.WOId).HasColumnName("wo_id");
				entity.Property(e => e.Date).HasColumnName("action_date");
				entity.Property(e => e.Text).HasColumnName("action_text");
				entity.Property(e => e.SubmittedBy).HasColumnName("submitted_by");
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
                entity.Property(e => e.Contact).HasColumnName("phone");
            });

            modelBuilder.Entity<AssignScheme>(entity =>
            {
                entity.HasNoKey();
                entity.Property(e => e.AssignRoundRobin).HasColumnName("assign_round_robin");
                entity.Property(e => e.ResetDate).HasColumnName("reset_date");
                entity.Property(e => e.NextTech).HasColumnName("next_tech");
            });

            modelBuilder.Entity<AssignmentSchemeTable>(entity =>
            {
                entity.ToTable("assign_scheme");
                entity.Property(e => e.AssignRoundRobin).HasColumnName("assign_round_robin");
                entity.Property(e => e.ResetDate).HasColumnName("reset_date");
            });

            modelBuilder.Entity<AssignmentStats>(entity =>
            {
                entity.HasNoKey();
            });

            modelBuilder.Entity<Files>(entity =>
            {
                entity.ToTable("wo_files");
                entity.Property(e => e.Id).HasColumnName("attach_id");
                entity.Property(e => e.WOId).HasColumnName("wo_id");
                entity.Property(e => e.Name).HasColumnName("attach_name");
                entity.Property(e => e.Extension).HasColumnName("file_ext");
            });

            modelBuilder.Entity<EmployeePreferences>(entity =>
            {
                entity.ToTable("EmployeePreferences");
            });
                       
        }
          
    


        
    }
}