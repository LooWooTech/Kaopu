﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ztop.Todo.Model;

namespace Ztop.Todo.Manager
{
    public class InvoiceManager:ManagerBase
    {
        public int Save(Invoice invoice)
        {
            if (invoice == null)
            {
                throw new Exception("invoice 为 null");
            }
            using (var db = GetDbContext())
            {
                var entry = db.UserGroupViews.FirstOrDefault(e => e.RealName == invoice.Applicant);
                invoice.GroupName = entry == null ? null : entry.Name;
                db.Invoices.Add(invoice);
                db.SaveChanges();
                return invoice.ID;
            }
        }
        public List<Invoice> GetByCID(int cid)
        {
            if (cid == 0)
            {
                return null;
            }
            using (var db = GetDbContext())
            {
                var query = db.Invoices.Where(e => e.CID == cid && e.Deleted == false).AsQueryable();
                return query.Count() > 0 ? query.ToList() : new List<Invoice>();
            }
        }
        public Invoice Get(int id)
        {
            using (var db = GetDbContext())
            {
                return db.Invoices.Find(id);
            }
        }
        public bool Improve(int id,DateTime fillTime,string number,string remark,InvoiceState state)
        {
            using (var db = GetDbContext())
            {
                var invoice = db.Invoices.Find(id);
                if (invoice == null)
                {
                    return false;
                }
                invoice.FillTime = fillTime;
                invoice.Number = number;
                invoice.Remark = remark;
                invoice.State = state;
                db.SaveChanges();
                return true;
            }
        }
        public bool Change(int id,InvoiceState state)
        {
            using (var db = GetDbContext())
            {
                var invoice = db.Invoices.Find(id);
                if (invoice == null)
                {
                    return false;
                }

                invoice.State = state;
                db.SaveChanges();
                return true;
            }
        }

       public List<Invoice> Search()
        {
            using (var db = GetDbContext())
            {
                var list = db.Invoices.ToList();
                foreach(var invoice in list)
                {
                    invoice.Contract = db.Contracts.Find(invoice.CID);
                    invoice.InvoiceBills = db.InvoiceBills.Where(e => e.IID == invoice.ID).ToList();
                }
                return list;
            }
        }
        public List<Invoice> Search(InvoiceParameter parameter)
        {
            using (var db = GetDbContext())
            {
                var query = db.Invoices.AsQueryable();
                foreach(var invoice in query)
                {
                    var abc = db.InvoiceBills.Where(e => e.IID == invoice.ID).ToList();
                    //invoice.Pay
                }
                if (!string.IsNullOrEmpty(parameter.Department))
                {
                    query = query.Where(e => e.GroupName == parameter.Department);
                }
                if (parameter.Status.HasValue)
                {
                    query = query.Where(e => e.State == parameter.Status.Value);
                }
                if (!string.IsNullOrEmpty(parameter.OtherSide))//对方单位
                {
                    query = query.Where(e => e.OtherSideCompany.Contains(parameter.OtherSide));
                }
                if (parameter.ZtopCompany.HasValue)
                {
                    query = query.Where(e => e.ZtopCompany == parameter.ZtopCompany.Value);
                }
                if (parameter.Recevied.HasValue)
                {
                    switch (parameter.Recevied.Value)
                    {
                        case Recevied.None:
                            query = query.Where(e => Math.Abs(e.Pay-0)<0.01);
                            break;
                        case Recevied.Part:
                            query = query.Where(e => e.Money > e.Pay);
                            break;
                        case Recevied.ALL:
                            query = query.Where(e => Math.Abs(e.Money - e.Pay) < 0.01);
                            break;
                    }
                }
                DateTime compareTime = DateTime.Now;
                switch (parameter.Time)
                {
                    case "本周":
                        compareTime = compareTime.AddDays(7);
                        query = query.Where(e => (DateTime.Compare(compareTime, e.Time) > 0));
                        break;
                    case "本月":
                        compareTime = compareTime.AddMonths(1);
                        query = query.Where(e => DateTime.Compare(compareTime, e.Time) > 0);
                        break;
                    case "本年":
                        compareTime = compareTime.AddYears(1);
                        query = query.Where(e => DateTime.Compare(compareTime, e.Time) > 0);
                        break;
                    default:break;
                }
                query = query.OrderByDescending(e => e.Time);
                var list = query.ToList();
                if (parameter.Page.RecordCount == 0)
                {
                    parameter.Page.RecordCount = list.Count;
                }
                list = list.Skip(parameter.Page.PageSize * (parameter.Page.PageIndex - 1)).Take(parameter.Page.PageSize).ToList();
                return list;
            }
        }

        /// <summary>
        /// 删除发票记录  只有当发票处于未开票的情况下才可以删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool Delete(int id)
        {
            if (id == 0)
            {
                return false;
            }

            using (var db = GetDbContext())
            {
                var invoice = db.Invoices.Find(id);
                if (invoice == null)
                {
                    return false;
                }
                if (invoice.State!=InvoiceState.None)
                {
                    return false;
                }
                invoice.Deleted = true;
                db.SaveChanges();
                return true;
            }
        }
        /// <summary>
        /// 编辑发票  只有当发票处于未开票的情况下 才可以编辑
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ztopcompany"></param>
        /// <param name="othersidecompany"></param>
        /// <param name="money"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public bool Edit(int id,ZtopCompany ztopcompany,string othersidecompany,double money,string content)
        {
            if (id == 0)
            {
                return false;
            }
            using (var db = GetDbContext())
            {
                var entry = db.Invoices.Find(id);
                if (entry == null||entry.State!=InvoiceState.None)
                {
                    return false;
                }
                entry.ZtopCompany = ztopcompany;
                entry.OtherSideCompany = othersidecompany;
                entry.Money = money;
                entry.Content = content;
                db.SaveChanges();
                return true;
            }
        }

    }
}
