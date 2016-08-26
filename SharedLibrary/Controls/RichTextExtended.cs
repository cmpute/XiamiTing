using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using HtmlAgilityPack;
using Windows.UI.Xaml.Media.Imaging;

namespace JacobC.Xiami.Controls
{
    //From http://michaelsync.net/2009/06/09/bindable-wpf-richtext-editor-with-xamlhtml-convertor
    public class RichTextExtended
    {
        //没有这两个访问器无法使依赖属性生效
        public static string GetDocument(DependencyObject dp)
        {
            return (string)dp.GetValue(DocumentProperty);
        }
        public static void SetDocument(DependencyObject dp, string document)
        {
            dp.SetValue(DocumentProperty, document);
        }

        /// <summary>
        /// 标识<see cref="Documet"/>依赖属性
        /// </summary>
        public static readonly DependencyProperty DocumentProperty = DependencyProperty.RegisterAttached("Document",
            typeof(string), typeof(RichTextExtended), new PropertyMetadata(null, OnDocumentChanged));
        private static void OnDocumentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBlock)
                LoadRichText((d as TextBlock).Inlines, e.NewValue as string);
            else if (d is Paragraph)
                LoadRichText((d as Paragraph).Inlines, e.NewValue as string);
        }

        public static void LoadRichText(InlineCollection container, string content)
        {
            container.Clear();
            if (content == null) return;
            HtmlDocument d = new HtmlDocument();
            d.LoadHtml(content);
            Analyse(d.DocumentNode, container, ParsingStyle.Normal);
        }
        
        internal static void Analyse(HtmlNode node, InlineCollection container, ParsingStyle style)
        {
            if (node == null)
                return;
            foreach (var item in node.ChildNodes)
            {
                switch (item.Name)
                {
                    //文本
                    case "#text":
                        container.Add(new Run() { Text = item.InnerText });
                        return;
                    case "strong":
                        style = style | ParsingStyle.Bold;
                        break;
                    //链接
                    case "a":
                        var link = new Hyperlink();
                        link.NavigateUri = new Uri(node.GetAttributeValue("href", ""));//针对链接需要进行分析
                        Analyse(item, link.Inlines, style);
                        container.Add(link);
                        return;
                    //图片
                    case "img":
                        var image = new Image() { Source = new BitmapImage(new Uri(node.GetAttributeValue("src", ""))) };
                        var cont = new InlineUIContainer();
                        cont.Child = image;
                        container.Add(cont);
                        return;
                    //换行
                    case "br":
                        container.Add(new LineBreak());
                        return;
                    //容器
                    case "span":
                    case "div":
                    case "p":
                        break;
                    //非文本
                    case "button":
                        return;
                    default:
                        return;
                }
                Analyse(item, container, style);
            }
        }
        [Flags]
        internal enum ParsingStyle
        {
            Normal = 0,
            Bold = 1,
            Italic = 2,
            Underline = 4
        }
    }
}
