using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterMind_bc
{
    static class Utils
    {
        public static bool Check(int[] mass, int limit)
        {
            for (int j = 0; j < limit; j++)
                if (mass[j] == mass[limit])
                    return true;
            return false;
        }
    }
}
