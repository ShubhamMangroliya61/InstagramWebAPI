using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace InstagramWebAPI.DAL.Models;

[Table("Search")]
public partial class Search
{
    [Key]
    public long SearchId { get; set; }

    public long LoginUserId { get; set; }

    public long SearchUserId { get; set; }

    public bool IsDeleted { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ModifiedDate { get; set; }

    [ForeignKey("LoginUserId")]
    [InverseProperty("SearchLoginUsers")]
    public virtual User LoginUser { get; set; } = null!;

    [ForeignKey("SearchUserId")]
    [InverseProperty("SearchSearchUsers")]
    public virtual User SearchUser { get; set; } = null!;
}
