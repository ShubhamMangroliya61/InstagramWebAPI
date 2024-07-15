using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace InstagramWebAPI.DAL.Models;

[Table("User")]
public partial class User
{
    [Key]
    public long UserId { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string UserName { get; set; } = null!;

    [StringLength(100)]
    [Unicode(false)]
    public string? Email { get; set; }

    [StringLength(255)]
    [Unicode(false)]
    public string Password { get; set; } = null!;

    [StringLength(15)]
    [Unicode(false)]
    public string? Gender { get; set; }

    [StringLength(15)]
    [Unicode(false)]
    public string? ContactNumber { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? DateOfBirth { get; set; }

    [Unicode(false)]
    public string? ProfilePictureUrl { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? ProfilePictureName { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Link { get; set; }

    [Column(TypeName = "text")]
    public string? Bio { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Name { get; set; }

    public bool IsVerified { get; set; }

    public bool IsPrivate { get; set; }

    public bool IsDeleted { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ModifiedDate { get; set; }

    [InverseProperty("User")]
    public virtual ICollection<Collection> Collections { get; set; } = new List<Collection>();

    [InverseProperty("User")]
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    [InverseProperty("User")]
    public virtual ICollection<Highlight> Highlights { get; set; } = new List<Highlight>();

    [InverseProperty("User")]
    public virtual ICollection<Like> Likes { get; set; } = new List<Like>();

    [InverseProperty("FromUser")]
    public virtual ICollection<Notification> NotificationFromUsers { get; set; } = new List<Notification>();

    [InverseProperty("ToUser")]
    public virtual ICollection<Notification> NotificationToUsers { get; set; } = new List<Notification>();

    [InverseProperty("User")]
    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();

    [InverseProperty("FromUser")]
    public virtual ICollection<Request> RequestFromUsers { get; set; } = new List<Request>();

    [InverseProperty("ToUser")]
    public virtual ICollection<Request> RequestToUsers { get; set; } = new List<Request>();

    [InverseProperty("LoginUser")]
    public virtual ICollection<Search> SearchLoginUsers { get; set; } = new List<Search>();

    [InverseProperty("SearchUser")]
    public virtual ICollection<Search> SearchSearchUsers { get; set; } = new List<Search>();

    [InverseProperty("User")]
    public virtual ICollection<Story> Stories { get; set; } = new List<Story>();

    [InverseProperty("StoryViewUser")]
    public virtual ICollection<StoryView> StoryViews { get; set; } = new List<StoryView>();
}
