using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BookStore.Models;
using BookStore.Data;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using BookStore.Helpers;
using System.Data.Entity;
using BookStore.Controllers;


namespace BookStore.Helpers
{
   
    public class HtmlExtensions
    {

        private static BookStoreContext db = new BookStoreContext();


        public static string ConvertTinhTrang(bool value)
        {
            return value ? "Có" : "Không có";
        }


        public static string ConvertMoney(decimal value)
        {
            string temp = value.ToString();
            string result = String.Empty;

            int count = 0;
            for (int i = temp.Length - 1; i >= 0; i--)
            {
                
                if (count >= 3)
                {
                    result = ' ' + result;
                    count = 0;
                }
                result = temp[i] + result;
                count++;
            }
            return result;
        }


        public static int ToHtmlInt(int i) => i;


        public static string ToHtmlPercent(double percent) => Double.IsNaN(percent) ? "Không xác định" : percent.ToString() + " %";


        public static string GetCategoryNameFromId(int id)
        {
            return db.Categories.Find(id).Name;
        }


        public static int TypeCount<T>()
        {
            return Enum.GetNames(typeof(T)).Length;
        }


        public static List<Pair> GenerateDropdownProductCategory(BookStoreContext db, ProductType type)
        {
            List<Pair> result = new List<Pair>();
            foreach (var item in db.Categories)
            {
                if (item.Type == type)
                {
                    result.Add(new Pair
                    {
                        Key = item.ID,
                        Value = item.Name,
                    });
                }
            }
            return result;
        }


        public static List<Pair> GenerateDropdownPublisher(BookStoreContext db)
        {
            List<Pair> result = new List<Pair>();
            foreach (var item in db.Publishers)
            {
                result.Add(new Pair
                {
                    Key = item.ID,
                    Value = item.Name,
                });
            }
            return result;
        }


        public static List<Pair> GenerateDropdownEnum<T>() 
        {
            List<Pair> result = new List<Pair>();
            for (int i = 0; i < TypeCount<T>(); i++)
            {
                result.Add(new Pair
                {
                    Key = i,
                    Value = i.GetDescription<T>(),
                });
            }
            return result;
        }

        public static List<Pair> GenerateDropdownEnumWithAny<T>()
        {
            List<Pair> result = new List<Pair>();
            for (int i = 0; i < TypeCount<T>(); i++)
            {
                result.Add(new Pair
                {
                    Key = i,
                    Value = i.GetDescription<T>(),
                });
            }
            result.Insert(0, new Pair { Key = -1, Value = "Tất cả" });
            return result;
        }


        public static List<Pair> GenerateDropdownProductType()
        {
            List<Pair> result = new List<Pair>();
            for (int i = 0; i < TypeCount<ProductType>(); i++)
            {
                result.Add(new Pair
                {
                    Key = i,
                    Value = ((ProductType)i).GetDescription(),
                });
            }
            return result;
        }


        public static string RandomString(int length)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        

        public static string GetImagePath(string relativePath, HttpContextBase contextBase)
        {
            if (relativePath != null && relativePath != String.Empty)
            {
                return UrlHelper.GenerateContentUrl(relativePath, contextBase);
            }
            else
            {
                return String.Empty;
            }
        }


        public static string CheckInStockAsString(int value)
        {
            return value > 0 ? value.ToString() : "Hết hàng";
        }

        public static string CheckStockState(int value)
        {
            return value > 0 ? "Còn hàng" : "Hết hàng";
        }
    }
}