namespace LabApp;

/// <summary>
/// Main logic class for the "BARHO" application.
/// Implements the colorimetric transformation algorithm (Odstín/Sytost).
/// </summary>
public partial class MainPage : ContentPage
{
	/// <summary>
	/// Hardcoded spectral data (3x27 matrix).
	/// These values represent the spectral boundaries used in the calculation.
	/// </summary>
	private static readonly double[,] fst = new double[3, 27]
	{
		{ 55.409, 54.945, 53.249, 51.390, 48.887, 46.063, 43.319, 43.080, 44.149, 48.573, 52.746, 41.172, 32.133, 26.956, 23.532, 22.043, 20.958, 21.844, 25.542, 26.757, 29.346, 34.081, 39.831, 49.469, 55.409, 54.945, 53.249 },
		{ 62.178, 55.560, 47.830, 42.206, 36.496, 31.155, 26.682, 25.144, 24.932, 26.079, 27.268, 21.786, 18.155, 16.496, 16.138, 16.874, 20.909, 27.351, 39.291, 46.760, 51.719, 56.476, 60.241, 64.565, 62.178, 55.560, 47.830 },
		{ 12.550, 9.610, 7.557, 6.794, 6.267, 6.779, 8.363, 13.249, 19.678, 39.024, 73.947, 79.064, 79.492, 79.633, 79.783, 79.918, 80.198, 80.373, 80.467, 65.558, 49.853, 36.339, 26.364, 17.854, 12.550, 9.610, 7.557 }
	};

	private double[,] f4st = new double[3, 4];
	private double[] fint = new double[3];

	public MainPage()
	{
		InitializeComponent();
	}

	/// <summary>
	/// Handles page initialization, focuses the input and positions the cursor.
	/// Also subscribes to window activation events.
	/// </summary>
	protected override void OnAppearing()
	{
		base.OnAppearing();
		
		if (Window != null)
		{
			Window.Activated += OnWindowActivated;
		}

		// Focus the X input and place cursor after the pre-filled "0,"
		inputX.Focus();
		inputX.CursorPosition = inputX.Text.Length;
	}

	protected override void OnDisappearing()
	{
		base.OnDisappearing();
		if (Window != null)
		{
			Window.Activated -= OnWindowActivated;
		}
	}

	/// <summary>
	/// Restores focus to the reset button when the window regains focus.
	/// Uses Dispatcher to ensure focus is set correctly on the UI thread after activation.
	/// </summary>
	private void OnWindowActivated(object? sender, EventArgs e)
	{
		if (btnNextSample.IsVisible)
		{
			Dispatcher.Dispatch(() => 
			{
				btnNextSample.Focus();
			});
		}
	}

	/// <summary>
	/// Moves focus from X input to Y input and positions the cursor.
	/// </summary>
	private void OnInputXCompleted(object? sender, EventArgs e)
	{
		inputY.Focus();
		inputY.CursorPosition = inputY.Text.Length;
	}

	/// <summary>
	/// Triggers the calculation when Enter is pressed in the Y input and focuses the reset button.
	/// </summary>
	private async void OnInputYCompleted(object? sender, EventArgs e)
	{
		OnCalculateClicked(null, EventArgs.Empty);
		
		btnNextSample.IsVisible = true;
		
		// Animations for a premium feel
		await Task.WhenAll(
			resultsFrame.FadeTo(1, 400, Easing.CubicOut),
			btnNextSample.FadeTo(1, 400, Easing.CubicOut)
		);
		
		btnNextSample.Focus();
	}

	/// <summary>
	/// Resets the form for the next sample.
	/// </summary>
	private async void OnNextSampleClicked(object? sender, EventArgs e)
	{
		// Fade out before reset
		await Task.WhenAll(
			resultsFrame.FadeTo(0, 250, Easing.CubicIn),
			btnNextSample.FadeTo(0, 250, Easing.CubicIn)
		);

		// Reset inputs
		inputX.Text = "0,";
		inputY.Text = "0,";
		
		// Clear results
		txtOdstinResult.Text = "";
		txtSytostResult.Text = "";
		
		// Hide this button
		btnNextSample.IsVisible = false;
		
		// Focus back to X
		inputX.Focus();
		inputX.CursorPosition = inputX.Text.Length;
	}

	/// <summary>
	/// Primary calculation logic triggered by the "VYPOČÍTAT" button.
	/// </summary>
	private void OnCalculateClicked(object? sender, EventArgs e)
	{
		// Sanitize inputs - replace commas with dots for universal double parsing
		string strX = inputX.Text?.Trim().Replace(',', '.') ?? "";
		string strY = inputY.Text?.Trim().Replace(',', '.') ?? "";

		if (double.TryParse(strX, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double xan) && 
			double.TryParse(strY, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double yan))
		{
			// Reference white point and transformation constants
			double xu = 98.07;
			double yu = 100;
			double zu = 118.22;
			double uo = 0.210526;
			double vo = 0.473684;

			// Coordinate normalization
			double x = xan / yan;
			double y = 1.0;
			double z = (1.0 - xan - yan) / yan;

			// Cross-product vectors for hue determination
			double fxu1 = (y * zu) - (z * yu);
			double fxu2 = (z * xu) - (x * zu);
			double fxu3 = (x * yu) - (y * xu);

			double dint, dins;

			// Check for distance from white point to avoid division by zero
			if (Math.Abs(fxu1) + Math.Abs(fxu2) + Math.Abs(fxu3) > 0.05)
			{
				int iton = 16;
				UpdateF4(iton);

				// Iteratively search for the correct segment in the spectral boundary
				while ((f4st[0, 2] * fxu1 + f4st[1, 2] * fxu2 + f4st[2, 2] * fxu3) <= 0)
				{
					iton++;
					UpdateF4(iton);
				}

				while ((f4st[0, 1] * fxu1 + f4st[1, 1] * fxu2 + f4st[2, 1] * fxu3) > 0)
				{
					iton--;
					UpdateF4(iton);
				}

				if (iton >= 25) iton -= 24;

				// Binary search for precise point on the boundary curve
				double x1 = 0;
				double x2 = 1;
				double tx = 0;

				for (int m = 1; m <= 12; m++)
				{
					tx = (x1 + x2) * 0.5;
					UpdatePol3(tx);
					if ((fint[0] * fxu1 + fint[1] * fxu2 + fint[2] * fxu3) <= 0)
						x1 = tx;
					else
						x2 = tx;
				}

				dint = iton + tx; // Final Odstín (Hue) result

				// Calculation of Dins (Saturation/Sytost)
				double xanz = x / (x + y * xu / yu + z * xu / zu);
				double yanz = y / (x * yu / xu + y + z * yu / zu);

				UpdatePol3(tx);
				double sumfz = fint[0] / xu + fint[1] / yu + fint[2] / zu;
				double xan6z = fint[0] / (xu * sumfz);
				double yan6z = fint[1] / (yu * sumfz);

				// Transformation to u', v' coordinates
				double uan6z = 4 * xan6z / (-2 * xan6z + 12 * yan6z + 3) - uo;
				double van6z = 9 * yan6z / (-2 * xan6z + 12 * yan6z + 3) - vo;
				double uanz = 4 * xanz / (-2 * xanz + 12 * yanz + 3) - uo;
				double vanz = 9 * yanz / (-2 * xanz + 12 * yanz + 3) - vo;

				// Saturation ratio calculation
				double r = Math.Sqrt(uanz * uanz + vanz * vanz);
				double r6 = Math.Sqrt(uan6z * uan6z + van6z * van6z);
				dins = 6 * r / r6;
			}
			else
			{
				// Neutral point fallback
				dins = 0;
				dint = 25;
			}

			// Round results to 2 decimal places
			var cz = new System.Globalization.CultureInfo("cs-CZ");
			double roundedOdstin = Math.Round(dint, 2, MidpointRounding.AwayFromZero);
			double roundedSytost = Math.Round(dins, 2, MidpointRounding.AwayFromZero);

			txtOdstinResult.Text = roundedOdstin.ToString("F2", cz);
			txtSytostResult.Text = roundedSytost.ToString("F2", cz);
		}
		else
		{
			DisplayAlert("Chyba vstupu", "Zadejte prosím platná čísla.", "OK");
		}
	}

	/// <summary>
	/// Updates the 4-point window for cubic interpolation from the main FST matrix.
	/// </summary>
	private void UpdateF4(int iton)
	{
		int it = iton;
		if (iton <= 1) it = iton + 24;
		int im1 = it - 1;
		for (int k = 0; k < 3; k++)
		{
			for (int l = 0; l < 4; l++)
			{
				int colIndex = im1 + l - 1;
				f4st[k, l] = fst[k, colIndex % 27];
			}
		}
	}

	/// <summary>
	/// Cubic spline interpolation routine.
	/// Calculates weights for the 4 nearest points and updates fint vector.
	/// </summary>
	private void UpdatePol3(double tx)
	{
		double a1 = (tx * (1 - tx) * (tx - 2)) / 6.0;
		double a2 = ((tx + 1) * (tx - 1) * (tx - 2)) * 0.5;
		double a3 = ((tx + 1) * tx * (2 - tx)) * 0.5;
		double a4 = ((tx + 1) * tx * (tx - 1)) / 6.0;

		for (int ji = 0; ji < 3; ji++)
		{
			fint[ji] = a1 * f4st[ji, 0] + a2 * f4st[ji, 1] + a3 * f4st[ji, 2] + a4 * f4st[ji, 3];
		}
	}
}
