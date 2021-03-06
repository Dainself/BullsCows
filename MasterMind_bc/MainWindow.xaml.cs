﻿using System;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Threading;
using System.Threading.Tasks;

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
        int first_field, second_field; //bulls and cows to CPU
        string third_field; // answer from user
        public MainWindow()
        {
            InitializeComponent();
        }

        CancellationTokenSource tokenSource;
        EventWaitHandle handleCPU = new AutoResetEvent(false);
        EventWaitHandle handleUser = new AutoResetEvent(false);
        bool is_handleCPU_set;
        bool is_handleUser_set;

        // waits for inputing bulls and cows, send them to calling class (to sender)
        void UserProcessing(object sender, EventArgs e)
        {
            handleCPU.WaitOne();
            if (first_field == 4) // win case
            {
                isCPUwins = true;
                tokenSource.Cancel();
                tokenSource.Cancel();
                if (is_handleUser_set)
                {
                    gameUser.isShowRequired = false;
                    handleUser.Set();
                }
            }
            if (is_handleCPU_set && is_handleUser_set) // reset case
            {
                is_handleCPU_set = false;
                is_handleUser_set = false;
            }
            ((GameCPUSide)sender).Bulls = first_field;
            ((GameCPUSide)sender).Cows = second_field;
        }

        // shows text (array from CPU)
        void ShowText(object sender, AppealToUserEventArgs e)
        {
            ans.Text = e.Arr;
        }

        // waits for inputing digit, send it to calling class (to sender)
        void UserProcessing2(object sender, EventArgs e)
        {
            handleUser.WaitOne();
            ((GameUserSide)sender).UserMass = Utils.StringToArr(third_field);
        }

        // shows text (arrays from user with numbers of bulls/cows)
        void ShowText2(object sender, AppealToUserEventArgs2 e)
        {
            if (e.Bulls == 4) // win case
            {
                isUserwins = true;
                tokenSource.Cancel();
                tokenSource.Cancel();
                if (is_handleCPU_set) handleCPU.Set();
            }
            if (is_handleCPU_set && is_handleUser_set) // reset case
            {
                is_handleCPU_set = false;
                is_handleUser_set = false;
            }
            StringBuilder sb = new StringBuilder(textBox1.Text.Length + 34);
            sb.AppendFormat("{0}{1}{2}{3}", textBox1.Text, third_field, GiveMeSpaces(15), e.Bulls.ToString());
            sb.AppendFormat("{0}{1}", GiveMeSpaces(12), e.Cows.ToString());
            sb.AppendLine();
            textBox1.Text = sb.ToString();
        }

        // START button
        private async void button2_Click(object sender, RoutedEventArgs e)
        {
            if (tokenSource != null) // if already playing, start new game. old game is cancelled
            {
                gameUser.isShowRequired = false;
                tokenSource.Cancel();
                handleCPU.Set();
                handleUser.Set();
            }
            isUserwins = false;
            isCPUwins = false;
            isPlaying = false;
            is_handleCPU_set = false;
            is_handleUser_set = false;
            first_field = 0;
            second_field = 0;
            third_field = "";

            status.Content = "Please wait, loading...";
            await Task.Run(() => Thread.Sleep(380));
            isPlaying = true;
            counting = 0;
            tokenSource = new CancellationTokenSource();
            CancellationToken token = tokenSource.Token;

            /** CPU Side **/
            gameCPU = new GameCPUSide();
            gameCPU.appeal_to_user += UserProcessing;
            gameCPU.show_text += ShowText;

            /** User Side **/
            gameUser = new GameUserSide();
            gameUser.appeal_to_user += UserProcessing2;
            gameUser.show_result += ShowText2;

            ans.Text = "";
            textBox1.Text = "";
            count.Text = counting.ToString();
            status.Content = "Playing";
            // game invoke
            Parallel.Invoke(async () =>
            {
                await Application.Current.Invoke(() => { gameCPU.Start(token); });
            }, async () =>
            {
                await Application.Current.Invoke(() => { gameUser.Start(token); });
            });
            // wait until classes finished work
            await Task.Run(() => gameCPU.main_handle.WaitOne());
            await Task.Run(() => gameUser.main_handle.WaitOne());
            if (!isCPUwins && !isUserwins) // rude cancellation case (new game requested)
            {
                return;
            }
            else
            {
                if (isCPUwins && isUserwins) status.Content = "Seems like it's a draw.";
                else if (isCPUwins) status.Content = "I gained the victory.";
                else if (isUserwins) status.Content = "You win!";
                isPlaying = false;
                gameCPU = null;
                gameUser = null;
                tokenSource = null;
            }
        }

        // reads number of bulls/cows from user
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            if (isPlaying && !is_handleCPU_set)
            {
                try
                {
                    first_field = Int32.Parse(textBox.Text);
                    second_field = Int32.Parse(textBox2.Text);
                }
                catch (FormatException fe)
                {
                    MessageBox.Show("Enter correct number");
                }
                textBox.Clear();
                textBox2.Clear();

                is_handleCPU_set = true;
                handleCPU.Set();

                if (is_handleUser_set)
                {
                    status.Content = "Playing";
                    count.Text = (++counting).ToString();
                }
                else status.Content = "Enter your digit";
            }
        }

        // EXIT button
        private void button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // supports only numberic text
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

        // shows history of user answers in MessageBox
        private void view_Click(object sender, RoutedEventArgs e)
        {
            if (isPlaying)
            {
                if (!String.IsNullOrEmpty(textBox1.Text))
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("{0}{1}{2}{3}{4}", "Answer", GiveMeSpaces(9), "Bulls", GiveMeSpaces(6), "Cows");
                    sb.AppendLine();
                    sb.Append(textBox1.Text);
                    MessageBox.Show(sb.ToString());
                }
                else
                    MessageBox.Show("Empty.");
            }
        }

        private void ans2_KeyDown(object sender, KeyEventArgs e)
        {
            if (IsNumbericChar(e.Key, 9)) e.Handled = true;
            if (ans2.Text.Length == 4) e.Handled = true;
        }

        // reads digit from user
        private void button3_Click(object sender, RoutedEventArgs e)
        {
            if (isPlaying && !is_handleUser_set)
            {
                // some text check
                if (ans2.Text == "")
                {
                    MessageBox.Show("Line is empty");
                    return;
                }
                int[] mass = Utils.StringToArr(ans2.Text);
                for (int i = 0; i < 4; i++)
                {
                    if (Utils.EqualToOtherNumbers(mass, i))
                    {
                        MessageBox.Show("Uncorrect line");
                        return;
                    }
                }
                third_field = ans2.Text;
                ans2.Clear();

                is_handleUser_set = true;
                handleUser.Set();
                if (is_handleCPU_set)
                {
                    status.Content = "Playing";
                    count.Text = (++counting).ToString();
                }
                else status.Content = "Enter bulls and cows";
            }
        }
        
        // makes " " symbols
        private string GiveMeSpaces(int amount)
        {
            StringBuilder temp_s = new StringBuilder();
            for (int i = 0; i < amount; i++)
            {
                temp_s.Append(" ");
            }
            return temp_s.ToString();
        }
    }
}
