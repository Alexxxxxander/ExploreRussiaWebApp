﻿using System;
using System.Collections.Generic;

namespace ExploreRussia.Domain.Models;

public partial class Guide
{
    public int Id { get; set; }

    public string LastName { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string? Patronymic { get; set; }

    public string Phone { get; set; } = null!;

    public string Email { get; set; } = null!;

    public int ExperienceYears { get; set; }

    public int GenderId { get; set; }

    public string? ImageUrl { get; set; }

    public virtual Gender Gender { get; set; } = null!;

    public virtual ICollection<Trip> Trips { get; set; } = new List<Trip>();
}
