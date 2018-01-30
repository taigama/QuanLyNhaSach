using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Web;

namespace BookStore.Models
{
    public static class EnumExtension
    {
        public static string GetDescription(this Enum value)
        {
            Type type = value.GetType();
            string name = Enum.GetName(type, value);
            if (name != null)
            {
                FieldInfo field = type.GetField(name);
                if (field != null)
                {
                    DescriptionAttribute attr =
                            Attribute.GetCustomAttribute(field,
                                typeof(DescriptionAttribute)) as DescriptionAttribute;
                    if (attr != null)
                    {
                        return attr.Description;
                    }
                    else
                    {
                        return name;
                    }
                }
                else
                {
                    return name;
                }
            }
            return null;
        }

        public static string GetDescription<T>(this int value)
        {
            if (typeof(T).IsEnum)
            {
                Type type = typeof(T);
                string name = Enum.GetName(type, value);
                //return name;
                if (name != null)
                {
                    FieldInfo field = type.GetField(name);
                    if (field != null)
                    {
                        DescriptionAttribute attr =
                                Attribute.GetCustomAttribute(field,
                                    typeof(DescriptionAttribute)) as DescriptionAttribute;
                        if (attr != null)
                        {
                            return attr.Description;
                        }
                        else
                        {
                            return name;
                        }
                    }
                    else
                    {
                        return name;
                    }
                }
            }
            return null;
        }
    }

    // Tình trạng đơn hàng
    public enum OrderStatus
    {
        //[Display(Name ="Mới")]
        New,
        //[Display(Name = "Đã đóng gói")]
        Packing,
        //[Display(Name = "Đang giao")]
        Delivering,
        //[Display(Name = "Hoàn tất")]
        Completed,
        //[Display(Name = "Chờ")]
        Pending,
        //[Display(Name = "Huỷ")]
        Canceled
    }

    // Tình trạng phiếu nhập
    public enum ImportStatus
    {
        [Description("Đã áp dụng")]
        Applied,
        [Description("Đã hủy")]
        Cancelled,
    }

    // Loại sản phẩm
    public enum ProductType
    {
        [Description("Sách")]
        Sach = 0,
        [Description("Văn phòng phẩm")]
        VanPhongPham = 1,
        [Description("Băng đĩa")]
        BangDia = 2
    }

    // Đơn vị tính
    public enum UnitType
    {
        Cuốn = 0,
        Cây = 1,
        Cái = 2,
        Lốc = 3,
    }

    // Loại bìa
    public enum CoverType
    {
        Cứng = 0,
        Mềm = 1,
    }
}