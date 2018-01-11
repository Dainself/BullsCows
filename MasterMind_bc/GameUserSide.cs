using System;
using System.Threading;
using System.Threading.Tasks;

namespace MasterMind_bc
{
    class AppealToUserEventArgs2 : EventArgs
    {
        readonly int bulls, cows;

        public AppealToUserEventArgs2(int a, int b)
        {
            bulls = a;
            cows = b;
        }

        public int Bulls
        {
            get { return bulls; }
        }

        public int Cows
        {
            get { return cows; }
        }
    }

    class GameUserSide
    {
        public EventWaitHandle main_handle = new AutoResetEvent(false);
        int[] mass, user_mass;
        int bulls, cows;
        bool isWin;
        public bool isShowRequired = true;

        public int[] UserMass
        {
            set { user_mass = value; }
        }

        public GameUserSide()
        {
            mass = new int[4];
            Random rand = new Random();
            // for debug
            //for (int i = 0; i < 4; ) mass[i] = ++i;
            for (int i = 0; i < 4; i++) // заполняю случ значениями
            {
                do
                {
                    mass[i] = 1 + rand.Next() % 9;
                }
                while (Utils.Check(mass, i));
            }
        }

        public async void Start(CancellationToken token)
        {
            while (!isWin)
            {
                try
                {
                    await Read(token);
                    if (!isShowRequired) token.ThrowIfCancellationRequested();
                    DoAnalyse();
                    Show(token);
                }
                catch (OperationCanceledException oce) { main_handle.Set(); return; }
            }
        }

        void DoAnalyse()
        {
            bulls = 0;
            cows = 0;
            for (int j = 0; j <4; j++)
            {
                int result = DoRealAnalyse(user_mass[j], j);
                switch (result)
                {
                    case 1:
                        bulls++;
                        break;
                    case -1:
                        cows++;
                        break;
                    case 0:
                        break;
                }
            }
            if (bulls == 4) isWin = true;
        }

        int DoRealAnalyse(int digit, int index)
        {
            for (int i = 0; i < 4; i++)
            {
                if (digit == mass[i])
                {
                    return (index == i) ? 1 : -1;
                    /*if (index == i)
                        return 1;
                    else return -1;*/
                }
            }
            return 0;
        }

        public event EventHandler appeal_to_user;
        async Task RaiseAppeal(EventArgs e)
        {
            await Task.Run(() => Volatile.Read(ref appeal_to_user)?.Invoke(this, e));
        }

        async Task Read(CancellationToken token)
        {
            // считываю от юзверя строку-массив
            // считаю сколько быков коров
            // и отвечаю
            token.ThrowIfCancellationRequested();
            await RaiseAppeal(null);
        }

        public event EventHandler<AppealToUserEventArgs2> show_result;
        void Show(CancellationToken token)
        {
            //if (bulls == 4) //уведомить о победе /// нет. на той стороне сами пусть проверяют
            ///говорю результат
            show_result(this, new AppealToUserEventArgs2(bulls, cows));
            token.ThrowIfCancellationRequested();
        }
    }
}
