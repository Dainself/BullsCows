using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Threading;

namespace MasterMind_bc
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int counting;
        bool isPlaying, isCPUwins, isUserwins;
        GameCPUSide gameCPU;
        GameUserSide gameUser;
        int first_field, second_field;
        string third_field;
        CancellationTokenSource tokenSource;
        CancellationToken token;
        public MainWindow()
        {
            InitializeComponent();
        }

        //EventWaitHandle main_handle = new AutoResetEvent(false);
        EventWaitHandle handleCPU = new AutoResetEvent(false);
        EventWaitHandle handleUser = new AutoResetEvent(false);
        bool is_handleCPU_set;
        bool is_handleUser_set;
        void UserProcessing(object sender, EventArgs e)
        {
            main_handle.WaitOne();
            if (first_field == 4)
            {
                isCPUwins = true;
                tokenSource.Cancel();
            }
            ((GameCPUSide)sender).Bulls = first_field;
            ((GameCPUSide)sender).Cows = second_field;
        }

        void ShowText(object sender, AppealToUserEventArgs e)
        {
            ans.Text = e.Arr;
        }

        void UserProcessing2(object sender, EventArgs e)
        {
            main_handle.WaitOne();
            ((GameUserSide)sender).UserMass = Utils.StringToArr(third_field);
        }

        void ShowText2(object sender, AppealToUserEventArgs2 e)
        {
            if (e.Bulls == 4)
            {
                isUserwins = true;
                tokenSource.Cancel();
            }
            textBox1.Text += third_field;
            textBox1.Text += GiveMeSpaces(15);
            textBox1.Text += e.Bulls.ToString();
            textBox1.Text += GiveMeSpaces(12);
            textBox1.Text += e.Cows.ToString();
            textBox1.Text += "\n";
        }

        private async void button2_Click(object sender, RoutedEventArgs e)
        {
            if (gameCPU != null && gameUser != null && tokenSource != null)
            {
                tokenSource.Cancel();
                main_handle.Set();
            }
            status.Content = "Загрузка, подождите...";
            await Task.Run(() => Thread.Sleep(420));
            isPlaying = true;
            counting = 0;
            tokenSource = new CancellationTokenSource();
            token = tokenSource.Token;
            /** CPU Side **/
            gameCPU = new GameCPUSide();
            gameCPU.appeal_to_user += UserProcessing;
            gameCPU.show_text += ShowText;
            /** User Side **/
            gameUser = new GameUserSide();
            gameUser.appeal_to_user += UserProcessing2;
            gameUser.show_result += ShowText2;

            count.Text = counting.ToString();
            status.Content = "Играем";
            try
            {
                Parallel.Invoke(async () => await Application.Current.Invoke(
                           () =>
                           {
                               gameCPU.Start(token);
                           }
                        ), async () => await Application.Current.Invoke(
                           () =>
                           {
                               gameUser.Start(token);
                           }
                        ));
                //Parallel.Invoke(async () => await gameCPU.Start(), async () => await gameUser.Start(token));
                //await Task.Run(() => Parallel.Invoke(dowork, dowork2));
            }
            catch (OperationCanceledException oce)
            {
                if (!isCPUwins && !isUserwins)
                {
                    if (isCPUwins) status.Content = "Победа за мной.";
                    else if (isUserwins) status.Content = "Вы победили!";
                    isUserwins = false;
                    isCPUwins = false;
                    isPlaying = false;
                    gameCPU = null;
                    gameUser = null;
                    tokenSource = null;
                }
            }
        }

        /*async void dowork() { await gameCPU.Start(); }

        async void dowork2() { await gameUser.Start(token); }*/

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            if (isPlaying)
            {
                first_field = Int32.Parse(textBox.Text);
                second_field = Int32.Parse(textBox2.Text);
                textBox.Clear();
                textBox2.Clear();
                //handleCPU.Set();
                //if (first_field == 4) isCPUwins = true;

                is_handleCPU_set = true;
                if (is_handleUser_set)
                {
                    status.Content = "Играем";
                    is_handleCPU_set = false;
                    is_handleUser_set = false;
                    count.Text = (++counting).ToString();
                    main_handle.Set();
                }
                else status.Content = "User, введите число";
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void textBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (IsNumbericChar(e.Key, 4)) e.Handled = true;
        }

        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (IsNumbericChar(e.Key, 4)) e.Handled = true;
        }

        private bool IsNumbericChar(Key key, int limit)
        {
            int[] res = new int[10];
            if (limit > -1)
                if (key != Key.D0 && key != Key.NumPad0) res[0]++;
            if (limit > 0)
                if (key != Key.D1 && key != Key.NumPad1) res[1]++;
            if (limit > 1)
                if (key != Key.D2 && key != Key.NumPad2) res[2]++;
            if (limit > 2)
                if (key != Key.D3 && key != Key.NumPad3) res[3]++;
            if (limit > 3)
                if (key != Key.D4 && key != Key.NumPad4) res[4]++;
            if (limit > 4)
                if (key != Key.D5 && key != Key.NumPad5) res[5]++;
            if (limit > 5)
                if (key != Key.D6 && key != Key.NumPad6) res[6]++;
            if (limit > 6)
                if (key != Key.D7 && key != Key.NumPad7) res[7]++;
            if (limit > 7)
                if (key != Key.D8 && key != Key.NumPad8) res[8]++;
            if (limit > 8)
                if (key != Key.D9 && key != Key.NumPad9) res[9]++;
            return (res.SumCount() == limit + 1) ? true : false;
        }

        private void ans2_KeyDown(object sender, KeyEventArgs e)
        {
            if (IsNumbericChar(e.Key, 9)) e.Handled = true;
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            if (isPlaying)
            {
                //try
                //{
                //third_field = ans2.Text;
                //}
                third_field = ans2.Text;
                ans2.Clear();
                //main_handle.Set(); ///
                is_handleUser_set = true;
                if (is_handleCPU_set)
                {
                    status.Content = "Играем";
                    is_handleCPU_set = false;
                    is_handleUser_set = false;
                    count.Text = (++counting).ToString();
                    main_handle.Set();
                }
                else status.Content = "User, введите кол-во быков и коров";
            }
        }

        private string GiveMeSpaces(int amount)
        {
            string temp_s = "";
            for (int i = 0; i < amount; i++)
            {
                temp_s += " ";
            }
            return temp_s;
        }
    }
}
