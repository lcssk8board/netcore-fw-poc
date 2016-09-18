using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace EFCore.Tests.Models
{
    public class ParameterData
    {
        public ParameterDirection Direction { get; set; }
        public DbType Type { get; set; }
        public int Size { get; set; }
        public string ParameterName { get; set; }
        public object Value { get; set; }
    }
}
