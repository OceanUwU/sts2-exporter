using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;

[ModInitializer("Load")]
public class ModEntry {
    [DllImport("libdl.so.2")]
    static extern IntPtr dlopen(string filename, int flags);
    
    [DllImport("libdl.so.2")]
    static extern IntPtr dlerror();
    
    [DllImport("libdl.so.2")]
    static extern IntPtr dlsym(IntPtr handle, string symbol);
    
    private static IntPtr _holder;
    public static void Load() {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
            Log.Info("Running on linux, manually dlopen libgcc for harmony");
            _holder = dlopen("libgcc_s.so.1", 2 | 256);
            if (_holder == IntPtr.Zero) {
                Log.Info("Or Nor: "+Marshal.PtrToStringAnsi(dlerror()));
            }
        }
        var currentDllPath = Assembly.GetExecutingAssembly().Location;
        Assembly.LoadFrom(currentDllPath[..(currentDllPath.LastIndexOf('/')+1)] + "Scriban.dll");

        new Harmony("visible_a_9").PatchAll();
    }   
}