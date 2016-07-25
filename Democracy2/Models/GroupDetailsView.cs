using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Democracy2.Models
{
    public class GroupDetailsView
    {
        public int Groupid { get; set; }



        // gets an sets the Group description
        public String Descripcion { get; set; }


        //List of members
        public List<GroupMember> Members { get; set; }

    }
}