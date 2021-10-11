public enum LoxodonBundleResolution
{
    // 模拟，仅在编辑器中可用，直接使用源作为 bundle
    Simulation, 

    // 内置，使用游戏包中自带的那一份 bundle，这份资源将会在打包机编包时指定
    Embeded,

    // 远端，使用当前环境远端指定的资源，如果本地没有缓存则会触发下载，本地会缓存已下载的资源
    Remote,
}