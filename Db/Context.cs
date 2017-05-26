using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinkedInHelper.Models;

namespace LinkedInHelper.Db
{
    class Context : DbContext
    {
        public Context() : base("DBConnection")
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<Context, Migrations.Configuration>("DBConnection"));
        }

        public DbSet<Friend> Friends { get; set; }
    }
}
