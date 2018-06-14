﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ztop.Todo.Model;

namespace Ztop.Todo.Manager
{
    public class FlowwNodeDataManager:ManagerBase
    {
        public int Save(FlowwNodeData nodeData)
        {
            DB.Floww_Node_Datas.Add(nodeData);
            DB.SaveChanges();
            return nodeData.ID;
        }

        public FlowwNodeData Get(int id)
        {
            return DB.Floww_Node_Datas.Find(id);
        }
    }
}
