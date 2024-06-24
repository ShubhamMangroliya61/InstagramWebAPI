using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace InstagramWebAPI.DAL.Models;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<Like> Likes { get; set; }

    public virtual DbSet<MediaType> MediaTypes { get; set; }

    public virtual DbSet<Post> Posts { get; set; }

    public virtual DbSet<PostMapping> PostMappings { get; set; }

    public virtual DbSet<Request> Requests { get; set; }

    public virtual DbSet<Story> Stories { get; set; }

    public virtual DbSet<StoryView> StoryViews { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=192.168.0.5;Database=shubhammangroliya_db;User Id=shubham1;Password=WKy6qka6;TrustServerCertificate=true;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.CommentId).HasName("PK__Comment__C3B4DFCA1FFEA2EA");

            entity.HasOne(d => d.Post).WithMany(p => p.Comments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Comment__PostId__793DFFAF");

            entity.HasOne(d => d.User).WithMany(p => p.Comments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Comment__UserId__7849DB76");
        });

        modelBuilder.Entity<Like>(entity =>
        {
            entity.HasKey(e => e.LikeId).HasName("PK__Like__A2922C14FCDE588C");

            entity.HasOne(d => d.Post).WithMany(p => p.Likes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Like__PostId__74794A92");

            entity.HasOne(d => d.User).WithMany(p => p.Likes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Like__UserId__73852659");
        });

        modelBuilder.Entity<MediaType>(entity =>
        {
            entity.HasKey(e => e.MediaTypeId).HasName("PK__MediaTyp__0E6FCB72DD952BBD");
        });

        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(e => e.PostId).HasName("PK__Post__AA12601819664A10");

            entity.HasOne(d => d.PostType).WithMany(p => p.Posts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Post__PostTypeId__6FB49575");

            entity.HasOne(d => d.User).WithMany(p => p.Posts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Post__UserId__6EC0713C");
        });

        modelBuilder.Entity<PostMapping>(entity =>
        {
            entity.HasKey(e => e.PostMappingId).HasName("PK__PostMapp__F2670387C1360299");

            entity.HasOne(d => d.MediaType).WithMany(p => p.PostMappings)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PostMappi__Media__7E02B4CC");

            entity.HasOne(d => d.Post).WithMany(p => p.PostMappings)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PostMappi__PostI__7D0E9093");
        });

        modelBuilder.Entity<Request>(entity =>
        {
            entity.HasKey(e => e.RequestId).HasName("PK__Request__33A8517A5B5D5EDF");

            entity.HasOne(d => d.FromUser).WithMany(p => p.RequestFromUsers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Request__FromUse__078C1F06");

            entity.HasOne(d => d.ToUser).WithMany(p => p.RequestToUsers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Request__ToUserI__0880433F");
        });

        modelBuilder.Entity<Story>(entity =>
        {
            entity.HasKey(e => e.StoryId).HasName("PK__Story__3E82C048237F511F");

            entity.HasOne(d => d.StoryType).WithMany(p => p.Stories)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Story__StoryType__03BB8E22");

            entity.HasOne(d => d.User).WithMany(p => p.Stories)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Story__UserId__02C769E9");
        });

        modelBuilder.Entity<StoryView>(entity =>
        {
            entity.HasKey(e => e.StoryViewId).HasName("PK__StoryVie__7604765744933535");

            entity.HasOne(d => d.Story).WithMany(p => p.StoryViews)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StoryView__Story__0B5CAFEA");

            entity.HasOne(d => d.StoryViewUser).WithMany(p => p.StoryViews)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StoryView__Story__0C50D423");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__User__1788CC4C55B7ED27");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
