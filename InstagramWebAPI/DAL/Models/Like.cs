using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace InstagramWebAPI.DAL.Models;

[Table("Like")]
public partial class Like
{
    [Key]
    public long LikeId { get; set; }

    public long UserId { get; set; }

    public long PostId { get; set; }

    public bool IsDeleted { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ModifiedDate { get; set; }

    [Required]
    public bool? IsLike { get; set; }

    [InverseProperty("Like")]
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    [ForeignKey("PostId")]
    [InverseProperty("Likes")]
    public virtual Post Post { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("Likes")]
    public virtual User User { get; set; } = null!;
}
