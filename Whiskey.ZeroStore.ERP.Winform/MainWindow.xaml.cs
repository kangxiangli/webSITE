using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Awesomium;
using Awesomium.Core;
using Awesomium.Web;
using Awesomium.Core.Data;
using Awesomium.Core.Dynamic;
using Awesomium.Core.Data.SQLite;
using Awesomium.Windows;
using Awesomium.ComponentModel;
using Whiskey.ZeroStore.ERP.Winform.Extensions;

namespace Whiskey.ZeroStore.ERP.Winform
{

    public partial class MainWindow : Window
    {

        public MainWindow()
        {

            string dataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Cookies";
            WebSession session = WebCore.CreateWebSession(dataPath, new WebPreferences()
            {
                WebGL = true,
                SmoothScrolling = true,
                CustomCSS = "",
                EnableGPUAcceleration = true,
                Plugins = true,
                CanScriptsCloseWindows = false,
                CanScriptsOpenWindows = false,
                AllowInsecureContent = true,
                WebSecurity = true,
                AppCache = false,
                
            });

            InitializeComponent();

            this.Title = "零时尚ERP管理平台 Ver2.0 — 我的任务是让你的工作变得更简单！";

            WebBrowser.WebSession = session;

            WebBrowser.Source = new Uri("http://localhost:8080/");

            WebBrowser.DocumentReady += (s, e) =>
            {
                BindBarcoder();
            };

            WebBrowser.ShowContextMenu += (s, e) =>
            {
                //e.Handled = true;
            };

            WebBrowser.LoadingFrameComplete += (s, e) =>
            {
                if (!e.IsMainFrame)
                {
                    if (WebBrowser.IsLoading)
                    {

                    }
                }
            };

            WebBrowser.ConsoleMessage+=(s,e)=>{
                MessageBox.Show(e.Message);
            };
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (MessageBox.Show("确认要退出零时尚ERP管理平台吗？", "退出", MessageBoxButton.YesNo)==MessageBoxResult.No) {
                e.Cancel = true;
            }
            base.OnClosing(e);
        }


        private void BindBarcoder()
        {
            if (WebBrowser.IsDocumentReady) {
                JSObject obj = WebBrowser.CreateGlobalJavascriptObject("whiskey");
                obj.Bind("barcode", (s, e) =>
                {
                    if (e.Arguments.Length == 2)
                    {
                        JSValue barcodeType = (JSValue)e.Arguments[0];
                        JSValue[] barcodes = (JSValue[])e.Arguments[1];
                        var codes = new List<BarcodeItem>();
                        foreach (JSValue m in barcodes)
                        {
                            var item = m.ToString().Split(',');
                            if (item.Length >= 7)
                            {
                                codes.Add(new BarcodeItem
                                {
                                    ProductNumber = item[0],
                                    ProductName = item[1],
                                    TagPrice = Convert.ToDecimal(item[2]),
                                    ColorName = item[3],
                                    BrandName = item[4],
                                    SizeName = item[5],
                                    DiscountName = item[6],
                                });
                            }
                        }
                        if( !barcodeType.IsNumber){
                             barcodeType=0;
                        }
                        Barcoder.BarcodeType = Convert.ToInt32(barcodeType.ToString());
                        Barcoder.Barcodes = codes;
                        var r = Barcoder.Print();
                        return r;
                    }
                    else {
                        return -4;
                    }

                });
            }
            

        }

        //webView.ExecuteJavascript("myMethod()");
        //webView.ExecuteJavascript("myMethodExpectingReturn()");
        //var result = webView.ExecuteJavascriptWithResult("myMethodProvidingReturn('foo')");






    }
}
