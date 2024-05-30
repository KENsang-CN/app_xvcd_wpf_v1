using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MaterialDesignThemes.Wpf;
using Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Linq;
using xvcd_wpf_v1.View;
using System.Threading.Tasks;

namespace xvcd_wpf_v1.ViewModel
{
    [JsonObject(MemberSerialization.OptIn)]
    public class AppConfig
    {
        [JsonProperty("JTAG线缆")] public string JtagCable;
        [JsonProperty("代理端口")] public int Port;
        [JsonProperty("JTAG速率")] public double TckFreq;
        [JsonProperty("代理使能")] public bool Enable;
    }

    public class XvcdViewModel : ViewModelBase
    {
        private MainViewModel mainVM;
        private ObservableCollection<XvcdViewModel> parent;

        private bool isSelected;
        public bool IsSelected { get => isSelected; set { isSelected = value; RaisePropertyChanged(() => IsSelected); } }

        /// <summary>
        /// selection changed command
        /// </summary>
        private RelayCommand selectionChangeCommand;
        public RelayCommand SelectionChangeCommand { get => selectionChangeCommand ?? (selectionChangeCommand = new RelayCommand(() => SelectionChangeAction())); }
        private void SelectionChangeAction()
        {
            if (IsSelected)
            {
                DeviceListRefreshAction();
                parent.Add(new XvcdViewModel(mainVM, parent));
            }
            else
            {
                parent.Remove(this);
            }
        }

        public ObservableCollection<string> DeviceList { get; } = new ObservableCollection<string>();
        private string deviceSel;
        public string DeviceSel { get => deviceSel; set { deviceSel = value; RaisePropertyChanged(() => DeviceSel); } }

        private RelayCommand deviceListRefreshCommand;
        public RelayCommand DeviceListRefreshCommand { get => deviceListRefreshCommand ?? (deviceListRefreshCommand = new RelayCommand(() => DeviceListRefreshAction())); }
        private void DeviceListRefreshAction()
        {
            var lstsel = DeviceSel;

            DeviceList.Clear();

            ///TODO:
            int num = LibXvcd.ftdi_device_scan();

            for (int i = 0; i < num; i++)
            {
                byte[] sn = new byte[16];
                UInt32 id = 0;
                if (LibXvcd.ftdi_device_info(i, null, ref id, sn, null) == 0)
                {
                    string info = i + ":" + Encoding.ASCII.GetString(sn).Split(new string[] { "\0" }, StringSplitOptions.None)[0];
                    DeviceList.Add(info);
                }
            }

            if (DeviceList.Contains(lstsel))
            {
                DeviceSel = lstsel;
            }
            else if (string.IsNullOrEmpty(lstsel) && (DeviceList.Count > 0))
            {
                DeviceSel = DeviceList[0];
            }
        }

        private int xvcdPort = 2542;
        public int XvcdPort { get => xvcdPort; set { xvcdPort = value; RaisePropertyChanged(() => XvcdPort); } }

        public List<double> TckFreqList { get; } = new List<double> { 0, 15e6, 10e6, 5e6, 2e6, 1e6, 500e3 };
        private double tckFreqSel = 0;
        public double TckFreqSel { get => tckFreqSel; set { tckFreqSel = value; RaisePropertyChanged(() => TckFreqSel); } }


        private bool isEnable;
        public bool IsEnable { get => isEnable; set { isEnable = value; RaisePropertyChanged(() => IsEnable); } }

        private bool isBusy;
        public bool IsBusy { get => isBusy; set { isBusy = value; RaisePropertyChanged(() => IsBusy); } }

        private bool isStarted;
        public bool IsStarted { get => isStarted; set { isStarted = value; RaisePropertyChanged(() => IsStarted); } }

        private RelayCommand runCommand;
        public RelayCommand RunCommand { get => runCommand ?? (runCommand = new RelayCommand(() => RunAction())); }

        private IntPtr xvcdHandle = IntPtr.Zero;
        private Thread infoThread;
        public void RunAction()
        {
            IsBusy = true;
            string msg = "";

            if (!IsStarted)
            {
                if (string.IsNullOrEmpty(DeviceSel))
                {
                    msg = $"XVCD-{XvcdPort} 未指定JTAG线缆";
                    goto DONE;
                }

                int index = Convert.ToInt32(DeviceSel.Split(new string[] { ":" }, StringSplitOptions.None)[0]);
                string targ_sn = DeviceSel.Split(new string[] { ":" }, StringSplitOptions.None)[1];

                int num = LibXvcd.ftdi_device_scan();
                if (index >= num)
                {
                    msg = $"XVCD-{XvcdPort} 找不到指定的线缆 ({num})";
                    goto DONE;
                }

                byte[] buf = new byte[32];
                UInt32 id = 0;
                if (LibXvcd.ftdi_device_info(index, null, ref id, buf, null) < 0)
                {
                    msg = $"XVCD-{XvcdPort} 指定的线缆已被占用";
                    goto DONE;
                }

                string sn = Encoding.ASCII.GetString(buf).Split(new string[] { "\0" }, StringSplitOptions.None)[0];
                if (sn != targ_sn)
                {
                    msg = $"XVCD-{XvcdPort} 线缆串号匹配失败({sn})";
                    goto DONE;
                }

                xvcdHandle = LibXvcd.xvcd_start(index, XvcdPort, 8192, TckFreqSel);
                if (xvcdHandle == IntPtr.Zero)
                {
                    msg = $"XVCD-{XvcdPort} 代理启动失败";
                    goto DONE;
                }

                infoThread = new Thread(vInfoThread);
                infoThread.IsBackground = true;
                infoThread.Start();

                IsStarted = true;
            }
            else
            {
                infoThread.Abort();
                LibXvcd.xvcd_stop(xvcdHandle);
                IsStarted = false;
                IsConnected = false;
            }

        DONE:
            if (!string.IsNullOrEmpty(msg))
            {
                mainVM.Enqueue(msg);
            }
            IsBusy = false;
        }

        private void vInfoThread()
        {
            try
            {
                while (true)
                {
                    byte[] ip = new byte[32];
                    int port = 0;
                    double freq = 0;
                    IsConnected = LibXvcd.xvcd_connect_info(xvcdHandle, ip, ref port, ref freq);
                    if (IsConnected)
                    {
                        ComIpInfo = $"{Encoding.ASCII.GetString(ip)}:{port}";
                        ActualTckFreq = freq;
                    }

                    Thread.Sleep(500);
                }
            }
            catch { }
        }


        private bool isConnected;
        public bool IsConnected { get => isConnected; set { isConnected = value; RaisePropertyChanged(() => IsConnected); } }

        private string comIpInfo;
        public string ComIpInfo { get => comIpInfo; set { comIpInfo = value; RaisePropertyChanged(() => ComIpInfo); } }

        private double actualTckFreq;
        public double ActualTckFreq { get => actualTckFreq; set { actualTckFreq = value; RaisePropertyChanged(() => ActualTckFreq); } }

        public XvcdViewModel(MainViewModel _mainVM, ObservableCollection<XvcdViewModel> _parent)
        {
            mainVM = _mainVM;
            parent = _parent;
        }

        public XvcdViewModel(MainViewModel _mainVM, ObservableCollection<XvcdViewModel> _parent, AppConfig config)
        {
            mainVM = _mainVM;
            parent = _parent;

            IsSelected = true;
            DeviceSel = config.JtagCable;
            DeviceList.Add(DeviceSel);
            XvcdPort = config.Port;
            TckFreqSel = config.TckFreq;
            IsEnable = config.Enable;
        }
    }

    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        /// <summary>
        /// is busy
        /// </summary>
        private bool isBusy;
        public bool IsBusy { get => isBusy; set { isBusy = value; RaisePropertyChanged(() => IsBusy); } }

        public SnackbarMessageQueue SnackbarMq { get; } = new SnackbarMessageQueue(TimeSpan.FromSeconds(2));
        public void Enqueue(string msg)
        {
            if (!string.IsNullOrEmpty(msg))
            {
                SnackbarMq.Enqueue(msg);
            }
        }

        /// <summary>
        /// about command
        /// </summary>
        private RelayCommand aboutCommand;
        public RelayCommand AboutCommand { get => aboutCommand ?? (aboutCommand = new RelayCommand(() => AboutActionAsync())); }
        private async void AboutActionAsync()
        {
            AboutDialog view = new AboutDialog();
            view.DataContext = this;

            _ = await DialogHost.Show(view, "RootDialog");
        }

        /// <summary>
        /// run command
        /// </summary>
        private RelayCommand runCommand;
        public RelayCommand RunCommand { get => runCommand ?? (runCommand = new RelayCommand(() => RunAction())); }
        private void RunAction()
        {
            IsBusy = true;

            Task.Run(() =>
            {
                foreach (var node in XvcdVM)
                {
                    if (!node.IsSelected || !node.IsEnable || node.IsStarted)
                    {
                        continue;
                    }

                    node.RunAction();
                }

                IsBusy = false;
            });
        }

        /// <summary>
        /// stop command
        /// </summary>
        private RelayCommand stopCommand;
        public RelayCommand StopCommand { get => stopCommand ?? (stopCommand = new RelayCommand(() => StopAction())); }
        private void StopAction()
        {
            IsBusy = true;

            Task.Run(() =>
            {
                foreach (var node in XvcdVM)
                {
                    if (!node.IsSelected || !node.IsEnable || !node.IsStarted)
                    {
                        continue;
                    }

                    node.RunAction();
                }

                IsBusy = false;
            });
        }

        public ObservableCollection<XvcdViewModel> XvcdVM { get; } = new ObservableCollection<XvcdViewModel>();

        public string MainInfoString { get; set; }
        public List<string> SubInfoStrings { get; } = new List<string>();
        public string VersionInfoString { get; set; }

        private void GetInfos()
        {
            var prg = Assembly.GetExecutingAssembly().GetName();
            MainInfoString = $"{prg.Name}, v{prg.Version.ToString(3)}";

            byte[] buf = new byte[64];
            if (LibXvcd.version(buf) == 0)
            {
                string info = $"libxvcd ({Encoding.ASCII.GetString(buf).Replace("\0", string.Empty)})";
                SubInfoStrings.Add(info);
            }

            try
            {
                Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"{prg.Name}.version.txt");

                buf = new byte[stream.Length];
                stream.Read(buf, 0, buf.Length);

                VersionInfoString = Encoding.UTF8.GetString(buf).Replace("\0", string.Empty);
            }
            catch
            {
            }
        }

        private static readonly string settingfile = System.AppDomain.CurrentDomain.BaseDirectory + "config.json";
        public void ImportSetting(string setting_file)
        {
            try
            {
                var file = new FileInfo(setting_file);
                file.Attributes &= ~FileAttributes.Hidden;

                var fs = new StreamReader(setting_file);

                char[] buf = new char[1024 * 16];
                int len = fs.Read(buf, 0, buf.Length);
                fs.Close();

                file.Attributes |= FileAttributes.Hidden;

                if (len <= 0)
                {
                    return;
                }

                string sin = new string(buf, 0, len);

                var configs = JsonConvert.DeserializeObject<List<AppConfig>>(sin);

                foreach (var c in configs)
                {
                    XvcdVM.Add(new XvcdViewModel(this, XvcdVM, c));
                }
            }
            catch
            {
            }
        }

        public void ExportSetting(string setting_file)
        {
            var configs = new List<AppConfig>();

            foreach (var node in XvcdVM)
            {
                if (!node.IsSelected)
                {
                    continue;
                }

                var c = new AppConfig
                {
                    JtagCable = node.DeviceSel,
                    Port = node.XvcdPort,
                    TckFreq = node.TckFreqSel,
                    Enable = node.IsEnable
                };

                configs.Add(c);
            }

            string sout = JsonConvert.SerializeObject(configs, Formatting.Indented);

            if (File.Exists(setting_file))
            {
                var sfile = new FileInfo(setting_file);
                sfile.Attributes &= ~FileAttributes.Hidden;
            }

            StreamWriter fs;
            try
            {
                fs = new StreamWriter(setting_file, false);
            }
            catch
            {
                return;
            }

            fs.Write(sout);
            fs.Flush();
            fs.Close();

            var hfile = new FileInfo(setting_file);
            hfile.Attributes |= FileAttributes.Hidden;
        }

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            GetInfos();

            ImportSetting(settingfile);
            
            XvcdVM.Add(new XvcdViewModel(this, XvcdVM));
        }
        ~MainViewModel()
        {
            ExportSetting(settingfile);
        }
    }
}