using System.Threading.Tasks;

/// <summary>
/// 判断是否是锁定状态，
/// 该锁定状态为自己标记
/// </summary>
public class JLocker
{
    private bool _locked;
    // 给定时间后如果没有恢复响应，则自动响应 (ms)
    private int _wakeDelay;
    private int _version;

    public bool IsOn => _locked;

    public JLocker()
    {
        _wakeDelay = 0;
    }

    public JLocker(int ms)
    {
        _wakeDelay = ms;
    }
    
    public async void On()
    {
        if (_locked) return;
        
        _locked = true;
        var version = _version;
        if (_wakeDelay > 0)
        {
            await Task.Delay(_wakeDelay);
            if (_version != version) return;

            Off();
        }
    }

    public void Off()
    {
        if (!_locked) return;
        
        _locked = false;
        ++_version;
    }
}