using Android.App;
using Android.Content;
using Microsoft.Identity.Client;

namespace CalculateX.Platforms.Android;

[Activity(Exported = true)]
[IntentFilter([Intent.ActionView],
	Categories = [Intent.CategoryBrowsable, Intent.CategoryDefault],
	DataHost = "auth",
	DataScheme = $"msal{TheOneDrive.ClientId}")
]
public class MsalActivity : BrowserTabActivity
{
}
