using System;
using UnityEngine;

namespace DataAssembly;

public static class DataClass
{
    public static string[] fieldsInterface;
    public static string[] fieldsSimulation;
    public static string[] fieldDescs;
    public static Func<bool> InInterface;
    public static Func<bool> ShouldRecord;
    public static Func<string> StateString;

    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Record(int field)
    {
        if (InInterface == null) return;
        if (!ShouldRecord()) return;

        if (InInterface())
            fieldsInterface[field] ??= StateString() + "\n" + StackTraceUtility.ExtractStackTrace();
        else
            fieldsSimulation[field] ??= StateString() + "\n" + StackTraceUtility.ExtractStackTrace();
    }
}
