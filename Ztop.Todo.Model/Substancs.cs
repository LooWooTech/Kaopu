﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ztop.Todo.Model
{
    /// <summary>
    /// 分项清单
    /// </summary>
    [Table("substancs")]
    public class Substancs
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        [Column(TypeName ="int")]
        public Category Category { get; set; }
        public string Details { get; set; }
        public double Price { get; set; }
        public int SID { get; set; }
    }

    public enum Category
    {
        [Description("招待费用")]
        Reception,
        [Description("办公用品")]
        OfficialBussiness,
        [Description("市内交通")]
        Traffic,
        [Description("设备费用")]
        Equipment,
        [Description("车辆费用")]
        Bus,
        [Description("评审费用")]
        Evaluate,
        [Description("会议费用")]
        Meeting,
        [Description("印刷装订")]
        Print,
        [Description("福利费用")]
        Welfare,
        [Description("快递电通信等")]
        Express,
        [Description("财务费用")]
        Financial,
        [Description("其他")]
        Other

    }
}
