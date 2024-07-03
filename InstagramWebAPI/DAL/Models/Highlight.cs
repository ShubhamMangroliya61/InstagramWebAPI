using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace InstagramWebAPI.DAL.Models;

public partial class Highlight
{
    [Key]
    public long HighlightsId { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string HighlightsName { get; set; } = null!;

    public long UserId { get; set; }

    public bool IsDeleted { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ModifiedDate { get; set; }

    [InverseProperty("Highlights")]
    public virtual ICollection<StoryHighlight> StoryHighlights { get; set; } = new List<StoryHighlight>();

    [ForeignKey("UserId")]
    [InverseProperty("Highlights")]
    public virtual User User { get; set; } = null!;
}
