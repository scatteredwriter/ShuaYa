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
    public sealed partial class JokesPage : Page
    {

        private string jokes_uri = "";//文字笑话接口

        private int uri_choice = 0;//选择文字笑话or图片笑话

        private string jokes_json = "";//笑话内容Json

        private int now_page = 1;//当前笑话的页数
        private int all_page;//返回的笑话页数

        Get_Resquest get_resquest = new Get_Resquest();
        Jokes jokes = new Jokes();

        private double x = 0;//用来接受手势水平滑动的长度

        public JokesPage()
        {
            this.InitializeComponent();
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
                if (App.g == 0)
                {
                    ReFresh_Box.RefreshThreshold = 70;
                    Changeable_Frame.Navigate(typeof(WeatherPage));
                    Jokes_Choice_Lists_Add();//初始化笑话选择列表
                    await Dispaly(0);//初始化新闻内容
                }
                App.g = 1;//表示已经第一次导航到该页面了
            }

            base.OnNavigatedTo(e);
        }

        private void Jokes_Choice_Lists_Add()//初始化笑话选择列表
        {
            App.jokes_choice_lists.Add("文字笑话");
            App.jokes_choice_lists.Add("图片笑话");
            pane_listview.ItemsSource = App.jokes_choice_lists;
        }

        private async Task Dispaly(int m)//将反序列化之后的类的内容显示到屏幕上
        {
            if (uri_choice == 0)//选择文字笑话
            {
                jokes_uri = "http://apis.baidu.com/showapi_open_bus/showapi_joke/joke_text?" + "page=" + now_page.ToString();
            }
            else if (uri_choice == 1)//选择图片笑话
            {
                jokes_uri = "http://apis.baidu.com/showapi_open_bus/showapi_joke/joke_pic?" + "page=" + now_page.ToString();
            }
            try
            {
                jokes_json = await get_resquest.Get_Resquset_Result(new Uri(jokes_uri));//网络请求拿到Json
                jokes_json = ConvertUnicodeToChinese.ConvertToChinese_Result(jokes_json);//内容转码
                ReFresh_List(jokes_json, m);//加载新闻内容
                now_page++;
                prs.Visibility = Visibility.Collapsed;//隐藏加载下一页新闻内容时显示的进度条
            }
            catch (Exception)
            {
                var dig = new MessageDialog("可能你的网络有问题哦", "刷呀笑话");
                var btnRetry = new UICommand("等会再试吧");
                dig.Commands.Add(btnRetry);
                var result = await dig.ShowAsync();
                if (null != result && result.Label == "等会再试吧")
                {
                    return;
                }
                throw;
            }
        }

        private void ReFresh_List(string jokes_json, int m)//刷新笑话列表
        {
            try
            {
                all_page = JsonToObject.All_Page("Jokes", this.jokes_json);//该关键字总共包含的页数
                if (now_page > all_page)
                {
                    return;
                }
                else
                {
                    if (m == 0)
                    {
                        App.jokeslists = new ObservableCollection<Jokes>();//清空newslists中的内容
                    }
                    for (int i = 0; i < 20; i++)
                    {
                        jokes = JsonToObject.Jokes_JsonConvert(this.jokes_json, i);
                        App.jokeslists.Add(jokes);
                    }
                }
                jokes_listview.ItemsSource = App.jokeslists;//绑定列表
            }
            catch (Exception)
            {

                throw;
            }
        }

        private async void pane_listview_ItemClick(object sender, ItemClickEventArgs e)//点击笑话选择列表
        {
            #region 初始化新闻频道切换
            string selected_joke = e.ClickedItem as string;
            if (selected_joke == "文字笑话")
            {
                uri_choice = 0;
            }
            else if (selected_joke == "图片笑话")
            {
                uri_choice = 1;
            }
            now_page = 1;
            #endregion

            splitview.IsPaneOpen = false;//关闭汉堡菜单
            await Dispaly(0);//加载新闻内容
        }

        private void menu_Click(object sender, RoutedEventArgs e)//展开汉堡菜单
        {
            splitview.IsPaneOpen = true;
        }

        private async void RefreshBox_RefreshInvoked(DependencyObject sender, object args)//下拉刷新
        {
            await Dispaly(0);
        }

        private async void jokes_listview_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)//浏览到列表底部时自动加载下一页
        {
            if (args.ItemIndex == (jokes_listview.Items.Count - 1))//如果已经将当前ListView中的倒数第二条新闻加载完成
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

        private void Travellists_Click(object sender, RoutedEventArgs e)//刷呀游记页
        {
            Frame.Navigate(typeof(TravelListsPage));
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

        private void To_Top_Click(object sender, RoutedEventArgs e)//回到笑话页顶部
        {
            jokes_listview.ScrollIntoView(jokes_listview.Items.First());
        }

    }

    // DataTemplate选择 类，用来选择jokes_listview的ItemTemplate的DataTemplate以此选择文字笑话或图片笑话
    public sealed class Jokes_DataTemplate_Selector : DataTemplateSelector//新建一个继承DataTemplateSelector类的新类，并将此类的实例赋值给jokes_listview的ItemTemplateSelector属性
    {
        public DataTemplate Jokes_Text_ItemTemplate { get; set; }//文字笑话数据模板
        public DataTemplate Jokes_Img_ItemTemplate { get; set; }//图片笑话数据模板

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)//重写DataTemplateSelector的虚方法SelectTemplateCore方法
        {
            Jokes jokes_item = item as Jokes;
            if (jokes_item.text == null)
            {
                return Jokes_Img_ItemTemplate;
            }
            else if (jokes_item.img == null)
            {
                return Jokes_Text_ItemTemplate;
            }
            return null;//返回空
        }

    }

}
