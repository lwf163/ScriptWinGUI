using System;
using System.Threading;

namespace Swg.Input;

/// <summary>
/// 与 FlaUI.Core.Input.Wait.UntilInputIsProcessed 行为一致的输入队列缓冲等待工具。
/// </summary>
public static class InputWait
{
    /// <summary>
    /// 等待输入处理。
    /// </summary>
    /// <param name="waitTimeout">可空；为 null 时默认等待 100ms。</param>
    public static void UntilInputIsProcessed(TimeSpan? waitTimeout = null)
    {
        // 让线程给系统硬件输入队列一些时间处理。
        // 参考思路：Old New Thing - 10499047。
        var waitTime = (waitTimeout ?? TimeSpan.FromMilliseconds(100)).TotalMilliseconds;
        Thread.Sleep((int)waitTime);
    }
}

