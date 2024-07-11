using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;

namespace CalculateX;
public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder =
			MauiApp
			.CreateBuilder()
			.UseMauiApp<App>()
			.UseMauiCommunityToolkit()
			.ConfigureLifecycleEvents(events =>
			{
#if ANDROID
				events.AddAndroid(platform =>
				{
					platform.OnActivityResult((activity, rc, result, data) =>
					{
						Microsoft.Identity.Client.AuthenticationContinuationHelper.SetAuthenticationContinuationEventArgs(rc, result, data);
					});
				});
#endif
			})
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			})
			// https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/appmodel/version-tracking
			.ConfigureEssentials(essentials =>
			{
				essentials.UseVersionTracking();
			});

//		Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping(nameof(Entry), (handler, view) =>
//		{
//#if ANDROID
//			if (Application.Current?.RequestedTheme == AppTheme.Dark)
//			{
//				handler.PlatformView.TextCursorDrawable?.SetTint(Colors.White.ToPlatform());
//			}
//			else
//			{
//				handler.PlatformView.TextCursorDrawable?.SetTint(Colors.Black.ToPlatform());
//			}

//			// These seem to have no effect.
//			//handler.PlatformView.SetBackgroundColor(Android.Graphics.Color.Orange);
//			//handler.PlatformView.SetHighlightColor(Android.Graphics.Color.Green);
//			// Text color
//			//handler.PlatformView.SetTextColor(Android.Graphics.Color.Red);
//			// Placeholder color
//			//handler.PlatformView.SetHintTextColor(Android.Graphics.Color.Yellow);
//#endif
//		});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
