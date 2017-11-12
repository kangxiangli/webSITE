using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Whiskey.ZeroStore.ERP.Winform.Extensions
{

    public static class Barcoder
    {
        const uint IMAGE_BITMAP = 0;
        const uint LR_LOADFROMFILE = 16;
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern IntPtr LoadImage(IntPtr hinst, string lpszName, uint uType,
           int cxDesired, int cyDesired, uint fuLoad);
        [DllImport("Gdi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int DeleteObject(IntPtr ho);
        const string szSavePath = "C:\\Whiskey\\Settings";
        const string szSaveFile = "C:\\Whiskey\\Settings\\Barcode.print";
        const string sznop1 = "nop_front\r\n";
        const string sznop2 = "nop_middle\r\n";
        [DllImport("Winppla.dll")]
        private static extern int A_Bar2d_Maxi(int x, int y, int primary, int secondary,
            int country, int service, char mode, int numeric, string data);
        [DllImport("Winppla.dll")]
        private static extern int A_Bar2d_Maxi_Ori(int x, int y, int ori, int primary,
            int secondary, int country, int service, char mode, int numeric, string data);
        [DllImport("Winppla.dll")]
        private static extern int A_Bar2d_PDF417(int x, int y, int narrow, int width, char normal,
            int security, int aspect, int row, int column, char mode, int numeric, string data);
        [DllImport("Winppla.dll")]
        private static extern int A_Bar2d_PDF417_Ori(int x, int y, int ori, int narrow, int width,
            char normal, int security, int aspect, int row, int column, char mode, int numeric,
            string data);
        [DllImport("Winppla.dll")]
        private static extern int A_Bar2d_DataMatrix(int x, int y, int rotation, int hor_mul,
            int ver_mul, int ECC, int data_format, int num_rows, int num_col, char mode,
            int numeric, string data);
        [DllImport("Winppla.dll")]
        private static extern void A_Clear_Memory();
        [DllImport("Winppla.dll")]
        private static extern void A_ClosePrn();
        [DllImport("Winppla.dll")]
        private static extern int A_CreatePrn(int selection, string filename);
        [DllImport("Winppla.dll")]
        private static extern int A_Del_Graphic(int mem_mode, string graphic);
        [DllImport("Winppla.dll")]
        private static extern int A_Draw_Box(char mode, int x, int y, int width, int height,
            int top, int side);
        [DllImport("Winppla.dll")]
        private static extern int A_Draw_Line(char mode, int x, int y, int width, int height);
        [DllImport("Winppla.dll")]
        private static extern void A_Feed_Label();
        [DllImport("Winppla.dll")]
        private static extern IntPtr A_Get_DLL_Version(int nShowMessage);
        [DllImport("Winppla.dll")]
        private static extern int A_Get_DLL_VersionA(int nShowMessage);
        [DllImport("Winppla.dll")]
        private static extern int A_Get_Graphic(int x, int y, int mem_mode, char format,
            string filename);
        [DllImport("Winppla.dll")]
        private static extern int A_Get_Graphic_ColorBMP(int x, int y, int mem_mode, char format,
            string filename);
        [DllImport("Winppla.dll")]
        private static extern int A_Get_Graphic_ColorBMPEx(int x, int y, int nWidth, int nHeight,
            int rotate, int mem_mode, char format, string id_name, string filename);
        [DllImport("Winppla.dll")]
        private static extern int A_Get_Graphic_ColorBMP_HBitmap(int x, int y, int nWidth, int nHeight,
           int rotate, int mem_mode, char format, string id_name, IntPtr hbm);
        [DllImport("Winppla.dll")]
        private static extern int A_Initial_Setting(int Type, string Source);
        [DllImport("Winppla.dll")]
        private static extern int A_WriteData(int IsImmediate, byte[] pbuf, int length);
        [DllImport("Winppla.dll")]
        private static extern int A_ReadData(byte[] pbuf, int length, int dwTimeoutms);
        [DllImport("Winppla.dll")]
        private static extern int A_Load_Graphic(int x, int y, string graphic_name);
        [DllImport("Winppla.dll")]
        private static extern int A_Open_ChineseFont(string path);
        [DllImport("Winppla.dll")]
        private static extern int A_Print_Form(int width, int height, int copies, int amount,
            string form_name);
        [DllImport("Winppla.dll")]
        private static extern int A_Print_Out(int width, int height, int copies, int amount);
        [DllImport("Winppla.dll")]
        private static extern int A_Prn_Barcode(int x, int y, int ori, char type, int narrow,
            int width, int height, char mode, int numeric, string data);
        [DllImport("Winppla.dll")]
        private static extern int A_Prn_Text(int x, int y, int ori, int font, int type,
            int hor_factor, int ver_factor, char mode, int numeric, string data);
        [DllImport("Winppla.dll")]
        private static extern int A_Prn_Text_Chinese(int x, int y, int fonttype, string id_name,
            string data, int mem_mode);
        [DllImport("Winppla.dll")]
        private static extern int A_Prn_Text_TrueType(int x, int y, int FSize, string FType,
            int Fspin, int FWeight, int FItalic, int FUnline, int FStrikeOut, string id_name,
            string data, int mem_mode);
        [DllImport("Winppla.dll")]
        private static extern int A_Prn_Text_TrueType_W(int x, int y, int FHeight, int FWidth,
            string FType, int Fspin, int FWeight, int FItalic, int FUnline, int FStrikeOut,
            string id_name, string data, int mem_mode);
        [DllImport("Winppla.dll")]
        private static extern int A_Set_Backfeed(int back);
        [DllImport("Winppla.dll")]
        private static extern int A_Set_BMPSave(int nSave, string pstrBMPFName);
        [DllImport("Winppla.dll")]
        private static extern int A_Set_Cutting(int cutting);
        [DllImport("Winppla.dll")]
        private static extern int A_Set_Darkness(int heat);
        [DllImport("Winppla.dll")]
        private static extern int A_Set_DebugDialog(int nEnable);
        [DllImport("Winppla.dll")]
        private static extern int A_Set_Feed(char rate);
        [DllImport("Winppla.dll")]
        private static extern int A_Set_Form(string formfile, string form_name, int mem_mode);
        [DllImport("Winppla.dll")]
        private static extern int A_Set_Margin(int position, int margin);
        [DllImport("Winppla.dll")]
        private static extern int A_Set_Prncomport(int baud, int parity, int data, int stop);
        [DllImport("Winppla.dll")]
        private static extern int A_Set_Prncomport_PC(int nBaudRate, int nByteSize, int nParity,
            int nStopBits, int nDsr, int nCts, int nXonXoff);
        [DllImport("Winppla.dll")]
        private static extern int A_Set_Sensor_Mode(char type, int continuous);
        [DllImport("Winppla.dll")]
        private static extern int A_Set_Speed(char speed);
        [DllImport("Winppla.dll")]
        private static extern int A_Set_Syssetting(int transfer, int cut_peel, int length,
            int zero, int pause);
        [DllImport("Winppla.dll")]
        private static extern int A_Set_Unit(char unit);
        [DllImport("Winppla.dll")]
        private static extern int A_Set_Gap(int gap);
        [DllImport("Winppla.dll")]
        private static extern int A_Set_Logic(int logic);
        [DllImport("Winppla.dll")]
        private static extern int A_Set_ProcessDlg(int nShow);
        [DllImport("Winppla.dll")]
        private static extern int A_Set_ErrorDlg(int nShow);
        [DllImport("Winppla.dll")]
        private static extern int A_Set_LabelVer(int centiInch);
        [DllImport("Winppla.dll")]
        private static extern int A_GetUSBBufferLen();
        [DllImport("Winppla.dll")]
        private static extern int A_EnumUSB(byte[] buf);
        [DllImport("Winppla.dll")]
        private static extern int A_CreateUSBPort(int nPort);
        [DllImport("Winppla.dll")]
        private static extern int A_CreatePort(int nPortType, int nPort, string filename);
        [DllImport("Winppla.dll")]
        private static extern int A_Clear_MemoryEx(int nMode);
        [DllImport("Winppla.dll")]
        private static extern void A_Set_Mirror();
        [DllImport("Winppla.dll")]
        private static extern int A_Bar2d_RSS(int x, int y, int ori, int ratio, int height,
            char rtype, int mult, int seg, string data1, string data2);
        [DllImport("Winppla.dll")]
        private static extern int A_Bar2d_QR_M(int x, int y, int ori, char mult, int value,
            int model, char error, int mask, char dinput, char mode, int numeric, string data);
        [DllImport("Winppla.dll")]
        private static extern int A_Bar2d_QR_A(int x, int y, int ori, char mult, int value,
            char mode, int numeric, string data);
        [DllImport("Winppla.dll")]
        private static extern int A_GetNetPrinterBufferLen();
        [DllImport("Winppla.dll")]
        private static extern int A_EnumNetPrinter(byte[] buf);
        [DllImport("Winppla.dll")]
        private static extern int A_CreateNetPort(int nPort);
        [DllImport("Winppla.dll")]
        private static extern int A_Prn_Text_TrueType_Uni(int x, int y, int FSize, string FType,
            int Fspin, int FWeight, int FItalic, int FUnline, int FStrikeOut, string id_name,
            byte[] data, int format, int mem_mode);
        [DllImport("Winppla.dll")]
        private static extern int A_Prn_Text_TrueType_UniB(int x, int y, int FSize, string FType,
            int Fspin, int FWeight, int FItalic, int FUnline, int FStrikeOut, string id_name,
            byte[] data, int format, int mem_mode);
        [DllImport("Winppla.dll")]
        private static extern int A_GetUSBDeviceInfo(int nPort, byte[] pDeviceName,
            out int pDeviceNameLen, byte[] pDevicePath, out int pDevicePathLen);
        [DllImport("Winppla.dll")]
        private static extern int A_Set_EncryptionKey(string encryptionKey);
        [DllImport("Winppla.dll")]
        private static extern int A_Check_EncryptionKey(string decodeKey, string encryptionKey,
            int dwTimeoutms);

        /// <summary>
        /// 条码数据
        /// </summary>
        public static List<BarcodeItem> Barcodes { get; set; }
        /// <summary>
        /// 条码类型
        /// </summary>
        public static int BarcodeType { get; set; }

        /// <summary>
        /// 打印条码选择器
        /// </summary>
        /// <returns></returns>
        public static int Print() {
            int result = 0;
            switch (BarcodeType) { 
                case 0:
                    result = PrintGoods();
                    break;
                case 1:
                    result = PrintLocations();
                    break;
                default:
                    result = PrintGoods();
                    break;
            }
            return result;
        }

        /// <summary>
        /// 获取字符串实际长度
        /// </summary>
        /// <param name="str"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        public static string FixedLength(string str, int num)
        {
            var len = System.Text.Encoding.Default.GetBytes(str).Length;
            if (len < num)
            {
                for (var i = 0; i < (num - len); i++)
                {
                    str += " ";
                }
            }
            else if (num > len)
            {
                str = str.Substring(0, num);
            }
            return str;
        }


        /// <summary>
        /// 打印商品条码
        /// </summary>
        /// <returns></returns>
        private static int PrintGoods(){
            int result=0;
            try
            {
                int nLen, ret, sw;
                byte[] pbuf = new byte[128];
                IntPtr ver;
                System.Text.Encoding encAscII = System.Text.Encoding.ASCII;
                System.Text.Encoding encUnicode = System.Text.Encoding.Unicode;
                ver = A_Get_DLL_Version(0);
                nLen = A_GetUSBBufferLen() + 1;
                if (nLen > 1)
                {
                    byte[] buf1, buf2;
                    int len1 = 128, len2 = 128;
                    buf1 = new byte[len1];
                    buf2 = new byte[len2];
                    A_EnumUSB(pbuf);
                    A_GetUSBDeviceInfo(1, buf1, out len1, buf2, out len2);
                    sw = 1;
                    if (1 == sw)
                    {
                        ret = A_CreatePrn(12, encAscII.GetString(buf2, 0, len2));// open usb.
                    }
                    else
                    {
                        ret = A_CreateUSBPort(1);// must call A_GetUSBBufferLen() function fisrt.
                    }
                    if (0 != ret)
                    {
                        result = -1;
                    }
                    else
                    {
                        if (2 == sw)
                        {
                            //get printer status.
                            pbuf[0] = 0x01;
                            pbuf[1] = 0x46;
                            pbuf[2] = 0x0D;
                            pbuf[3] = 0x0A;
                            A_WriteData(1, pbuf, 4);//<SOH>F
                            ret = A_ReadData(pbuf, 2, 1000);
                        }
                    }
                }
                else
                {
                    System.IO.Directory.CreateDirectory(szSavePath);
                    ret = A_CreatePrn(0, szSaveFile);// open file.
                }

                if (0 != ret)
                    result=-2;

                A_Set_DebugDialog(1);
                A_Set_Unit('n');
                A_Set_Syssetting(2, 0, 0, 0, 2);
                A_Set_Backfeed(320);
                A_Set_Darkness(8);
                A_Set_LabelVer(57);
                A_Clear_Memory();
                A_WriteData(0, encAscII.GetBytes(sznop2), sznop2.Length);
                A_WriteData(1, encAscII.GetBytes(sznop1), sznop1.Length);

                var count = 0;
                var margin = 16;
                var fontSize = 30;
                for (var i = 0; i < Barcodes.Count; i++)
                {
                    var item = Barcodes[i];
                    count++;
                    if (count % 2 == 1)
                    {
                        margin = 16;
                    }
                    else
                    {
                        margin = 220;
                    }
                    A_Prn_Text_TrueType(margin, 5, fontSize, "微软雅黑", 1, 600, 0, 0, 0, "A1", item.ProductNumber, 1);
                    A_Prn_Barcode(margin, 20, 1, 'o', 2, 1, 20, 'N', 1, item.ProductNumber);
                    A_Prn_Text_TrueType(margin, 45, fontSize, "微软雅黑", 1, 600, 0, 0, 0, "A2", "尺码：" + FixedLength(item.SizeName, 10) + "折扣：" + FixedLength(item.DiscountName, 10), 1);
                    A_Prn_Text_TrueType(margin, 60, fontSize, "微软雅黑", 1, 600, 0, 0, 0, "A3", "颜色：" + FixedLength(item.ColorName, 10) + "品牌：" + FixedLength(item.BrandName, 10), 1);
                    A_Prn_Text_TrueType(margin, 75, fontSize, "微软雅黑", 1, 600, 0, 0, 0, "A4", "价格：￥" + item.TagPrice, 1);
                    A_Prn_Text_TrueType(margin, 90, fontSize, "微软雅黑", 1, 600, 0, 0, 0, "A5", "名称：" + item.ProductName, 1);
                    if (count % 2 == 0)
                    {
                        A_Print_Out(1, 1, 1, 1);
                    }
                }
                if (Barcodes.Count % 2 == 1)
                {
                    A_Print_Out(1, 1, 1, 1);
                }

                A_ClosePrn();

            }
            catch (Exception ex)
            {
                MessageBox.Show("系统无法打印条码，错误如下：" + ex.Message,ex.ToString());
                result=-3;
            }

            return result;
        }


        /// <summary>
        /// 打印货位条码
        /// </summary>
        /// <returns></returns>
        private static int PrintLocations() {
            int result = 0;

            return result;
        }

    }
}
