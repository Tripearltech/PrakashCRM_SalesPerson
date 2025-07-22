using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrakashCRM.Data.Models
{
    public class SPRoles
    {
        [Required(ErrorMessage = "Role Name is required")]
        public string Role_Name { get; set; }

        public bool IsActive { get; set; }
    }
    public class SPRolesResponse
    {
        public string No { get; set; }

        public string Role_Name { get; set; }

        public bool IsActive { get; set; }
    }
    public class SPRoleList
    {
        public string No { get; set; }

        public string Role_Name { get; set; }

        public bool IsActive { get; set; }
    }
}
