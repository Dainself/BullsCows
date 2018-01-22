using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MasterMind_bc
{
    // class for sending CPU answer
    class AppealToUserEventArgs : EventArgs
    {
        readonly string arr;

        public AppealToUserEventArgs(string str) { arr = str; }

        public string Arr
        {
            get { return arr; }
        }
    }

    class GameCPUSide
    {
        public EventWaitHandle main_handle = new AutoResetEvent(false); // for waiting in main window
        SortedSet<int> unnecessary_digitals, cows_list, COWS_THREAD, ready_indexes;
        SortedSet<int>[] used_digitals;
        int bulls, cows, prev_bulls, prev_cows, prev_value;
        int[] mass;
        bool isSelectedDuetoAdvance, isWin, isCOWS_THREAD;
        public event EventHandler appeal_to_user;

        public int Bulls
        {
            set { bulls = value; }
        }

        public int Cows
        {
            set { cows = value; }
        }

        public GameCPUSide()
        {
            isSelectedDuetoAdvance = false;
            isCOWS_THREAD = false;
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
            Random rand = new Random();
            for (int i = 0; i < 4; i++) // init array by random numbers
            {
                do
                {
                    mass[i] = 1 + rand.Next() % 9;
                }
                while (Utils.Check(mass, i));
            }
            for (int i = 1; i < 4; i++) // add current digits to already used
            {
                used_digitals[i].Add(mass[i]);
            }
        }

		// main strategy - quickly find all cows, and then do transpositions
        public async void Start(CancellationToken token)
        {
            int k = 0; // index for mass
            bool first_time_flag = true, do_continue;
            // game cycle
            while (!isWin)
            {
                try
                {
                    await ShowRead(token); // show data and read result from user
                    token.ThrowIfCancellationRequested();
                    do_continue = await DoPreAnalyse(k, first_time_flag, token); // fast-analyse
                    if (first_time_flag) // no real analyse in first time
                    {
                        first_time_flag = false;
                        goto for_the_first_time;
                    }
                    if (do_continue) DoAnalyse(ref k); // do analyse
                    if (isWin) return;
                    for_the_first_time:
                    { // remember and change current value
                        used_digitals[k].Add(mass[k]);
                        prev_value = mass[k];
                        ChangeValue(k);
                    }
                }
				// if game finished or cancelled by button START
				// (otherwords, this is done in any situations)
                catch (OperationCanceledException oce) { main_handle.Set(); return; }
            }
        }

		// check simply cases
		// maybe this's a real analyse, not pre. it is worth considering
        async Task<bool> DoPreAnalyse(int index, bool status, CancellationToken token)
        {
            if (bulls == 4)
            {
                isWin = true;
                return false;
            }
            else if (bulls + cows == 4) // in this case do transpositions
            {
                if (prev_bulls < bulls && !status)
                {
                    ready_indexes.Add(index);
                }
                await DoTransposition(token);
                return false;
            }
            else if (bulls + cows == 0)
            {
                for (int i = 0; i < 4; i++)
                {
                    unnecessary_digitals.Add(mass[i]);
                }
                return false;
            }
            return true;
        }

		// analyse. there are deals with different situations
        void DoAnalyse(ref int index)
        {
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
                    ready_indexes.Add(index); // fix the index
                    index++;
                }
                else if (prev_bulls > bulls)
                {
                    unnecessary_digitals.Add(mass[index]);
                    mass[index] = prev_value;
                    bulls++;
                    ready_indexes.Add(index); // fix the index
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
                    mass[index] = cows_list.First();
                    cows_list.Remove(cows_list.First());
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
                else if (prev_bulls > bulls)
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

		// consistently swap 2 elements
        async Task DoTransposition(CancellationToken token)
        {
            int first_hand = -1;
            int second_hand = -1;
            while (bulls != 4)
            {
                IndexShift(ref first_hand, ref second_hand);
                Swap(ref mass[first_hand], ref mass[second_hand]);
                await ShowRead(token);
                token.ThrowIfCancellationRequested();
                if (bulls == prev_bulls + 1) // questionable, need to find out bull
                {
                    await FindSingleBull(first_hand, second_hand, token);
                    first_hand = -1;
                    second_hand = -1;
                }
                else if (bulls == prev_bulls + 2) // clearly
                {
                    ready_indexes.Add(first_hand);
                    ready_indexes.Add(second_hand);
                    first_hand = -1;
                    second_hand = -1;
                }
                else if (bulls == prev_bulls - 1)
                {
                    Swap(ref mass[first_hand], ref mass[second_hand]);
                    bulls++;
                    await FindSingleBull(first_hand, second_hand, token);
                    first_hand = -1;
                    second_hand = -1;
                }
            }
            isWin = true;
        }

		// shift swapping indexes
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

		// select indexes to swap
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

		// disambiguation when the bull appears
        async Task FindSingleBull(int a, int b, CancellationToken token)
        {
            Random r = new Random();
            int prev_v = mass[a];
            do
                mass[a] = 1 + r.Next() % 9; // change a first digit to random
            while (prev_v == mass[a] || Utils.EqualToOtherNumbers(mass, a));
            await ShowRead(token);
            token.ThrowIfCancellationRequested();
            mass[a] = prev_v; // reset value
			// and make conclusion
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

        async Task RaiseAppeal(EventArgs e)
        {
			// as an alternative:
            //EventHandler<AppealToUserEventArgs> temp = Volatile.Read(ref appeal_to_user);
            //if (temp != null) await Task.Run(() => temp(this, e));
            await Task.Run(() => Volatile.Read(ref appeal_to_user)?.Invoke(this, e));
        }

		// show array and reading answer from user
        public event EventHandler<AppealToUserEventArgs> show_text;
        async Task ShowRead(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            string temp_s = Utils.ArrToString(mass);
            AppealToUserEventArgs e = new AppealToUserEventArgs(temp_s);
            prev_bulls = bulls;
            prev_cows = cows;
            show_text(this, e);
            await RaiseAppeal(null);
        }

		// change value in array
        void ChangeValue(int i)
        {
            Random r = new Random();
            prev_value = mass[i];
            if (cows_list.Count != 0) // consult with cows_list
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

		// check new value to be suitable
        bool CheckValue(int i)
        {
            //if mass[i] contains in #ич
            foreach (int elem in used_digitals[i])
            {
                if (mass[i] == elem)
                    return true;
            }
            //if mass[i] contains in #ннч
            foreach (int elem in unnecessary_digitals)
            {
                if (mass[i] == elem)
                    return true;
            }
            //if mass[i] equals to other numbers in array
            if (Utils.EqualToOtherNumbers(mass, i)) return true;
            return false;
        }
    }
}
