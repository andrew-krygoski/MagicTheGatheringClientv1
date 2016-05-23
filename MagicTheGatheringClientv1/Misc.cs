using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicTheGatheringClientv1
{

    class Mana
    {
        /*
        0 == Generic
        1 == White
        2 == Blue
        3 == Black
        4 == Red
        5 == Green
        6 == Colorless
        */
        public int color { get; set; }
        public string stipulations { get; set; }
        public Mana(int colors, string stipulation) { this.color = colors; this.stipulations = stipulation; }
    }

    class Counter
    {
        public string type { get; set; }
        public bool temporary { get; set; }
        public Counter(string t, bool tmp) { this.type = t; this.temporary = tmp; }
    }
}
