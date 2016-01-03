﻿using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwiftCourier.Models
{
    public class Role : IdentityRole<int>
    {
        public Role() : base()
        {
            RolePermissions = new HashSet<RolePermission>();
            /*
            Claims = new HashSet<RoleClaim>();
            Users = new HashSet<UserRole>();
            */
        }

        public virtual ICollection<RolePermission> RolePermissions { get; set; }
        /*
        public virtual new ICollection<RoleClaim> Claims { get; set; }
        public virtual new ICollection<UserRole> Users { get; set; }
        */
    }
}
