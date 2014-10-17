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
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ClipboardHistory.ViewModel;

namespace ClipboardHistory.View
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ClipboardWatcher clipboardWatcher;
        private HwndSource hWndSource;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new MainViewModel();
        }

        private void ClipboardChanged(object sender, EventArgs e)
        {
            var viewModel = (MainViewModel)DataContext;
            if (viewModel.cmdClipboardChanged.CanExecute(null))
                viewModel.cmdClipboardChanged.Execute(null);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WindowInteropHelper wih = new WindowInteropHelper(this);
            hWndSource = HwndSource.FromHwnd(wih.Handle);
            clipboardWatcher = new ClipboardWatcher(hWndSource.Handle);
            clipboardWatcher.OnClipboardChanged += new EventHandler(ClipboardChanged);
            clipboardWatcher.SetClipboardHandle();
            hWndSource.AddHook(clipboardWatcher.ReceiveMessage);
        }
    }
}
