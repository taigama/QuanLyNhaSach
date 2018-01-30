using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BookStore.Models
{
    /// <summary>
    /// Let the front-end know how their action was done
    /// </summary>
    public class ResultWeb
    {
        public enum ResultType
        {
            SOMETHING_NOT_RIGHT = -1,
            OK = 0,
            OK_ADD = 1,
            OK_UPDATE = 2,
            OK_DELETE = 3,
            FIELD_INVALID = 4,
            NOT_FOUND = 5,
            SERVER_NOT_READY = 6,
            ACCESS_VIOLENCE = 7,
            OUT_OF_STOCK = 8
        }

        public ResultType Type { get; set; }
        public int Number { get; set; }
        public string StringValue { get; set; }
    }
}