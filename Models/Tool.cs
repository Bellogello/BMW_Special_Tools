using System;
using System.Collections.Generic;

namespace BMW_Special_Tools.Models;

public partial class Tool
{
    public int Id { get; set; }

    public string Number { get; set; } = null!;

    public string Bmwnumber { get; set; } = null!;

    public string? KitNumber { get; set; }

    public string? BmwkitNumber { get; set; }

    public string EnglishName { get; set; } = null!;

    public string ArabicName { get; set; } = null!;

    public decimal CostPrice { get; set; }

    public string? MainImagePath { get; set; }

    public int StatusId { get; set; }

    public int TypeId { get; set; }

    public string QrTag { get; set; } = null!;
}
