using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomDownloader.Models
{
    public class VimmGameListItem
    {
        public string Name { get; set; }

        public string Url { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}