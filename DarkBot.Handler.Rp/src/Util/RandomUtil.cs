namespace DarkBot.Handler.Rp.Util;

public static class RandomUtil {
    private static ulong A { get; } = 13806865742100181928;

    private static ulong C { get; } = 1186245766092220683;

    private static ulong M { get; } = ulong.MaxValue;

    public static ulong GetRandom(ulong seed) {
        return (A * seed + C) % M;
    }
}