// using System;
// using System.Collections.Generic;
//
// namespace Plot.Runtime
// {
//     public class PlotComicsTrigger
//     {
//         private static PlotComicsPlayCtrl _playCtrl;
//         private static Bucket Bucket => BucketManager.Stuff.Plot;
//
//         public static async void Preload(List<string> comicsAddressList, string comicsAddress, Action onComp = null)
//         {
//         }
//
//         public static async void Trigger(int plotId, PlotComicsPreviewConfigObject comicsData, Action onComp = null,
//             Action onFinishLoading = null, bool showSkip = false)
//         {
//             if (comicsData == null)
//             {
//                 onComp?.Invoke();
//                 return;
//             }
//
//             // 这里需要判断如果存在comicsPage 则先back
//             var page = UIEngine.Stuff.FindPage<PlotComicsPage>();
//             if (page != null)
//             {
//                 UIEngine.Stuff.RemoveFromStack<PlotComicsPage>();
//             }
//
//             // onFinishLoading?.Invoke();
//             // 打开ui comics界面
//             UIEngine.Stuff.Forward<PlotComicsPage>(new PlotComicsPageParams()
//             {
//                 ComicsData = comicsData,
//                 OnComp = onComp,
//                 PlotId = plotId,
//                 ShowSkip = showSkip,
//             });
//         }
//     }
// }