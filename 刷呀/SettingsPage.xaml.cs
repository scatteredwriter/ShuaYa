using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace 刷呀
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SettingsPage : Page
    {

        private ApplicationDataContainer appdata = ApplicationData.Current.LocalSettings;//获取应用的本地设置

        public SettingsPage()
        {
            this.InitializeComponent();
            SystemNavigationManager.GetForCurrentView().BackRequested += Frame_BackRequested;//订阅Page返回事件
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (App.open_weather == 0)//如果可以使用刷呀天气
            {
                tg_switch.IsOn = true;
            }
            else if (App.open_weather == 1)
            {
                tg_switch.IsOn = false;
            }

            base.OnNavigatedTo(e);
        }

        private void Frame_BackRequested(object sender, BackRequestedEventArgs e)//返回事件
        {
            if (Frame.CanGoBack && e.Handled == false)
            {
                e.Handled = true;
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
                Frame.GoBack();
            }
        }

        private void tg_switch_Toggled(object sender, RoutedEventArgs e)
        {
            if (tg_switch.IsOn == false)
            {
                appdata.Values["Open_Weather"] = 1;//关闭刷呀天气功能
                App.open_weather = (int)appdata.Values["Open_Weather"];

            }
            else if (tg_switch.IsOn == true)
            {
                appdata.Values["Open_Weather"] = 0;//开启刷呀天气功能
                App.open_weather = (int)appdata.Values["Open_Weather"];

            }
        }
    }
}
