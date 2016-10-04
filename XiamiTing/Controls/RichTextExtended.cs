using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using HtmlAgilityPack;
using Windows.UI.Xaml.Media.Imaging;
using JacobC.Xiami.Models;
using Template10.Services.NavigationService;
using System.Windows.Input;
using JacobC.Xiami.Views;
using Windows.UI.Xaml.Media;

namespace JacobC.Xiami.Controls
{
    public class RichTextExtended
    {
        #region HtmlToXaml
        //From http://michaelsync.net/2009/06/09/bindable-wpf-richtext-editor-with-xamlhtml-convertor

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
        /// 标识<see cref="Document"/>依赖属性
        /// </summary>
        public static readonly DependencyProperty DocumentProperty = DependencyProperty.RegisterAttached("Document",
            typeof(string), typeof(RichTextExtended), new PropertyMetadata(null, OnDocumentChanged));
        private static void OnDocumentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBlock)
                LoadRichText((d as TextBlock).Inlines, e.NewValue as string);
            else if (d is Paragraph)
                LoadRichText((d as Paragraph).Inlines, e.NewValue as string);
            else if (d is Span)
                LoadRichText((d as Span).Inlines, e.NewValue as string);
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
                        continue;
                    case "strong":
                        style = style | ParsingStyle.Bold;
                        break;
                    //链接
                    case "a":
                        var link = new Hyperlink();
                        ParseRelativeUrl(item.GetAttributeValue("href", ""), link, App.Current.NavigationService);
                        Analyse(item, link.Inlines, style);
                        container.Add(link);
                        continue;
                    //图片
                    case "img":
                        var image = new Image() { Source = new BitmapImage(new Uri(item.GetAttributeValue("src", ""))) };
                        var cont = new InlineUIContainer();
                        cont.Child = image;
                        container.Add(cont);
                        continue;
                    //换行
                    case "br":
                        container.Add(new LineBreak());
                        continue;
                    //容器
                    case "span":
                    case "div":
                    case "p":
                        break;
                    case "b":
                        //TODO: 设置属性，是否要加红搜索显示结果
                        if(node.GetAttributeValue("class","") == "key_red")
                        {
                            Span p = new Span();
                            p.Foreground = new SolidColorBrush(Colors.Red);
                            Analyse(item, container, style);
                            continue;
                        }
                        else break;
                    //非文本
                    case "button":
                        continue;
                    default:
                        continue;
                }
                Analyse(item, container, style);
            }
        }
        internal static void ParseRelativeUrl(string url, Hyperlink linkobject, INavigationService nav)
        {
            if (url == null)
                return;
            else if (url.Length <= 1)
                return;
            else if (url.StartsWith("/"))
            {
                var data = url.Split('/');
                switch (data[1])
                {
                    case "song":
                        //导航SongPage
                        break;
                    case "album":
                        linkobject.Click += (sender, e) =>
                              nav.Navigate(typeof(AlbumPage), uint.Parse(data[2]));
                        break;
                    case "artist":
                        linkobject.Click += (sender, e) =>
                              nav.Navigate(typeof(ArtistPage), uint.Parse(data[2]));
                        break;
                }
            }
            else if (url.IndexOf("www.xiami.com") >= 0)
                linkobject.NavigateUri = new Uri(url);
            else
#if DEBUG
                System.Diagnostics.Debugger.Break();
#else
                throw new InvalidCastException("Url地址格式转换错误");
#endif

        }

        [Flags]
        internal enum ParsingStyle
        {
            Normal = 0,
            Bold = 1,
            Italic = 2,
            Underline = 4
        }
        #endregion

        #region Genres

        /// <summary>
        /// 标识<see cref="Genres"/>依赖属性
        /// </summary>
        public static readonly DependencyProperty GenresProperty = DependencyProperty.RegisterAttached("Genres",
            typeof(IEnumerable<GenreModel>), typeof(RichTextExtended), new PropertyMetadata(null, OnGenresChanged));
        private static void OnGenresChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            InlineCollection icollect;
            if (d is TextBlock)
                icollect = (d as TextBlock).Inlines;
            else if (d is Span)
                icollect = (d as Span).Inlines;
            else return;
            icollect.Clear();
            bool first = true;
            foreach (var item in e.NewValue as IEnumerable<GenreModel>)
            {
                if (first)
                    first = false;
                else
                    icollect.Add(new Run() { Text = " , " });
                Hyperlink link = new Hyperlink();
                link.Inlines.Add(new Run() { Text = item.Name });
                link.Click += (sender, args) => GetNavigate(d)?.Execute(item);
                icollect.Add(link);
            }
        }

        public static IEnumerable<GenreModel> GetGenres(DependencyObject dp)
        {
            return dp.GetValue(GenresProperty) as IEnumerable<GenreModel>;
        }
        public static void SetGenres(DependencyObject dp, IEnumerable<GenreModel> genres)
        {
            dp.SetValue(GenresProperty, genres);
        }


        /// <summary>
        /// 标识<see cref="Navigate"/>依赖属性
        /// </summary>
        public static readonly DependencyProperty NavigateProperty = DependencyProperty.RegisterAttached("Navigate",
            typeof(ICommand), typeof(RichTextExtended), new PropertyMetadata(null));

        public static ICommand GetNavigate(DependencyObject dp)
        {
            return dp.GetValue(NavigateProperty) as ICommand;
        }
        public static void SetNavigate(DependencyObject dp, ICommand nav)
        {
            dp.SetValue(NavigateProperty, nav);
        }

        #endregion
    }
}
