using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using AndroidX.CardView.Widget;
using AndroidX.RecyclerView.Widget;
using System;
using Android.Util;

namespace SMAPIGameLoader.Launcher;

internal class ModViewHolder : RecyclerView.ViewHolder
{
    private readonly TextView _name;
    private readonly TextView _version;
    private readonly SwitchCompat _toggle;
    private readonly Action<ModInfo> _onToggled;
    private readonly Action<ModInfo, View> _onClicked;
    private ModInfo? _mod;
    private bool _binding;

    private ModViewHolder(View root, TextView name, TextView version,
                          SwitchCompat toggle, Action<ModInfo> onToggled,
                          Action<ModInfo, View> onClicked) : base(root)
    {
        _name = name;
        _version = version;
        _toggle = toggle;
        _onToggled = onToggled;
        _onClicked = onClicked;

        _toggle.CheckedChange += (s, e) =>
        {
            if (_binding || _mod == null) return;
            if (_mod.Enabled == e.IsChecked) return;   // ไม่มีการเปลี่ยนจริง ข้าม
            _mod.Enabled = e.IsChecked;
            _onToggled(_mod);

            //assert
            _toggle.Checked = _mod.Enabled;
        };

        root.Click += (s, e) =>
        {
            if (_mod != null) _onClicked(_mod, root);
        };
    }

    public static ModViewHolder Create(Context context,
                                       Action<ModInfo> onToggled,
                                       Action<ModInfo, View> onClicked)
    {
        // Card ด้านนอก
        var card = new CardView(context)
        {
            Radius = Ui.Dp(context, 12),
            CardElevation = Ui.Dp(context, 4),
            UseCompatPadding = true
        };
        card.SetCardBackgroundColor(Color.ParseColor("#1A1D24"));
        card.LayoutParameters = new RecyclerView.LayoutParams(
            ViewGroup.LayoutParams.MatchParent,
            ViewGroup.LayoutParams.WrapContent)
        {
            BottomMargin = Ui.Dp(context, 12)
        };

        // แถวภายใน card: ซ้าย (ชื่อ+path) | ขวา (version+switch)
        var row = new LinearLayout(context) { Orientation = Orientation.Horizontal };
        row.SetGravity(GravityFlags.CenterVertical);
        row.SetPadding(Ui.Dp(context, 16), Ui.Dp(context, 14),
                       Ui.Dp(context, 16), Ui.Dp(context, 14));
        card.AddView(row);

        // Left
        var left = new LinearLayout(context) { Orientation = Orientation.Vertical };
        left.SetGravity(GravityFlags.Left);

        var name = new TextView(context);
        name.SetTextSize(ComplexUnitType.Sp, 16);
        name.SetTextColor(Color.White);
        name.SetTypeface(null, TypefaceStyle.Bold);

        var version = new TextView(context);
        version.SetTextSize(ComplexUnitType.Sp, 12);
        version.SetTextColor(Color.ParseColor("#9AA0A6"));
        version.LayoutParameters = new LinearLayout.LayoutParams(
            ViewGroup.LayoutParams.MatchParent,
            ViewGroup.LayoutParams.WrapContent)
        { TopMargin = Ui.Dp(context, 2) };

        left.AddView(name);
        left.AddView(version);

        // Right
        var toggle = new SwitchCompat(context);

        row.AddView(left, new LinearLayout.LayoutParams(0, ViewGroup.LayoutParams.WrapContent, 1f));
        row.AddView(toggle);

        return new ModViewHolder(card, name, version, toggle, onToggled, onClicked);
    }

    public void Bind(ModInfo mod)
    {
        _mod = mod;
        _name.Text = mod.NameUIText;
        _version.Text = mod.VersionUIText;

        _binding = true;
        _toggle.Checked = mod.Enabled;
        _binding = false;
    }
}