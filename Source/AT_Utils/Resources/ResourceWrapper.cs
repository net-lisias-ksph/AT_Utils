﻿//   ResourceWrapper.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2015 Allis Tauri

using System;
using System.Collections.Generic;
using AT_Utils;

namespace AT_Utils
{
    public abstract class ResourceWrapper<Res> where Res : ResourceWrapper<Res>, new()
    {
        public PartResourceDefinition Resource { get; protected set; }
        public string Name { get { return Resource.name; } }
        public virtual bool Valid { get { return Resource != null; } }

        public abstract void LoadDefinition(string resource_definition);

        protected float load_definition(string resource_definition)
        {
            var name_and_value = resource_definition.Split(new []{' '}, 
                StringSplitOptions.RemoveEmptyEntries);
            var my_name = GetType().Name;
            if(name_and_value.Length != 2) 
            {
                Utils.Log("{}: Invalid format of tank resource definition. " +
                          "Should be 'ResourceName value', got {}", my_name, resource_definition);
                return -1;
            }
            float val;
            if(!float.TryParse(name_and_value[1], out val) || val <= 0)
            {
                Utils.Log("{}: Invalid format of value. " +
                          "Should be positive float value, got: {}", my_name, name_and_value[1]);
                return -1;
            }
            var res_def = PartResourceLibrary.Instance.GetDefinition(name_and_value[0]);
            if(res_def == null) 
            {
                Utils.Log("{}: Resource does not exist: {}", my_name, name_and_value[0]);
                return -1;
            }
            Resource = res_def;
            return val;
        }

        static Col parse_resources<Col>(string resources, Action<Col, Res> add_to_collection) 
            where Col : class, new()
        {
            var res_col = new Col();
            if(string.IsNullOrEmpty(resources)) return res_col;
            //remove comments
            var comment = resources.IndexOf("//");
            if(comment >= 0) resources = resources.Remove(comment);
            if(resources == string.Empty) return res_col;
            //parse resource definitions
            foreach(var res_str in resources.Split(new []{';'}, 
                    StringSplitOptions.RemoveEmptyEntries))
            {
                var res = new Res();
                res.LoadDefinition(res_str.Trim());
                if(!res.Valid) continue;
                try { add_to_collection(res_col, res); }
                catch { Utils.Log("ResourceWrapper.parse_resources: unable to add {} to collection.", res.Name); }
            }
            return res_col;
        }

        static public List<Res> ParseResourcesToList(string resources)
        { return parse_resources<List<Res>>(resources, (c, r) => c.Add(r)); }

        static public SortedList<string, Res> ParseResourcesToSortedList(string resources)
        { return parse_resources<SortedList<string, Res>>(resources, (c, r) => c.Add(r.Name, r)); }
    }
}

