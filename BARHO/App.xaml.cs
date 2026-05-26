using Microsoft.Extensions.DependencyInjection;

namespace LabApp;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		var window = new Window(new AppShell());
		
		window.Width = 450;
		window.Height = 700;

		// Force the size with a slight delay to override system defaults
		window.Dispatcher.Dispatch(() => {
			window.Width = 450;
			window.Height = 600;
		});
		
		return window;
	}
}