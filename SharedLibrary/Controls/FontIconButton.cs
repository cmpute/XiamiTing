using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;


namespace JacobC.Xiami.Controls
{
    public sealed class FontIconButton : Button
    {
        FontIcon Icon;
        public FontIconButton()
        {
            Icon = new FontIcon();
            Icon.Glyph = NormalGlyph;
            this.Content = Icon;
        }

        public string NormalGlyph
        {
            get { return (string)GetValue(NormalGlyphProperty); }
            set { SetValue(NormalGlyphProperty, value); }
        }
        public static readonly DependencyProperty NormalGlyphProperty =
            DependencyProperty.Register("NormalGlyph", typeof(string), typeof(FontIconButton), new PropertyMetadata(null));

        public string HoverGlyph
        {
            get { return (string)GetValue(HoverGlyphProperty); }
            set { SetValue(HoverGlyphProperty, value); }
        }
        public static readonly DependencyProperty HoverGlyphProperty =
            DependencyProperty.Register("HoverGlyph", typeof(string), typeof(FontIconButton), new PropertyMetadata(null,(d,e)=> {
                if ((d as FontIconButton).GetValue(HoverGlyphProperty) == null)
                    (d as FontIconButton).SetValue(HoverGlyphProperty, e.NewValue);
            }));

        public Brush NormalForground
        {
            get { return (Brush)GetValue(NormalForgroundProperty); }
            set { SetValue(NormalForgroundProperty, value); }
        }
        public static readonly DependencyProperty NormalForgroundProperty =
            DependencyProperty.Register("NormalForground", typeof(Brush), typeof(FontIconButton), new PropertyMetadata(null));

        public Brush HoverForground
        {
            get { return (Brush)GetValue(HoverForgroundProperty); }
            set { SetValue(HoverForgroundProperty, value); }
        }
        public static readonly DependencyProperty HoverForgroundProperty =
            DependencyProperty.Register("HoverForground", typeof(Brush), typeof(FontIconButton), new PropertyMetadata(null));

        public Brush PressedForground
        {
            get { return (Brush)GetValue(PressedForgroundProperty); }
            set { SetValue(PressedForgroundProperty, value); }
        }
        public static readonly DependencyProperty PressedForgroundProperty =
            DependencyProperty.Register("PressedForground", typeof(Brush), typeof(FontIconButton), new PropertyMetadata(null));

        public Brush DisabledForground
        {
            get { return (Brush)GetValue(DisabledForgroundProperty); }
            set { SetValue(DisabledForgroundProperty, value); }
        }
        public static readonly DependencyProperty DisabledForgroundProperty =
            DependencyProperty.Register("DisabledForground", typeof(Brush), typeof(FontIconButton), new PropertyMetadata(null));
    }
}
