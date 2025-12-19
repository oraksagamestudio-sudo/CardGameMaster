public static class AuthServiceLocator
{
    private static IAuthService _instance;
    public static IAuthService Instance => _instance ??= new DummyAuthService();
}