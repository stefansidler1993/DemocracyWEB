using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Web;

namespace Democracy2.Models
{
    public class DemocracyContext : DbContext
    {
        public DemocracyContext()
            : base("DefaultConnection")
        {

        }
        // this method is used to "disable" the cascade deleting mode
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
        }


        public DbSet<State> States { get; set; }

        public DbSet<Group> Groups { get; set; }

        public DbSet<Voting> Votings { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<GroupMember> GroupMembers { get; set; }

        public DbSet<VotingGroup> VotingGroups { get; set; }


        public DbSet<Candidate> Candidates { get; set; }


        public DbSet<VotingDetail> VotingDetails { get; set; }



    }
}