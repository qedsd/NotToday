using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NotToday.Models;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Drawing;
using CommunityToolkit.Mvvm.Input;
using NotToday.Wins;
using System.Windows;
using System.Windows.Controls;
using OpenCvSharp.Dnn;
using NotToday.Services;
using NotToday.Helpers;

namespace NotToday.ViewModels
{
    public class LocalIntelViewModel : ObservableObject
    {
        private ObservableCollection<ProcessInfo> processes = new ObservableCollection<ProcessInfo>();
        public ObservableCollection<ProcessInfo> Processes
        {
            get => processes;
            set => SetProperty(ref processes, value);
        }

        private ProcessInfo selectedProcess;
        public ProcessInfo SelectedProcess
        {
            get => selectedProcess;
            set
            {
                if (SetProperty(ref selectedProcess, value))
                {
                    LocalIntelItems = GetLocalIntelItems(value);
                    SelectedLocalIntelItem = LocalIntelItems?.FirstOrDefault();
                }
            }
        }

        public LocalIntelSetting Setting { get; set; }

        private ObservableCollection<LocalIntelItem> _localIntelItems;
        public ObservableCollection<LocalIntelItem> LocalIntelItems
        {
            get => _localIntelItems;
            set => SetProperty(ref _localIntelItems, value);
        }

        private LocalIntelItem _selectedLocalIntelItem;
        public LocalIntelItem SelectedLocalIntelItem
        {
            get => _selectedLocalIntelItem;
            set
            {
                if (SetProperty(ref _selectedLocalIntelItem, value))
                {
                    if (value != null)
                    {
                        
                    }
                }
            }
        }

        private LocalIntelColor _selectedLocalIntelColor;
        public LocalIntelColor SelectedLocalIntelColor
        {
            get => _selectedLocalIntelColor;
            set
            {
                SetProperty(ref _selectedLocalIntelColor, value);
            }
        }

        private bool running;
        public bool Running
        {
            get => running;
            set => SetProperty(ref running, value);
        }

        private System.Windows.Media.Imaging.WriteableBitmap _img;
        public System.Windows.Media.Imaging.WriteableBitmap Img
        {
            get => _img;
            set => SetProperty(ref _img, value);
        }

        private readonly Dictionary<string, ObservableCollection<LocalIntelItem>> _runningDic = new Dictionary<string, ObservableCollection<LocalIntelItem>>();
        private static readonly string SettingFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", "LocalIntelSetting.json");
        public LocalIntelViewModel()
        {
            Init();
        }
        private void Init()
        {
            if (System.IO.File.Exists(SettingFilePath))
            {
                string json = System.IO.File.ReadAllText(SettingFilePath);
                if (string.IsNullOrEmpty(json))
                {
                    Setting = new LocalIntelSetting();
                }
                else
                {
                    Setting = JsonConvert.DeserializeObject<LocalIntelSetting>(json);
                }
            }
            else
            {
                Setting = new LocalIntelSetting();
            }
            GetProcesses();
        }
        private async void GetProcesses()
        {
            var allProcesses = Process.GetProcesses();
            if (allProcesses != null && allProcesses.Any())
            {
                List<ProcessInfo> targetProcesses = new List<ProcessInfo>();
                await Task.Run(() =>
                {
                    List<Process> processes;
                    if (!string.IsNullOrEmpty(Setting.ProcessName))
                    {
                        processes = allProcesses.Where(p => p.ProcessName == Setting.ProcessName).ToList();
                    }
                    else
                    {
                        processes = allProcesses.ToList();
                    }
                    foreach (var process in processes)
                    {
                        if (process.MainWindowHandle != IntPtr.Zero)
                        {
                            ProcessInfo processInfo = new ProcessInfo()
                            {
                                MainWindowHandle = process.MainWindowHandle,
                                ProcessName = process.ProcessName,
                                WindowTitle = process.MainWindowTitle,
                                Process = process
                            };
                            targetProcesses.Add(processInfo);
                        }
                    }
                });
                //获取所有目标进程

                if (targetProcesses != null && targetProcesses.Any())
                {
                    List<ProcessInfo> targetProcessesForShow;//最终要显示的目标进程
                    if (Processes != null && Processes.Any())//当前列表不为空，需要保留运行中的进程
                    {
                        targetProcessesForShow = new List<ProcessInfo>();
                        var runnings = Processes.Where(p => p.Running).ToList();
                        if (runnings != null && runnings.Any())//存在运行中，
                        {
                            //将运行中从新增里排除
                            foreach (var running in runnings)
                            {
                                var item = targetProcesses.FirstOrDefault(p => p.MainWindowHandle == running.MainWindowHandle);
                                if (item != null)
                                {
                                    targetProcesses.Remove(item);
                                }
                                targetProcessesForShow.Add(running);
                            }
                        }
                        if (targetProcesses.Any())//添加未运行中的
                        {
                            foreach (var item in targetProcesses)
                            {
                                targetProcessesForShow.Add(item);
                            }
                        }
                    }
                    else//当前列表为空，保存显示全部
                    {
                        targetProcessesForShow = targetProcesses;
                    }
                    Processes.Clear();
                    foreach (var item in targetProcessesForShow)
                    {
                        Processes.Add(item);
                    }
                }
            }
            else
            {
                Processes.Clear();
            }
        }
        private ObservableCollection<LocalIntelItem> GetLocalIntelItems(ProcessInfo processInfo)
        {
            if (processInfo == null)
            {
                return null;
            }
            if (processInfo.LocalIntelItems == null)
            {
                ObservableCollection<LocalIntelItem> items = new ObservableCollection<LocalIntelItem>();
                var targets = Setting.ItemConfigs.Where(p => p.WindowTitle == processInfo.WindowTitle);
                if (targets == null || !targets.Any())
                {
                    var newConfig = CreateDefaultConfig(processInfo.WindowTitle, processInfo.MainWindowHandle, processInfo.GUID);
                    newConfig.ConfigName += "1";
                    targets = new List<LocalIntelItemConfig>() { newConfig };
                    Setting.ItemConfigs.Add(newConfig);
                }
                foreach (var item in targets)
                {
                    item.HWnd = processInfo.MainWindowHandle;
                    items.Add(new LocalIntelItem(item));
                }
                processInfo.LocalIntelItems = items;
            }
            return processInfo.LocalIntelItems;
        }
        private LocalIntelItemConfig CreateDefaultConfig(string windowTitle,nint hwnd, string processGUID)
        {
            return new LocalIntelItemConfig()
            {
                WindowTitle = windowTitle,
                HWnd = hwnd,
                ProcessGUID = processGUID,
                ConfigName = "配置",
                Colors = new ObservableCollection<LocalIntelColor>()
                            {
                                new LocalIntelColor()
                                {
                                    Name = "红",
                                    Color = System.Windows.Media.Color.FromRgb(111, 4, 4),
                                },
                                new LocalIntelColor()
                                {
                                    Name = "白",
                                    Color = System.Windows.Media.Color.FromRgb(110,110,110),
                                },
                            }
            };
        }

        public ICommand StartCommand => new RelayCommand(() =>
        {
            Start(SelectedProcess);
            Services.LocalIntelScreenshotService.Current.Start();
            Save();
        });

        public ICommand StopCommand => new RelayCommand(() =>
        {
            Stop(SelectedProcess);
        });

        public ICommand StartAllCommand => new RelayCommand(() =>
        {
            foreach (var p in Processes.Where(p => !p.Running))
            {
                GetLocalIntelItems(p);
                Start(p);
            }
            Services.LocalIntelScreenshotService.Current.Start();
            Save();
        });

        public ICommand StopAllCommand => new RelayCommand(() =>
        {
            foreach (var p in Processes.Where(p=>p.Running))
            {
                Stop(p);
            }
        });
        public ICommand SettingCommand => new RelayCommand(() =>
        {
            SettingWindow settingWindow = new SettingWindow(Setting);
            if(settingWindow.ShowDialog() == true)
            {
                Save();
            }
        });
        public ICommand AddColorCommand => new RelayCommand(() =>
        {
            SelectedLocalIntelItem.Config.Colors.Add(new LocalIntelColor()
            {
                Name = "自定义",
                Color = System.Windows.Media.Color.FromRgb(111, 4, 4),
            });
        });
        public ICommand DeleteColorCommand => new RelayCommand(() =>
        {
            if(SelectedLocalIntelColor != null)
            {
                SelectedLocalIntelItem.Config.Colors.Remove(SelectedLocalIntelColor);
            }
        });
        private bool Start(ProcessInfo processInfo)
        {
            try
            {
                bool running = false;
                foreach (var item in processInfo.LocalIntelItems)
                {
                    if(item.Start())
                    {
                        running = true;
                        item.OnScreenshotChanged += Item_OnScreenshotChanged;
                    }
                }
                if (running)
                {
                    _runningDic.Add(processInfo.GUID, processInfo.LocalIntelItems);
                    processInfo.Running = true;
                    Running = true;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        private void Item_OnScreenshotChanged(LocalIntelItem sender, Bitmap img)
        {
            if(SelectedLocalIntelItem == sender)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Img = ImageHelper.ConvertBitmapToWriteableBitmapDirect(img);
                });
            }
        }

        private void Stop(ProcessInfo processInfo)
        {
            try
            {
                foreach (var item in processInfo.LocalIntelItems)
                {
                    item.Stop();
                }
                processInfo.Running= false;
                _runningDic.Remove(processInfo.GUID);
                Running = _runningDic.Count > 0;
                if (!Running)
                {
                    LocalIntelNotifyService.Current.ClearWindow();
                    LocalIntelNotifyService.Current.HideWindow();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Save()
        {
            string dir = System.IO.Path.GetDirectoryName(SettingFilePath);
            if (!System.IO.Directory.Exists(dir))
            {
                System.IO.Directory.CreateDirectory(dir);
            }
            string json = JsonConvert.SerializeObject(Setting);
            System.IO.File.WriteAllText(SettingFilePath, json);
        }
        public ICommand RefreshProcessListCommand => new RelayCommand(() =>
        {
            GetProcesses();
        });
        public ICommand AddStandingCommand => new RelayCommand(() =>
        {
            SelectedLocalIntelItem.Config.Colors.Add(new LocalIntelColor()
            {
                Name = "红",
                Color = System.Windows.Media.Color.FromRgb(111, 4, 4)
            });
        });
        public ICommand SelectRegionCommand => new RelayCommand<LocalIntelItem>((item) =>
        {
            SelecteCaptureAreaWindow window = new SelecteCaptureAreaWindow(item.Config.HWnd);
            if(window.ShowDialog() == true)
            {
                if(item.Config.IntelRect == null)
                {
                    item.Config.IntelRect = new LocalIntelRect();
                }
                item.Config.IntelRect.X = (int)window.CroppedRegion.X;
                item.Config.IntelRect.Y = (int)window.CroppedRegion.Y;
                item.Config.IntelRect.Width = (int)window.CroppedRegion.Width;
                item.Config.IntelRect.Height = (int)window.CroppedRegion.Height;
            }
        });
        public ICommand AddIntelItemCommand => new RelayCommand(() =>
        {
            var newConfig = CreateDefaultConfig(SelectedProcess.WindowTitle, SelectedProcess.MainWindowHandle, SelectedProcess.GUID);
            newConfig.ConfigName += $"{LocalIntelItems.Count + 1}";
            Setting.ItemConfigs.Add(newConfig);
            LocalIntelItems.Add(new LocalIntelItem(newConfig));
            SelectedLocalIntelItem = LocalIntelItems.LastOrDefault();
        });
        public ICommand DeleteIntelItemCommand => new RelayCommand(() =>
        {
            if(SelectedLocalIntelItem != null)
            {
                Setting.ItemConfigs.Remove(SelectedLocalIntelItem.Config);
                LocalIntelItems.Remove(SelectedLocalIntelItem);
                SelectedLocalIntelItem = LocalIntelItems.LastOrDefault();
            }
        });
    }
}
