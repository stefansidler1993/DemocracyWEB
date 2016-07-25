using Democracy2.Migrations;
using Democracy2.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Democracy2
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {

           // Database.SetInitializer(new MigrateDatabaseToLatestVersion<Models.StoreContext, Migrations.Configuration>());
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<DemocracyContext, Configuration>());
            this.CreateSuperuser();
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);


        }


        private void CreateSuperuser()
        {
            var userContext = new ApplicationDbContext();
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(userContext));
            var db = new DemocracyContext();
            this.CheckRole("Admin", userContext);
            this.CheckRole("User", userContext);

            var user = db.Users
                .Where(u => u.UserName.ToLower().Equals("mshadows@gmail.com")).FirstOrDefault();

            if(user== null){

                 user = new User
                {
                    Adress = "Dark Side Of The Moon",
                    FirstName = "Matt",
                    LastName = "Sanders",
                    Phone = "327 344 5237",
                    UserName = "mshadows@gmail.com",
                    Photo = "~/Content/Photos/m shadows my heart.jpg",

                };

                 db.Users.Add(user);
                 db.SaveChanges();
            }

            var userASP = userManager.FindByName(user.UserName);

            if(userASP==null){

               userASP = new ApplicationUser
                {
                    UserName = user.UserName,
                    Email = user.UserName,
                    PhoneNumber = user.Phone,
                };

                userManager.Create(userASP, "mshadows123.");

            }

            userManager.AddToRole(userASP.Id, "Admin");

        }

        private void CheckRole(string roleName, ApplicationDbContext userContext)
        {
            // User management

            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(userContext));

            // Check to see if Role Exists, if not create it
            if (!roleManager.RoleExists(roleName))
            {
                roleManager.Create(new IdentityRole(roleName));
            }


        }
    }
}
