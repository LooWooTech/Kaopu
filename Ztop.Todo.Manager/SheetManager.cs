﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Text;
using System.Threading.Tasks;
using Ztop.Todo.ActiveDirectory;
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
            return DB.Sheets.FirstOrDefault(e => e.ID == id);
        }
        public Sheet GetSheet(int id,SheetType type,string name)
        {
            if (id == 0)
            {
                var newSheet = new Sheet
                {
                    Type = type,
                    Number = string.Format("{0}{1}", DateTime.Now.Year.ToString("0000"), DateTime.Now.Month.ToString("00")),
                    Evection = type == SheetType.Errand ? new Evection() : null,
                    Coding = DateTime.Now.Ticks.ToString()
                };
                return newSheet;
            }
            var sheet = GetAllModel(id);
            if (sheet == null)
            {
                throw new ArgumentException("参数错误，未找到相关报销单信息");
            }
            return sheet;
        }

        public int GetNumberExt(string number)
        {
            using (var db = GetDbContext())
            {
                var last = db.Sheets.Where(e => e.Number == number).OrderByDescending(e=>e.ID).FirstOrDefault();
                return last == null ? 1 : last.NumberExt + 1;
            }
        }

        public int GetCheckExt(DateTime time)
        {
            using (var db = GetDbContext())
            {
                var last = db.Sheets.Where(e => e.CheckTime.HasValue && e.CheckExt.HasValue && e.CheckTime.Value.Year == time.Year && e.CheckTime.Value.Month == time.Month).OrderByDescending(e => e.CheckExt.Value).FirstOrDefault();
                return last == null ? 1 : last.CheckExt.Value + 1;
            }
        }

        /// <summary>
        /// 作用：通过ID获取出差详情
        /// 作者：汪建龙
        /// 编写时间：2017年1月16日13:31:37
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Evection GetEvection(int id)
        {
            if (id == 0)
            {
                return null;
            }

            using (var db = GetDbContext())
            {
                var entry = db.Evections.FirstOrDefault(e => e.ID == id);
                if (entry != null)
                {
                    entry.Errands = db.Errands.Where(e => e.EID == entry.ID).ToList();
                    entry.TCosts = db.Traffics.Where(e => e.EID == entry.ID).ToList();
                }
                return entry;
            }
        }

        /// <summary>
        /// 作用：获取报销单的详细信息
        /// 作者：汪建龙
        /// 编写时间：2017年1月13日15:35:33
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Sheet GetFull(int id)
        {
            if (id == 0) return null;
            var model = DB.Sheets.FirstOrDefault(e => e.ID == id);
            if (model != null)
            {
                if (model.Type == SheetType.Errand)
                {
                    model.Evection = DB.Evections.FirstOrDefault(e => e.SID == id);//获取出差报销分项清单
                    if (model.Evection != null)
                    {
                        model.Evection.Errands = DB.Errands.Where(e => e.EID == model.Evection.ID).ToList();//获取出差人数列表
                        model.Evection.TCosts = DB.Traffics.Where(e => e.EID == model.Evection.ID).ToList();//获取用车类型列表
                    }
                }else if (model.Type == SheetType.Reception)
                {
                    DB.Entry(model).Reference(e => e.Reception).Load();
                    DB.Entry(model.Reception).Collection(e => e.Items).Load();
                }
                else
                {
                    model.Substancs_Views = DB.Substancs_View.Where(e => e.SID == id).OrderBy(e => e.ID).ToList();
                }
                model.Verifys = DB.Verifys.Where(e => e.SID == id).OrderBy(e => e.ID).ToList();
                var similars = DB.Sheets.Where(e => e.Type == model.Type && e.Deleted == false && Math.Abs(e.Money - model.Money) < double.Epsilon && e.ID != model.ID).ToList();
                model.SimilarCount = similars.Count;
            }

            return model;
        }

        /// <summary>
        /// 作用：获取类似报销单
        /// 作者：汪建龙
        /// 编写时间：2017年1月13日15:43:46
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<Sheet> GetSimilars(int id)
        {
            using (var db = GetDbContext())
            {
                var model = db.Sheets.FirstOrDefault(e => e.ID == id);
                if (model == null)
                {
                    return null;
                }
                var list= db.Sheets.Where(e => e.Type == model.Type && e.Deleted == false && Math.Abs(e.Money - model.Money) < double.Epsilon && e.ID != model.ID).OrderByDescending(e=>e.Time).Take(10).ToList();
                foreach (var entry in list)
                {
                    if (entry.Type == SheetType.Errand)
                    {
                        entry.Evection = db.Evections.FirstOrDefault(e => e.SID == entry.ID);
                        if (entry.Evection != null)
                        {
                            entry.Evection.Errands = db.Errands.Where(e => e.EID == entry.Evection.ID).ToList();
                            entry.Evection.TCosts = db.Traffics.Where(e => e.EID == entry.Evection.ID).ToList();
                        }
                    }
                    else
                    {
                        entry.Substancs_Views = db.Substancs_View.Where(e => e.SID == entry.ID).OrderBy(e => e.ID).ToList();
                    }
                }
                return list;
            }
        }

        public Sheet GetAllModel(int id)
        {
            if (id == 0) return null;
            var model = DB.Sheets.FirstOrDefault(e => e.ID == id);
            if (model != null)
            {
                if (model.Type == SheetType.Errand)
                {
                    model.Evection = DB.Evections.FirstOrDefault(e => e.SID == id);//获取出差报销分项清单
                    if (model.Evection != null)
                    {
                        model.Evection.Errands = DB.Errands.Where(e => e.EID == model.Evection.ID).ToList();//获取出差人数列表
                        model.Evection.TCosts = DB.Traffics.Where(e => e.EID == model.Evection.ID).ToList();//获取用车类型列表
                    }
                }
                else if (model.Type == SheetType.Reception)
                {
                    if (model.Reception != null)
                    {
                        foreach (var item in model.Reception.Items)
                        {

                        }
                    }
                }
                else
                {
                    model.Substances = DB.Substances.Where(e => e.SID == id).OrderBy(e => e.ID).ToList();
                }

                /* */
                #region   后续删除
                model.Verifys = DB.Verifys.Where(e => e.SID == id).OrderBy(e => e.ID).ToList();
                model.Similars = DB.Sheets.Where(e => e.Type == model.Type && e.Deleted == false && Math.Abs(e.Money - model.Money) < double.Epsilon && e.ID != model.ID).ToList();
                foreach (var entry in model.Similars)
                {
                    if (entry.Type == SheetType.Daily)
                    {
                        //entry.Substancs_Views = db.Substancs_View.Where(e => e.SID == entry.ID).OrderBy(e => e.ID).ToList();
                        entry.Substances = DB.Substances.Where(e => e.SID == entry.ID).OrderBy(e => e.ID).ToList();
                    }
                    else
                    {
                        entry.Evection = DB.Evections.FirstOrDefault(e => e.SID == entry.ID);
                        if (entry.Evection != null)
                        {
                            entry.Evection.Errands = DB.Errands.Where(e => e.EID == entry.Evection.ID).ToList();
                            entry.Evection.TCosts = DB.Traffics.Where(e => e.EID == entry.Evection.ID).ToList();
                        }
                    }
                }
                #endregion
            }
            return model;
        }

        public void SaveSheet(Sheet sheet)
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
            }
        }
        /// <summary>
        /// 用于保存报销单
        /// </summary>
        /// <param name="sheet"></param>
        public void Save(Sheet sheet)
        {
            if (sheet == null) return;
            if (sheet.NumberExt == 0)
            {
                sheet.NumberExt = GetNumberExt(sheet.Number);
            }
            if (string.IsNullOrEmpty(sheet.BarCode))
            {
                sheet.BarCode = BarCode.GetBarCodePath(sheet.PrintNumber);
            }
            if (sheet.ID == 0)
            {
                DB.Sheets.Add(sheet);
            }
            else
            {
                var entry = DB.Sheets.FirstOrDefault(e => e.ID == sheet.ID);
                if (entry != null)
                {
                    //sheet.ReceptionId = entry.ReceptionId;
                    //if (entry.ReceptionId.HasValue)
                    //{
                    //    //sheet.Reception.ID = entry.ReceptionId.Value;
                    //    var reception = DB.Receptions.Find(entry.ReceptionId.Value);
                    //    if (reception == null)
                    //    {
                    //        DB.Receptions.Add(sheet.Reception);
                    //        DB.SaveChanges();
                    //        sheet.ReceptionId = sheet.Reception.ID;
                    //    }
                    //    else
                    //    {
                    //        var currents = sheet.Reception.Items;
                    //        sheet.Reception.ID = entry.ReceptionId.Value;
                    //        DB.Entry(reception).CurrentValues.SetValues(sheet.Reception);
                    //        DB.SaveChanges();
                    //        var items = DB.ReceptionItems.Where(e => e.ReceptionId == sheet.Reception.ID);
                    //        if (items != null)
                    //        {
                    //            DB.ReceptionItems.RemoveRange(items);
                    //            DB.SaveChanges();
                    //        }
                    //        if (currents != null && currents.Count > 0)
                    //        {
                    //            foreach(var item in currents)
                    //            {
                    //                item.ReceptionId = sheet.Reception.ID;
                    //            }
                    //            DB.ReceptionItems.AddRange(currents);
                    //            DB.SaveChanges();
                    //        }
                    //    }
                    //}
                    DB.Entry(entry).CurrentValues.SetValues(sheet);
                    entry.Reception = sheet.Reception;
                }
            }
            DB.SaveChanges();
            #region 日常报销
            if ((sheet.Type == SheetType.Daily || sheet.Type == SheetType.Transfer) && sheet.Substances != null)//第一次填写 日常报销的时候，substances不为空  当用户在草稿中点击提交的时候，分类项目清单已经存入
            {
                var older = DB.Substances.Where(e => e.SID == sheet.ID).ToList();//如果重新编辑了报销单，则删除之前所有的子清单
                if (older != null)
                {
                    DB.Substances.RemoveRange(older);
                    DB.SaveChanges();
                }

                DB.Substances.AddRange(sheet.Substances.OrderBy(e => e.ID).Select(e => new Substancs
                {
                    RID = e.RID,
                    SRID = e.SRID,
                    Details = e.Details,
                    Price = e.Price,
                    SID = sheet.ID,
                    EnterprisePay = e.EnterprisePay
                }));
            }
            #endregion
            if (sheet.Type == SheetType.Errand && sheet.Evection != null)
            {
                #region  更新出差报销中的分项清单

                sheet.Evection.SID = sheet.ID;
                var entry = DB.Evections.FirstOrDefault(e => e.SID == sheet.ID);
                if (entry == null)
                {
                    DB.Evections.Add(sheet.Evection);
                }
                else
                {
                    sheet.Evection.ID = entry.ID;
                    DB.Entry(entry).CurrentValues.SetValues(sheet.Evection);
                }
                DB.SaveChanges();
                #endregion

                #region  更新出差人数的列表
                if (sheet.Evection.Errands != null)
                {
                    var older = DB.Errands.Where(e => e.EID == sheet.Evection.ID).ToList();
                    if (older != null)
                    {
                        DB.Errands.RemoveRange(older);
                        DB.SaveChanges();
                    }
                    DB.Errands.AddRange(sheet.Evection.Errands.Select(e => new Errand
                    {
                        StartTime = e.StartTime,
                        EndTime = e.EndTime,
                        Peoples = e.Peoples,
                        Days = e.Days,
                        Users = e.Users,
                        EID = sheet.Evection.ID
                    }));
                    DB.SaveChanges();

                }

                #endregion

                #region  更新交通费用列表

                if (sheet.Evection.TCosts != null)
                {
                    var OLD = DB.Traffics.Where(e => e.EID == sheet.Evection.ID).ToList();
                    if (OLD != null && OLD.Count != 0)
                    {
                        DB.Traffics.RemoveRange(OLD);
                        DB.SaveChanges();
                    }
                    sheet.Evection.TCosts = sheet.Evection.TCosts.Select(e => new Traffic
                    {
                        Type = e.Type,
                        Cost = e.Cost,
                        Toll = e.Toll,
                        Petrol = e.Petrol,
                        Driver = e.Driver,
                        CarPetty = e.CarPetty,
                        Plate = e.Plate,
                        Times = e.Times,
                        KiloMeters = e.KiloMeters,
                        EID = sheet.Evection.ID
                    }).ToList();
                    DB.Traffics.AddRange(sheet.Evection.TCosts);
                    DB.SaveChanges();
                }

                #endregion
            }
            DB.SaveChanges();
        }

        private List<Sheet> GetSheets()
        {
            using (var db = GetDbContext())
            {
                
                return db.Sheets.ToList();
            }
        }

        /// <summary>
        /// 作用：通过ID组 获取所有关联的Sheets报销单列表
        /// 作者：汪建龙
        /// 编写时间：2016年11月21日20:20:31
        /// 
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public List<Sheet> GetSheets(List<int> ids)
        {
            var list = new List<Sheet>();
            foreach(var id in ids)
            {
                list.Add(GetFull(id));
                //list.Add(GetAllModel(id));
            }
            return list;
        }

        /// <summary>
        /// 作用：单独获取某年某月的报销单
        /// 作者：汪建龙
        /// 备注时间：2016年11月21日20:03:42
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        public List<Sheet> GetSheets(int year,int? month=null,string name=null)
        {
            var list = Collect(year, month);
            if (!string.IsNullOrEmpty(name))
            {
                list = list.Where(e => e.Name.ToLower() == name.ToLower()).ToList();
            }
            var query = list.Select(e=>e.ID).ToList();
            return GetSheets(query);
        }

        /// <summary>
        /// 作用：单独获取某一个人的所有成功报销的报销单
        /// 作者：汪建龙
        /// 编写时间：2017年1月17日09:58:20
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public List<Sheet> GetDone(string name)
        {
            using (var db = GetDbContext())
            {
                var list = db.Sheets.Where(e => e.Name.ToLower()==name.ToLower()&&e.CheckTime.HasValue && e.Deleted == false&&e.Status==Status.Examined).ToList();
                if (list == null)
                {
                    return null;
                }
                foreach(var item in list)
                {
                    if (item.Type == SheetType.Errand)
                    {
                        item.Evection = db.Evections.FirstOrDefault(e => e.SID == item.ID);//获取出差报销分项清单
                        if (item.Evection != null)
                        {
                            item.Evection.Errands = db.Errands.Where(e => e.EID == item.Evection.ID).ToList();//获取出差人数列表
                            item.Evection.TCosts = db.Traffics.Where(e => e.EID == item.Evection.ID).ToList();//获取用车类型列表
                        }
                    }else if (item.Type == SheetType.Reception)
                    {
                        db.Entry(item).Reference(e => e.Reception).Load();
                        db.Entry(item.Reception).Collection(e => e.Items).Load();
                    }
                    else
                    {
                        item.Substancs_Views = db.Substancs_View.Where(e => e.SID == item.ID).OrderBy(e => e.ID).ToList();
                    }
                }

                return list;

            }
        }
        public List<Sheet> GetSheets(SheetQueryParameter2 parameter)
        {
            var query = DB.SheetViews.Where(e => e.Deleted == false).AsQueryable();
            if (parameter.GroupId.HasValue)
            {
                if (!string.IsNullOrEmpty(parameter.Name))
                {
                    query = query.Where(e => e.Name.ToUpper() == parameter.Name.ToUpper());
                }
                else
                {
                    query = query.Where(e => e.GroupID == parameter.GroupId.Value);
                }
            }
            if (parameter.StartTime.HasValue)
            {
                query = query.Where(e => e.CheckTime >= parameter.StartTime.Value);
            }
            if (parameter.EndTime.HasValue)
            {
                query = query.Where(e => e.CheckTime <= parameter.EndTime.Value);
            }
            if (parameter.Type.HasValue)
            {
                query = query.Where(e => e.Type == parameter.Type.Value);
                switch (parameter.Type.Value)
                {
                    case SheetType.Daily:
                    case SheetType.Transfer:
                        if (!string.IsNullOrEmpty(parameter.Category))
                        {
                            query = query.Where(e => e.SubName.ToLower() == parameter.Category.ToLower());
                        }
                        break;
                    default:
                        break;
                }
            }
            if (!string.IsNullOrEmpty(parameter.Memo))
            {
                query = query.Where(e => e.Mark.Contains(parameter.Memo) || e.Details.Contains(parameter.Memo));
            }

            var result = query.ToList().Select(e => e.ID).Distinct().ToList();
            var list = GetSheets(result);
            return list;
        }

        /// <summary>
        /// 统计某年某月的报销金额
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        public double Sum(int year,int month)
        {
            var query = Collect(year, month);
            if (query.Count() == 0)
            {
                return .0;
            }
            return query.Sum(e => e.Money);
        }


        public List<Sheet> Collect(int year,int? month=null)
        {
            using (var db = GetDbContext())
            {
                var query = db.Sheets.Where(e => e.CheckTime.HasValue && e.CheckTime.Value.Year == year).AsQueryable();
                if (month.HasValue)
                {
                    query = query.Where(e => e.CheckTime.Value.Month == month.Value);
                }
                return query.ToList();
            }
        }
        /// <summary>
        /// 作用：获取已经完成报销以及通过财务审核的报销单列表
        /// 作者：汪建龙
        /// 备注时间：2016年11月19日10:07:40
        /// </summary>
        /// <returns></returns>
        public List<Sheet> Collect()
        {
            using (var db = GetDbContext())
            {
                return db.Sheets.Where(e => e.Status == Status.Examined || e.Status == Status.Filing).Where(e => e.CheckTime.HasValue).ToList();
            }
        }

        public List<Sheet> GetSheets(SheetQueryParameter parameter)
        {
            var list = GetSheets();
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

        public List<Sheet> GetSheetsByKey(string key)
        {
            if (string.IsNullOrEmpty(key))
                return null;
            var list= GetSheets(new SheetQueryParameter() { Status = Status.Filing }).Where(e => e.PrintNumber.Contains(key)).ToList();
            //以下操作主要对应报销归档时候，返回日常招待时候，忽略外键实体
            foreach (var item in list)
            {
                if (item.Type == SheetType.Reception)
                {
                    item.ReceptionId = null;
                }
            }
            return list;
           // return GetSheets(new SheetQueryParameter() { Status = Status.Filing }).Where(e => e.SerialNumber.Coding.Contains(key)).ToList();
        }
        public List<Sheet> GetSheets(QueryParameter parameter,string name)
        {
            var list = GetSheets();
            var query = list.AsQueryable();
            query = query.Where(e => e.Deleted == false);
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


            if (parameter.MinMoney.HasValue)
            {
                query = query.Where(e => e.Money >= parameter.MinMoney.Value);
            }

            if (parameter.MaxMoney.HasValue)
            {
                query = query.Where(e => e.Money <= parameter.MaxMoney.Value);
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
                query = query.OrderByDescending(e=>e.Time);
            }
            else
            {
                query = query.OrderByDescending(e=>e.Money);
            }
            if (parameter.Page != null)
            {
                query = query.SetPage(parameter.Page);
            }
            return query.ToList();
        }
        public List<Sheet> GetSheets(string name)
        {
            var list = Core.VerifyManager.GetSheetID(name);
            return list.Select(e => GetAllModel(e)).Where(e=>e.Deleted==false).ToList();
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
                    var val = sheet.Evection.Traffic + sheet.Evection.SubSidy + sheet.Evection.Hotel + sheet.Evection.Other;
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
        
        /// <summary>
        /// 获取完成报销单
        /// </summary>
        /// <returns></returns>
        public List<Sheet> GetCompletes()
        {
            using (var db = GetDbContext())
            {
                return db.Sheets.Where(e => e.Deleted == false && (e.Status == Status.Examined || e.Status == Status.Filing)).ToList();
            }
        }

        public double Pay(int[] sid)
        {
            var sum = .0;
            
            var finance = XmlHelper.GetFinance();
            using (var db = GetDbContext())
            {
                var vlist = new List<Verify>();
                foreach (var id in sid)
                {
                    var entry = db.Sheets.FirstOrDefault(e => e.ID == id);
                    if (entry == null)
                    {
                        continue;
                    }
                    if (entry.Status != Status.Cash)
                    {
                        continue;
                    }
                    entry.Status = Status.Examined;
                    entry.Controler = entry.Name;
                    sum += entry.Money;
                    db.SaveChanges();
                    vlist.Add(new Verify
                    {
                        Name = finance,
                        SID = entry.ID,
                        Time = DateTime.Now,
                        Position = Position.Check,
                        Step = Step.Cash
                    });
                }
                db.Verifys.AddRange(vlist);
                db.SaveChanges();
            }
            return sum;
        }

        /// <summary>
        /// 作用：对转账支出单进行到账确认时间
        /// 作者：汪建龙
        /// 编写时间：2017年4月27日10:35:21
        /// </summary>
        /// <param name="id"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public bool CheckTranfer(int id,DateTime time,Cost cost)
        {
            using (var db = GetDbContext())
            {
                var model = db.Sheets.Find(id);
                if (model == null)
                {
                    return false;
                }
                if (model.Type != SheetType.Transfer)
                {
                    return false;
                }
                model.CheckTime = time;
                model.Status = Status.Examined;
                model.Cost = cost;
                db.Verifys.Add(new Verify()
                {
                    Step = Step.Affirm,
                    Time = DateTime.Now,
                    Name = model.Name,
                    Position = Position.Check,
                    SID = model.ID
                });
                db.SaveChanges();
                return true;
            }
        }
        public bool UnCheck(int id,string reason)
        {
            using (var db = GetDbContext())
            {
                var model = db.Sheets.Find(id);
                if (model == null)
                {
                    return false;
                }
                if (model.Type != SheetType.Transfer)
                {
                    return false;
                }
                model.CheckTime = DateTime.Now;
                model.Status = Status.Repeal;
                model.Remarks += string.Format("已作废，作废原因：{0}", reason);
                db.Verifys.Add(new Verify()
                {
                    Step=Step.Repeal,
                    Time=DateTime.Now,
                    Name=model.Name,
                    Position=Position.Check,
                    SID=model.ID,
                    Reason=reason
                });
                db.SaveChanges();
                return true;
            }
               
        }

    }
}
