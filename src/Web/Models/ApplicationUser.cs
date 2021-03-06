﻿using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace Web.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public virtual List<NamedDistribution> LibraryAccessList { get; set; } = new List<NamedDistribution>();
    }
}
