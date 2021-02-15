using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;

namespace MediaClassifier
{
    /// <summary>
    /// Interaction logic for Dialog.xaml
    /// </summary>
    public partial class FileMoverDialog : Window, INotifyPropertyChanged
    {
        private string _currentPath;
        private string fileNameToAppendToTheDestination = "";
        private ObservableCollection<MyClass> _classes;
        public ObservableCollection<MyClass> MyClasses
        {
            get { return _classes; }
            set
            {
                _classes = value;
                OnPropertyChanged();
            }
        }

        public FileMoverDialog(string currentPath)
        {
            InitializeComponent();
            _currentPath = currentPath;
            txtFile.Text = currentPath;
            MyClasses = new ObservableCollection<MyClass>();
            fileNameToAppendToTheDestination = currentPath.Substring(currentPath.LastIndexOf('/') + 1); //gets file name
            RefreshClasses();
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
                MyClasses.Add(new MyClass() { ID = count, Name = dri.Name });
                count++;
            }

        }

        public string ResponseText
        {
            get { return txtFile1.Text; }
            set { txtFile1.Text = value; }
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            MyClass currentClass = lbFolders.SelectedItem as MyClass;
            if (currentClass == null) return;
            string source = txtFile.Text; //Path.Combine(txtFile.Text, lbFolders.SelectedItem.ToString());
            string dest = Path.Combine(Directory.GetCurrentDirectory(), currentClass.Name.ToString());
            Move(source, dest);
        }

        private void Move(string source, string destination)
        {
            try
            {
                MyClass item = lbFolders.SelectedItem as MyClass;
                if (item == null) return;
                string newDest = Path.Combine(item.Name, destination);
                string dest = Path.Combine(newDest, fileNameToAppendToTheDestination);
                File.Move(source, dest);
                txtFile1.Text = "File Moved !!";
                Thread.SpinWait(1000);
                DialogResult = true;
            }
            catch (Exception ex)
            {

            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
