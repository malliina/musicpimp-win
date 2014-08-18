
namespace Mle.Util {
    public interface ISettingsManager {
        void Save<T>(string key, T value);
        T Load<T>(string key, T def = default(T));
    }
}
