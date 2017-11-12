using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace Whiskey.Utility.Graphic
{
    //操作图片
    //yxk 2015-12-30
   public class GraphicHelper
    {
       public static string[] ReDraw(string savePath,bool exisRedraw,params string[] origPath) {
           List<string> retpath = new List<string>(); 
           if (origPath != null && origPath.Count() > 0)
           {
               string filepath = "";
               foreach (var item in origPath)
               {
                   string basepath = AppDomain.CurrentDomain.BaseDirectory;
                   if (!Directory.Exists(basepath + savePath))
                       Directory.CreateDirectory(basepath + savePath);
                    string imgName=Path.GetFileName(item);
                    string fullsavepath = basepath + savePath + "/" + imgName;
                    if (File.Exists(fullsavepath) && !exisRedraw)
                    {
                        filepath = savePath + "/" + imgName;
                        retpath.Add(filepath);
                        continue;
                    }
                   string pat = basepath + item;
                   if(!File.Exists(pat)) continue;
                   Bitmap bp = new Bitmap(pat);
                   Bitmap bp1 = new Bitmap(40, 40);
                    Graphics grp = Graphics.FromImage(bp1);
                   // grp.Clear(Color.White);
                    //grp.DrawImage(bp, new Point[] { new Point() { X = 0, Y = 0 }, new Point() { X = 0, Y = 10 }, new Point() { X = 10, Y = 0 } }, new Rectangle() { X = bp.Width / 2 - 2, Y = bp.Height / 2 - 2, Width = 2, Height = 2 }, GraphicsUnit.Pixel);
                    grp.DrawImage(bp, new Point[]{
                       new Point(){X=0,Y=0},
                       new Point(){X=40,Y=0},
                       new Point(){X=0,Y=40}
                    }, new Rectangle() {X=bp.Width/2-20,Y=bp.Height/2-20,Width=40,Height=40 },GraphicsUnit.Pixel);
                   // grp.DrawImage(bp, new Rectangle() { X = 0, Y = 0, Height = 3, Width = 3 });
                  
                   filepath=savePath+"/"+imgName;
                   retpath.Add(filepath);
                   bp1.Save(basepath + filepath);
                   bp.Dispose();
                   bp1.Dispose();
               }
           }
           return retpath.ToArray();
       }
    }
}
