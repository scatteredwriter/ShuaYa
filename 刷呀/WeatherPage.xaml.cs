using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    public sealed partial class WeatherPage : Page
    {

        private double latitude = 0;//纬度
        private double longitude = 0;//经度

        private string position_xml = "";//返回的位置的xml
        private string cityname = "";//位置所在的城市名
        private string weather_json = "";//天气Json

        private string convert_position_uri = "";//坐标方向解析api
        private string weather_uri = "";//天气api

        Weather weather_mes = new Weather();
        Get_Resquest get_request = new Get_Resquest();

        public WeatherPage()
        {
            this.InitializeComponent();
            SystemNavigationManager.GetForCurrentView().BackRequested += Frame_BackRequested;//订阅Page返回事件
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)//初始化
        {
            if (App.open_weather == 1)
            {
                roading.Visibility = Visibility.Collapsed;
                Body_Re.Visibility = Visibility.Collapsed;
                close.Visibility = Visibility.Visible;
                return;
            }
            if (e.NavigationMode == NavigationMode.New || e.NavigationMode == NavigationMode.Back)
            {
                if (App.h == 0)
                {
                    await Get_Position();//获取设备位置
                    if (App.allowed_get_position == 1)
                    {
                        return;
                    }
                    await Convert_Position();//转化GPS坐标为城市
                    Get_Weather();//获取天气信息
                    App.h = 1;//表示已经第一次导航到该页
                }
                else if (App.h == 1)
                {
                    roading.IsActive = false;
                    city.Text = App._city;
                    date.Text = App._date;
                    high_temp.Text = App._h_tmp;
                    low_temp.Text = App._l_tmp;
                    temp.Text = App._temp;
                    temp_c.Visibility = Visibility.Visible;
                    time.Text = App._time;
                    weather_info.Text = App._weather;
                }
            }

            base.OnNavigatedTo(e);
        }

        private void Frame_BackRequested(object sender, BackRequestedEventArgs e)//返回事件
        {
            if (Frame.CanGoBack && e.Handled == false)
            {
                e.Handled = true;
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
                App.Go_To_Frame = false;
                Frame.GoBack();
            }
        }

        private async Task Get_Position()//获取设备信息
        {
            Geolocator geolocator = new Geolocator();//实例化一个Geolocator类
            GeolocationAccessStatus accessstatus = new GeolocationAccessStatus();//实例化一个GeolocationAccessStatus类，该类为枚举类型，其枚举值代表应用是否具有访问位置的权限
            accessstatus = await Geolocator.RequestAccessAsync();//请求获取位置
            switch (accessstatus)//判断请求结果
            {
                case GeolocationAccessStatus.Allowed: App.geoposition_allow = 1; break;//已经允许使用位置
                case GeolocationAccessStatus.Denied://已经拒绝使用位置共享
                    {
                        roading.Visibility = Visibility.Collapsed;
                        Body_Re.Visibility = Visibility.Collapsed;
                        close.Visibility = Visibility.Visible;
                        App.allowed_get_position = 1;
                        return;
                    };
            }
            if (App.geoposition_allow == 1)//允许使用位置
            {
                try
                {
                    geolocator.DesiredAccuracyInMeters = 0;//设置位置获取的精确度为0米
                    Geoposition geoposition = await geolocator.GetGeopositionAsync();//获取位置
                    latitude = geoposition.Coordinate.Point.Position.Latitude;//获取位置的纬度
                    longitude = geoposition.Coordinate.Point.Position.Longitude;//获取位置的经度
                }
                catch (Exception)
                {

                    throw;
                }

            }
        }

        private async Task Convert_Position()//将GPS坐标转化为所在的城市
        {
            if (App.allowed_get_position == 0)
            {
                convert_position_uri = "http://api.map.baidu.com/geocoder?" + "location=" + latitude.ToString() + "," + longitude.ToString() + "&" + "coord_type=wgs84";//反向地址解析api
                position_xml = await get_request.Get_Resquset_Result(new Uri(convert_position_uri));//得到位置的xml
                int first_char = position_xml.IndexOf("<city>") + "<city>".Length;//获取xml中字符串“<city>”中'>'后一位的位置索引
                int last_char = position_xml.IndexOf("</city>");//获取xml中字符串“</city>”中'<'的位置索引
                cityname = position_xml.Substring(first_char, (last_char - first_char));//字符串截取，截取结果为<city>和</city>中的内容
                cityname = cityname.TrimEnd('市');//移除尾部的“市”，仅保留城市名字
                cityname = Uri.EscapeUriString(cityname);//将UTF-8编码的字符串转换成Uri的转义形式
            }
        }

        private async void Get_Weather()//获取天气信息
        {
            if (App.allowed_get_position == 0)
            {
                try
                {
                    weather_uri = "http://apistore.baidu.com/microservice/weather?" + "cityname=" + cityname;//天气api
                    weather_json = await get_request.Get_Resquset_Result(new Uri(weather_uri));//得到天气Json
                    weather_json = ConvertUnicodeToChinese.ConvertToChinese_Result(weather_json);
                    weather_mes = JsonToObject.Weather_JsonConvert(weather_json);

                    #region 给天气的相关属性赋值
                    roading.IsActive = false;
                    city.Text = App._city = weather_mes.city;
                    date.Text = App._date = weather_mes.date;
                    high_temp.Text = App._h_tmp = weather_mes.h_tmp + "°";
                    low_temp.Text = App._l_tmp = weather_mes.l_tmp + "°";
                    temp.Text = App._temp = weather_mes.temp;
                    temp_c.Visibility = Visibility.Visible;
                    time.Text = App._time = weather_mes.time;
                    weather_info.Text = App._weather = weather_mes.weather;
                    #endregion
                }
                catch (Exception)
                {
                    var dig = new MessageDialog("无法获取你的位置", "刷呀天气");
                    var btnRetry = new UICommand("等会再试吧");
                    dig.Commands.Add(btnRetry);
                    var result = await dig.ShowAsync();
                    return;
                    throw;
                }
            }
        }

    }
}
