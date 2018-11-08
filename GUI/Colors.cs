﻿//   Colors.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2018 Allis Tauri
using System.Collections.Generic;
using UnityEngine;
using AT_Utils.UI;

namespace AT_Utils
{
    public class Colors : ConfigNodeObject
    {
        [Persistent] public PersistentColor Enabled = PersistentColor.green;
        [Persistent] public PersistentColor Active = PersistentColor.yellow;
        [Persistent] public PersistentColor Inactive = PersistentColor.grey;
        [Persistent] public PersistentColor Confirm = PersistentColor.green;
        [Persistent] public PersistentColor Open = PersistentColor.green;
        [Persistent] public PersistentColor Close = PersistentColor.red;
        [Persistent] public PersistentColor Good = PersistentColor.green;
        [Persistent] public PersistentColor Warning = PersistentColor.yellow;
        [Persistent] public PersistentColor Danger = PersistentColor.red;
        [Persistent] public PersistentColor Selected1 = PersistentColor.cyan;
        [Persistent] public PersistentColor Selected2 = PersistentColor.magenta;

        public SimpleGradient FractionGradient = new SimpleGradient{
            PersistentColor.red,
            PersistentColor.yellow,
            PersistentColor.white,
            PersistentColor.green
        };

        public override void Load(ConfigNode node)
        {
            base.Load(node);
            FractionGradient = new SimpleGradient { Danger, Warning, PersistentColor.white, Good };
        }
    }

    public class PersistentColor : ConfigNodeObject, IColored
    {
        [Persistent] string html = "#FFFFFF";
        Color _color = Color.white;

        public static PersistentColor white => new PersistentColor();
        public static PersistentColor red => new PersistentColor(Color.red);
        public static PersistentColor green => new PersistentColor(Color.green);
        public static PersistentColor blue => new PersistentColor(Color.blue);
        public static PersistentColor black => new PersistentColor(Color.black);
        public static PersistentColor grey => new PersistentColor(Color.grey);
        public static PersistentColor yellow => new PersistentColor(Color.yellow);
        public static PersistentColor magenta => new PersistentColor(Color.magenta);
        public static PersistentColor cyan => new PersistentColor(Color.cyan);
        public static PersistentColor clear => new PersistentColor(Color.clear);

        public PersistentColor() { }

        public PersistentColor(string html)
        {
            this.html = html;
            parse();
        }

        public PersistentColor(Color color)
        {
            this.color = color;
        }

        public string s => html;

        public Color color
        {
            get { return _color; }
            set
            {
                _color = value;
                html = "#" + ColorUtility.ToHtmlStringRGBA(_color);
            }
        }

        public string Tag(string msg) =>
        string.Format("<color={0}>{1}</color>", html, msg);

        void parse()
        {
            if(!ColorUtility.TryParseHtmlString(html, out _color))
            {
                html = "#FFFFFF";
                color = Color.white;
                Utils.Log("Unable to parse color: {}", html);
            }
        }

        public override void Load(ConfigNode node)
        {
            base.Load(node);
            parse();
        }

        public static implicit operator Color(PersistentColor c) => c.color;
    }

    public class SimpleGradient : PersistentList<PersistentColor>
    {
        Gradient gradient;

        public static implicit operator Gradient(SimpleGradient g) => g.gradient;

        public SimpleGradient() { }

        public SimpleGradient(IEnumerable<PersistentColor> content)
            : base(content)
        { update(); }

        void update()
        {
            if(Count > 1)
            {
                gradient = new Gradient();
                gradient.mode = GradientMode.Blend;
                GradientColorKey[] colors = new GradientColorKey[Count];
                for(int i = 0, count = Count; i < count; i++)
                    colors[i] = new GradientColorKey { color = this[i], time = (float)i / count };
                gradient.colorKeys = colors;
                gradient.alphaKeys = new[]{
                    new GradientAlphaKey{alpha=1, time=0},
                    new GradientAlphaKey{alpha=1, time=1}
                };
                Utils.Log("Set up SimpleGradient: {}", this);//debug
            }
            else
                gradient = null;
        }

        public override void Load(ConfigNode node)
        {
            base.Load(node);
            update();
        }

        public Color Evaluate(float frac)
        {
            if(gradient != null)
                return gradient.Evaluate(frac);
            if(Count > 0)
                return this[0];
            return Color.black;
        }
    }
}
