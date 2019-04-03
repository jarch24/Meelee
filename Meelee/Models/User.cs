using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Meelee.Models
{
    public class User: TableEntity
    {
        public User()
        {

        }

        public User(string firstname, string lastname)
        {
            PartitionKey = firstname;
            RowKey = lastname;
        }
        public string  Email { get; set; }
        public string Phone { get; set; }
    }
}
