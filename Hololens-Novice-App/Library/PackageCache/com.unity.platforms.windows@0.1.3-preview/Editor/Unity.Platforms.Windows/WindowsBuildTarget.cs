using System.Diagnostics;
using System.IO;

namespace Unity.Platforms.Windows
{
    public abstract class WindowsBuildTarget : BuildTarget
    {
        public override bool HideInBuildTargetPopup => UnityEngine.Application.platform != UnityEngine.RuntimePlatform.WindowsEditor;

        public override string GetExecutableExtension()
        {
            return ".exe";
        }

        public override string GetUnityPlatformName()
        {
            return nameof(UnityEditor.BuildTarget.StandaloneWindows64);
        }

        public override bool Run(FileInfo buildTarget)
        {
            var startInfo = new ProcessStartInfo();
            startInfo.FileName = buildTarget.FullName;
            startInfo.WorkingDirectory = buildTarget.Directory.FullName;
            startInfo.CreateNoWindow = true;
            var process = Process.Start(startInfo);
            return process != null;
        }
    }

    class DotNetWindowsBuildTarget : WindowsBuildTarget
    {
#if UNITY_EDITOR_WIN
        protected override bool IsDefaultBuildTarget => true;
#endif

        public override string GetDisplayName()
        {
            return "Windows .NET";
        }

        public override string GetBeeTargetName()
        {
            return "windows-dotnet";
        }
    }

    class IL2CPPWindowsBuildTarget : WindowsBuildTarget
    {
        public override string GetDisplayName()
        {
            return "Windows IL2CPP";
        }

        public override string GetBeeTargetName()
        {
            return "windows-il2cpp";
        }
    }
}
