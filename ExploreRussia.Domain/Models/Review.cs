using System;
using System.Collections.Generic;

namespace ExploreRussia.Domain.Models;

public partial class Review
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int TripId { get; set; }

    public int Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime ReviewDate { get; set; }

    public virtual Trip Trip { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
