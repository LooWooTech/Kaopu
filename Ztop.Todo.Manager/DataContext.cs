﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ztop.Todo.Model;

namespace Ztop.Todo.Manager
{
    public class DataContext : DbContext
    {
        public DataContext():base("name=DbConnection")
        {
            Database.SetInitializer<DataContext>(null);
        }

        public DbSet<User> Users { get; set; }

        public DbSet<UserGroup> UserGroups { get; set; }

        public DbSet<Model.Task> Tasks { get; set; }

        public DbSet<UserTask> UserTasks { get; set; }

        public DbSet<UserTaskView> UserTaskViews { get; set; }

        public DbSet<TaskQuery> TaskQueries { get; set; }

        public DbSet<Comment> Comments { get; set; }

        public DbSet<Attachment> Attachments { get; set; }
        public DbSet<Authorize> Authorizes { get; set; }
        public DbSet<DataBook> DataBooks { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<SerialNumber> SerialNumbers { get; set; }
        public DbSet<Sheet> Sheets { get; set; }
        public DbSet<Substancs> Substances { get; set; }
        public DbSet<Verify> Verifys { get; set; }
        public DbSet<Record> Records { get; set; }
        public DbSet<Evection> Evections { get; set; }
        public DbSet<Errand> Errands { get; set; }
        public DbSet<Traffic> Traffics { get; set; }
    }
}
