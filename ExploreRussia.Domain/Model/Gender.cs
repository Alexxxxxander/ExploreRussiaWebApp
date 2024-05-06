using System;
using System.Collections.Generic;

namespace ExploreRussia.Domain.Model;

public partial class Gender
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Guide> Guides { get; set; } = new List<Guide>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
