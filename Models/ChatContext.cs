using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace S5TcpChat.Models
{
    public class ChatContext : DbContext
    {
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Message> Messages { get; set; }
        public ChatContext()
        {
            
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) 
            => optionsBuilder
            .UseNpgsql("Host=localhost; Username=postgres; Password=example; DataBase=Seminar5;")
            .LogTo(Console.WriteLine);
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(user => user.Id);
                entity.ToTable("users");
                entity.Property(user => user.Id)
                      .HasColumnName("id");
                entity.Property(user => user.UserName)
                      .HasColumnName("name")
                      .HasMaxLength(255);

                entity.HasMany(user => user.RecievedMessage)
                      .WithOne(message => message.Consumer)
                      .HasForeignKey(message => message.ConsumerId)
                      .HasConstraintName("messages_from_user_id_fk_author_id")
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Message>(entity =>
            {
                entity.HasKey(message => message.Id);
                entity.Property(message => message.Content)
                      .HasColumnName("text");

                entity.HasOne(message => message.Author)
                      .WithMany(user => user.SendedMessage)
                      .HasForeignKey(message => message.AuthorId);
            });
            base.OnModelCreating(modelBuilder);
        }
    }
}
