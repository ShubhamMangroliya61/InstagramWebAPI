using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace InstagramWebAPI.DAL.Models;

[Table("Notification")]
public partial class Notification
{
    [Key]
    public long NotificationId { get; set; }

    public long FromUserId { get; set; }

    public long ToUserId { get; set; }

    public int NotificationType { get; set; }

    public long? PostId { get; set; }

    public long? LikeId { get; set; }

    public long? CommentId { get; set; }

    public long? StoryId { get; set; }

    public bool IsDeleted { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ModifiedDate { get; set; }

    public long? RequestId { get; set; }

    [ForeignKey("CommentId")]
    [InverseProperty("Notifications")]
    public virtual Comment? Comment { get; set; }

    [ForeignKey("FromUserId")]
    [InverseProperty("NotificationFromUsers")]
    public virtual User FromUser { get; set; } = null!;

    [ForeignKey("LikeId")]
    [InverseProperty("Notifications")]
    public virtual Like? Like { get; set; }

    [ForeignKey("PostId")]
    [InverseProperty("Notifications")]
    public virtual Post? Post { get; set; }

    [ForeignKey("RequestId")]
    [InverseProperty("Notifications")]
    public virtual Request? Request { get; set; }

    [ForeignKey("StoryId")]
    [InverseProperty("Notifications")]
    public virtual Story? Story { get; set; }

    [ForeignKey("ToUserId")]
    [InverseProperty("NotificationToUsers")]
    public virtual User ToUser { get; set; } = null!;
}
