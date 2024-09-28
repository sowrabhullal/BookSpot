using BookSpot.DataAccess.Data;
using BookSpot.Models;
using BookSpot.Utility;
using BookSpot.DataAccess.DbInitializer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookSpot.DataAccess.DBInitializer
{
    public class DBInitializer : IDBInitializer
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<IdentityUser> userManager;
        private readonly ApplicationDbContext db;
        public DBInitializer(RoleManager<IdentityRole> r,
        UserManager<IdentityUser> u,
        ApplicationDbContext a) {
            roleManager = r;
            userManager = u;
            db = a;
        }

        //Invoked in program.cs
        public void Initialize()
        {
            //apply migrations if not applied
            try {
                if (db.Database.GetPendingMigrations().Count() > 0) { 
                    db.Database.Migrate();
                }
            }
            catch(Exception Ex) { 
            
            }

            //create roles if not created
            //checks if the role exists in rolemanager databse
            if (!roleManager.RoleExistsAsync(SD.Role_Customer).GetAwaiter().GetResult())
            {
                roleManager.CreateAsync(new IdentityRole(SD.Role_Customer)).GetAwaiter().GetResult();
                roleManager.CreateAsync(new IdentityRole(SD.Role_Company)).GetAwaiter().GetResult();
                roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).GetAwaiter().GetResult();
                roleManager.CreateAsync(new IdentityRole(SD.Role_Employee)).GetAwaiter().GetResult();

                //if roles not created, create admin user as well
                userManager.CreateAsync(new ApplicationUser
                {
                    UserName = "admin@gmail.com",
                    Email = "admin@gmail.com",
                    Name = "Sowrabh S Ullal",
                    PhoneNumber = "9731784327",
                    StreetAddress = "Marathahalli",
                    State = "KAR",
                    PostalCode = "575020",
                    City = "Bangalore"
                }, "admin@123").GetAwaiter().GetResult();

                ApplicationUser user = db.ApplicationUser.FirstOrDefault(u => u.Email == "admin@gmail.com");
                userManager.AddToRoleAsync(user, SD.Role_Admin).GetAwaiter().GetResult();
            }

            return;
        }
    }
}
