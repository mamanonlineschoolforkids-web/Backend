namespace Maman.Core.Settings;

public class AzureStorageSettings
{
	public string ConnectionString { get; set; } = string.Empty;
	public string ProfilePicturesContainer { get; set; } = "profile-pictures";
}