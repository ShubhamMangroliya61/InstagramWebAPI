using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace InstagramWebAPI.DAL.Models;

public partial class Message
{
    [Key]
    public long MessageId { get; set; }

    public long ChatId { get; set; }

    public long FromUserId { get; set; }

    public long ToUserId { get; set; }

    public string MessageText { get; set; } = null!;

    public bool IsSeen { get; set; }

    public bool IsDelivered { get; set; }

    public bool IsDeleted { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ModifiedDate { get; set; }

    [ForeignKey("ChatId")]
    [InverseProperty("Messages")]
    public virtual Chat Chat { get; set; } = null!;

    [ForeignKey("FromUserId")]
    [InverseProperty("MessageFromUsers")]
    public virtual User FromUser { get; set; } = null!;

    [ForeignKey("ToUserId")]
    [InverseProperty("MessageToUsers")]
    public virtual User ToUser { get; set; } = null!;
}
