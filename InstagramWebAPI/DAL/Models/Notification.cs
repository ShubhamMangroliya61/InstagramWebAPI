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

    public long UserId { get; set; }

    public long NotifireUserId { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string NotificationType { get; set; } = null!;

    public long? PostId { get; set; }

    public bool IsDeleted { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ModifiedDate { get; set; }

    [ForeignKey("NotifireUserId")]
    [InverseProperty("NotificationNotifireUsers")]
    public virtual User NotifireUser { get; set; } = null!;

    [ForeignKey("PostId")]
    [InverseProperty("Notifications")]
    public virtual Post? Post { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("NotificationUsers")]
    public virtual User User { get; set; } = null!;
}
