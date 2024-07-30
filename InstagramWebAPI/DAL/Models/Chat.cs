using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace InstagramWebAPI.DAL.Models;

public partial class Chat
{
    [Key]
    public long ChatId { get; set; }

    public long FromUserId { get; set; }

    public long ToUserId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? LastSeen { get; set; }

    public bool IsDeleted { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ModifiedDate { get; set; }

    [ForeignKey("FromUserId")]
    [InverseProperty("ChatFromUsers")]
    public virtual User FromUser { get; set; } = null!;

    [InverseProperty("Chat")]
    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    [ForeignKey("ToUserId")]
    [InverseProperty("ChatToUsers")]
    public virtual User ToUser { get; set; } = null!;
}
