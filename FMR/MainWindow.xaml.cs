using FMR.Core;
using FMR.Core.UI;
using Microsoft.Win32;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Path = System.IO.Path;
using System.Drawing;
using System.Diagnostics;

namespace FMR
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static readonly Version CurrentVersion = new(1, 0, 0, 0);

        internal IRenderModule[] modules;
        internal Assembly[] assemblies;
        internal string fullMidiPath;
        internal string fullExportPath;
        internal bool loading = false;
        internal Config config;

        internal IVideoExport[] availableExport;
        internal IMidiLoader[] availableLoaders;
        internal IMidiRenderer[] availableRenderers;

        internal IRenderModule currentModule;
        internal IVideoExport currentExport;
        internal IMidiRenderer currentRenderer;
        internal IMidiLoader currentLoader;

        internal ColorManager colorManager;
        internal RenderHost host;
        internal Task renderTask;
        internal TaskState lastState;

        public MainWindow()
        {
            InitializeComponent();
            
            #region About Tab
            // 初始化版本信息
            version.Content = CurrentVersion;
            apiVersion.Content = APIVersion.CurrentAPIVersion;
            osVersion.Content = Environment.OSVersion.Version;
            fullExportPath = "output.mp4";
            processorCount.Content = Environment.ProcessorCount;
            is64BitProcess.Content = GetLocalizedString(Environment.Is64BitProcess);
#if DEBUG
            isDebugVer.Content = GetLocalizedString(true);
#endif
            machineName.Content = Environment.MachineName;
            #endregion

            fullExportPath = "output.mp4";
            try
            {
                config = new Config();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"配置文件异常, 将清除配置内容:\n{ex.Message}", "错误.");
                if (File.Exists(Config.ConfigurationFilePath))
                {
                    File.Delete(Config.ConfigurationFilePath);
                }
                try
                {
                    config = new Config();
                }
                catch (Exception exInternal)
                {
                    MessageBox.Show($"配置文件异常, 请删除配置文件后重启程序.\n{exInternal.Message}", "错误.");
                    Environment.Exit(0);
                }
            }
            colorManager = new ColorManager();
            
            lastState = TaskState.Stopped;

            LoadModules();
            LoadExport();
            PushModules();
            PushExports();

            SelectFirst(moduleFiles);
            SelectFirst(moduleVideoExports);

            Logging.LogReceiver += (data) =>
            {
                while (logData.Count >= 300)
                {
                    logData.Dequeue();
                }
                logData.Enqueue(data);
                Dispatcher.Invoke(UpdateLogBox);
            };

            Core.Scripting.Initializer.Init();
        }

        internal void LoadModules()
        {
            if (!Directory.Exists("Modules"))
            {
                Directory.CreateDirectory("Modules");
            }
            string[] modulePaths = Directory.GetFiles("Modules").Where((s) => s.EndsWith(".dll")).ToArray();
            List<IRenderModule> modules = [];
            List<Assembly> validAssemblies = [];
            foreach (string modulePath in modulePaths)
            {
                Assembly asm;
                try
                {
                    asm = AssemblyHelper.Load(modulePath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"加载{modulePath}时发生错误:\n{ex.Message}.", "错误.");
                    continue;
                }
                IRenderModule[] modulesFound = AssemblyHelper.GetImplementationsOf<IRenderModule>(asm);
                if (modulesFound.Length == 0)
                {
                    MessageBox.Show($"在{modulePath}中没有发现自述文件.\n此模块会被忽略", "加载错误.");
                    continue;
                }
                if (modulesFound.Length > 1)
                {
                    MessageBox.Show($"模块{modulePath}的实现可能不规范:\n包含了超过一个自述对象.", "加载警告.");
                }
                foreach (IRenderModule module in modulesFound)
                {
                    if (module.APIVersion.Major != APIVersion.CurrentAPIVersion.Major)
                    {
                        MessageBox.Show($"模块{module.Name}的API版本与当前主程序的API版本可能不兼容.", "加载警告.");
                    }
                    modules.Add(module);
                    validAssemblies.Add(asm); // 对应的程序集, 选择后再取出所有类型.
                }
            }
            this.modules = [.. modules];
            assemblies = [.. validAssemblies];
        }

        internal void LoadExport()
        {
            if (!Directory.Exists("Export"))
            {
                Directory.CreateDirectory("Export");
            }
            string[] exportSettingsPath = Directory.GetFiles("Export").Where((s) => s.EndsWith(".dll")).ToArray();
            List<IVideoExport> exportImplements = [];
            foreach (string implementPath in exportSettingsPath)
            {
                Assembly asm;
                try
                {
                    asm = AssemblyHelper.Load(implementPath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"加载输出设置{implementPath}时出错:\n{ex.Message}.", "错误.");
                    continue;
                }
                IVideoExport[] implementsFound = AssemblyHelper.GetImplementationsOf<IVideoExport>(asm);
                exportImplements.AddRange(implementsFound);
            }
            availableExport = [.. exportImplements];
        }

        internal void PushModules()
        {
            moduleFiles.Items.Clear();
            foreach (IRenderModule module in modules)
            {
                moduleFiles.Items.Add(module.Name);
            }
        }

        internal void PushExports()
        {
            moduleVideoExports.Items.Clear();
            foreach (IVideoExport export in availableExport)
            {
                moduleVideoExports.Items.Add(export.Name);
            }
        }

        internal static void PushToListBox(ListBox target, ICollection<INamedObject> namedObjects)
        {
            target.Items.Clear();
            foreach (INamedObject obj in namedObjects)
            {
                target.Items.Add(obj.Name);
            }
        }

        internal static void ApplyControls(Grid target, Control source)
        {
            if (source.Parent is not null)
            {
                (source.Parent as Panel)?.Children.Clear();
            }
            target.Children.Clear();
            target.Children.Add(source);
            source.HorizontalAlignment = HorizontalAlignment.Left;
        }

        private void browseFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new()
            {
                Filter = "Midi (*.mid)|*.mid",
                InitialDirectory = config.Get(Config.MidiPath) as string
            };
            bool? result = dialog.ShowDialog();
            if (result is null)
            {
                return;
            }
            if ((bool)result)
            {
                fullMidiPath = Path.GetFullPath(dialog.FileName);
                midiPath.Text = Path.GetFileName(fullMidiPath);

                config.Update(Config.MidiPath, Path.GetDirectoryName(fullMidiPath));
            }
        }

        private void selectOutput_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new()
            {
                Filter = "视频 (*.mp4, *.avi, *.mov)|*.mp4;*.avi;*.mov|所有文件|*.*",
                OverwritePrompt = true,
                InitialDirectory = config.Get(Config.OutputPath) as string
            };
            bool? result = dialog.ShowDialog();
            if (result is null)
            {
                fullExportPath = outputPath.Text;
                return;
            }
            if ((bool)result)
            {
                fullExportPath = dialog.FileName;
                outputPath.Text = Path.GetFileName(fullExportPath);

                config.Update(Config.OutputPath, Path.GetDirectoryName(fullExportPath));
            }
        }

        private void moduleVideoExports_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is null)
            {
                return;
            }
            if (sender is ListBox box)
            {
                if (box.Items.Count == 0)
                {
                    return;
                }
                int index = box.SelectedIndex;
                currentExport = availableExport[index];
                ApplyControls(exportSettings, currentExport.ControlPanel);
            }
        }

        private void numberBox_ValueChanged(object sender, RoutedPropertyChangedEventArgs<decimal> args)
        {
            if (sender is null)
            {
                return;
            }
            NumberBox? box = sender as NumberBox;
            if (args.NewValue - Math.Floor(args.NewValue) != 0)
            {
                if (box is not null)
                {
                    box.Value = Math.Min(Math.Max(Math.Floor(args.NewValue), box.Minimum), box.Maximum);
                }
            }
        }

        internal static void SelectFirst(ListBox box)
        {
            if (box.Items.Count > 0)
            {
                box.SelectedIndex = 0;
            }
        }

        private void reloadExport_Click(object sender, RoutedEventArgs e)
        {
            moduleVideoExports.Items.Clear();
            LoadExport();
            PushExports();
            SelectFirst(moduleVideoExports);
        }

        private void moduleFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is null)
            {
                return;
            }
            if (sender is ListBox box)
            {
                if (box.SelectedIndex == -1)
                {
                    return;
                }
                int index = box.SelectedIndex;
                currentModule = modules[index];

                Assembly assembly = assemblies[index];
                availableLoaders = AssemblyHelper.GetImplementationsOf<IMidiLoader>(assembly);
                availableRenderers = AssemblyHelper.GetImplementationsOf<IMidiRenderer>(assembly);

                PushToListBox(moduleMidiLoaders, availableLoaders);
                PushToListBox(moduleRenderers, availableRenderers);
                SelectFirst(moduleMidiLoaders);
                SelectFirst(moduleRenderers);
            }
        }

        private void moduleMidiLoaders_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is null)
            {
                return;
            }
            if (sender is ListBox box)
            {
                if (box.SelectedIndex == -1)
                {
                    return;
                }
                int index = box.SelectedIndex;
                currentLoader = availableLoaders[index];

                midiLoaderName.Content = currentLoader.Name;

                // 获取翻译
                ResourceDictionary localization = Resources.MergedDictionaries[0].MergedDictionaries[0];
                string yes = (string)localization["yes"];
                string no = (string)localization["no"];

                supportTickBased.Content = currentLoader.TickBased ? yes : no;
                supportTimeBased.Content = currentLoader.TimeBased ? yes : no;
                supportStreamLoading.Content = currentLoader.CanParseOnRender ? yes : no;
            }
        }

        private void moduleRenderers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is null)
            {
                return;
            }
            if (sender is ListBox box)
            {
                if (box.SelectedIndex == -1)
                {
                    return;
                }
                int index = box.SelectedIndex;
                currentRenderer = availableRenderers[index];

                StringBuilder sb = new();
                sb.AppendLine("模块概述: ").AppendLine(currentModule.Description).AppendLine();
                sb.AppendLine("渲染器概述: ").AppendLine(currentRenderer.Description);

                moduleDescription.Text = sb.ToString();
                ApplyControls(renderSettings, currentRenderer.ControlPanel);
            }
        }

        private void unloadMidi_Click(object sender, RoutedEventArgs e)
        {
            if (sender is null)
            {
                return;
            }
            currentLoader?.Reset();
            midiDuration.Content = "??:??:??";
            midiNoteCount.Content = "???";
            midiDivision.Content = "???";
            midiTrackCount.Content = "???";

            ResourceDictionary localization = Resources.MergedDictionaries[0].MergedDictionaries[0];
            string no = (string)localization["no"];
            midiPreloaded.Content = no;
            midiLoaded.Content = no;

            Resources["midiPreloaded"] = false;
            GC.Collect();
        }

        private void loadMidi_Click(object sender, RoutedEventArgs e)
        {
            if (sender is null)
            {
                return;
            }
            if (loading)
            {
                MessageBox.Show("Midi正在加载.", "信息");
                return;
            }
            if (currentLoader is null)
            {
                MessageBox.Show("没有合适的Midi加载器.", "错误");
                return;
            }
            if (currentLoader.PreLoaded)
            {
                MessageBox.Show("该Midi已经被加载过.", "错误");
                return;
            }
            if (!File.Exists(fullMidiPath))
            {
                MessageBox.Show("Midi 文件不存在.", "错误");
                return;
            }
            currentLoader.OpenFile(fullMidiPath);
            // 获取翻译
            ResourceDictionary localization = Resources.MergedDictionaries[0].MergedDictionaries[0];
            string yes = (string)localization["yes"];
            string no = (string)localization["no"];
            string midiLoading = (string)localization["midiLoading"];
            string rawLoadMidiText = (string)localization["loadMidi"];

            Task t = new(() =>
            {
                try
                {
                    currentLoader.PreLoad();
                    Logging.Write(LogLevel.Info, "MIDI successfully loaded.");
                    TimeSpan duration = currentLoader.Duration;
                    Dispatcher.Invoke(() =>
                    {
                        midiDuration.Content = $"{(int)duration.TotalMinutes:00}:{duration.Seconds:00}.{duration.Milliseconds:000}";
                        midiNoteCount.Content = currentLoader.NoteCount;
                        midiDivision.Content = currentLoader.Division;
                        midiTrackCount.Content = currentLoader.TrackCount;
                        midiPreloaded.Content = yes;
                        midiLoaded.Content = currentLoader.Parsed ? yes : no;
                        loadMidi.Content = rawLoadMidiText;
                        Resources["midiPreloaded"] = true;
                        loading = false;
                    });
                }
                catch (Exception ex)
                {
                    Logging.Write(LogLevel.Error, $"{ex.Message}\n{ex.StackTrace}");
                }
            });
            
            loading = true;
            loadMidi.Content = midiLoading;
            t.Start();
        }

        private void randomizeColorOrder_Click(object sender, RoutedEventArgs e)
        {
            colorManager.Shuffle();
            MessageBox.Show("颜色顺序已随机化.", "信息");
        }

        private void useDefaultColors_Click(object sender, RoutedEventArgs e)
        {
            colorManager.Default();
            MessageBox.Show("已经切换为默认颜色.", "信息");
        }

        private void generateOutput_Click(object sender, RoutedEventArgs e)
        {
            if (sender is null)
            {
                return;
            }
            if (currentExport is null)
            {
                MessageBox.Show("输出设置为空.", "错误.");
                return;
            }
            if (currentLoader is null)
            {
                MessageBox.Show("Midi 加载为空.", "错误.");
                return;
            }
            if (currentRenderer is null)
            {
                MessageBox.Show("渲染器为空.", "错误.");
                return;
            }
            if (!currentLoader.PreLoaded)
            {
                MessageBox.Show("当前 Midi 未加载.", "错误.");
                return;
            }
            if (lastState == TaskState.Running)
            {
                MessageBox.Show("当前已经有一个渲染进程正在进行.", "错误.");
                return;
            }
            currentExport.SetExportPath(fullExportPath);
            if (currentRenderer.SupportedColorType.HasFlag(ColorType.RGBA_Int))
            {
                currentRenderer.SetColors(colorManager.Colors);
            }
            else if (currentRenderer.SupportedColorType.HasFlag(ColorType.RGBA_Float))
            {
                currentRenderer.SetColors(colorManager.Vec4Colors);
            }

            ResourceDictionary localization = Resources.MergedDictionaries[0].MergedDictionaries[0];
            string stopped = (string)localization["stopped"];
            string running = (string)localization["rendering"];
            string success = (string)localization["success"];

            try
            {
                RenderHost host = new(currentLoader, currentRenderer, currentExport, colorManager, (progress, frameSpeed, avgSpeed, state, msg) =>
                {
                    double barProgress = progress;
                    if (barProgress < 0.0)
                    {
                        barProgress = 0.0;
                    }
                    if (barProgress > 1.0)
                    {
                        barProgress = 1.0;
                    }
                    lastState = state;
                    Dispatcher.Invoke(() =>
                    {
                        progressBar.Value = barProgress;
                        percentage.Content = Math.Round(progress * 100.0, 4);
                        renderFPS.Content = Math.Round(avgSpeed, 3);
                        instantFPS.Content = Math.Round(frameSpeed, 3);
                        switch (state)
                        {
                            case TaskState.Running:
                                renderStatus.Content = running;
                                break;
                            case TaskState.Success:
                                renderStatus.Content = success;
                                break;
                            case TaskState.Error:
                                renderStatus.Content = stopped;
                                MessageBox.Show($"错误信息:\n{msg}", "捕获到错误");
                                Logging.Write(LogLevel.Error, $"捕获到错误:{msg}.");
                                break;
                            case TaskState.Stopped:
                                renderStatus.Content = stopped;
                                break;
                        }
                    });
                }, new RenderConfiguration { Width = (int)outputWidth.Value, Height = (int)outputHeight.Value, FPS = (int)fpsValue.Value });
                this.host = host;
                renderTask = host.RenderAsync();
                menuTab.SelectedIndex = 5;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"初始化 RenderHost 时发生了异常:\n{ex.Message}", "错误");
                Logging.Write(LogLevel.Error, $"{ex.Message}\n{ex.StackTrace}");
            }
        }

        private void stopRender_Click(object sender, RoutedEventArgs e)
        {
            if (sender is null)
            {
                return;
            }
            if (host is null)
            {
                MessageBox.Show("渲染未开始.", "错误.");
                return;
            }
            if (lastState != TaskState.Running)
            {
                MessageBox.Show("渲染未在进行中.\n请检查任务状态.", "错误.");
                return;
            }
            host.Stop();
        }

        private void loadColors_Click(object sender, RoutedEventArgs e)
        {
            if (sender is null)
            {
                return;
            }
            ColorType fileType = ColorType.RGBA_Int;
            if (fileColorType is not null)
            {
                int index = fileColorType.SelectedIndex;
                switch (index)
                {
                    case 0:
                        fileType = ColorType.RGBA_Int;
                        break;
                    case 1:
                        fileType = ColorType.RGBA_Float;
                        break;
                }
            }
            string colorFilePath = colorPath.Text;
            if (!File.Exists(colorFilePath))
            {
                MessageBox.Show("颜色文件不存在.", "错误");
                return;
            }
            if (colorFilePath.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    colorManager.ReadJson(colorFilePath, fileType);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"读取颜色时发生了异常:\n{ex.Message}", "错误");
                    return;
                }
            }
            else
            {
                try
                {
                    using Bitmap bmp = new(colorFilePath);
                    colorManager.ReadBitmap(bmp);
                    MessageBox.Show("颜色加载完成.", "信息");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"读取颜色时发生了异常:\n{ex.Message}", "错误");
                    return;
                }
            }
            colorCount.Content = colorManager.ColorCount;
        }

        internal string GetLocalizedString(string key)
        {
            ResourceDictionary localization = Resources.MergedDictionaries[0].MergedDictionaries[0];
            return (string)localization[key];
        }

        internal string GetLocalizedString(bool key)
        {
            string sKey = key ? "yes" : "no";
            return GetLocalizedString(sKey);
        }

        private void openColor_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new()
            {
                Filter = "位图 (*.bmp, *.jpg, *.png)|*.bmp;*.jpg;*.png|Json|*.json",
                InitialDirectory = config.Get(Config.ColorPath) as string
            };
            bool? result = dialog.ShowDialog();
            if (result is null)
            {
                return;
            }
            if ((bool)result)
            {
                colorPath.Text = dialog.FileName;

                config.Update(Config.ColorPath, colorPath.Text);
            }
        }

        private readonly Queue<LogData> logData = new(300);
        private void UpdateLogBox()
        {
            StringBuilder sb = new();
            foreach (LogData data in logData)
            {
                DateTime tm = data.Time;
                sb.AppendLine($"[{tm.Year}/{tm.Month:00}/{tm.Day:00} {tm.Hour:00}:{tm.Minute:00}:{tm.Second:00}][{data.Level}] {data.Message}");
            }
            logBox.Text = sb.ToString();
            logBox.Select(sb.Length, 0);
        }

        private void logBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is null)
            {
                return;
            }
            logBox.Select(logBox.Text.Length, 0);
        }

        private void logBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is null)
            {
                return;
            }
            logBox.Select(logBox.Text.Length, 0);
        }

        private void openInExplorer_Click(object sender, RoutedEventArgs e)
        {
            string? dirName = Path.GetDirectoryName(Path.GetFullPath(fullExportPath));
            dirName ??= "";
            Process.Start("explorer.exe", dirName);
        }
    }
}