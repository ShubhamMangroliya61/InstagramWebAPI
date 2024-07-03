using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace InstagramWebAPI.DAL.Models;

[Table("Post")]
public partial class Post
{
    [Key]
    public long PostId { get; set; }

    public long UserId { get; set; }

    [Column(TypeName = "text")]
    public string? Caption { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? Location { get; set; }

    public int PostTypeId { get; set; }

    public bool IsDeleted { get; set; }

    public bool IsSaved { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ModifiedDate { get; set; }

    [InverseProperty("Post")]
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    [InverseProperty("Post")]
    public virtual ICollection<Like> Likes { get; set; } = new List<Like>();

    [InverseProperty("Post")]
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    [InverseProperty("Post")]
    public virtual ICollection<PostMapping> PostMappings { get; set; } = new List<PostMapping>();

    [ForeignKey("PostTypeId")]
    [InverseProperty("Posts")]
    public virtual MediaType PostType { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("Posts")]
    public virtual User User { get; set; } = null!;
}
