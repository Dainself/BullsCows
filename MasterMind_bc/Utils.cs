using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using System.Threading.Tasks;

namespace MasterMind_bc
{
    static class Utils
    {
        public async static Task Invoke(this Application app, Action action, DispatcherPriority priority = DispatcherPriority.Normal)
        {
            if (app != null)
            {
                await app.Dispatcher.BeginInvoke(priority, action);
            }
            else
                action();
        }

        public static bool Check(int[] mass, int limit)
        {
            for (int j = 0; j < limit; j++)
                if (mass[j] == mass[limit])
                    return true;
            return false;
        }

        public static bool EqualToOtherNumbers(int[] mass, int i)
        {
            for (int j = 0; j < 4; j++)
            {
                if (mass[j] == mass[i] && j != i)
                    return true;
            }
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

        public static int SumCount(this int[] arr)
        {
            int result = 0;
            foreach (int elem in arr)
                result += elem;
            return result;
        }
    }
}
