﻿using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrakashCRM.Data.Models
{
    public class SPRoleRights
    {
        public string Role_No { get; set; }

        public string Menu_No { get; set; }

        public string Sub_Menu_No { get; set; }

        public bool Full_Rights { get; set; }

        public bool Add_Rights { get; set; }

        public bool Edit_Rights { get; set; }

        public bool View_Rights { get; set; }

        public bool Delete_Rights { get; set; }

        public bool IsActive { get; set; }
    }
    public class SPRoleRightsResponse
    {
        public string No { get; set; }

        public string Role_No { get; set; }

        public string Menu_No { get; set; }

        public string Sub_Menu_No { get; set; }

        public bool Full_Rights { get; set; }

        public bool Add_Rights { get; set; }

        public bool Edit_Rights { get; set; }

        public bool View_Rights { get; set; }

        public bool Delete_Rights { get; set; }

        public bool IsActive { get; set; }
    }
    public class SPRolesForDDL
    {
        public string No { get; set; }

        public string Role_Name { get; set; }
    }
    public class SPMenusSubMenusOfRole
    {
        public string No { get; set; }

        public string Role_No { get; set; }

        public string Menu_No { get; set; }

        public string Sub_Menu_No { get; set; }

        public bool Full_Rights { get; set; }

        public bool Add_Rights { get; set; }

        public bool Edit_Rights { get; set; }

        public bool Delete_Rights { get; set; }

        public bool View_Rights { get; set; }
    }
    public class SPMenusRightsForDel
    {
        public string roleid { get; set; }
    }
    public class SPMenusRightsForDelRes
    {
        public bool value { get; set; }
    }
}
