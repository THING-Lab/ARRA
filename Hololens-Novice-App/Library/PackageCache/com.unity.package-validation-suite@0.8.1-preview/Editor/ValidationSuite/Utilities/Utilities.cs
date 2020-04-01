using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Semver;
using UnityEngine;
using UnityEditor.Compilation;
using UnityEditor.PackageManager.ValidationSuite.ValidationTests;

namespace UnityEditor.PackageManager.ValidationSuite
{
    public static class Utilities
    {
        internal const string PackageJsonFilename = "package.json";
        internal const string ChangeLogFilename = "CHANGELOG.md";
        internal const string EditorAssemblyDefintionSuffix = ".Editor.asmdef";
        internal const string EditorTestsAssemblyDefintionSuffix = ".EditorTests.asmdef";
        internal const string RuntimeAssemblyDefintionSuffix = ".Runtime.asmdef";
        internal const string RuntimeTestsAssemblyDefintionSuffix = ".RuntimeTests.asmdef";
        internal const string ThirdPartyNoticeFile = "Third-Party Notices.md";
        internal const string LicenseFile = "LICENSE.md";
        internal const string VSuiteName = "com.unity.package-validation-suite";


        public static bool NetworkNotReachable { get { return Application.internetReachability == NetworkReachability.NotReachable; } }

        public static string CreatePackageId(string name, string version)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(version))
                throw new ArgumentNullException("Both name and version must be specified.");

            return string.Format("{0}@{1}", name, version);
        }

        public static bool IsPreviewVersion(string version)
        {
            var semVer = SemVersion.Parse(version);
            return semVer.Prerelease.Contains("preview") || semVer.Major == 0;
        }

        internal static T GetDataFromJson<T>(string jsonFile)
        {
            return JsonUtility.FromJson<T>(File.ReadAllText(jsonFile));
        }

        internal static string CreatePackage(string path, string workingDirectory)
        {
            //No Need to delete the file, npm pack always overwrite: https://docs.npmjs.com/cli/pack
            var packagePath =  Path.Combine(Path.Combine(Application.dataPath, ".."), path);

            var launcher = new NodeLauncher();
            launcher.WorkingDirectory = workingDirectory;
            launcher.NpmPack(packagePath);

            var packageName = launcher.OutputLog.ToString().Trim();
            return packageName;
        }

        internal static PackageManager.PackageInfo[] UpmSearch(string packageIdOrName = null, bool throwOnRequestFailure = false)
        {
            var request = string.IsNullOrEmpty(packageIdOrName) ? Client.SearchAll() : Client.Search(packageIdOrName);
            while (!request.IsCompleted)
            {
                if (Utilities.NetworkNotReachable)
                    throw new Exception("Failed to fetch package infomation: network not reachable");
                System.Threading.Thread.Sleep(100);
            }
            if (throwOnRequestFailure && request.Status == StatusCode.Failure)
                throw new Exception("Failed to fetch package infomation.  Error details: " + request.Error.errorCode + " " + request.Error.message);
            return request.Result;
        }

        internal static PackageManager.PackageInfo[] UpmListOffline(string packageIdOrName = null)
        {
#if UNITY_2019_2_OR_NEWER
            var request = Client.List(true, true);
#else
            var request = Client.List(true);
#endif

            while (!request.IsCompleted)
                System.Threading.Thread.Sleep(100);
            var result = new List<PackageManager.PackageInfo>();
            foreach (var upmPackage in request.Result)
            {
                if (!string.IsNullOrEmpty(packageIdOrName) && !(upmPackage.name == packageIdOrName || upmPackage.packageId == packageIdOrName))
                    continue;
                result.Add(upmPackage);
            }
            return result.ToArray();
        }

        internal static string DownloadPackage(string packageId, string workingDirectory)
        {
            //No Need to delete the file, npm pack always overwrite: https://docs.npmjs.com/cli/pack
            var launcher = new NodeLauncher();
            launcher.WorkingDirectory = workingDirectory;
            launcher.NpmRegistry = NodeLauncher.ProductionRepositoryUrl;

            try
            {
                launcher.NpmPack(packageId);
            }
            catch (ApplicationException exception)
            {
                exception.Data["code"] = "fetchFailed";
                throw exception;
            }

            var packageName = launcher.OutputLog.ToString().Trim();
            return packageName;
        }

        internal static bool PackageExistsOnProduction(string packageId)
        {
            var launcher = new NodeLauncher();
            launcher.NpmRegistry = NodeLauncher.ProductionRepositoryUrl;

            try
            {
                launcher.NpmView(packageId);
            }
            catch (ApplicationException exception)
            {
                if (exception.Message.Contains("npm ERR! code E404") && exception.Message.Contains("is not in the npm registry."))
                    return false;
                exception.Data["code"] = "fetchFailed";
                throw exception;
            }

            var packageData = launcher.OutputLog.ToString().Trim();
            return !string.IsNullOrEmpty(packageData);
        }

        public static string ExtractPackage(string fullPackagePath, string workingPath, string outputDirectory, string packageName, bool deleteOutputDir = true)
        {
            //verify if package exists
            if (!fullPackagePath.EndsWith(".tgz"))
                throw new ArgumentException("Package should be a .tgz file");

            if (!File.Exists(fullPackagePath))
                throw new FileNotFoundException(fullPackagePath + " was not found.");

            if (deleteOutputDir)
            {
                try
                {
                    if (Directory.Exists(outputDirectory))
                        Directory.Delete(outputDirectory, true);

                    Directory.CreateDirectory(outputDirectory);
                }
                catch (IOException e)
                {
                    if (e.Message.ToLowerInvariant().Contains("1921"))
                        throw new ApplicationException("Failed to remove previous module in " + outputDirectory + ". Directory might be in use.");

                    throw;
                }
            }

            var tarPath = fullPackagePath.Replace(".tgz", ".tar");
            if (File.Exists(tarPath))
            {
                File.Delete(tarPath);
            }

            //Unpack the tgz into temp. This should leave us a .tar file
            PackageBinaryZipping.Unzip(fullPackagePath, workingPath);

            //See if the tar exists and unzip that
            var tgzFileName = Path.GetFileName(fullPackagePath);
            var targetTarPath = Path.Combine(workingPath, packageName +"-tar");
            if (Directory.Exists(targetTarPath))
            {
                Directory.Delete(targetTarPath, true);
            }

            if (File.Exists(tarPath))
            {
                PackageBinaryZipping.Unzip(tarPath, targetTarPath);
            }

            //Move the contents of the tar file into outputDirectory
            var packageFolderPath = Path.Combine(targetTarPath, "package");
            if (Directory.Exists(packageFolderPath))
            {
                //Move directories and meta files
                foreach (var dir in Directory.GetDirectories(packageFolderPath))
                {
                    var dirName = Path.GetFileName(dir);
                    if (dirName != null)
                    {
                        Directory.Move(dir, Path.Combine(outputDirectory, dirName));
                    }
                }

                foreach (var file in Directory.GetFiles(packageFolderPath))
                {
                    if (file.Contains("package.json") &&
                        !fullPackagePath.Contains(".tests") &&
                        !fullPackagePath.Contains(".samples") ||
                        !file.Contains("package.json"))
                    {
                        File.Move(file, Path.Combine(outputDirectory, Path.GetFileName(file)));
                    }
                }
            }

            //Remove the .tgz and .tar artifacts from temp
            List<string> cleanupPaths = new List<string>();
            cleanupPaths.Add(fullPackagePath);
            cleanupPaths.Add(tarPath);
            cleanupPaths.Add(targetTarPath);

            foreach (var p in cleanupPaths)
            {
                try
                {
                    FileAttributes attr = File.GetAttributes(p);
                    if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        // This is a directory
                        Directory.Delete(targetTarPath, true);
                        continue;
                    }

                    File.Delete(p);
                }
                catch (DirectoryNotFoundException)
                {
                    //Pass since there is nothing to delete
                }
            }

            return outputDirectory;
        }

        public static string GetMonoPath()
        {
            var monoPath = Path.Combine(EditorApplication.applicationContentsPath, "MonoBleedingEdge/bin", Application.platform == RuntimePlatform.WindowsEditor ? "mono.exe" : "mono");
            return monoPath;
        }
        
        public static bool IsTestAssembly(Assembly assembly)
        {
            // see https://unity.slack.com/archives/C26EP4SUQ/p1555485851157200?thread_ts=1555441110.131100&cid=C26EP4SUQ for details about how this is verified
            if (assembly.allReferences.Contains("TestAssemblies"))
            {
                return true;
            }
            
            // Marking an assembly with UNITY_INCLUDE_TESTS means: 
            // Include this assembly in the Unity project only if that package is in a testable state.
            // Otherwise, the assembly is ignored
            //
            // for now, we must read the test assembly file directly
            // because the defineConstraints field is not available on the assembly object
            AssemblyInfo assemblyInfo = Utilities.AssemblyInfoFromAssembly(assembly);
            AssemblyDefinition assemblyDefinition = Utilities.GetDataFromJson<AssemblyDefinition>(assemblyInfo.asmdefPath);
            return assemblyDefinition.defineConstraints.Contains("UNITY_INCLUDE_TESTS");
        }

        /// <summary>
        /// Returns the Assembly instances which contain one or more scripts in a package, given the list of files in the package.
        /// </summary>
        public static IEnumerable<Assembly> AssembliesForPackage(string packageRootPath)
        {
            var filesInPackage = Directory.GetFiles(packageRootPath, "*", SearchOption.AllDirectories);
            filesInPackage = filesInPackage.Select(p => p.Replace('\\', '/')).ToArray();

            var projectAssemblies = CompilationPipeline.GetAssemblies();
            var assemblyHash = new HashSet<Assembly>();

            foreach (var path in filesInPackage)
            {
                if (!string.Equals(Path.GetExtension(path), ".cs", StringComparison.OrdinalIgnoreCase))
                    continue;

                var assembly = GetAssemblyFromScriptPath(projectAssemblies, path);
                if (assembly != null && !Utilities.IsTestAssembly(assembly))
                {
                    assemblyHash.Add(assembly);
                }
            }

            return assemblyHash;
        }

        private static Assembly GetAssemblyFromScriptPath(Assembly[] assemblies, string scriptPath)
        {
            var fullScriptPath = Path.GetFullPath(scriptPath);

            foreach (var assembly in assemblies)
            {
                foreach (var packageSourceFile in assembly.sourceFiles)
                {
                    var fullSourceFilePath = Path.GetFullPath(packageSourceFile);

                    if (fullSourceFilePath == fullScriptPath)
                    {
                        return assembly;
                    }
                }
            }

            return null;
        }
        
        // Return all types from an assembly that can be loaded
        internal static IEnumerable<Type> GetTypesSafe(System.Reflection.Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (System.Reflection.ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null);
            }
        }
        
        internal static AssemblyInfo AssemblyInfoFromAssembly(Assembly assembly)
        {
            var path = CompilationPipeline.GetAssemblyDefinitionFilePathFromAssemblyName(assembly.name);
            if (string.IsNullOrEmpty(path))
                return null;

            var asmdefPath = Path.GetFullPath(path);
            return new AssemblyInfo(assembly, asmdefPath);
        }
    }
}
