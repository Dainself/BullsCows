using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

        System.Threading.EventWaitHandle handle = new System.Threading.AutoResetEvent(false);

        public void UserProccesing()
        {
            handle.WaitOne();
        }

        private async void button2_Click(object sender, RoutedEventArgs e)
        {
            Init();
            await Task.Run(() => Start());
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            bulls = Int32.Parse(textBox.Text);
            cows = Int32.Parse(textBox2.Text);
            textBox.Clear();
            textBox2.Clear();
            handle.Set();
        }
    }
}
