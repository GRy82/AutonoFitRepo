using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutonoFit.Classes
{
    public class ModerateRun : CardioComponent
    {
        private new readonly double paceCoefficient = 1.43;

        public override int GetRunDuration(int sessionMinutes)
        {
            return Math.Min(sessionMinutes, 45);
        }
    }
}
