using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Win32;
using Path = System.IO.Path;

namespace MediaClassifier
{
    public class MyClass
    {
        public int ID { get; set; }
        public string Name { get; set; }

    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        string currentFolder;
        private bool mediaPlayerIsPlaying = false;
        private bool userIsDraggingSlider = false;
        private ObservableCollection<MyClass> _classes;
        public ObservableCollection<MyClass> MyClasses
        {
            get { return _classes;}
            set
            {
                _classes = value;
                OnPropertyChanged();
            }
        }
        public MainWindow()
        {
            InitializeComponent();
            MyClasses = new ObservableCollection<MyClass>();
            RefreshClasses();

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();
        }

        private void RefreshClasses()
        {
            MyClasses.Clear();
            //lbFolder.Items.Clear();
            // Make a reference to a directory.
            DirectoryInfo di = new DirectoryInfo(Directory.GetCurrentDirectory());
            // Get a reference to each directory in that directory.
            DirectoryInfo[] diArr = di.GetDirectories();

            int count = 1;
            // Display the names of the directories.
            foreach (DirectoryInfo dri in diArr)
            {
                MyClasses.Add(new MyClass(){ID = count, Name = dri.Name});
                count++;
            }
            //  lbFolder.Items.Add(dri.Name);
        }



       
        void timer_Tick(object sender, EventArgs e)
        {
            if ((mePlayer.Source != null) && (mePlayer.NaturalDuration.HasTimeSpan) && (!userIsDraggingSlider))
            {
                sliProgress.Minimum = 0;
                sliProgress.Maximum = mePlayer.NaturalDuration.TimeSpan.TotalSeconds;
                sliProgress.Value = mePlayer.Position.TotalSeconds;
            }
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            lblStatus.Content = "";
            if (lbFiles.SelectedIndex != -1)
            {
                mePlayer.Source = new Uri(Path.Combine(currentFolder, lbFiles.SelectedItem.ToString()));
            }
            mePlayer.Play();
        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            mePlayer.Pause();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            mePlayer.Stop();
        }

        private void btnOpenFiles_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "Text files (*.m4v)|*.m4v|All files (*.*)|*.*";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);
            if (openFileDialog.ShowDialog() == true)
            {

                foreach (string filename in openFileDialog.FileNames)
                {
                    currentFolder = System.IO.Path.GetDirectoryName(openFileDialog.FileName);
                    lbFiles.Items.Add(Path.GetFileName(filename));
                }
            }
        }

        private void MePlayer_OnMediaEnded(object sender, RoutedEventArgs e)
        {
            string str = (e.OriginalSource as MediaElement).Source.AbsolutePath;
            
            FileMoverDialog dialog = new FileMoverDialog(str);
            if (dialog.ShowDialog() != true) return;
            int index = -1;
            if (!string.IsNullOrEmpty(dialog.ResponseText))
            {
                object? matched = "";
                foreach (var item in lbFiles.Items)
                {
                    if (str.Contains(item.ToString()))
                        matched = item;
                }
                if(matched != null)
                    lbFiles.Items.Remove(matched);
                index = lbFiles.Items.IndexOf(matched);
                if (lbFiles.Items.Count > 0)
                {
                    lbFiles.SelectedItem = lbFiles.Items.GetItemAt(0).ToString();
                    mePlayer.Source = new Uri(Path.Combine(currentFolder, lbFiles.Items.GetItemAt(0).ToString()));
                    mePlayer.Play();
                }
            }

           

        }

        private void MePlayer_OnMediaOpened(object sender, RoutedEventArgs e)
        {
           //MyProgressBar.Maximum = mePlayer.NaturalDuration.TimeSpan.TotalMilliseconds;
        }

        private void sliProgress_DragStarted(object sender, DragStartedEventArgs e)
        {
            userIsDraggingSlider = true;
        }

        private void sliProgress_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            userIsDraggingSlider = false;
            mePlayer.Position = TimeSpan.FromSeconds(sliProgress.Value);
        }

        private void sliProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            lblProgressStatus.Text = TimeSpan.FromSeconds(sliProgress.Value).ToString(@"hh\:mm\:ss");
        }

        private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            mePlayer.Volume += (e.Delta > 0) ? 0.1 : -0.1;
        }

        private void Btn_Folder_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new Dialog();
            if (dialog.ShowDialog() != true) return;
            if (!string.IsNullOrEmpty(dialog.ResponseText))
            {
               Directory.CreateDirectory(dialog.ResponseText);
            }

            RefreshClasses();
        }

        private void Move(string source,string destination)
        {
            lblStatus.Content = "";
            File.Move(source, destination);
            lbFiles.Items.Remove(lbFiles.SelectedItem);
            lblStatus.Content = "File moved !!";
            if (lbFiles.Items.Count > 0)
            {
                lbFiles.SelectedItem = lbFiles.Items.GetItemAt(0).ToString();
                mePlayer.Source = new Uri(Path.Combine(currentFolder, lbFiles.Items.GetItemAt(0).ToString()));
                mePlayer.Play();
            }
        }

        private void Btn_Move_OnClick(object sender, RoutedEventArgs e)
        {
            if (lbFolder.SelectedIndex > -1 && lbFiles.SelectedIndex > -1)
            {
                lblStatus.Content = "";
                mePlayer.Stop();
                string source = Path.Combine(currentFolder, lbFiles.SelectedItem.ToString());
                MyClass item = lbFolder.SelectedItem as MyClass;
                if (item == null) return;
                string dest = Path.Combine(item.Name, lbFiles.SelectedItem.ToString());
                Move(source, dest);
            }
               
        }

        private void EventSetter_OnHandler(object sender, MouseButtonEventArgs e)
        {
            var item = e.Source as ListBoxItem;
            if (item != null)
            {
                mePlayer.Stop();
                mePlayer.Source = new Uri(Path.Combine(currentFolder, item.Content.ToString()));
                lblStatus.Content = "";
                mePlayer.Play();
            }

        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string _selectedClass = "";
        public string SelectedClass
        {
            get { return _selectedClass; }
            set
            {
                _selectedClass = value;
                OnPropertyChanged();
            }
        }

        int id = -1;
        private void MainWindow_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.D0 || e.Key == Key.D1 || e.Key == Key.D2 || e.Key == Key.D3 || e.Key == Key.D4 || e.Key == Key.D5 || e.Key == Key.D6 || e.Key == Key.D7
                    || e.Key == Key.D8 || e.Key == Key.D9)
                {
                   // id = Convert.ToInt16(new KeyConverter().ConvertFromString(e.Key.ToString()));
                    id = Convert.ToInt32(new KeyConverter().ConvertToString(e.Key));
                    if (id > 0)
                    {
                        var currentClass = MyClasses.SingleOrDefault(p => p.ID == id);
                        if (currentClass == null)
                        {
                            id = -1;
                            return;
                        }
                        SelectedClass = currentClass.Name;
                    }

                }

                if (e.Key == Key.Enter)
                {
                    if (id > 0)
                    {
                        if (lbFiles.SelectedItem != null)
                        {
                            string source = Path.Combine(currentFolder, lbFiles.SelectedItem.ToString());
                            MyClass currentClass = MyClasses.SingleOrDefault(p => p.ID == id);
                            if (currentClass != null)
                            {
                                string dest = Path.Combine(currentClass.Name, lbFiles.SelectedItem.ToString());
                                Move(source, dest);
                                SelectedClass = "";
                                id = -1;
                            }
                            
                        }
                    }
                }

                if (e.Key == Key.Escape)
                {
                    id = -1;
                    _selectedClass = "";
                }
            }
            catch (Exception ex)
            {
                id = -1;
                _selectedClass = "";
            }

        }
    }


}
