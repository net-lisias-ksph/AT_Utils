﻿//   ConfigNodeWrapper.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2016 Allis Tauri

using System;
using System.Collections.Generic;
using KSP.IO;

namespace AT_Utils
{
    //fuck you Squad! How come the ConfigNode is not serializable anymore? O_o
    [Obsolete("SerializableFiledsPartModule now handles ConfigNodes and IConfigNodes seamlessly.")]
    [Serializable]
    public class ConfigNodeWrapper
    {
        public string name;
        public ConfigNode.ValueList values;
        public List<ConfigNodeWrapper> nodes = new List<ConfigNodeWrapper>();

        public ConfigNodeWrapper(ConfigNode node)
        {
            name = node.name;
            values = node.values;
            foreach(ConfigNode n in node.nodes)
                nodes.Add(new ConfigNodeWrapper(n));
        }

        public ConfigNode ToConfigNode()
        {
            var node = new ConfigNode(name);
            foreach(ConfigNode.Value v in values) 
                node.AddValue(v.name, v.value);
            foreach(var n in nodes)
                node.AddNode(n.ToConfigNode());
            return node;
        }

        public static byte[] SaveConfigNode(ConfigNode node)
        { 
            byte[] data = null;
            try { data = IOUtils.SerializeToBinary(new ConfigNodeWrapper(node)); }
            catch(Exception ex) { Utils.Log("{}", ex); }
            return data;
        }

        public static ConfigNode RestoreConfigNode(byte[] data)
        { 
            ConfigNodeWrapper node = null;
            try { node = IOUtils.DeserializeFromBinary(data) as ConfigNodeWrapper; }
            catch(System.Exception ex) { Utils.Log("{}", ex); }
            return node != null? node.ToConfigNode() : null;
        }

        public static implicit operator ConfigNode(ConfigNodeWrapper wrp)
        { return wrp.ToConfigNode(); }

        public override string ToString() { return ToConfigNode().ToString(); }
    }
}

