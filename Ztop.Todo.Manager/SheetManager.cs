﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ztop.Todo.Common;
using Ztop.Todo.Model;

namespace Ztop.Todo.Manager
{
    public class SheetManager : ManagerBase
    {
        private static string[] colors = { "#F7464A", "#46BFBD", "#FDB45C", "#949FB1", "#4D5360" };
        private static string[] highlights = { "#FF5A5E", "#5AD3D1", "#FFC870", "#A8B3C5", "#616774" };
        public Sheet GetModel(int id)
        {
            if (id == 0) return null;
            using (var db = GetDbContext())
            {
                return db.Sheets.FirstOrDefault(e => e.ID == id);
            }
        }
        public Sheet GetSerialNumberModel(int id,SheetType type)
        {
            if (id == 0)
            {
                return new Sheet
                {
                    Type = type,
                    Evection = type == SheetType.Errand ? new Evection() : null,
                    SerialNumber = Core.SerialNumberManager.GetNewModel()
                };
            }
            var sheet = GetAllModel(id);
            if (sheet == null)
            {
                throw new ArgumentException("参数错误，未找到相关报销单信息");
            }
            return sheet;

        }
        public Sheet GetAllModel(int id)
        {
            if (id == 0) return null;
            using (var db = GetDbContext())
            {
                var model = db.Sheets.FirstOrDefault(e => e.ID == id);
                if (model != null)
                {
                    model.SerialNumber = db.SerialNumbers.FirstOrDefault(e => e.SID == model.ID);
                    if (model.Type == SheetType.Daily)
                    {
                        model.Substances = db.Substances.Where(e => e.SID == id).OrderBy(e => e.ID).ToList();
                    }
                    else
                    {
                        model.Evection = db.Evections.FirstOrDefault(e => e.SID == id);//获取出差报销分项清单
                        model.Evection.Errands = db.Errands.Where(e => e.EID == model.Evection.ID).ToList();//获取出差人数列表
                    }
                    
                    model.Verifys = db.Verifys.Where(e => e.SID == id).OrderBy(e => e.ID).ToList();
                }
                return model;
            }
        }
        /// <summary>
        /// 用于保存报销单
        /// </summary>
        /// <param name="sheet"></param>
        public void Save(Sheet sheet)
        {
            if (sheet == null) return;
            using (var db = GetDbContext())
            {
                if (sheet.ID == 0)
                {
                    db.Sheets.Add(sheet);
                }
                else
                {
                    var entry = db.Sheets.FirstOrDefault(e => e.ID == sheet.ID);
                    if (entry != null)
                    {
                        db.Entry(entry).CurrentValues.SetValues(sheet);
                    }
                }
                db.SaveChanges();
                if (sheet.Type==SheetType.Daily&&sheet.Substances != null)//第一次填写 日常报销的时候，substances不为空  当用户在草稿中点击提交的时候，分类项目清单已经存入
                {
                    var older = db.Substances.Where(e => e.SID == sheet.ID).ToList();//如果重新编辑了报销单，则删除之前所有的子清单
                    if (older != null)
                    {
                        db.Substances.RemoveRange(older);
                        db.SaveChanges();
                    }
                    
                    db.Substances.AddRange(sheet.Substances.OrderBy(e => e.ID).Select(e => new Substancs
                    {
                        Category = e.Category,
                        Details = e.Details,
                        Price = e.Price,
                        SID = sheet.ID
                    }));
                }
                if (sheet.Type == SheetType.Errand && sheet.Evection != null)
                {
                    #region  更新出差报销中的分项清单

                    sheet.Evection.SID = sheet.ID;
                    var entry = db.Evections.FirstOrDefault(e => e.SID == sheet.ID);
                    if (entry == null)
                    {
                        db.Evections.Add(sheet.Evection);
                    }
                    else
                    {
                        sheet.Evection.ID = entry.ID;
                        db.Entry(entry).CurrentValues.SetValues(sheet.Evection);
                    }
                    #endregion

                    #region  更新出差人数的列表
                    var older = db.Errands.Where(e => e.EID == sheet.Evection.ID).ToList();
                    if (older != null)
                    {
                        db.Errands.RemoveRange(older);
                        db.SaveChanges();
                    }
                    db.Errands.AddRange(sheet.Evection.Errands.Select(e => new Errand
                    {
                        StartTime=e.StartTime,
                        EndTime=e.EndTime,
                        Peoples = e.Peoples,
                        Days = e.Days,
                        EID = sheet.Evection.ID
                    }));
                    #endregion
                }

                db.SaveChanges();

            }
        }
        private SerialNumber GetSerialNumber(int sid)
        {
            if (sid == 0) return null;
            using (var db = GetDbContext())
            {
                return db.SerialNumbers.FirstOrDefault(e => e.SID == sid);
            }
        }
        private List<Sheet> GetSheets()
        {
            using (var db = GetDbContext())
            {
                return db.Sheets.ToList();
            }
        }
        private List<Sheet> GetSerialNumberSheets()
        {
            var list = GetSheets();
            foreach(var sheet in list)
            {
                sheet.SerialNumber = GetSerialNumber(sheet.ID);
            }
            return list;
        }
        public List<Sheet> GetSheets(SheetQueryParameter parameter)
        {
            var list = GetSerialNumberSheets();
            var query = list.AsQueryable();
            if (!string.IsNullOrEmpty(parameter.Name))
            {
                query = query.Where(e => e.Name == parameter.Name);
            }
            if (!string.IsNullOrEmpty(parameter.Controler))
            {
                query = query.Where(e => e.Controler == parameter.Controler);
            }
            if (parameter.Deleted.HasValue)
            {
                query = query.Where(e => e.Deleted == parameter.Deleted.Value);
            }
            if (parameter.Status.HasValue)
            {
                query = query.Where(e => e.Status == parameter.Status.Value);
            }
            return query.ToList();
        }
        public List<Sheet> GetSheets(QueryParameter parameter,string name)
        {
            var list = GetSerialNumberSheets();
            var query = list.AsQueryable();
            if (parameter.Type.HasValue)
            {
                query = query.Where(e => e.Type == parameter.Type.Value);
            }
            if (parameter.Creater == Operator.我)
            {
                query = query.Where(e => e.Name == name);
            }else if (parameter.Creater == Operator.自定义)
            {
                query = query.Where(e => e.Name == parameter.Custom.Trim().ToUpper());
            }

            switch (parameter.Status)
            {
                case StatusPosition.草稿:
                    query = query.Where(e => e.Status == Status.OutLine);
                    break;
                case StatusPosition.未审核:
                    query = query.Where(e => e.Status == Status.ExaminingDirector || e.Status == Status.ExaminingManager || e.Status == Status.ExaminingFinance);
                    break;
                case StatusPosition.已审核:
                    query = query.Where(e => e.Status == Status.Examined);
                    break;
            }
            var days = 0;
            switch (parameter.Time)
            {
                case Time.一周内:
                    days = 7;
                    break;
                case Time.一年内:
                    days = 365;
                    break;
                case Time.一月内:
                    days = 31;
                    break;
                case Time.半年内:
                    days = 183;
                    break;
            }
            if (days != 0)
            {
                var time = DateTime.Now.AddDays(days);
                query = query.Where(e => e.Time <= time);
            }
            if (parameter.Order == Order.Time)
            {
                query = query.OrderBy(e => e.Time);
            }
            else
            {
                query = query.OrderBy(e => e.Money);
            }
            if (parameter.Page != null)
            {
                query = query.SetPage(parameter.Page);
            }
            return query.ToList();
        }
        public void Delete(int id)
        {
            using (var db = GetDbContext())
            {
                var entry = db.Sheets.FirstOrDefault(e => e.ID == id);
                if (entry != null)
                {
                    entry.Deleted = true;
                    db.SaveChanges();
                }
            }
        }
        private List<Sheet> GetSheets(QueryParameter parameter,string name, DateTime startTime,DateTime endTime)
        {
            var temp = GetSheets(parameter, name);
            temp = temp.Where(e => e.Time >= startTime && e.Time <= endTime).ToList();
            var list = new List<Sheet>();
            foreach (var item in temp)
            {
                list.Add(GetAllModel(item.ID));
            }
            return list;
        }
        /// <summary>
        /// 某个人在某段时间内各个业务的统计结果
        /// </summary>
        /// <param name="name">用户名</param>
        /// <param name="startTime">起始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns></returns>
        public List<PieData> Statistic(string name,DateTime startTime,DateTime endTime)
        {
            var dict = new Dictionary<string, double>();
            var list = GetSheets(new QueryParameter()
            {
                Creater = Operator.我,
                Status = StatusPosition.已审核,
                Order = Order.Time
            }, name, startTime, endTime);
            foreach(var sheet in list)
            {
                if (sheet.Type == SheetType.Daily)
                {
                    foreach (var substance in sheet.Substances)
                    {
                        var category = substance.Category.GetDescription();
                        if (dict.ContainsKey(category))
                        {
                            dict[category] += substance.Price;
                        }
                        else
                        {
                            dict.Add(category, substance.Price);
                        }
                    }
                }
                else
                {
                    var key = "出差报销";
                    var val = sheet.Evection.Traffic + sheet.Evection.SubSidy + sheet.Evection.Hotel + sheet.Evection.Other + sheet.Evection.Toll;
                    if (dict.ContainsKey(key))
                    {
                        dict[key] += val;
                    }
                    else
                    {
                        dict.Add(key, val);
                    }
                } 
            }
            var random = new Random(unchecked((int)DateTime.Now.Ticks));
            int index = 0;
            var results = dict.Select(e => new PieData()
            {
                value = e.Value,
                color = colors[index=random.Next(0,5)],
                highlight = highlights[index],
                label = e.Key
            }).ToList();
            return results;
        }
        /// <summary>
        /// 某个人某种业务某段时间的报销金额
        /// </summary>
        /// <param name="name"></param>
        /// <param name="category"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public Dictionary<string,double> Statistic(string name,Category category,DateTime startTime,DateTime endTime)
        {
            var dict = new Dictionary<string, double>();
            var list = GetSheets(new QueryParameter()
            {
                Creater = Operator.我,
                Status = StatusPosition.已审核,
                Order = Order.Time,
                Type = SheetType.Daily
            }, name, startTime, endTime);
            foreach(var sheet in list)
            {
                var time = sheet.Time.ToLongDateString();
                var val = sheet.Substances.Where(e => e.Category == category).Sum(e => e.Price);
                if (dict.ContainsKey(time))
                {
                    dict[time] += val;
                }
                else
                {
                    dict.Add(time, val);
                }
            }
            return dict;
        }
    }
}
