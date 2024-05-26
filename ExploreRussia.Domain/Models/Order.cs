using System;
using System.Collections.Generic;

namespace ExploreRussia.Domain.Models;

public partial class Order
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int TripId { get; set; }

    public DateTime OrderDate { get; set; }

    public decimal TotalAmount { get; set; }

    public virtual Trip Trip { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
