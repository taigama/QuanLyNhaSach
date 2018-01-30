using BookStore.Models;
using System.Collections.Generic;
using System.Data.Entity;

namespace BookStore.Data
{
    public class BookStoreInitializer : DropCreateDatabaseIfModelChanges<BookStoreContext>
    {
        protected override void Seed(BookStoreContext context)
        {
            var categories = new List<Category>
            {
                new Category { Name = "Truyện dài" }
            };
            categories.ForEach(x => context.Categories.Add(x));
            context.SaveChanges();
            
            var roles = new List<Role>
            {
                new Role { Name = "Admin" },
                new Role { Name = "ThuNgan" }
            };
            roles.ForEach(y => context.Roles.Add(y));
            context.SaveChanges();

            var users = new List<User>
            {
                new User { Username = "admin", Password = "daylapass", Birthday = new System.DateTime(2017, 11, 10), RoleId = 1 , FirstName = "Admin"},
                new User { Username = "thungan", Password = "daylapass", Birthday = new System.DateTime(2017, 11, 10), RoleId = 2 , FirstName = "Thu ngân"}
            };
            users.ForEach(x => context.Users.Add(x));
            context.SaveChanges();

            base.Seed(context);
        }
    }
}