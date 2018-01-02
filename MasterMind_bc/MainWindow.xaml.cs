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
        Game game;
        int first_field, second_field;
        CancellationTokenSource tokenSource;
        public MainWindow()
        {
            InitializeComponent();
        }

        EventWaitHandle handle = new AutoResetEvent(false);

        void UserProcessing(object sender, EventArgs e)
        {
            handle.WaitOne();
            ((Game)sender).Bulls = first_field;
            ((Game)sender).Cows = second_field;
        }

        void ShowText(object sender, AppealToUserEventArgs e)
        {
            ans.Text = e.Arr;
        }

        private async void button2_Click(object sender, RoutedEventArgs e)
        {
            if (game != null && tokenSource != null)
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
            game = new Game();
            game.appeal_to_user += UserProcessing;
            game.show_text += ShowText;
            count.Text = counting.ToString();
            status.Content = "Играем";
            try
            {
                await game.Start(token);
                status.Content = "Это победа";
                isPlaying = false;
                game = null;
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
    }
}
