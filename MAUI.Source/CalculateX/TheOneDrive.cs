#if ANDROID

using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Identity.Client;
using System.Diagnostics;
using System.Xml.Linq;

namespace CalculateX;

// https://youtu.be/YYcuyNfNdRw?si=2qzlMIPjVqczxrEN
internal static class TheOneDrive
{
	public const string ClientId = "997d15f1-76ba-4915-a8f3-a28480e22dab";
	//public const string ClientId = "182938ae-b61b-43da-a599-b538d19795e8";

	private static readonly string[] Scopes = [ "User.Read", "Files.Read", "offline_access", "Files.ReadWrite", ];

	//private const string RedirectURI = "msauth://com.x12noon.calculatex/fRiy%2FGhoksI7ZWcdNQknJYLW5rE%3D";

	//private static readonly string URLMSGraph = "https://graph.microsoft.com/v1.0/";
	//private static readonly string URLOneDrive = "https://api.onedrive.com/v1.0";

	//Microsoft.OneDriveSDK
	//private static OneDriveClient? _client;


	//Microsoft.Identity.Client
	private static IPublicClientApplication? IdentityClient { get; set; }


	public static async Task ConnectAsync()
	{
		// NuGet:
		//		Microsoft.OneDriveSDK
		//		Microsoft.OneDriveSDK.Authentication
		//using Microsoft.OneDrive.Sdk;
		//using Microsoft.OneDrive.Sdk.Authentication;
		//MsaAuthenticationProvider msaAuthenticationProvider = new(
		//	Platform.AppContext,
		//	ClientId,
		//	"https://login.live.com/oauth20_desktop.srf",
		//	Scopes);
		//await msaAuthenticationProvider.AuthenticateUserAsync();

		//_client = new(URLOneDrive, msaAuthenticationProvider);


		//------------------
		// NuGet:
		//		Microsoft.Identity
		//var app = PublicClientApplicationBuilder
		//			.Create(ClientId)
		//			.WithBroker()
		//			.WithRedirectUri(RedirectURI)
		//			.Build();

		//AuthenticationResult? result = await app
		//	.AcquireTokenInteractive(Scopes)
		//	.WithParentActivityOrWindow(App.Current)
		//	.ExecuteAsync();


		//------------------
		// NuGet:
		//		Microsoft.Identity.Client
		//
		AuthenticationResult? result = await GetAuthenticationTokenAsync();
	}

	// https://learn.microsoft.com/en-us/azure/developer/mobile-apps/azure-mobile-apps/quickstarts/maui/authentication
	public static async Task<AuthenticationResult?> GetAuthenticationTokenAsync()
	{
		if (IdentityClient is null)
		{
#if ANDROID
			IdentityClient = PublicClientApplicationBuilder
				 .Create(ClientId)
				 .WithAuthority(AzureCloudInstance.AzurePublic, "common")
				 .WithRedirectUri($"msal{ClientId}://auth")
				 .WithParentActivityOrWindow(() => Platform.CurrentActivity)
				 .Build();
#elif IOS
        IdentityClient = PublicClientApplicationBuilder
            .Create(Constants.ClientId)
            .WithAuthority(AzureCloudInstance.AzurePublic, "common")
            .WithIosKeychainSecurityGroup("com.microsoft.adalcache")
            .WithRedirectUri($"msal{Constants.ClientId}://auth")
            .Build();
#else
        IdentityClient = PublicClientApplicationBuilder
            .Create(Constants.ClientId)
            .WithAuthority(AzureCloudInstance.AzurePublic, "common")
            .WithRedirectUri("https://login.microsoftonline.com/common/oauth2/nativeclient")
            .Build();
#endif
		}

		var accounts = await IdentityClient.GetAccountsAsync();
		AuthenticationResult? result = null;
		bool tryInteractiveLogin = false;

		try
		{
			result = await IdentityClient
				 .AcquireTokenSilent(Scopes, accounts.FirstOrDefault())
				 .ExecuteAsync();
		}
		catch (MsalUiRequiredException)
		{
			tryInteractiveLogin = true;
		}
		catch (Exception ex)
		{
			Debug.WriteLine($"MSAL Silent Error: {ex.Message}");
		}

		if (tryInteractiveLogin)
		{
			try
			{
				result = await IdentityClient
					 .AcquireTokenInteractive(Scopes)
					 .ExecuteAsync();
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"MSAL Interactive Error: {ex.Message}");
			}
		}

		//return new AuthenticationToken()
		//{
		//	DisplayName = result?.Account?.Username ?? string.Empty,
		//	ExpiresOn = result?.ExpiresOn ?? DateTimeOffset.MinValue,
		//	Token = result?.AccessToken ?? string.Empty,
		//	UserId = result?.Account?.Username ?? string.Empty,
		//};

		//Debug.WriteLine($"User: {result?.Account.Username}");
		//Debug.WriteLine($"Expiry: {result?.ExpiresOn}");
		//Debug.WriteLine($"Token: {result?.AccessToken}");

		return result;
	}

	public static GraphServiceClient GetGraphServiceClient()
	{
		Microsoft.Kiota.Abstractions.Authentication.BaseBearerTokenAuthenticationProvider authenticationProvider = new(new MyTokenProvider());
		GraphServiceClient graphServiceClient = new(authenticationProvider);
		return graphServiceClient;
	}


	public static string GetFile(string filename)
	{
		// https://github.com/microsoftgraph/msgraph-sdk-dotnet?tab=readme-ov-file
		throw new NotImplementedException();
	}

	public static string GetDocumentsFile(string filename)
	{
		throw new NotImplementedException();
	}

	public static async Task<XDocument?> DownloadXDocument(string filename)
	{
		using Stream? stmFile = await DownloadStream(filename);
		if (stmFile is null)
		{
			return null;
		}

		XDocument xmlDoc = XDocument.Load(stmFile);
		return xmlDoc;
	}
	/// <summary>
	/// Download file from OneDrive.
	/// </summary>
	/// <remarks>
	/// Caller must dispose of the returned stream.
	/// </remarks>
	/// <param name="filename">Filename--not path--of file in user's Documents folder.</param>
	/// <returns>Stream of downloaded file (must be disposed).</returns>
	public static async Task<Stream?> DownloadStream(string filename)
	{
		try
		{
			GraphServiceClient graphClient = GetGraphServiceClient();

			// https://github.com/microsoftgraph/msgraph-sdk-dotnet/blob/feature/5.0/docs/upgrade-to-v5.md#drive-item-paths
			// GET ONEDRIVE
			Drive? drive =
				await graphClient
				.Me
				.Drive
				.GetAsync();
			if (drive is null)
			{
				Debug.WriteLine("Unable to access Drive.");
				return null;
			}
			Debug.WriteLine($"Drive type: {drive.DriveType}");


			// GET ONEDRIVE SPECIAL FOLDER
			// https://learn.microsoft.com/en-us/graph/api/drive-get-specialfolder?view=graph-rest-1.0&tabs=csharp
			DriveItem? docs = await
				graphClient
				.Drives[drive.Id]
				.Special["documents"]
				.GetAsync(
				//requestConfiguration.QueryParameters.Select = ["id", "createdDateTime", "displayName"];
				//requestConfiguration.QueryParameters.Expand = ["members"];
				//requestConfiguration.QueryParameters.Filter = "startswith(displayName%2C+'J')";
				);
			if (docs is null)
			{
				Debug.WriteLine("No Documents folder.");
				return null;
			}

			// GET ONEDRIVE SPECIAL FOLDER CHILDREN
			DriveItemCollectionResponse? docsChildren = await
				graphClient
				.Drives[drive.Id]
				.Items[docs.Id]
				.Children
				.GetAsync();
			if (docsChildren is null)
			{
				Debug.WriteLine("No Documents children.");
				return null;
			}
			Debug.WriteLine($"docsChildren.Count = {docsChildren.OdataCount}");

			List<DriveItem>? documentsItems = docsChildren.Value;
			if (documentsItems is null)
			{
				Debug.WriteLine("No Documents items.");
				return null;
			}

			// Get ID of file to download
			DriveItem? itemFile = null;
			foreach (DriveItem item in documentsItems)
			{
				Debug.WriteLine($"\t{item.Name}");
				if (item.Name == filename)
				{
					itemFile = item;
					break;
				}
			}
			if (itemFile is null)
			{
				Debug.WriteLine("Unable to find file.");
				return null;
			}

			// DOWNLOAD (if exists) CalculateX.xml
			using Stream? stmFile = await
				graphClient
				.Drives[drive.Id]
				.Items[itemFile.Id]
				.Content
				.GetAsync();
			if (stmFile is null)
			{
				Debug.WriteLine("No file stream.");
				return null;
			}

			return stmFile;
		}
		catch (Exception ex)
		{
			Debug.WriteLine(ex.GetType() + ": " + ex.Message);
			Debug.WriteLineIf(ex.InnerException is not null, ex.InnerException?.Message);
		}

		return null;
	}
}

/// <summary>
///
/// https://learn.microsoft.com/en-us/openapi/kiota/authentication?tabs=csharp
/// https://github.com/microsoftgraph/msgraph-sdk-dotnet/blob/feature/5.0/docs/upgrade-to-v5.md#authentication
/// </summary>
internal class MyTokenProvider : Microsoft.Kiota.Abstractions.Authentication.IAccessTokenProvider
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
	public Microsoft.Kiota.Abstractions.Authentication.AllowedHostsValidator AllowedHostsValidator { get; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

	/// Return null if request could/should not be authenticated.
	/// https://learn.microsoft.com/en-us/openapi/kiota/authentication?tabs=csharp
	public async Task<string> GetAuthorizationTokenAsync(Uri uri, Dictionary<string, object>? additionalAuthenticationContext = null, CancellationToken cancellationToken = default)
	{
		AuthenticationResult? result = await TheOneDrive.GetAuthenticationTokenAsync();
#pragma warning disable CS8603 // Possible null reference return.
		return result?.AccessToken;
#pragma warning restore CS8603 // Possible null reference return.
	}
}

/// <summary>
/// An authentication provider is responsible for putting
/// the authentication token in the request header.
///
/// https://learn.microsoft.com/en-us/openapi/kiota/authentication?tabs=csharp#base-bearer-token-authentication-provider
/// </summary>
/// <example>
/// public static GraphServiceClient GetGraphServiceClient()
/// {
///	GraphServiceClient graphServiceClient = new(new MyAuthenticationProvider());
///	return graphServiceClient;
/// }
/// </example>
internal class MyAuthenticationProvider : Microsoft.Kiota.Abstractions.Authentication.IAuthenticationProvider
{
	public MyAuthenticationProvider() { }

	public async Task AuthenticateRequestAsync(Microsoft.Kiota.Abstractions.RequestInformation request, Dictionary<string, object>? additionalAuthenticationContext = null, CancellationToken cancellationToken = default)
	{
		AuthenticationResult? result = await TheOneDrive.GetAuthenticationTokenAsync();
		if (result is null)
		{
			return;
		}

		// https://learn.microsoft.com/en-us/answers/questions/512372/c-rest-httprequest-headers-authorization-bearer-ne
		request.Headers.Add("Authorization", $"Bearer {result.AccessToken}");
	}
}

/*
			User? me = await graphClient
				.Me
				.GetAsync(
					requestConfiguration => requestConfiguration.QueryParameters.Select =
					[
						"displayName",
					]
				);
			if (me is null)
			{
				return;
			}
			Debug.WriteLine(me.UserPrincipalName);
			Debug.WriteLine(me.DisplayName);
 */
/*
			// GET ONEDRIVE ROOT
			DriveItem? root = await
				graphClient
				.Drives[drive.Id]
				.Root //same as .Items["root"]
				.GetAsync(
					requestConfiguration => requestConfiguration.QueryParameters.Select =
					[
						"name",
						"description",
					]
				);
			if (root is null)
			{
				Debug.WriteLine("No root.");
				return;
			}
			Debug.WriteLine($"Name: {root.Name}");
			Debug.WriteLine($"Description: {root.Description}");

			// GET ONEDRIVE ROOT FOLDER'S CHILDREN
			DriveItemCollectionResponse? rootChildren = await
				graphClient
				.Drives[drive.Id]
				.Items["root"]
				.Children
				.GetAsync();
			if (rootChildren is null)
			{
				Debug.WriteLine("No root children.");
				return;
			}
			Debug.WriteLine($"rootChildren.Count = {rootChildren.OdataCount}");
 */
		//try
		//{
		//	GraphServiceClient graphClient = TheOneDrive.GetGraphServiceClient();

		//	const string FILE_NEWNAME = "MyNewFile.txt";

		//	// DELETE EXISTING FILE IN ONEDRIVE/Documents/
		//	await
		//		graphClient
		//		.Drives[drive.Id]
		//		.Items[docs.Id]
		//		.ItemWithPath(FILE_NEWNAME)
		//		.DeleteAsync();


		//	string myString = $"This is a string {DateTime.UtcNow} {DateTime.UtcNow.Ticks}" + Environment.NewLine;
		//	Debug.WriteLine($"String = {myString}");

		//	// UPLOAD NEW FILE TO ONEDRIVE/Documents/
		//	byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(myString);
		//	using MemoryStream stmNewFile = new(byteArray);
		//	/*
		//		Creating a new file:
		//		HTTP PUT https://graph.microsoft.com/v1.O/me/drive/root:/myNewSmallFile.txt:/content
		//		Content—Type: text/plain

		//		This is a new small file
		//	 */
		//	DriveItem? uploadedFile = await
		//		graphClient
		//		.Drives[drive.Id]
		//		.Items[docs.Id]
		//		.ItemWithPath(FILE_NEWNAME)
		//		.Content
		//		.PutAsync(stmNewFile);
		//	if (uploadedFile is null)
		//	{
		//		Debug.WriteLine("Failed to create file.");
		//	}


		//	myString += $"This is an updated string {DateTime.UtcNow} {DateTime.UtcNow.Ticks}" + Environment.NewLine;
		//	Debug.WriteLine($"String = {myString}");

		//	// UPDATE EXISTING FILE IN ONEDRIVE/Documents/
		//	byte[] updatedByteArray = System.Text.Encoding.UTF8.GetBytes(myString);
		//	using MemoryStream stmUpdatedFile = new(updatedByteArray);
		//	/*
		//		Updating an existing file:
		//		HTTP PUT https://graph.microsoft.com/v1.O/me/drive/items/{item—id}/content
		//		Content—Type: text/plain

		//		A new small file
		//	 */
		//	DriveItem? updatedFile = await
		//		graphClient
		//		.Drives[drive.Id]
		//		.Items[docs.Id]
		//		.ItemWithPath(FILE_NEWNAME)
		//		.Content
		//		.PutAsync(stmUpdatedFile);
		//	if (uploadedFile is null)
		//	{
		//		Debug.WriteLine("Failed to update file.");
		//	}
		//}
		//catch (Exception ex)
		//{
		//	Debug.WriteLine(ex.GetType() + ": " + ex.Message);
		//	Debug.WriteLineIf(ex.InnerException is not null, ex.InnerException?.Message);
		//}

#endif
