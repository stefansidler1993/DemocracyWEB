using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Democracy2.Models
{
    public class GroupMember
    {
        [Key]
        public int GroupMemberId { get; set; }

        public int GroupId { get; set; }

        public int UserId { get; set; }

        // part many of the relationship, it means a groupmember belongs to a group
        public virtual Group Group { get; set; }

        // part many of the relationship, it means a user can belong to many groups
        public virtual User User{ get; set; }

    }
}