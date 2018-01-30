using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BookStore.Data;

namespace BookStore.Models
{
    public class AuthorVM : ViewModel
    {
        public Author Author { get; set; }

        public static AuthorVM GenerateAuthorVM(BookStoreContext db, bool success = false)
        {
            AuthorVM model = new AuthorVM
            {
                Author = new Author(),
                Success = success,
            };
            return model;
        }

        public static AuthorVM GenerateAuthorVM(BookStoreContext db, int? id, bool success = false)
        {
            AuthorVM model = new AuthorVM
            {
                Author = db.Authors.Find(id),
                Success = success,
            };
            return model;
        }
    }
}