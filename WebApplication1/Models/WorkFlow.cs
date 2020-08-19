using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    public class WorkFlow
    {
        /// <summary>
        /// 节点
        /// </summary>
        public class Node
        {
            public string id { get; set; }
            public string name { get; set; }
            public string left { get; set; }
            public string top { get; set; }
            public string type { get; set; }
            public string width { get; set; }
            public string height { get; set; }
            public string alt { get; set; }
        }
        /// <summary>
        /// 线
        /// </summary>
        public class Line
        {
            public string id { get; set; }
            public string type { get; set; }
            public string from { get; set; }
            public string to { get; set; }
            public string name { get; set; }
            public string alt { get; set; }
        }
        /// <summary>
        /// 区域
        /// </summary>
        public class Area
        {
            public string name { get; set; }
            public string left { get; set; }
            public string top { get; set; }
            public string color { get; set; }
            public string width { get; set; }
            public string height { get; set; }
            public string alt { get; set; }
        }
        /// <summary>
        /// 根
        /// </summary>
        public class RootObject
        {
            public string title { get; set; }
            public string initNum { get; set; }
        }
    }
}
