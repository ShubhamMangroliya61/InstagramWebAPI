using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace InstagramWebAPI.DAL.Models;

[Table("StoryView")]
public partial class StoryView
{
    [Key]
    public long StoryViewId { get; set; }

    public long StoryId { get; set; }

    public long StoryViewUserId { get; set; }

    public bool? IsLike { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedDate { get; set; }

    [ForeignKey("StoryId")]
    [InverseProperty("StoryViews")]
    public virtual Story Story { get; set; } = null!;

    [ForeignKey("StoryViewUserId")]
    [InverseProperty("StoryViews")]
    public virtual User StoryViewUser { get; set; } = null!;
}
