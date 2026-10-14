using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Util;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMAPIGameLoader.Launcher;

public static class Ui
{
    public static int Dp(Context context, float dp) =>
        (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, dp, context.Resources!.DisplayMetrics!);


}
