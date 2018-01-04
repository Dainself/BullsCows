using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Threading;

namespace MasterMind_bc
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int counting;
        bool isPlaying;
        GameCPUSide gameCPU;
        GameUserSide gameUser;
        int first_field, second_field;
        string third_field;
        CancellationTokenSource tokenSource;
        public MainWindow()
        {
            InitializeComponent();
        }

        EventWaitHandle main_handle = new AutoResetEvent(false);
        EventWaitHandle handleCPU = new AutoResetEvent(false);
        EventWaitHandle handleUser = new AutoResetEvent(false);

        void UserProcessing(object sender, EventArgs e)
        {
            main_handle.WaitOne();
            ((GameCPUSide)sender).Bulls = first_field;
            ((GameCPUSide)sender).Cows = second_field;
        }

        void ShowText(object sender, AppealToUserEventArgs e)
        {
            ans.Text = e.Arr;
        }

        void UserProcessing2(object sender, EventArgs e)
        {
            handleUser.WaitOne();
            ((GameUserSide)sender).UserMass = Utils.StringToArr(third_field);
        }

        void ShowText2(object sender, AppealToUserEventArgs2 e)
        {
            textBox1.Text += third_field;
            textBox1.Text += GiveMeSpaces(15);
            textBox1.Text += e.Bulls.ToString();
            textBox1.Text += GiveMeSpaces(12);
            textBox1.Text += e.Cows.ToString();
            textBox1.Text += "\n";
        }

        private async void button2_Click(object sender, RoutedEventArgs e)
        {
            if (gameCPU != null && tokenSource != null)
            {
                tokenSource.Cancel();
                handle.Set();
            }
            status.Content = "Загрузка, подождите...";
            await Task.Run(() => Thread.Sleep(420));
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


            count.Text = counting.ToString();
            status.Content = "Играем";
            try
            {
                await gameCPU.Start(token);
                status.Content = "Это победа";
                isPlaying = false;
                gameCPU = null;
                tokenSource = null;
            }
            catch (OperationCanceledException oce) { }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            if (isPlaying)
            {
                try
                {
                    first_field = Int32.Parse(textBox.Text);
                    second_field = Int32.Parse(textBox2.Text);
                }
                catch (FormatException ex)
                {
                    MessageBox.Show(ex.Message + "\nПожалуйста, попробуйте еще раз.");
                    return;
                }
                textBox.Clear();
                textBox2.Clear();
                count.Text = (++counting).ToString();
                handle.Set();
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void textBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (IsNumbericChar(e.Key)) e.Handled = true;
        }

        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (IsNumbericChar(e.Key)) e.Handled = true;
        }

        private bool IsNumbericChar(Key key)
        {
            if (key != Key.D0 && key != Key.NumPad0 &&
                key != Key.D1 && key != Key.NumPad1 &&
                key != Key.D2 && key != Key.NumPad2 &&
                key != Key.D3 && key != Key.NumPad3 &&
                key != Key.D4 && key != Key.NumPad4)
                return true;
            return false;
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
