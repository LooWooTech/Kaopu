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
    /// 出差报销  分类项目
    /// </summary>
    [Table("evections")]
    public class Evection
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        /// <summary>
        /// 出差地点
        /// </summary>
        public string Place { get; set; }
        /// <summary>
        /// 公里数
        /// </summary>
        public double KiloMeters { get; set; }
        /// <summary>
        /// 用车类型
        /// </summary>
        public BusType BusType { get; set; }
        /// <summary>
        /// 交通费
        /// </summary>
        public double Traffic { get; set; }
        /// <summary>
        /// 出差补贴
        /// </summary>
        public int SubSidy { get; set; }
        /// <summary>
        /// 出差住宿
        /// </summary>
        public double Hotel { get; set; }
        /// <summary>
        /// 其他内容
        /// </summary>
        public string Mark { get; set; }
        /// <summary>
        /// 其他金额
        /// </summary>
        public double Other { get; set; }
        /// <summary>
        /// 过路费
        /// </summary>
        public double Toll { get; set; }
        /// <summary>
        /// 出差人  名字
        /// </summary>
        public string Names { get; set; }
        public int SID { get; set; }
        [NotMapped]
        public List<Errand> Errands { get; set; }
        [NotMapped]
        public List<Traffic> TCosts { get; set; }
    }

    [Table("traffic")]
    public class Traffic
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public BusType Type { get; set; }
        public double Cost { get; set; }
        /// <summary>
        /// 公司派车或者私家车使用时  填写
        /// </summary>
        public string Plate { get; set; }
        /// <summary>
        /// 企业滴滴或者出租车填写的时候  填写次数
        /// </summary>
        public int Times { get; set; }
        public int EID { get; set; }
    }

    public enum BusType
    {
        [Description("飞机")]
        Plane,
        [Description("火车")]
        Train,
        [Description("客运大巴")]
        Bus,
        [Description("公司派车（油费）")]
        Company,
        [Description("私家车")]
        Personal,
        [Description("企业滴滴")]
        Didi,
        [Description("公共交通")]
        PublicBus,
        [Description("出租车")]
        Taxi
    }
}
