///
/// Default build is for the Microsoft Store.
/// Licensing is enabled if defined (e.g., Release on GitHub) or in Debug.
///
/// Uncomment this to build Release with licensing support.
///
//#define LICENSING

using System;
using System.Diagnostics;
using System.Windows;
using Shared;

namespace CalculateX;

/// <summary>
/// This class may contain values that are not to be committed to the Git repository.
/// </summary>
/// <remarks>
/// Although, there is nothing secret about the product ID and
/// public key because they are designed to be just that--public.
/// </remarks>
internal static class Secrets
{
	/// <summary>
	/// Licensing support using <see cref="LicenseManager"/>.
	///
	/// This product ID and public key are for the trial license file
	/// in the GitHub release. You will want to provide your own.
	///
	/// https://github.com/skst/LicenseManager
	/// </summary>
	/// <remarks>
	/// These strings must match the ones used to create the license.
	/// </remarks>
#if LICENSING
#warning "Be sure to replace the values of the product ID and public key with your own for licensing."
#endif
	private const string ProductID = @"Calculate X GitHub";
	private const string PublicKey = @"MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAElXHQQ6M7J460oxfBywAH48Tm3rb0QU3QPkbFZk0yJ0pr1vZ0S95aOkQXhxh0s+g4TCwqPPs6dh5lLj38r/FBlQ==";

	internal static string License_Name = string.Empty;
	internal static string License_Email = string.Empty;
	internal static string License_Company = string.Empty;

	internal static void CheckLicense()
	{
		CheckLicense(ProductID, PublicKey);
	}

	[Conditional("LICENSING"), Conditional("DEBUG")]
	private static void CheckLicense(string productID, string publicKey)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(productID);
		ArgumentException.ThrowIfNullOrWhiteSpace(publicKey);

		LicenseManager manager = new();
		string errorMessages = manager.IsLicenseValid(productID, publicKey);
		if (!string.IsNullOrEmpty(errorMessages))
		{
			MessageBox.Show(errorMessages, "Calculate X", MessageBoxButton.OK, MessageBoxImage.Error);
			Application.Current.Shutdown();
			return;
		}

		/// Set info for MainWindow title
		License_Name = manager.Name;
		License_Email = manager.Email;
		License_Company = manager.Company;

		// TODO: display info about expiry?
		//if (manager.ExpirationDays > 0)
		//{
		//	int daysExpiry = manager.Expiration.Subtract(DateTime.Now).TotalDays;
		//	if (daysExpiry == 1)
		//	{
		//		MessageBox.Show($"The license expires tomorrow.", Error);
		//	}
		//	else if (daysExpiry <= 7)
		//	{
		//		MessageBox.Show($"The license expires in {daysExpiry} days.", Warning);
		//	}
		//}
	}
}
