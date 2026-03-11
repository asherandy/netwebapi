public class RequestTracker
{
    private int _activeRequests = 0;
    private bool _isEnabled = true;

    public bool IsEnabled => _isEnabled;
    public int ActiveRequests => _activeRequests;

    // 禁用实例
    public void Disable()
    {
        _isEnabled = false;
    }

    // 启用实例
    public void Enable()
    {
        _isEnabled = true;
    }

    // 请求计数器
    public void Increment() => Interlocked.Increment(ref _activeRequests);
    public void Decrement() => Interlocked.Decrement(ref _activeRequests);
}