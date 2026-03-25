using System;
using System.Collections.Generic;

namespace BusinessObjects;

public partial class Admin
{
    public int AdminId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Status { get; set; } = null!;
}
