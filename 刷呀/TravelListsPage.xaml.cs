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

    public sealed partial class TravelListsPage : Page
    {

        private string travellists_uri = "";//游记接口
        private string cities_lists_uri = "";//城市列表接口

        private string travellists_json = "";//游记内容Json
        private string cities_lists_json = "";//城市列表Json

        private int count;//返回的游记Json中游记的总数
        private int temp_count;//临时变量
        private int now_page = 1;//当前游记的页数

        Get_Resquest get_resquest = new Get_Resquest();
        Travellists travellists = new Travellists();
        Cities_Lists cities_lists = new Cities_Lists();

        private double x = 0;//用来接受手势水平滑动的长度

        public TravelListsPage()
        {
            this.InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Enabled;//缓存页面内容
            this.ManipulationCompleted += The_ManipulationCompleted;//订阅手势滑动结束后的事件
            this.ManipulationDelta += The_ManipulationDelta;//订阅手势滑动事件
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
                if (App.k == 0)
                {
                    Refresh_Box.RefreshThreshold = 70;
                    Changeable_Frame.Navigate(typeof(WeatherPage));
                    await Cities_Lists_Add();
                    await Dispaly(0);
                }
                App.k = 1;//表示已经第一次导航到该页面了
            }

            base.OnNavigatedTo(e);
        }

        private async Task Cities_Lists_Add()//初始化城市列表
        {
            cities_lists_uri = "http://apis.baidu.com/tngou/hospital/city";//城市列表接口
            try
            {
                cities_lists_json = await get_resquest.Get_Resquset_Result(new Uri(cities_lists_uri));//网络请求拿到Json
                cities_lists_json = ConvertUnicodeToChinese.ConvertToChinese_Result(cities_lists_json);//内容转码
                pane_listview.ItemsSource = App.cities_lists_list;//列表绑定

                #region 添加外国地名在列表之前
                App.cities_lists_list.Add(new Cities_Lists { province = "美洲" });
                App.cities_lists_list.Add(new Cities_Lists { province = "欧洲" });
                App.cities_lists_list.Add(new Cities_Lists { province = "非洲" });
                App.cities_lists_list.Add(new Cities_Lists { province = "澳洲" });
                App.cities_lists_list.Add(new Cities_Lists { province = "俄罗斯" });
                App.cities_lists_list.Add(new Cities_Lists { province = "迪拜" });
                App.cities_lists_list.Add(new Cities_Lists { province = "韩国" });
                App.cities_lists_list.Add(new Cities_Lists { province = "日本" });
                App.cities_lists_list.Add(new Cities_Lists { province = "泰国" });
                App.cities_lists_list.Add(new Cities_Lists { province = "马来西亚" });
                App.cities_lists_list.Add(new Cities_Lists { province = "新加坡" });
                #endregion

                for (int i = 0; i < 34; i++)
                {
                    cities_lists = JsonToObject.Cities_Lists_JsonConvert(cities_lists_json, i);
                    App.cities_lists_list.Add(cities_lists);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private async Task Dispaly(int m)//将反序列化之后的类的内容显示到屏幕上
        {
            travellists_uri = "http://apis.baidu.com/qunartravel/travellist/travellist?" + "query=" + App.travellists_query + "&" + "page=" + now_page.ToString();
            try
            {
                travellists_json = await get_resquest.Get_Resquset_Result(new Uri(travellists_uri));//网络请求拿到Json
                travellists_json = ConvertUnicodeToChinese.ConvertToChinese_Result(travellists_json);//内容转码
                ReFresh_List(travellists_json, m);//加载游记内容
                now_page++;
                prs.Visibility = Visibility.Collapsed;//隐藏加载下一页新闻内容时显示的进度条
            }
            catch (Exception)
            {
                var dig = new MessageDialog("可能你的网络有问题哦", "刷呀游记");
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

        private void ReFresh_List(string news_json, int m)//刷新游记列表
        {
            try
            {
                count = JsonToObject.Travellists_Count(travellists_json);//游记Json返回的游记总数
                if (m == 0)
                {
                    App.travellists_list = new ObservableCollection<Travellists>();//清空travellistss中的内容
                }
                if (count > 10)//如果游记总数超过20
                {
                    temp_count = 10;
                }
                else
                {
                    temp_count = count;
                }
                for (int i = 0; i < temp_count; i++)
                {
                    travellists = JsonToObject.Travellists_JsonConvert(travellists_json, i);
                    if (travellists.elite == true)
                    {
                        travellists.IsElite = Visibility.Visible;
                    }
                    else if (travellists.elite == false)
                    {
                        travellists.IsElite = Visibility.Collapsed;
                    }
                    App.travellists_list.Add(travellists);
                }

                count -= temp_count;//减掉已循环的条数

                travellists_listview.ItemsSource = App.travellists_list;//绑定列表

            }
            catch (Exception)
            {

                throw;
            }
        }

        private async void Search()//搜索游记
        {
            try
            {
                #region 初始化搜索
                App.travellists_query = Search_Box.Text;
                now_page = 1;
                #endregion

                await Dispaly(0);//加载游记内容
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

        private void travellists_listview_ItemClick(object sender, ItemClickEventArgs e)//点击游记内容
        {
            Travellists travellists = e.ClickedItem as Travellists;
            if (Window.Current.Bounds.Width < 600)
            {
                App.Go_To_Frame = true;
                Frame.Navigate(typeof(WebViewPage), travellists);
            }
            else
            {
                App.Changeable_Frame = Changeable_Frame;
                Changeable_Frame.Navigate(typeof(WebViewPage), travellists);
            }
        }

        private async void pane_listview_ItemClick(object sender, ItemClickEventArgs e)//点击城市列表
        {
            #region 初始化搜索城市游记
            Cities_Lists cities_name = e.ClickedItem as Cities_Lists;
            App.travellists_query = cities_name.province;
            now_page = 1;
            #endregion

            splitview.IsPaneOpen = false;//关闭汉堡菜单
            await Dispaly(0);//加载新闻内容
        }

        private void Search_Box_KeyDown(object sender, KeyRoutedEventArgs e)//请求搜索游记
        {
            if (e.Key == VirtualKey.Enter)
            {
                Search();
            }
        }

        private void Search_Appbut_Click(object sender, RoutedEventArgs e)//请求搜索游记
        {
            Search();
        }

        private void menu_Click(object sender, RoutedEventArgs e)//展开汉堡菜单
        {
            splitview.IsPaneOpen = true;
        }

        private async void Refresh_Box_RefreshInvoked(DependencyObject sender, object args)//下拉刷新
        {
            await Dispaly(0);
        }

        private async void travellists_listview_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)//浏览到列表底部时自动加载下一页
        {
            if (args.ItemIndex == (travellists_listview.Items.Count - 1))//如果已经将当前ListView中的倒数第二条新闻加载完成
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

        private void News_Click(object sender, RoutedEventArgs e)//刷呀新闻页
        {
            Frame.Navigate(typeof(NewsPage));
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

        private void To_Top_Click(object sender, RoutedEventArgs e)//回到游记页顶部
        {
            travellists_listview.ScrollIntoView(travellists_listview.Items.First());
        }

    }
}
