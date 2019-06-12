using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

//[assembly:
//    InternalsVisibleTo("AnyEquation.Droid.CustomRenderers"),
    //internalsvisibleto("xlabs.forms.ios"),
    //internalsvisibleto("xlabs.forms.wp8")
//    ]

namespace AnyEquation.Common
{
    public class ImprovedEntry : Entry
    {
        public static readonly BindableProperty TextPositionProperty =
            BindableProperty.Create(
                "TextPosition",
                typeof(int?),
                typeof(ImprovedEntry),
                null);

        public int? TextPosition
        {
            set { SetValue(TextPositionProperty, value); }
            get { return (int?)GetValue(TextPositionProperty); }
        }

        public static readonly BindableProperty TextToInsertProperty =
            BindableProperty.Create(
                "TextToInsert",
                typeof(string),
                typeof(ImprovedEntry),
                null);

        public string TextToInsert
        {
            set { SetValue(TextToInsertProperty, value); }
            get { return (string)GetValue(TextToInsertProperty); }
        }



        // ---------------------------- The following properties were copied from XLab ExtendedEntry: https://github.com/XLabs/Xamarin-Forms-Labs/wiki/ExtendedEntry

        /// <summary>
        /// The font property
        /// </summary>
        public static readonly BindableProperty FontProperty =
            BindableProperty.Create("Font", typeof(Font), typeof(ImprovedEntry), new Font());

        /// <summary>
        /// The XAlign property
        /// </summary>
        public static readonly BindableProperty XAlignProperty =
            BindableProperty.Create("XAlign", typeof(TextAlignment), typeof(ImprovedEntry),
            TextAlignment.Start);

        /// <summary>
        /// The HasBorder property
        /// </summary>
        public static readonly BindableProperty HasBorderProperty =
            BindableProperty.Create("HasBorder", typeof(bool), typeof(ImprovedEntry), true);


        /// <summary>
        /// The PlaceholderTextColor property
        /// </summary>
        public static readonly BindableProperty PlaceholderTextColorProperty =
            BindableProperty.Create("PlaceholderTextColor", typeof(Color), typeof(ImprovedEntry), Color.Default);

        /// <summary>
        /// The MaxLength property
        /// </summary>
        public static readonly BindableProperty MaxLengthProperty =
            BindableProperty.Create("MaxLength", typeof(int), typeof(ImprovedEntry), int.MaxValue);

        /// <summary>
        /// Gets or sets the MaxLength
        /// </summary>
        public int MaxLength
        {
            get { return (int)this.GetValue(MaxLengthProperty); }
            set { this.SetValue(MaxLengthProperty, value); }
        }

        /// <summary>
        /// Gets or sets the Font
        /// </summary>
        public Font Font
        {
            get { return (Font)GetValue(FontProperty); }
            set { SetValue(FontProperty, value); }
        }

        /// <summary>
        /// Gets or sets the X alignment of the text
        /// </summary>
        public TextAlignment XAlign
        {
            get { return (TextAlignment)GetValue(XAlignProperty); }
            set { SetValue(XAlignProperty, value); }
        }

        /// <summary>
        /// Gets or sets if the border should be shown or not
        /// </summary>
        public bool HasBorder
        {
            get { return (bool)GetValue(HasBorderProperty); }
            set { SetValue(HasBorderProperty, value); }
        }

        /// <summary>
        /// Sets color for placeholder text
        /// </summary>
        public Color PlaceholderTextColor
        {
            get { return (Color)GetValue(PlaceholderTextColorProperty); }
            set { SetValue(PlaceholderTextColorProperty, value); }
        }

        /// <summary>
        /// The left swipe
        /// </summary>
        public EventHandler LeftSwipe;

        /// <summary>
        /// The right swipe
        /// </summary>
        public EventHandler RightSwipe;

        public void OnLeftSwipe(object sender, EventArgs e)
        {
            var handler = this.LeftSwipe;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public void OnRightSwipe(object sender, EventArgs e)
        {
            var handler = this.RightSwipe;
            if (handler != null)
            {
                handler(this, e);
            }
        }

    }
}
