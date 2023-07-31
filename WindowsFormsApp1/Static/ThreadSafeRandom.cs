using System;
public static  class ThreadSafeRandom
{
	private static readonly Random _global = new Random(4951965);
	//private static readonly Random _global = new Random(Guid.NewGuid().GetHashCode());
	[ThreadStatic] private static Random _local;
    private static readonly object _busy = new object();

    public static int Next()
    {
        if (_local == null)
        {
            lock (_busy)
            {
                if (_local == null)
                {
                    _local = new Random(_global.Next());
                }
            }
        }

        return _local.Next();
    }

    public static int Next(int n)
    {
        if (_local == null)
        {
            lock (_busy)
            {
                if (_local == null)
                {
                    _local = new Random(_global.Next());
                }
            }
        }

        return _local.Next(n);
    }

    public static int Next(int n, int m)
    {
        if (_local == null)
        {
            lock (_busy)
            {
                if (_local == null)
                {
                    _local = new Random(_global.Next());
                }
            }
        }

        return _local.Next(n, m);
    }

    public static double NextDouble()
    {
        if (_local == null)
        {
            lock (_busy)
            {
                if (_local == null)
                {
                    _local = new Random(_global.Next());
                }
            }
        }

        return _local.NextDouble();
    }
}

//internal static class ThreadSafeRandom
//{
//    [ThreadStatic]
//    private static Random random;

//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    public static Random ObtainThreadStaticRandom() => ObtainRandom();

//    private static Random ObtainRandom()
//    {
//        return random ?? (random = new Random(Guid.NewGuid().GetHashCode()));
//    }
//}

//private static unsafe Guid GuidRandomCachedInstance()
//{
//    var bytes = stackalloc byte[16];
//    var dst = bytes;

//    var random = ThreadSafeRandom.ObtainThreadStaticRandom();
//    for (var i = 0; i < 4; i++)
//    {
//        *(int*)dst = random.Next();
//        dst += 4;
//    }

//    return *(Guid*)bytes;
//}
