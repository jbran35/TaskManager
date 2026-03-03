using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskManager.Domain.Entities;
using TaskManager.Domain.ValueObjects;


namespace TaskManager.Infrastructure
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<User, IdentityRole<Guid>, Guid>(options)
    {
        public DbSet<Project> Projects { get; set; }
        public DbSet<TodoItem> TodoItems { get; set; }
        public DbSet<UserConnection> UserConnections { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Project>(builder =>
            {
                builder.HasKey(p => p.Id);

                builder.HasOne(p => p.Owner)
                    .WithMany()
                    .HasForeignKey(p => p.OwnerId)
                    .OnDelete(DeleteBehavior.Restrict);

                builder.Property(p => p.Title).HasConversion(
                    title => title.Value,
                    value => Title.Create(value).Value).HasMaxLength(200);
       

                builder.Property(p => p.Description).HasConversion(
                    description => description.Value, 
                    value => Description.Create(value).Value).HasMaxLength(2000);
            });



            modelBuilder.Entity<UserConnection>(b =>
            {
                b.HasKey(uc => uc.Id);

                b.HasOne(uc => uc.User)
                 .WithMany(u => u.Connections)
                 .HasForeignKey(uc => uc.UserId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(uc => uc.Assignee)
                 .WithMany(u => u.ConnectedTo)
                 .HasForeignKey(uc => uc.AssigneeId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.ToTable("UserConnections");
            });
         

            modelBuilder.Entity<TodoItem>(builder =>
            {
                builder.HasKey(t => t.Id);

                builder.Property(t => t.Id).ValueGeneratedOnAdd();

                builder.HasOne(t => t.Project)           
                    .WithMany(p => p.TodoItems)                    
                    .HasForeignKey(t => t.ProjectId) 
                    .OnDelete(DeleteBehavior.Cascade);


                builder.HasOne(t => t.Owner)                
                    .WithMany()                          
                    .HasForeignKey(t => t.OwnerId)       
                    .OnDelete(DeleteBehavior.Restrict);

                builder.HasOne(t => t.Assignee)
                    .WithMany()
                    .HasForeignKey(t => t.AssigneeId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.SetNull);

                builder.Property(t => t.Title).HasConversion(
                     title => title.Value,
                     value => Title.Create(value).Value).HasMaxLength(200);

                builder.Property(t => t.Description).HasConversion(
                    description => description.Value,
                    value => Description.Create(value).Value).HasMaxLength(2000);
            });
        }
    }
}
