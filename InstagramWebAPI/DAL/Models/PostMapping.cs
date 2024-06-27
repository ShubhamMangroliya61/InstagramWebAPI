using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace InstagramWebAPI.DAL.Models;

[Table("PostMapping")]
public partial class PostMapping
{
    [Key]
    public long PostMappingId { get; set; }

    public long PostId { get; set; }

    public int MediaTypeId { get; set; }

    [Column("MediaURL")]
    [Unicode(false)]
    public string MediaUrl { get; set; } = null!;

    [StringLength(100)]
    [Unicode(false)]
    public string MediaName { get; set; } = null!;

    public bool IsDeleted { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ModifiedDate { get; set; }

    [ForeignKey("MediaTypeId")]
    [InverseProperty("PostMappings")]
    public virtual MediaType MediaType { get; set; } = null!;

    [ForeignKey("PostId")]
    [InverseProperty("PostMappings")]
    public virtual Post Post { get; set; } = null!;
}
