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
    /// 合同
    /// </summary>
    [Table("contracts")]
    public class Contract
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        /// <summary>
        /// 合同流水号
        /// </summary>
        [MaxLength(255)]
        public string Coding { get; set; }
        /// <summary>
        /// 合同名称
        /// </summary>
        [MaxLength(1023)]
        public string Name { get; set; }
        /// <summary>
        /// 对方单位
        /// </summary>
        [MaxLength(1023)]
        public string Company { get; set; }
        /// <summary>
        /// 创建人员
        /// </summary>
        [MaxLength(255)]
        public string Creator { get; set; }
        /// <summary>
        /// 是否归档
        /// </summary>
        public bool Archived { get; set; }
        /// <summary>
        /// 是否删除
        /// </summary>
        public bool Deleted { get; set; }
        [Column(TypeName ="int")]
        public ZtopCompany ZtopCompany { get; set; }
        /// <summary>
        /// 合同起始时间->合同流转时间
        /// </summary>
        public DateTime StartTime { get; set; }
        /// <summary>
        /// 合同结束时间->合同取回时间
        /// </summary>
        public DateTime? EndTime { get; set; }
        /// <summary>
        /// 合同金额
        /// </summary>
        public double Money { get; set; }
        /// <summary>
        /// 履约保证金
        /// </summary>
        public double PerformanceBond { get; set; }
        /// <summary>
        /// 所在部门
        /// </summary>
        public string Department { get; set; }
        /// <summary>
        /// 付款方式
        /// </summary>
        [MaxLength(1023)]
        public string PayWay { get; set; }
        /// <summary>
        /// 发票开具情况
        /// </summary>
        public ContractState Status { get; set; }
        /// <summary>
        /// 到账情况
        /// </summary>
        public Recevied Recevied { get; set; }
        public double Leave { get; set; }
        /// <summary>
        /// 合同相关文件
        /// </summary>
        [NotMapped]
        public List<ContractFile> ContractFiles { get; set; }

        /// <summary>
        /// deleted为falase的发票列表
        /// </summary>
        [NotMapped]
        public List<Invoice> Invoices { get; set; }
        /// <summary>
        /// 项目
        /// </summary>
        [NotMapped]
        public List<Article> Articles { get; set; }
        /// <summary>
        /// 项目洽谈  （通过登记编号与项目洽谈登记编号关联）
        /// </summary>
        [NotMapped]
        public Article Article { get; set; }
        [NotMapped]
        public List<BillContract> BillContracts { get; set; }

        /// <summary>
        /// 登记编号
        /// </summary>
        public string Number { get; set; }
        /// <summary>
        /// 合同路径
        /// </summary>
        public string FilePath { get; set; }

        [NotMapped]
        public string ContractName
        {
            get
            {
                try
                {
                    return System.IO.Path.GetFileNameWithoutExtension(FilePath);
                }
                catch
                {
                    return null;
                }
            }
        }
        /// <summary>
        /// 当前合同可开发票金额
        /// </summary>
        [NotMapped]
        public double Remain
        {
            get
            {
                try
                {
                    if (Invoices != null)
                    {
                        var a= Money - Invoices.Where(e => e.State == InvoiceState.Have || e.State == InvoiceState.None).Sum(e => e.Money);
                        return a > 0 ? a : .0;
                    }
                }
                catch
                {

                }

                return .0;
       
            }
        }
        /// <summary>
        /// 录入时间
        /// </summary>
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
    }

    public enum ZtopCompany
    {
        [Description("智拓规划公司")]
        Evaluation,
        [Description("智拓评估公司")]
        Projection,
        [Description("浙江大学")]
        ZhejiangUniversity,
        [Description("其他")]
        Other
    }
}
