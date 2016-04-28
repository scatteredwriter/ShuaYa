using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace 刷呀
{
    /// <summary>
    /// 提供特定于应用程序的行为，以补充默认的应用程序类。
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// 初始化单一实例应用程序对象。这是执行的创作代码的第一行，
        /// 已执行，逻辑上等同于 main() 或 WinMain()。
        /// </summary>

        public static string news_title { get; set; } = "";
        public static string news_channel_name { get; set; } = "";
        public static string travellists_query { get; set; } = "海南";
        public static Frame Changeable_Frame { get; set; }
        public static bool Go_To_Frame { get; set; } = false;

        public static int f = 0;//标识是否为第一次导航到刷呀新闻页
        public static int k = 0;//标识是否为第一次导航到刷呀游记页
        public static int g = 0;//标识是否为第一次导航到刷呀笑话页
        public static int h = 0;//标识是否为第一次导航到刷呀天气页

        public static int allowed_get_position = 0;//默认允许获取设备设置

        public static ObservableCollection<News> newslists = new ObservableCollection<News>();//建立News类型的集合
        public static ObservableCollection<News_Channel> news_channel_list = new ObservableCollection<News_Channel>();//建立News_Channel类型的集合

        public static ObservableCollection<Travellists> travellists_list = new ObservableCollection<Travellists>();//建立Travellists类型的集合
        public static ObservableCollection<Cities_Lists> cities_lists_list = new ObservableCollection<Cities_Lists>();//建立Cities_Lists类型的集合

        public static ObservableCollection<Jokes> jokeslists = new ObservableCollection<Jokes>();//建立Jokes类型的集合
        public static ObservableCollection<string> jokes_choice_lists = new ObservableCollection<string>();//建立用来显示笑话类型的集合

        public static int geoposition_allow = 0;//是否允许获取位置权限

        private ApplicationDataContainer appdata = ApplicationData.Current.LocalSettings;//获取应用本地设置
        public static int open_weather = 0;//默认使用刷呀天气

        #region 刷呀天气的相关属性
        public static string _city { get; set; }
        public static string _date { get; set; }
        public static string _time { get; set; }
        public static string _weather { get; set; }
        public static string _temp { get; set; }
        public static string _l_tmp { get; set; }
        public static string _h_tmp { get; set; }
        #endregion

        public App()
        {
            Microsoft.ApplicationInsights.WindowsAppInitializer.InitializeAsync(
                Microsoft.ApplicationInsights.WindowsCollectors.Metadata |
                Microsoft.ApplicationInsights.WindowsCollectors.Session);
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// 在应用程序由最终用户正常启动时进行调用。
        /// 将在启动应用程序以打开特定文件等情况下使用。
        /// </summary>
        /// <param name="e">有关启动请求和过程的详细信息。</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG

            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar")) //手机状态栏
            {
                StatusBar statusBar = StatusBar.GetForCurrentView();
                statusBar.BackgroundOpacity = 1;
                statusBar.BackgroundColor = Color.FromArgb(100, 58, 177, 181);//标题栏背景色
                statusBar.ForegroundColor = Colors.White;//标题栏前景色
            }
            else //PC状态栏
            {
                var titleBar = ApplicationView.GetForCurrentView().TitleBar;
                titleBar.BackgroundColor = Color.FromArgb(100, 43, 160, 164);//标题栏背景色
                titleBar.ForegroundColor = Colors.White;//标题栏前景色
                titleBar.ButtonHoverBackgroundColor = Color.FromArgb(100, 43, 160, 164);//鼠标悬浮在三键上时的颜色
                titleBar.ButtonBackgroundColor = Color.FromArgb(100, 43, 160, 164);//三键背景色
                titleBar.ButtonForegroundColor = Colors.White;
            }

            if (appdata.Values.ContainsKey("Open_Weather"))//判断是否设置了刷呀天气的使用或关闭
            {
                open_weather = (int)appdata.Values["Open_Weather"];
            }

            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif
            Frame rootFrame = Window.Current.Content as Frame;

            // 不要在窗口已包含内容时重复应用程序初始化，
            // 只需确保窗口处于活动状态
            if (rootFrame == null)
            {
                // 创建要充当导航上下文的框架，并导航到第一页
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: 从之前挂起的应用程序加载状态
                }

                // 将框架放在当前窗口中
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // 当导航堆栈尚未还原时，导航到第一页，
                    // 并通过将所需信息作为导航参数传入来配置
                    // 参数
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                // 确保当前窗口处于活动状态
                Window.Current.Activate();
            }
        }

        /// <summary>
        /// 导航到特定页失败时调用
        /// </summary>
        ///<param name="sender">导航失败的框架</param>
        ///<param name="e">有关导航失败的详细信息</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// 在将要挂起应用程序执行时调用。  在不知道应用程序
        /// 无需知道应用程序会被终止还是会恢复，
        /// 并让内存内容保持不变。
        /// </summary>
        /// <param name="sender">挂起的请求的源。</param>
        /// <param name="e">有关挂起请求的详细信息。</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: 保存应用程序状态并停止任何后台活动
            deferral.Complete();
        }
    }
}
