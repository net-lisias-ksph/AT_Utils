﻿//   SimplePartCategorizer.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2017 Allis Tauri

using System;
using System.Linq;
using UnityEngine;
using RUI.Icons.Selectable;
using KSP.UI.Screens;
using KSP.Localization;
using System.Collections.Generic;
using System.IO;

namespace AT_Utils
{
    /// <summary>
    /// This is the base class for easy creaton of custom part filters.
    /// Its main purpouse is to allow creation of dynamic filters responsive 
    /// to in-game part modifications.
    /// 
    /// The code is adapted from the RealChute mod by Christophe Savard (stupid_chris):
    /// https://github.com/StupidChris/RealChute/blob/master/RealChute/RCFilterManager.cs
    /// Many thanks to Chris for figuring this out so fast!
    /// </summary>
    public abstract class SimplePartFilter : MonoBehaviour
    {
        List<string> modules;
        protected List<string> MODULES { get { return modules; } }
        protected string CATEGORY = Localizer.Format("#autoLOC_453547"); // Filter by Function
        protected string SUBCATEGORY = "";
        protected string FOLDER = "";
        protected string ICON = "";

        void Awake()
        {
            GameEvents.onGUIEditorToolbarReady.Add(add_filter);
        }

        protected void SetMODULES(IEnumerable<Type> types)
        {
            modules = types.Select(t => KSPUtil.PrintModuleName(t.Name)).ToList();
        }

        protected virtual bool filter(AvailablePart part)
        {
            return part.category != PartCategories.none &&
                part.moduleInfos.Any(info => MODULES.Any(m => m == info.moduleName));
        }

        void set_modules_icon(Icon icon)
        {
            if(modules != null && modules.Count > 0)
            {
                PartCategorizer.Instance.filters
                    .Find(f => f.button.categoryName == Localizer.Format("#autoLOC_453705")) // Filter by Module
                    .subcategories.FindAll(s => modules.Any(m => m == s.button.categoryName))
                    .ForEach(c => c.button.SetIcon(icon));
            }
        }

        static Icon load_icon(string icon_name, string folder)
        {
            if(PartCategorizer.Instance.iconLoader.iconDictionary.ContainsKey(icon_name))
                return PartCategorizer.Instance.iconLoader.GetIcon(icon_name);
            var icon_path = Path.Combine(folder, icon_name);
            var icon = TextureCache.GetTexture(icon_path);
            var icon_s = TextureCache.GetTexture(icon_path + "_selected") ?? icon;
            var selectable_icon = new Icon(icon_name, icon, icon_s, icon == icon_s);
            PartCategorizer.Instance.iconLoader.icons.Add(selectable_icon);
            PartCategorizer.Instance.iconLoader.iconDictionary.Add(icon_name, selectable_icon);
            return selectable_icon;
        }

        protected static bool check_module<T>(T m) where T : PartModule
        => m != null && m.isEnabled;

        protected virtual void add_filter()
        {
            if(string.IsNullOrEmpty(ICON) ||
               string.IsNullOrEmpty(CATEGORY) ||
               string.IsNullOrEmpty(SUBCATEGORY))
                return;
            //load the icon
            var icon = load_icon(ICON, FOLDER);
            //get category
            var category = PartCategorizer.Instance.filters
                .Find(f => f.button.categoryName == CATEGORY);
            //add custom function filter
            PartCategorizer.AddCustomSubcategoryFilter(category, SUBCATEGORY, SUBCATEGORY, icon, filter);
            //Apparently needed to make sure the icon actually shows at first
            var button = category.button.activeButton;
            button.Value = false;
            button.Value = true;
            //set icon(s) for all the modules
            set_modules_icon(icon);
        }
    }
}
