using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace InstagramWebAPI.DAL.Models;

[Table("PostCollection")]
public partial class PostCollection
{
    [Key]
    public long PostCollectionId { get; set; }

    public long CollectionId { get; set; }

    public long PostId { get; set; }

    public bool IsDeleted { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ModifiedDate { get; set; }

    [ForeignKey("CollectionId")]
    [InverseProperty("PostCollections")]
    public virtual Collection Collection { get; set; } = null!;

    [ForeignKey("PostId")]
    [InverseProperty("PostCollections")]
    public virtual Post Post { get; set; } = null!;
}
