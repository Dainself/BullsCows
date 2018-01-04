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

        public static int[] StringToArr(string str)
        {
            int len = str.Length;
            int[] arr = new int[len];
            for (int i = 0; i < len; i++)
            {
                arr[i] = (int)Char.GetNumericValue(str[i]);
            }
            return arr;
        }

        public static string ArrToString(int[] arr)
        {
            string str = "";
            foreach (int elem in arr)
            {
                str += elem.ToString();
            }
            return str;
        }
    }
}
