using UltraLiteDB;

namespace Core.Services.PersistService.API
{
    public interface IPersistSystem
    {
        void SaveValue<T>(string key, T value, bool withUid = false);

        object GetValue<T>(string key, bool withUid = false);

        void DeletePrefsValue(string key, bool withUid = false);

        void DeleteDB<T>(string key, bool withUid = false);
        
        void DeleteAll();
        
        public UltraLiteCollection<T> GetCollection<T>(string key);
    }
}