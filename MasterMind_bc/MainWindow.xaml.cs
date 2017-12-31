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
        bool isPlaying;
        Game game;
        int first_field, second_field;
        public MainWindow()
        {
            InitializeComponent();
        }

        EventWaitHandle handle = new AutoResetEvent(false);

        /*public void UserProccesing()
        {
            handle.WaitOne();
        }*/

        public void UserProcessing(object sender, EventArgs e)
        {
            handle.WaitOne();
            ((Game)sender).bulls = first_field;
            ((Game)sender).cows = second_field;
        }

        public void ShowText(object sender, AppealToUserEventArgs e)
        {
            ans.Text = e.Arr;
        }

        private async void button2_Click(object sender, RoutedEventArgs e)
        {
            //if (!isPlaying)
            //{
                status.Content = "Загрузка, подождите...";
                isPlaying = true;
            game = null;
                game = new Game(this);
                //game.appeal_to_user += UserProcessing;
                //game.show_text += ShowText;
                await Task.Run(() => Thread.Sleep(500));
                status.Content = "Играем";
                await game.Start();
                status.Content = "Это победа";
                isPlaying = false;
                counting = 0;
            //}
        }

        int counting = 0;
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
                    MessageBox.Show(ex.Message +"\nПожалуйста, попробуйте еще раз.");
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
