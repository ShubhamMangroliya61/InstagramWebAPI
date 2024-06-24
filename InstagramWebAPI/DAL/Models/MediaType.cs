using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace InstagramWebAPI.DAL.Models;

[Table("MediaType")]
public partial class MediaType
{
    [Key]
    public int MediaTypeId { get; set; }

    [Column("MediaType")]
    [StringLength(20)]
    [Unicode(false)]
    public string MediaType1 { get; set; } = null!;

    [InverseProperty("MediaType")]
    public virtual ICollection<PostMapping> PostMappings { get; set; } = new List<PostMapping>();

    [InverseProperty("PostType")]
    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();

    [InverseProperty("StoryType")]
    public virtual ICollection<Story> Stories { get; set; } = new List<Story>();
}
