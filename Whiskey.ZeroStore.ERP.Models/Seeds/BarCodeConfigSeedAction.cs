using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using Whiskey.Core.Data.Entity.Migrations;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class BarCodeConfigSeedAction : ISeedAction
    {
        /// <summary>
        /// 获取 操作排序，数值越小越先执行
        /// </summary>
        public int Order { get { return 0; } }

        public void Action(DbContext context)
        {
            List<BarCodeConfig> list = new List<BarCodeConfig>()
            {
                new BarCodeConfig() {DIYBrand="0FASHION",IsDefaultBrand=true,PrinterPaperDirection= Enums.PrinterPaperDirection._横版,PrinterPaperType= Enums.PrinterPaperType._30_80 },
                new BarCodeConfig() {DIYBrand="0FASHION",IsDefaultBrand=true,PrinterPaperDirection= Enums.PrinterPaperDirection._横版,PrinterPaperType= Enums.PrinterPaperType._40_80 },
                new BarCodeConfig() {DIYBrand="0FASHION",IsDefaultBrand=true,PrinterPaperDirection= Enums.PrinterPaperDirection._竖版,PrinterPaperType= Enums.PrinterPaperType._40_80 },
                new BarCodeConfig() {DIYBrand="0FASHION",IsDefaultBrand=true,PrinterPaperDirection= Enums.PrinterPaperDirection._竖版,PrinterPaperType= Enums.PrinterPaperType._50_120 },
            };

            context.Set<BarCodeConfig>().AddOrUpdate(list.ToArray());
            context.SaveChanges();
        }
    }
}