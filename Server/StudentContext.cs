using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
namespace Server
{
    class StudentsContext : DbContext
    {
        public StudentsContext() : base("DBConnection")
        { }
        public DbSet<Student> Students { get; set; }
    }
}
