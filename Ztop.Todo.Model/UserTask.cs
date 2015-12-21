﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ztop.Todo.Model
{
    [Table("user_task")]
    public class UserTask
    {
        public UserTask()
        {
            CreateTime = new DateTime();
        }

        [Key]
        [DatabaseGenerated(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public int TaskID { get; set; }

        public int UserID { get; set; }

        public bool IsCompleted { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime? CompletedTime { get; set; }
    }
}