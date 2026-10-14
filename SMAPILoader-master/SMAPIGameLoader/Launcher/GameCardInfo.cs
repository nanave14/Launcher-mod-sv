using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.CardView.Widget;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using static Android.Provider.MediaStore.Audio;

namespace SMAPIGameLoader.Launcher
{
    public class GameInfoCard : CardView
    {
        TextView _gameVersion;
        TextView _launcherVersion, _launcherBuild, _suportGameVersion;

        public GameInfoCard(Context context) : base(context)
        {
            Radius = Ui.Dp(context, 12);
            CardElevation = Ui.Dp(context, 4);
            SetCardBackgroundColor(Color.ParseColor("#1A1D24"));

            var inner = new LinearLayout(context) { Orientation = Orientation.Vertical };
            inner.SetPadding(Ui.Dp(context, 16), Ui.Dp(context, 16), Ui.Dp(context, 16), Ui.Dp(context, 16));
            AddView(inner);

            var title = new TextView(context) { Text = "App Game" };
            title.SetTextSize(ComplexUnitType.Sp, 16);
            title.SetTextColor(Color.White);
            title.SetTypeface(null, TypefaceStyle.Bold);
            inner.AddView(title);

            var buildDateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(int.Parse(AppInfo.BuildString));
            var localDateTimeOffset = buildDateTimeOffset.ToLocalTime();
            var localDateTimeString = localDateTimeOffset.ToString("HH:mm:ss dd/MM/yyyy");
            var launcherBuildTextString = $"Build: {localDateTimeString} (d/m/y)";

            _gameVersion = CreateTextView("Game Version: " + StardewApkTool.CurrentGameVersion, 14, "#9AA0A6");
            _launcherVersion = CreateTextView("Launcher Version: " + AppInfo.VersionString, 14, "#9AA0A6");
            _launcherBuild = CreateTextView(launcherBuildTextString, 14, "#9AA0A6");
            _suportGameVersion = CreateTextView($"Support Game Version: {StardewApkTool.GameVersionSupport} Or Later", 14, "#9AA0A6");

            var lines = new List<TextView> { _gameVersion, _suportGameVersion,
                _launcherVersion, _launcherBuild };
            foreach (var tv in lines)
            {
                inner.AddView(tv, new LayoutParams(
                    ViewGroup.LayoutParams.MatchParent,
                    ViewGroup.LayoutParams.WrapContent)
                { TopMargin = Ui.Dp(context, 4) });
            }

        }
        TextView CreateTextView(string text, int textSizeSp, string textColorHex, TypefaceStyle typefaceStyle = TypefaceStyle.Normal)
        {
            var textView = new TextView(Context) { Text = text };
            textView.SetTextSize(ComplexUnitType.Sp, textSizeSp);
            textView.SetTextColor(Color.ParseColor(textColorHex));
            textView.SetTypeface(null, typefaceStyle);
            return textView;
        }
    }
}
