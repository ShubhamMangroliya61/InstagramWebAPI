using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace InstagramWebAPI.DAL.Models;

[Table("Request")]
public partial class Request
{
    [Key]
    public long RequestId { get; set; }

    public long FromUserId { get; set; }

    public long ToUserId { get; set; }

    public bool? IsCloseFriend { get; set; }

    public bool IsAccepted { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ModifiedDate { get; set; }

    public bool IsDeleted { get; set; }

    [ForeignKey("FromUserId")]
    [InverseProperty("RequestFromUsers")]
    public virtual User FromUser { get; set; } = null!;

    [InverseProperty("Request")]
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    [ForeignKey("ToUserId")]
    [InverseProperty("RequestToUsers")]
    public virtual User ToUser { get; set; } = null!;
}
