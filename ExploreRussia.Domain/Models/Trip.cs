using System;
using System.Collections.Generic;

namespace ExploreRussia.Domain.Models;

public partial class Trip
{
    public int Id { get; set; }

    public string TripName { get; set; } = null!;

    public string Description { get; set; } = null!;

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public decimal? Price { get; set; }

    public int? MaxParticipants { get; set; }

    public int? GuideId { get; set; }

    public string? ImageUrl { get; set; }

    public virtual Guide? Guide { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    //среднее значение рейтинга
    public double AverageRating => Reviews.Any() ? Reviews.Average(r => r.Rating) : 0;
}
