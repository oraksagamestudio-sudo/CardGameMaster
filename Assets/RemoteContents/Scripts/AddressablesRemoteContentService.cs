// RemoteContent/Scripts/AddressablesRemoteContentService.cs
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;

public class AddressablesRemoteContentService : IRemoteContentService
{
    public async Task InitializeAsync(string remoteCatalogUrl)
    {
        await Addressables.InitializeAsync().Task;

        if (!string.IsNullOrEmpty(remoteCatalogUrl))
        {
            var op = Addressables.LoadContentCatalogAsync(remoteCatalogUrl);
            await op.Task; // 카탈로그 추가(업데이트도 이 방식으로)
        }
    }

    public async Task<T> LoadAsync<T>(string address) where T : class
    {
        var handle = Addressables.LoadAssetAsync<T>(address);
        var asset = await handle.Task;
        return asset; // 간단 버전: 핸들은 따로 안 보관하고 자산만 리턴
    }

    public void Release(object asset)
    {
        if (asset != null)
            Addressables.Release(asset);
    }
}