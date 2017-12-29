using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterMind_bc
{
    partial class MainWindow
    {
        SortedSet<int> unnecessary_digitals, cows_list, COWS_THREAD, ready_indexes;
        SortedSet<int>[] used_digitals;
        int bulls, prev_bulls, cows, prev_cows, prev_value;
        int[] mass;
        bool isSelectedDuetoAdvance, isWin, isCOWS_THREAD;
        public void Init()
        {
            unnecessary_digitals = new SortedSet<int>();
            cows_list = new SortedSet<int>();
            COWS_THREAD = new SortedSet<int>();
            ready_indexes = new SortedSet<int>();
            used_digitals = new SortedSet<int>[4];
            for (int i = 0; i < 4; i++)
            {
                used_digitals[i] = new SortedSet<int>();
            }
            mass = new int[4];
        }

        public async void Start()
        {
            Random rand = new Random();
            int k = 0; // для индексации mass
            bool first_time_flag = true, do_continue;
            for (int i = 0; i < 4; i++) // заполняю случ значениями
            {
                do
                {
                    mass[i] = 1 + rand.Next() % 9;
                }
                while (Check(i));
            }
            for (int i = 1; i < 4; i++)
            {
                used_digitals[i].Add(mass[i]);
            }

            while (!isWin) // вот здесь начинается цикл
            {
                ShowArr();
                await Read();
                do_continue = DoPreAnalyse(ref k);
                if (first_time_flag)
                {
                    first_time_flag = false;
                    goto for_the_first_time;
                }
                if (do_continue) DoAnalyse(ref k);
                if (isWin) return;
                for_the_first_time:
                {
                    used_digitals[k].Add(mass[k]);
                    prev_value = mass[k];
                    ChangeValue(k);
                }
            }
        }

        bool DoPreAnalyse(ref int index)
        {
            if (bulls == 4)
            {
                isWin = true;
                return false;
            }
            else if (bulls + cows == 4)
            {
                if (prev_bulls < bulls)
                {
                    ready_indexes.Add(index);
                }
                DoTransposition();
                return false;
            }
            else if (bulls + cows == 0)
            {
                for (int i = 0; i < 4; i++)
                {
                    used_digitals[i].Add(mass[i]);
                }
                return false;
            }
            return true;
        }

        void DoAnalyse(ref int index)
        {
            Console.WriteLine("Analyze...");
            if (prev_cows == cows)
            {
                if (prev_bulls == bulls)
                {
                    if (!isSelectedDuetoAdvance)
                    {
                        //COWS_THREAD START
                        isCOWS_THREAD = true;
                        COWS_THREAD.Add(prev_value);
                        COWS_THREAD.Add(mass[index]);
                    }
                    else
                    {
                        cows_list.Add(prev_value);
                        isSelectedDuetoAdvance = false;
                        index++;
                    }
                }
                else if (prev_bulls < bulls)
                {
                    if (isCOWS_THREAD)
                    {
                        foreach (int elem in COWS_THREAD)
                        {
                            unnecessary_digitals.Add(elem);
                        }
                        COWS_THREAD.Clear();
                        isCOWS_THREAD = false;
                    }
                    else unnecessary_digitals.Add(prev_value);
                    ready_indexes.Add(index); //закрепить индекс
                    index++;
                }
                else if (prev_bulls > bulls)
                {
                    unnecessary_digitals.Add(mass[index]);
                    mass[index] = prev_value;
                    bulls++;
                    ready_indexes.Add(index); //закрепить индекс
                    index++;
                }
            }
            else if (prev_cows > cows)
            {
                if (prev_bulls == bulls)
                {
                    if (isCOWS_THREAD)
                    {
                        foreach (int elem in COWS_THREAD)
                        {
                            cows_list.Add(elem);
                        }
                        COWS_THREAD.Clear();
                        isCOWS_THREAD = false;
                    }
                    else cows_list.Add(prev_value);
                    unnecessary_digitals.Add(mass[index]);
                    mass[index] = cows_list.First(); // возможно проблем
                    cows_list.Remove(cows_list.First()); // тоже
                    cows++;
                    index++;
                }
                else if (prev_bulls < bulls)
                {
                    if (isCOWS_THREAD)
                    {
                        foreach (int elem in COWS_THREAD)
                        {
                            cows_list.Add(elem);
                        }
                        COWS_THREAD.Clear();
                        isCOWS_THREAD = false;
                    }
                    else cows_list.Add(prev_value);
                    ready_indexes.Add(index);
                    index++;
                }
            }
            else if (prev_cows < cows)
            {
                if (prev_bulls == bulls)
                {
                    if (isCOWS_THREAD)
                    {
                        foreach (int elem in COWS_THREAD)
                        {
                            unnecessary_digitals.Add(elem);
                        }
                        COWS_THREAD.Clear();
                        isCOWS_THREAD = false;
                    }
                    else unnecessary_digitals.Add(prev_value);
                    index++;
                }
                else if (prev_bulls > bulls) //то знач заносим в кч. на это место ставим знач_п, знач_п в ннч (ли?) и б++, к--. i++
                {
                    cows_list.Add(mass[index]);
                    mass[index] = prev_value;
                    ready_indexes.Add(index);
                    bulls++;
                    cows--;
                    index++;
                }
            }
            isSelectedDuetoAdvance = false;
        }

        async void DoTransposition()
        {
            int first_hand = -1;
            int second_hand = -1;
            while (bulls != 4)
            {
                IndexShift(ref first_hand, ref second_hand);
                Swap(ref mass[first_hand], ref mass[second_hand]);
                //считать
                ShowArr();
                await Read();
                if (bulls == prev_bulls + 1)
                {
                    FindSingleBull(first_hand, second_hand);
                    first_hand = -1;
                    second_hand = -1;
                }
                else if (bulls == prev_bulls + 2)
                {
                    ready_indexes.Add(mass[first_hand]);
                    ready_indexes.Add(mass[second_hand]);
                    first_hand = -1;
                    second_hand = -1;
                }
                else if (bulls == prev_bulls - 1)
                {
                    Swap(ref mass[first_hand], ref mass[second_hand]);
                    bulls++;
                    FindSingleBull(first_hand, second_hand);
                    first_hand = -1;
                    second_hand = -1;
                }
            }
            isWin = true;
        }

        void IndexShift(ref int a, ref int b)
        {
            if (a == -1 && b == -1)
            {
                int gran = -1;
                FindFirstFreeIndex(ref a, ref gran);
                FindFirstFreeIndex(ref b, ref gran);
            }
            else
            {
                a = b;
                int temp = b;
                FindFirstFreeIndex(ref b, ref temp);
                if (b == 4)
                {
                    int gran = -1;
                    FindFirstFreeIndex(ref b, ref gran);
                }
            }
        }

        void FindFirstFreeIndex(ref int f, ref int gran) // MAYBE CORRECT
        {
            do
            {
                gran++;
                f = gran;
            }
            while (ready_indexes.Contains(f));
        }

        void Swap(ref int a, ref int b)
        {
            int temp = a;
            a = b;
            b = temp;
        }

        async void FindSingleBull(int a, int b)
        {
            Random r = new Random();
            int prev_v = mass[a];
            do
                mass[a] = 1 + r.Next() % 9;
            while (prev_v == mass[a] || EqualToOtherNumbers(a));
            ShowArr();
            await Read();
            mass[a] = prev_v;
            if (prev_bulls > bulls)
            {
                bulls++;
                ready_indexes.Add(a);
            }
            else if (prev_bulls == bulls)
            {
                ready_indexes.Add(b);
            }
        }

        bool Check(int limit)
        {
            for (int j = 0; j < limit; j++)
                if (mass[j] == mass[limit])
                    return true;
            return false;
        }

        async Task Read()
        {
            prev_bulls = bulls;
            prev_cows = cows;
            await Task.Run(() => UserProccesing());
        }

        void ShowArr()
        {
            string temp_s = "";
            foreach (int elem in mass)
            {
                temp_s += elem.ToString();
            }
            textBlock.Text = temp_s;
        }

        void ChangeValue(int i)
        {
            Random r = new Random();
            prev_value = mass[i];
            if (cows_list.Count != 0) // все-таки посоветуемся с листом коров
            {
                mass[i] = cows_list.First();
                cows_list.Remove(cows_list.First());
                isSelectedDuetoAdvance = true;
            }
            else
            {
                do
                    mass[i] = 1 + r.Next() % 9;
                while (prev_value == mass[i] || CheckValue(i));
            }
        }

        bool CheckValue(int i)
        {
            //если mass[i] содержится в #ич
            foreach (int elem in used_digitals[i])
            {
                if (mass[i] == elem)
                    return true;
            }
            //если mass[i] содержится в #ннч
            foreach (int elem in unnecessary_digitals)
            {
                if (mass[i] == elem)
                    return true;
            }
            //если mass[i] равен другим числам в массиве
            if (EqualToOtherNumbers(i)) return true;
            return false;
        }

        bool EqualToOtherNumbers(int i)
        {
            for (int j = 0; j < 4; j++)
            {
                if (mass[j] == mass[i] && j != i)
                    return true;
            }
            return false;
        }
    }
}
