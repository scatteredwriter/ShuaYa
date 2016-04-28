using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Input;
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
    public sealed partial class NewsPage : Page
    {

        private string news_uri = "";//新闻接口
        private string channel_uri = "";//新闻频道列表接口

        private string news_json = "";//新闻内容Json
        private string channel_json = "";//新闻频道Json

        private int now_page = 1;//当前新闻的页数
        private int all_page;//返回的新闻页数
        private int all_channel_num;//新闻频道数量
        private int all_news_num;//返回的新闻条数

        Get_Resquest get_resquest = new Get_Resquest();
        News news = new News();
        News_Channel news_channel = new News_Channel();

        private double x = 0;//用来接受手势水平滑动的长度

        public NewsPage()
        {
            this.InitializeComponent();
            //this.SizeChanged += Page_SizeChanged;//订阅页面窗口尺寸变化的事件
            NavigationCacheMode = NavigationCacheMode.Enabled;//缓存页面内容
            ManipulationCompleted += The_ManipulationCompleted;//订阅手势滑动结束后的事件
            ManipulationDelta += The_ManipulationDelta;//订阅手势滑动事件
        }

        private void The_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)//手势滑动中
        {
            x += e.Delta.Translation.X;//将滑动的值赋给x
        }

        private void The_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)//手势滑动结束
        {
            if (x > 100)//判断滑动的距离是否符合条件
            {
                splitview.IsPaneOpen = true;//打开汉堡菜单
            }
            x = 0;
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)//页面初始化
        {
            if (e.NavigationMode == NavigationMode.New)
            {
                if (App.f == 0)
                {
                    ReFresh_Box.RefreshThreshold = 70;
                    Changeable_Frame.Navigate(typeof(WeatherPage));
                    await News_Channel_Add();
                    await Dispaly(0);//初始化新闻内容
                }
                App.f = 1;//表示已经第一次导航到该页面了
            }

            base.OnNavigatedTo(e);
        }

        private async Task News_Channel_Add()//加载汉堡菜单中的新闻频道列表
        {
            pane_listview.ItemsSource = App.news_channel_list;//将集合news_channel_list绑定新闻频道列表pane_listview
            channel_uri = "http://apis.baidu.com/showapi_open_bus/channel_news/channel_news";//新闻频道列表接口
            channel_json = await get_resquest.Get_Resquset_Result(new Uri(channel_uri));//网络请求拿到新闻频道的Json
            channel_json = ConvertUnicodeToChinese.ConvertToChinese_Result(channel_json);//内容转码
            try
            {
                all_channel_num = JsonToObject.All_News_Channel_Num(channel_json);
                for (int i = 0; i < all_channel_num; i++)
                {
                    news_channel = JsonToObject.News_Channel_JsonConvert(channel_json, i);
                    App.news_channel_list.Add(news_channel);
                }
            }

            catch (Exception)
            {

                throw;
            }
        }

        private async Task Dispaly(int m)//将反序列化之后的类的内容显示到屏幕上
        {
            news_uri = "http://apis.baidu.com/showapi_open_bus/channel_news/search_news?" + "channelName=" + App.news_channel_name + "&" + "title=" + App.news_title + "&" + "page=" + now_page.ToString();
            try
            {
                news_json = await get_resquest.Get_Resquset_Result(new Uri(news_uri));//网络请求拿到Json
                news_json = ConvertUnicodeToChinese.ConvertToChinese_Result(news_json);//内容转码
                ReFresh_List(news_json, m);//加载新闻内容
                now_page++;
                prs.Visibility = Visibility.Collapsed;//隐藏加载下一页新闻内容时显示的进度条
            }
            catch (Exception)
            {
                var dig = new MessageDialog("可能你的网络有问题哦", "刷呀新闻");
                var btnRetry = new UICommand("等会再试吧");
                dig.Commands.Add(btnRetry);
                var result = await dig.ShowAsync();
                if (null != result && result.Label == "等会再试吧")
                {
                    Search_Box.Focus(FocusState.Programmatic);
                    return;
                }
                throw;
            }
        }

        private void ReFresh_List(string news_json, int m)//刷新新闻列表
        {
            try
            {
                all_page = JsonToObject.All_Page("News", news_json);//该关键字总共包含的页数
                if (now_page > all_page)
                {
                    return;
                }
                else
                {
                    if (m == 0)
                    {
                        App.newslists = new ObservableCollection<News>();//清空newslists中的内容
                    }
                    all_news_num = JsonToObject.All_News_Num(news_json);
                    all_news_num -= (now_page - 1) * 20;
                    if (all_news_num >= 20)//如果总新闻条数超过20条
                    {
                        all_news_num = 20;
                    }
                    for (int i = 0; i < all_news_num; i++)
                    {
                        news = JsonToObject.News_JsonConvert(news_json, i);
                        App.newslists.Add(news);
                    }
                    news_listview.ItemsSource = App.newslists;//绑定列表
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private async void Search()//搜索新闻
        {
            try
            {
                #region 初始化搜索
                App.news_channel_name = "";
                App.news_title = Search_Box.Text;
                now_page = 1;
                #endregion

                await Dispaly(0);//加载新闻内容
            }
            catch (Exception)
            {
                var dig = new MessageDialog("可能你的网络有问题哦", "错误的那点事");
                var btnRetry = new UICommand("等会再试吧");
                dig.Commands.Add(btnRetry);
                var result = await dig.ShowAsync();
                if (null != result && result.Label == "等会再试吧")
                {
                    Search_Box.Focus(FocusState.Programmatic);
                }
                throw;
            }
        }

        private void news_listview_ItemClick(object sender, ItemClickEventArgs e)//点击新闻
        {
            News news = e.ClickedItem as News;
            if (Window.Current.Bounds.Width < 600)
            {
                App.Go_To_Frame = true;
                Frame.Navigate(typeof(WebViewPage), news);
            }
            else
            {
                App.Changeable_Frame = Changeable_Frame;
                Changeable_Frame.Navigate(typeof(WebViewPage), news);
            }
        }

        private async void pane_listview_ItemClick(object sender, ItemClickEventArgs e)//点击新闻频道
        {
            #region 初始化新闻频道切换
            News_Channel news_channel = e.ClickedItem as News_Channel;
            App.news_channel_name = news_channel.name;
            App.news_title = "";
            now_page = 1;
            #endregion

            splitview.IsPaneOpen = false;//关闭汉堡菜单
            await Dispaly(0);//加载新闻内容
        }

        private void Search_Box_KeyDown(object sender, KeyRoutedEventArgs e)//请求搜索新闻
        {
            if (e.Key == VirtualKey.Enter)
            {
                Search();
            }
        }

        private void Search_Appbut_Click(object sender, RoutedEventArgs e)//请求搜索新闻
        {
            Search();
        }

        private void menu_Click(object sender, RoutedEventArgs e)//展开汉堡菜单
        {
            splitview.IsPaneOpen = true;
        }

        private async void RefreshBox_RefreshInvoked(DependencyObject sender, object args)//下拉刷新
        {
            await Dispaly(0);
        }

        private async void news_listview_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)//浏览到列表底部时自动加载下一页
        {
            if (args.ItemIndex == (news_listview.Items.Count - 1))//如果已经将当前ListView中的倒数第二条新闻加载完成
            {
                prs.Visibility = Visibility.Visible;//显示进度条
                await Task.Factory.StartNew(async () =>
                {
                    await Task.Delay(2000);//延迟两秒
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                    {
                        await Dispaly(1);//加载下一页的新闻内容
                    });
                });
            }
        }

        private void Travellists_Click(object sender, RoutedEventArgs e)//刷呀游记页
        {
            Frame.Navigate(typeof(TravelListsPage));
        }

        private void Jokes_Click(object sender, RoutedEventArgs e)//刷呀笑话页
        {
            Frame.Navigate(typeof(JokesPage));
        }

        private async void Wheather_Click(object sender, RoutedEventArgs e)//刷呀天气页
        {
            if (App.open_weather == 0)
            {
                if (Window.Current.CoreWindow.Bounds.Width < 600)//判断当前窗口的宽度是否小于600
                {
                    SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;//显示返回键
                }
                Frame.Navigate(typeof(WeatherPage));
            }
            else
            {
                var dig = new MessageDialog("您已经关闭了刷呀天气", "刷呀天气");
                var btnRetry = new UICommand("请到设置中开启");
                dig.Commands.Add(btnRetry);
                var result = await dig.ShowAsync();
                if (null != result && result.Label == "请到设置中开启")
                {
                    return;
                }
            }
        }

        private void Settings_Click(object sender, RoutedEventArgs e)//刷呀设置页
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;//显示返回键
            if (Window.Current.Bounds.Width < 600)
            {
                App.Go_To_Frame = true;
                Frame.Navigate(typeof(SettingsPage));
            }
            else
            {
                App.Changeable_Frame = Changeable_Frame;
                Changeable_Frame.Navigate(typeof(SettingsPage));
            }
        }

        private void To_Top_Click(object sender, RoutedEventArgs e)//回到新闻页顶部
        {
            news_listview.ScrollIntoView(news_listview.Items.First());
        }

    }

    // DataTemplate选择 类，用来选择news_listview的ItemTemplate的DataTemplate以此选择有图片的新闻或无图片的新闻
    public sealed class News_DataTemplate_Selector : DataTemplateSelector//新建一个继承DataTemplateSelector类的新类，并将此类的实例赋值给news_listview的ItemTemplateSelector属性
    {
        public DataTemplate DataTemplate_Without_Photo { get; set; }
        public DataTemplate DataTemplate_With_Photo { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)//重写DataTemplateSelector的虚方法SelectTemplateCore方法
        {
            News news_item = item as News;
            if (news_item.imageurls == "")
            {
                return DataTemplate_Without_Photo;
            }
            else
            {
                return DataTemplate_With_Photo;
            }
        }
    }

}
