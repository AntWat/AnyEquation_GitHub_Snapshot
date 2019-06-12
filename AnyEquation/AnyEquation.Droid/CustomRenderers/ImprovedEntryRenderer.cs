using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using AnyEquation.Common;

using Android.Util;
using Android.Widget;
using Android.Views;
using System.ComponentModel;



using System;

using Android.Graphics;

using Android.Graphics.Drawables;

using Android.Graphics.Drawables.Shapes;

using Android.Text;

using Android.Text.Method;

using XLabs.Forms.Controls;
using XLabs.Forms.Extensions;

using Color = Xamarin.Forms.Color;


[assembly: ExportRenderer(typeof(ImprovedEntry),
                          typeof(AnyEquation.Droid.CustomRenderers.ImprovedEntryRenderer))]


namespace AnyEquation.Droid.CustomRenderers
{
    public class ImprovedEntryRenderer : EntryRenderer      // ViewRenderer<ImprovedEntry, ImprovedEditText>
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Entry> args)
        {
            bool bAddEvent = (Control == null);

            base.OnElementChanged(args);

            if (bAddEvent)
            {
                //Control.TextChanged += TextChangedEvent;
                //Control.Touch += TouchEvent;
            }

            XLabs_OnElementChanged(args);

        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == ImprovedEntry.TextPositionProperty.PropertyName)
            {
                ImprovedEntry improvedEntry = Element as ImprovedEntry;

                if ((improvedEntry != null) && (improvedEntry.TextPosition != null))
                {
                    int iTextPosition = improvedEntry.TextPosition ?? 0;

                    if (iTextPosition > Control.Text.Length)
                    {
                        iTextPosition = Control.Text.Length;
                    }
                    else if (iTextPosition < 0)
                    {
                        iTextPosition = 0;
                    }

                    if (iTextPosition != Control.SelectionStart)
                    {
                        Control.SetSelection(iTextPosition);
                    }
                }
            }

            if (e.PropertyName == ImprovedEntry.TextToInsertProperty.PropertyName)
            {
                ImprovedEntry improvedEntry = Element as ImprovedEntry;

                if ((improvedEntry != null) && (improvedEntry.TextToInsert?.Length>0))
                {
                    int iSelectionStart = Control.SelectionStart;
                    bool bAtEnd = (iSelectionStart == Control.Text.Length);
                    string sText = Control.Text.Substring(0, iSelectionStart) + improvedEntry.TextToInsert + Control.Text.Substring(iSelectionStart);
                    Control.Text = sText;

                    //if (!bAtEnd)
                    //{
                        Control.SetSelection(iSelectionStart + improvedEntry.TextToInsert.Length);
                    //}

                    improvedEntry.TextToInsert = null;
                }
            }

            XLabs_OnElementPropertyChanged(sender, e);
        }


        void TextChangedEvent(object sender, Android.Text.TextChangedEventArgs e)
        {
            NotifyTextPosition();
            //e.Handled = false;
        }
        void TouchEvent(object sender, Android.Views.View.TouchEventArgs e)
        {
            NotifyTextPosition();
            e.Handled = false;
        }

        private void NotifyTextPosition()
        { 
            int iSelectionStart = Control.SelectionStart;
            int iSelectionEnd = Control.SelectionEnd;

            ImprovedEntry improvedEntry = Element as ImprovedEntry;

            if (improvedEntry != null)
            {
                improvedEntry.TextPosition = iSelectionStart;
            }

        }


        // ---------------------------- The following properties were copied from XLab ImprovedEntry: https://github.com/XLabs/Xamarin-Forms-Labs/wiki/ImprovedEntry

        private const int MinDistance = 10;

        private float downX, downY, upX, upY;

        private Drawable originalBackground;

        /// <summary>
        /// Called when [element changed].
        /// </summary>
        /// <param name="e">The e.</param>
        private void XLabs_OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            //base.OnElementChanged(e);

            var view = (ImprovedEntry)Element;

            if (Control != null && e.NewElement != null && e.NewElement.IsPassword)
            {
                Control.SetTypeface(Typeface.Default, TypefaceStyle.Normal);
                Control.TransformationMethod = new PasswordTransformationMethod();
            }

            if (originalBackground == null)
                originalBackground = Control.Background;

            SetFont(view);

            SetTextAlignment(view);

            //SetBorder(view);

            SetPlaceholderTextColor(view);

            SetMaxLength(view);

            if (e.NewElement == null)
            {
                this.Touch -= HandleTouch;
            }

            if (e.OldElement == null)
            {
                this.Touch += HandleTouch;
            }
        }


        /// <summary>
        /// Handles the touch.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="Android.Views.View.TouchEventArgs"/> instance containing the event data.</param>
        void HandleTouch(object sender, TouchEventArgs e)
        {
            var element = (ImprovedEntry)this.Element;

            switch (e.Event.Action)
            {
                case MotionEventActions.Down:
                    this.downX = e.Event.GetX();
                    this.downY = e.Event.GetY();
                    return;

                case MotionEventActions.Up:

                case MotionEventActions.Cancel:

                case MotionEventActions.Move:
                    this.upX = e.Event.GetX();
                    this.upY = e.Event.GetY();

                    float deltaX = this.downX - this.upX;

                    float deltaY = this.downY - this.upY;

                    // swipe horizontal?
                    if (Math.Abs(deltaX) > Math.Abs(deltaY))
                    {
                        if (Math.Abs(deltaX) > MinDistance)
                        {
                            if (deltaX < 0)
                            {
                                element.OnRightSwipe(this, EventArgs.Empty);
                                return;
                            }

                            if (deltaX > 0)
                            {
                                element.OnLeftSwipe(this, EventArgs.Empty);
                                return;
                            }
                        }
                        else
                        {
                            Log.Info("ImprovedEntry", "Horizontal Swipe was only " + Math.Abs(deltaX) + " long, need at least " + MinDistance);
                            return; // We don't consume the event
                        }
                    }

                    // swipe vertical?
                    //                    else 
                    //                    {
                    //                        if(Math.abs(deltaY) > MIN_DISTANCE){
                    //                            // top or down
                    //                            if(deltaY < 0) { this.onDownSwipe(); return true; }
                    //                            if(deltaY > 0) { this.onUpSwipe(); return true; }
                    //                        }
                    //                        else {
                    //                            Log.i(logTag, "Vertical Swipe was only " + Math.abs(deltaX) + " long, need at least " + MIN_DISTANCE);
                    //                            return false; // We don't consume the event
                    //                        }
                    //                    }

                    return;
            }
        }



        /// <summary>
        /// Handles the <see cref="E:ElementPropertyChanged" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void XLabs_OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var view = (ImprovedEntry)Element;

            if (e.PropertyName == ImprovedEntry.FontProperty.PropertyName)
            {
                SetFont(view);
            }
            if (e.PropertyName == ImprovedEntry.XAlignProperty.PropertyName)
            {
                SetTextAlignment(view);
            }
            else if (e.PropertyName == ImprovedEntry.HasBorderProperty.PropertyName)
            {
                //return;   
            }
            else if (e.PropertyName == ImprovedEntry.PlaceholderTextColorProperty.PropertyName)
            {
                SetPlaceholderTextColor(view);
            }
            else
            {
                //base.OnElementPropertyChanged(sender, e);

                if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName)
                {
                    this.Control.SetBackgroundColor(view.BackgroundColor.ToAndroid());
                }
            }
        }



        ///// <summary>
        ///// Sets the border.
        ///// </summary>
        ///// <param name="view">The view.</param>
        private void SetBorder(ImprovedEntry view)
        {
            if (view.HasBorder == false)
            {
                var shape = new ShapeDrawable(new RectShape());

                shape.Paint.Alpha = 0;
                shape.Paint.SetStyle(Paint.Style.Stroke);
                Control.SetBackgroundDrawable(shape);
            }
            else
            {
                Control.SetBackground(originalBackground);
            }
        }


        /// <summary>
        /// Sets the text alignment.
        /// </summary>
        /// <param name="view">The view.</param>
        private void SetTextAlignment(ImprovedEntry view)
        {
            switch (view.XAlign)
            {
                case Xamarin.Forms.TextAlignment.Center:
                    Control.Gravity = GravityFlags.CenterHorizontal;
                    break;

                case Xamarin.Forms.TextAlignment.End:
                    Control.Gravity = GravityFlags.End;
                    break;

                case Xamarin.Forms.TextAlignment.Start:
                    Control.Gravity = GravityFlags.Start;
                    break;
            }
        }



        /// <summary>
        /// Sets the font.
        /// </summary>
        /// <param name="view">The view.</param>
        private void SetFont(ImprovedEntry view)
        {
            if (view.Font != Font.Default)
            {
                Control.TextSize = view.Font.ToScaledPixel();
                Control.Typeface = view.Font.ToExtendedTypeface(Context);
            }
        }


        /// <summary>
        /// Sets the color of the placeholder text.
        /// </summary>
        /// <param name="view">The view.</param>
        private void SetPlaceholderTextColor(ImprovedEntry view)
        {
            if (view.PlaceholderTextColor != Color.Default)
            {
                Control.SetHintTextColor(view.PlaceholderTextColor.ToAndroid());
            }
        }

        /// <summary>
        /// Sets the MaxLength characteres.
        /// </summary>
        /// <param name="view">The view.</param>
        private void SetMaxLength(ImprovedEntry view)
        {
            Control.SetFilters(new IInputFilter[] { new InputFilterLengthFilter(view.MaxLength) });
        }

    }
}
