// RemoteContent/Scripts/IRemoteContentService.cs
using System.Threading.Tasks;

public interface IRemoteContentService
{
    Task InitializeAsync(string remoteCatalogUrl);     // 카탈로그 등록/갱신
    Task<T> LoadAsync<T>(string address) where T : class; // 에셋 로드
    void Release(object handleOrAsset);                // 해제 (간단히 자원 정리용)
}