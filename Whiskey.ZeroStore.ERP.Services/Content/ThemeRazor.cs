using RazorEngine.Configuration;
using RazorEngine.Templating;
using System;
using Whiskey.ZeroStore.ERP.Models;

namespace Whiskey.ZeroStore.ERP.Services.Content
{
    public static class ThemeRazor
    {
        private static IRazorEngineService service = RazorEngineService.Create(new TemplateServiceConfiguration());

        public static void AllCompile(TemplateTheme theme)
        {
            service = RazorEngineService.Create(new TemplateServiceConfiguration());//必须Create否则会造成键已存在
            //string NavTemplateName = Enum.GetName(typeof(ThemeRazorType), ThemeRazorType.NavContent);
            //string MenuTemplateName = Enum.GetName(typeof(ThemeRazorType), ThemeRazorType.MenuContent);
            //string FooterTemplateName = Enum.GetName(typeof(ThemeRazorType), ThemeRazorType.FooterContent);
            //service.Compile(theme.NavContent ?? string.Empty, NavTemplateName, null);
            //service.Compile(theme.MenuContent ?? string.Empty, MenuTemplateName, null);
            //service.Compile(theme.FooterContent ?? string.Empty, FooterTemplateName, null);
        }
        /// <summary>
        /// 编译通过返回True
        /// </summary>
        /// <param name="themeContent"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public static bool TryCompile(string themeContent,object model)
        {
            try
            {
                RazorEngine.Razor.Parse(themeContent ?? string.Empty, model);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static string Run(ThemeRazorType themetype, object model = null)
        {
            string templateName = Enum.GetName(typeof(ThemeRazorType), themetype);
            return service.Run(templateName, null, model, null);
        }
    }
    public enum ThemeRazorType
    {
        NavContent,
        MenuContent,
        FooterContent
    }
}