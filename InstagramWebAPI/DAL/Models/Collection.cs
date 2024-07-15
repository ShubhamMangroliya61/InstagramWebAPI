using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace InstagramWebAPI.DAL.Models;

[Table("Collection")]
public partial class Collection
{
    [Key]
    public long CollectionId { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string CollectionName { get; set; } = null!;

    public long UserId { get; set; }

    public bool IsDeleted { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ModifiedDate { get; set; }

    [InverseProperty("Collection")]
    public virtual ICollection<PostCollection> PostCollections { get; set; } = new List<PostCollection>();

    [ForeignKey("UserId")]
    [InverseProperty("Collections")]
    public virtual User User { get; set; } = null!;
}
