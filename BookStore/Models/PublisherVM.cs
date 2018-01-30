using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BookStore.Data;

namespace BookStore.Models
{
    public class PublisherVM : ViewModel
    {
        public Publisher Publisher { get; set; }

        public static PublisherVM GeneratePublisherVM(BookStoreContext db, bool success = false)
        {
            PublisherVM model = new PublisherVM
            {
                Publisher = new Publisher(),
                Success = success,
            };
            return model;
        }

        public static PublisherVM GeneratePublisherVM(BookStoreContext db, int? id, bool success = false)
        {
            PublisherVM model = new PublisherVM
            {
                Publisher = db.Publishers.Find(id),
                Success = success,
            };
            return model;
        }
    }
}