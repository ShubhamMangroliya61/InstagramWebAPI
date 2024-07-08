using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace InstagramWebAPI.DAL.Models;

[Table("Story")]
public partial class Story
{
    [Key]
    public long StoryId { get; set; }

    public long UserId { get; set; }

    public int StoryTypeId { get; set; }

    [Column("StoryURL")]
    [StringLength(150)]
    [Unicode(false)]
    public string StoryUrl { get; set; } = null!;

    [StringLength(100)]
    [Unicode(false)]
    public string StoryName { get; set; } = null!;

    public int? StoryDuration { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Caption { get; set; }

    public bool IsHighlighted { get; set; }

    public bool IsDeleted { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ModifiedDate { get; set; }

    [InverseProperty("Story")]
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    [InverseProperty("Story")]
    public virtual ICollection<StoryHighlight> StoryHighlights { get; set; } = new List<StoryHighlight>();

    [ForeignKey("StoryTypeId")]
    [InverseProperty("Stories")]
    public virtual MediaType StoryType { get; set; } = null!;

    [InverseProperty("Story")]
    public virtual ICollection<StoryView> StoryViews { get; set; } = new List<StoryView>();

    [ForeignKey("UserId")]
    [InverseProperty("Stories")]
    public virtual User User { get; set; } = null!;
}
