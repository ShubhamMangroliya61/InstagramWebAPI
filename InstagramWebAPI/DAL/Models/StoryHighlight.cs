using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace InstagramWebAPI.DAL.Models;

[Table("StoryHighlight")]
public partial class StoryHighlight
{
    [Key]
    public long StoryHighlightId { get; set; }

    public long HighlightsId { get; set; }

    public long StoryId { get; set; }

    public bool IsDeleted { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ModifiedDate { get; set; }

    [ForeignKey("HighlightsId")]
    [InverseProperty("StoryHighlights")]
    public virtual Highlight Highlights { get; set; } = null!;

    [ForeignKey("StoryId")]
    [InverseProperty("StoryHighlights")]
    public virtual Story Story { get; set; } = null!;
}
