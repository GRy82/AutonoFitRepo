using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutonoFit.Classes
{
    public class EasyRun : CardioComponent
    {
        public override int GetRunDuration(int sessionMinutes)
        {
            int halfSessionMinutes = sessionMinutes / 2;
            return Math.Min(30, halfSessionMinutes);
        }

    }
}
