namespace EveOPreview.Configuration
{
	public interface IConfigurationStorage
	{
		void SetConfigurationFilename(string filename);
	    void Load();
		void Save();
	}
}