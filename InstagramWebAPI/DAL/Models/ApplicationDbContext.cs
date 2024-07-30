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

    public virtual DbSet<Chat> Chats { get; set; }

    public virtual DbSet<Collection> Collections { get; set; }

    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<Highlight> Highlights { get; set; }

    public virtual DbSet<Like> Likes { get; set; }

    public virtual DbSet<MediaType> MediaTypes { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Post> Posts { get; set; }

    public virtual DbSet<PostCollection> PostCollections { get; set; }

    public virtual DbSet<PostMapping> PostMappings { get; set; }

    public virtual DbSet<Request> Requests { get; set; }

    public virtual DbSet<Search> Searches { get; set; }

    public virtual DbSet<Story> Stories { get; set; }

    public virtual DbSet<StoryHighlight> StoryHighlights { get; set; }

    public virtual DbSet<StoryView> StoryViews { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=192.168.0.5;Database=shubhammangroliya_db;User=shubham1;Password=WKy6qka6;Encrypt=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Chat>(entity =>
        {
            entity.HasKey(e => e.ChatId).HasName("PK__Chats__A9FBE7C6EEB090F3");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.FromUser).WithMany(p => p.ChatFromUsers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Chats__FromUserI__51EF2864");

            entity.HasOne(d => d.ToUser).WithMany(p => p.ChatToUsers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Chats__ToUserId__52E34C9D");
        });

        modelBuilder.Entity<Collection>(entity =>
        {
            entity.HasKey(e => e.CollectionId).HasName("PK__Collecti__7DE6BC044241DF8B");

            entity.HasOne(d => d.User).WithMany(p => p.Collections)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Collectio__UserI__318258D2");
        });

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

        modelBuilder.Entity<Highlight>(entity =>
        {
            entity.HasKey(e => e.HighlightsId).HasName("PK__Highligh__18642F6086857EDE");

            entity.HasOne(d => d.User).WithMany(p => p.Highlights)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Highlight__UserI__1F63A897");
        });

        modelBuilder.Entity<Like>(entity =>
        {
            entity.HasKey(e => e.LikeId).HasName("PK__Like__A2922C14FCDE588C");

            entity.Property(e => e.IsLike).HasDefaultValueSql("((1))");

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

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.MessageId).HasName("PK__Messages__C87C0C9C0E19304F");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Chat).WithMany(p => p.Messages)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Messages__ChatId__5B78929E");

            entity.HasOne(d => d.FromUser).WithMany(p => p.MessageFromUsers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Messages__FromUs__59904A2C");

            entity.HasOne(d => d.ToUser).WithMany(p => p.MessageToUsers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Messages__ToUser__5A846E65");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__20CF2E12BC647EA9");

            entity.HasOne(d => d.Comment).WithMany(p => p.Notifications).HasConstraintName("FK__Notificat__Comme__2BC97F7C");

            entity.HasOne(d => d.FromUser).WithMany(p => p.NotificationFromUsers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Notificat__FromU__27F8EE98");

            entity.HasOne(d => d.Like).WithMany(p => p.Notifications).HasConstraintName("FK__Notificat__LikeI__2AD55B43");

            entity.HasOne(d => d.Post).WithMany(p => p.Notifications).HasConstraintName("FK__Notificat__PostI__29E1370A");

            entity.HasOne(d => d.Request).WithMany(p => p.Notifications).HasConstraintName("FK__Notificat__Reque__2DB1C7EE");

            entity.HasOne(d => d.Story).WithMany(p => p.Notifications).HasConstraintName("FK__Notificat__Story__2CBDA3B5");

            entity.HasOne(d => d.ToUser).WithMany(p => p.NotificationToUsers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Notificat__ToUse__28ED12D1");
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

        modelBuilder.Entity<PostCollection>(entity =>
        {
            entity.HasKey(e => e.PostCollectionId).HasName("PK__PostColl__251EAA4584A25737");

            entity.HasOne(d => d.Collection).WithMany(p => p.PostCollections)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PostColle__Colle__3552E9B6");

            entity.HasOne(d => d.Post).WithMany(p => p.PostCollections)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PostColle__PostI__36470DEF");
        });

        modelBuilder.Entity<PostMapping>(entity =>
        {
            entity.HasKey(e => e.PostMappingId).HasName("PK__PostMapp__F267038701C597D5");

            entity.HasOne(d => d.MediaType).WithMany(p => p.PostMappings)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PostMappi__Media__14E61A24");

            entity.HasOne(d => d.Post).WithMany(p => p.PostMappings)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PostMappi__PostI__13F1F5EB");
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

        modelBuilder.Entity<Search>(entity =>
        {
            entity.HasKey(e => e.SearchId).HasName("PK__Search__21C535F4B22B53A3");

            entity.HasOne(d => d.LoginUser).WithMany(p => p.SearchLoginUsers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Search__LoginUse__3A179ED3");

            entity.HasOne(d => d.SearchUser).WithMany(p => p.SearchSearchUsers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Search__SearchUs__3B0BC30C");
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

        modelBuilder.Entity<StoryHighlight>(entity =>
        {
            entity.HasKey(e => e.StoryHighlightId).HasName("PK__StoryHig__BCCFBC40AD1FF9B9");

            entity.HasOne(d => d.Highlights).WithMany(p => p.StoryHighlights)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StoryHigh__Highl__2334397B");

            entity.HasOne(d => d.Story).WithMany(p => p.StoryHighlights)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StoryHigh__Story__24285DB4");
        });

        modelBuilder.Entity<StoryView>(entity =>
        {
            entity.HasKey(e => e.StoryViewId).HasName("PK__StoryVie__7604765732134E9F");

            entity.HasOne(d => d.Story).WithMany(p => p.StoryViews)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StoryView__Story__3DE82FB7");

            entity.HasOne(d => d.StoryViewUser).WithMany(p => p.StoryViews)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StoryView__Story__3EDC53F0");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__User__1788CC4C55B7ED27");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
