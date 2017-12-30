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
        public MainWindow()
        {
            InitializeComponent();
        }

        EventWaitHandle handle = new AutoResetEvent(false);

        public void UserProccesing()
        {
            handle.WaitOne();
        }

        private async void button2_Click(object sender, RoutedEventArgs e)
        {
            status.Content = "Загрузка, подождите...";
            await Task.Run(() => Thread.Sleep(500));
            status.Content = "Играем";
            Init();
            await Start();
            status.Content = "Это победа";
            counting = 0;
        }
        int counting = 0;
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            if (!isWin)
            {
                try
                {
                    bulls = Int32.Parse(textBox.Text);
                    cows = Int32.Parse(textBox2.Text);
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

        private bool IsNumbericChar(Key key)
        {
            if (key != Key.D1 && key != Key.NumPad1 &&
                key != Key.D2 && key != Key.NumPad2 &&
                key != Key.D3 && key != Key.NumPad3 &&
                key != Key.D4 && key != Key.NumPad4)
                return true;
            return false;
        }
    }
}
