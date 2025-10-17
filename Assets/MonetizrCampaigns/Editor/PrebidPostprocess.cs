#if UNITY_IOS
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.iOS.Xcode;
using UnityEditor.iOS.Xcode.Extensions;

public class PrebidPostprocess : IPostprocessBuildWithReport
{
    public int callbackOrder => 999;

    // Package-first roots; we also probe legacy "Assets" paths so publishers
    // who import the SDK as source still work.
    static readonly string[] CandidateRoots =
    {
        "Packages/com.monetizr.unityplugin/Plugins/iOS",
        "Assets/MonetizrCampaigns/Plugins/iOS",
        "Assets/Plugins/iOS"
    };

    const string BRIDGE_STEM = "PrebidBridge";
    const string MOBILE_STEM = "PrebidMobile";

    public void OnPostprocessBuild(BuildReport report)
    {
        if (report.summary.platform != BuildTarget.iOS) return;

        string projPath = PBXProject.GetPBXProjectPath(report.summary.outputPath);
        var proj = new PBXProject();
        proj.ReadFromFile(projPath);

        string appTarget = proj.GetUnityMainTargetGuid();
        string ufTarget  = proj.GetUnityFrameworkTargetGuid();

        // 1) Remove any stale .xcframework references Unity may have added.
        RemoveXCRefIfPresent(proj, appTarget, ufTarget, $"Libraries/com.monetizr.unityplugin/Plugins/iOS/{BRIDGE_STEM}.xcframework");
        RemoveXCRefIfPresent(proj, appTarget, ufTarget, $"Libraries/com.monetizr.unityplugin/Plugins/iOS/{MOBILE_STEM}.xcframework");
        RemoveXCRefIfPresent(proj, appTarget, ufTarget, $"Frameworks/com.monetizr.unityplugin/Plugins/iOS/{BRIDGE_STEM}.xcframework");
        RemoveXCRefIfPresent(proj, appTarget, ufTarget, $"Frameworks/com.monetizr.unityplugin/Plugins/iOS/{MOBILE_STEM}.xcframework");

        // 2) Locate device slices inside the package/Assets.
        string bridgeSrc = FindFrameworkInsideXC(BRIDGE_STEM);
        string mobileSrc = FindFrameworkInsideXC(MOBILE_STEM);

        // 3) Copy into exported Xcode project’s Frameworks/
        string frameworksDir = Path.Combine(report.summary.outputPath, "Frameworks");
        Directory.CreateDirectory(frameworksDir);

        string bridgeDst = Path.Combine(frameworksDir, BRIDGE_STEM + ".framework");
        string mobileDst = Path.Combine(frameworksDir, MOBILE_STEM + ".framework");

        CopyDirectoryFresh(bridgeSrc, bridgeDst);
        CopyDirectoryFresh(mobileSrc,  mobileDst);

        // 4) Add, link in both targets, and embed in the app target.
        string bridgeFile = AddFrameworkFile(proj, bridgeDst);
        string mobileFile = AddFrameworkFile(proj, mobileDst);

        proj.AddFileToBuild(appTarget, bridgeFile);
        proj.AddFileToBuild(appTarget, mobileFile);
        proj.AddFileToBuild(ufTarget,   bridgeFile);
        proj.AddFileToBuild(ufTarget,   mobileFile);

        PBXProjectExtensions.AddFileToEmbedFrameworks(proj, appTarget, bridgeFile);
        PBXProjectExtensions.AddFileToEmbedFrameworks(proj, appTarget, mobileFile);

        // 5) Ensure search paths / runpaths (both targets)
        EnsureBuildSettings(proj, appTarget);
        EnsureBuildSettings(proj, ufTarget);

        proj.WriteToFile(projPath);
    }

    // ---------- Helpers ----------

    static string FindFrameworkInsideXC(string stem)
    {
        // Look for <Root>/<Stem>.xcframework/ios-arm64/<Stem>.framework
        foreach (var root in CandidateRoots)
        {
            string xc = Path.Combine(root, $"{stem}.xcframework");
            string full = Path.Combine(Directory.GetCurrentDirectory(), xc);
            if (!Directory.Exists(full)) continue;

            string device = Path.Combine(full, "ios-arm64", $"{stem}.framework");
            if (Directory.Exists(device)) return device;
        }

        throw new DirectoryNotFoundException(
            $"Missing xcframework or device slice for {stem}. " +
            $"Looked under:\n - {string.Join("\n - ", CandidateRoots)}");
    }

    static void RemoveXCRefIfPresent(PBXProject proj, string appTarget, string ufTarget, string projectPath)
    {
        var guid = proj.FindFileGuidByProjectPath(projectPath);
        if (string.IsNullOrEmpty(guid)) return;
        proj.RemoveFileFromBuild(appTarget, guid);
        proj.RemoveFileFromBuild(ufTarget,   guid);
        proj.RemoveFile(guid);
    }

    static void CopyDirectoryFresh(string srcDir, string dstDir)
    {
        if (Directory.Exists(dstDir)) Directory.Delete(dstDir, true);
        CopyAll(new DirectoryInfo(srcDir), new DirectoryInfo(dstDir));
    }

    static void CopyAll(DirectoryInfo source, DirectoryInfo target)
    {
        Directory.CreateDirectory(target.FullName);
        foreach (var f in source.GetFiles())
            f.CopyTo(Path.Combine(target.FullName, f.Name), true);
        foreach (var d in source.GetDirectories())
            CopyAll(d, target.CreateSubdirectory(d.Name));
    }

    static string AddFrameworkFile(PBXProject proj, string absolutePath)
    {
        // Add as “Frameworks/<Name>.framework” in the project
        string rel = "Frameworks/" + Path.GetFileName(absolutePath);
        return proj.AddFile(rel, rel, PBXSourceTree.Source);
    }

    static void EnsureBuildSettings(PBXProject proj, string target)
    {
        proj.AddBuildProperty(target, "FRAMEWORK_SEARCH_PATHS", "$(inherited)");
        proj.AddBuildProperty(target, "FRAMEWORK_SEARCH_PATHS", "$(PROJECT_DIR)/Frameworks");

        proj.AddBuildProperty(target, "LD_RUNPATH_SEARCH_PATHS", "$(inherited)");
        proj.AddBuildProperty(target, "LD_RUNPATH_SEARCH_PATHS", "@executable_path/Frameworks");
        proj.AddBuildProperty(target, "LD_RUNPATH_SEARCH_PATHS", "@loader_path/Frameworks");
    }
}
#endif
