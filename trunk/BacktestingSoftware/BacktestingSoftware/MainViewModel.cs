using System.ComponentModel;

namespace BacktestingSoftware
{
    internal class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private string _algorithmFileName;

        public string AlgorithmFileName
        {
            get
            {
                return _algorithmFileName;
            }
            set
            {
                if (value != _algorithmFileName)
                {
                    _algorithmFileName = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("AlgorithmFileName"));
                }
            }
        }

        private string _dataFileName;

        public string DataFileName
        {
            get
            {
                return _dataFileName;
            }
            set
            {
                if (value != _dataFileName)
                {
                    _dataFileName = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("DataFileName"));
                }
            }
        }
    }
}