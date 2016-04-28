using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace 刷呀
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class WebViewPage : Page
    {

        Travellists travellists = new Travellists();
        private string url = "";

        public WebViewPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)//初始化WebViewPage的内容
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;//显示返回键
            SystemNavigationManager.GetForCurrentView().BackRequested += Frame_BackRequested;//订阅Page返回事件

            #region 判断传入的类的类型
            if (e.Parameter is News)
            {
                News news = new News();
                news = e.Parameter as News;
                url = news.link;
            }
            else if (e.Parameter is Travellists)
            {
                Travellists travellists = new Travellists();
                travellists = e.Parameter as Travellists;
                url = travellists.bookUrl;
            }
            #endregion

            base.OnNavigatedTo(e);
        }

        private async void webview_Loaded(object sender, RoutedEventArgs e)
        {
            HttpRequestMessage HttpRequestMessage = new HttpRequestMessage(HttpMethod.Get, new Uri(url));
            //更改UserAgent以获得手机版网页
            var add = "Mozilla/5.0 (Windows Phone 10.0; Android 4.2.1; Microsoft; Lumia950) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2486.0 Mobile Safari/537.36 Edge/13.10586";
            HttpRequestMessage.Headers.Add("User-Agent", add);
            webview.NavigateWithHttpRequestMessage(HttpRequestMessage);
            await Task.Delay(2000);
            loading.Visibility = Visibility.Collapsed;
        }

        private void Frame_BackRequested(object sender, BackRequestedEventArgs e)//返回事件
        {
            if (App.Go_To_Frame == true)
            {
                if (Frame.CanGoBack && e.Handled == false)
                {
                    e.Handled = true;
                    SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
                    App.Go_To_Frame = false;
                    Frame.GoBack();
                }
            }
            else if (App.Changeable_Frame != null && App.Changeable_Frame.CanGoBack && e.Handled == false)
            {
                e.Handled = true;
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
                App.Changeable_Frame.GoBack();
            }
        }
    }
}
