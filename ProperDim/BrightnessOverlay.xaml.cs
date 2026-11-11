using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using static ProperDim.NativeMethods;

namespace ProperDim;

public partial class BrightnessOverlay : Window
{
	private readonly DispatcherTimer _hideTimer = new() { Interval = TimeSpan.FromSeconds(1.5) };

	public BrightnessOverlay()
	{
		InitializeComponent();
		_hideTimer.Tick += HideTimer_Tick;
	}

	protected override void OnSourceInitialized(EventArgs e)
	{
		base.OnSourceInitialized(e);
		var handle = new WindowInteropHelper(this).Handle;
		int styles = GetWindowLong(handle, GWL_EXSTYLE);
		int overlayStyles = WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE;
		SetWindowLong(handle, GWL_EXSTYLE, styles | overlayStyles);
		if ((GetWindowLong(handle, GWL_EXSTYLE) & overlayStyles) != overlayStyles)
		{
			throw new InvalidOperationException("Unable to configure the brightness overlay as a non-interactive window.");
		}
	}

	public void ShowBrightness(double brightness, bool increasing)
	{
		DirectionText.Text = increasing ? "Brightness increasing" : "Brightness decreasing";
		UpdateBrightness(brightness);

		Rect workArea = SystemParameters.WorkArea;
		Left = Math.Max(workArea.Left, workArea.Right - Width - 24);
		Top = Math.Max(workArea.Top, workArea.Top + (workArea.Height - Height) / 2);

		if (!IsVisible) Show();
		_hideTimer.Stop();
		_hideTimer.Start();
	}

	public void UpdateBrightness(double brightness)
	{
		PercentageText.Text = $"{Math.Round(brightness * 100)}%";
		BrightnessBar.Value = brightness;
	}

	private void HideTimer_Tick(object sender, EventArgs e)
	{
		_hideTimer.Stop();
		Hide();
	}

	protected override void OnClosed(EventArgs e)
	{
		_hideTimer.Stop();
		_hideTimer.Tick -= HideTimer_Tick;
		base.OnClosed(e);
	}
}
