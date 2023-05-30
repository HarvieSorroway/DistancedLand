using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.LowLevel;

namespace DistancedLand
{
    static class Enums
    {
        public static CreatureTemplate.Type NavyLizard;

        public static void Register()
        {
            NavyLizard = new CreatureTemplate.Type("NavyLizard", true);
        }

        public static void Unregister()
        {
            if(NavyLizard != null)
                NavyLizard.Unregister();
            NavyLizard = null;
        }
    }
}
