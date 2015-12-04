using Alba.StaticFiles;
using StoryTeller;
using StoryTeller.Grammars.Tables;

namespace Alba.Testing.Fixtures
{
    public class AssetSettingsFixture : Fixture
    {
        private AssetSettings _settings;

        public override void SetUp()
        {
            _settings = new AssetSettings();
        }

        [FormatAs("Add extension {extension} as allowed")]
        public void AddAllowedExtension(string extension)
        {
            _settings.AllowableExtensions.Add(extension);
        }

        [ExposeAsTable("Allowed Filenames")]
        public void IsAllowed(string Filename, out bool IsAllowed)
        {
            IsAllowed = _settings.IsAllowed(new StaticFile(Filename));
        }
    }
}