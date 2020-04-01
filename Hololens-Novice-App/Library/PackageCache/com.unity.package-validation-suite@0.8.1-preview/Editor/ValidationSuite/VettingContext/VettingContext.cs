using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq;
using System.Threading;
using Semver;

using UnityEngine;
using UnityEngine.Networking;
using UnityEditor.PackageManager.ValidationSuite.ValidationTests;

namespace UnityEditor.PackageManager.ValidationSuite
{
    /// <summary>
    /// Class containing package data required for vetting.
    /// </summary>
    public class VettingContext
    {
        public bool IsCore { get; set; }

        public ManifestData ProjectPackageInfo { get; set; }
        public ManifestData PublishPackageInfo { get; set; }
        public ManifestData PreviousPackageInfo { get; set; }

        public ManifestData VSuiteInfo { get; set; }

        public string PreviousPackageBinaryDirectory { get; set; }
        public ValidationType ValidationType { get; set; }
        public const string PreviousVersionBinaryPath = "Temp/ApiValidationBinaries";
        public List<RelatedPackage> relatedPackages = new List<RelatedPackage>();

        public static VettingContext CreatePackmanContext(string packageId, ValidationType validationType)
        {
            VettingContext context = new VettingContext();
            var packageParts = packageId.Split('@');
            var packageList = Utilities.UpmListOffline();
            var packageInfo = packageList.SingleOrDefault(p => p.name == packageParts[0] && p.version == packageParts[1]);

            if (packageInfo == null)
            {
                throw new ArgumentException("Package Id " + packageId + " is not part of this project.");
            }

#if UNITY_2019_1_OR_NEWER
            context.IsCore = packageInfo.source == PackageSource.BuiltIn && packageInfo.type != "module";
#else
        context.IsCore = false; // there are no core packages before 2019.1
#endif
            context.ValidationType = validationType;
            context.ProjectPackageInfo = GetManifest(packageInfo.resolvedPath);

            if (context.ValidationType == ValidationType.LocalDevelopment || context.ValidationType == ValidationType.LocalDevelopmentInternal)
            {
                var publishPackagePath = PublishPackage(context);
                context.PublishPackageInfo = GetManifest(publishPackagePath);
            }
            else
            {
                context.PublishPackageInfo = GetManifest(packageInfo.resolvedPath);
            }

            foreach (var relatedPackage in context.PublishPackageInfo.relatedPackages)
            {
                // Check to see if the package is available locally
                // We are only focusing on local packages to avoid validation suite failures in CI
                // when the situation arises where network connection is impaired
                var foundRelatedPackage = Utilities.UpmListOffline().Where(p => p.name.Equals(relatedPackage.Key));
                var relatedPackageInfo = foundRelatedPackage.ToList();
                if (!relatedPackageInfo.Any())
                {
                    Debug.Log(String.Format("Cannot find the relatedPackage {0} ", relatedPackage.Key));
                    continue;
                }
                context.relatedPackages.Add(new RelatedPackage(relatedPackage.Key, relatedPackage.Value,
                    relatedPackageInfo.First().resolvedPath));
            }

            // No need to compare against the previous version of the package if we're testing out the verified set.
            if (context.ValidationType != ValidationType.VerifiedSet)
            {
                var previousPackagePath = GetPreviousPackage(context.ProjectPackageInfo);
                if (!string.IsNullOrEmpty(previousPackagePath))
                {
                    context.PreviousPackageInfo = GetManifest(previousPackagePath);
                    context.DownloadAssembliesForPreviousVersion();
                }
            }
            else
            {
                context.PreviousPackageInfo = null;
            }

            context.VSuiteInfo = GetPackageValidationSuiteInfo(packageList);

            return context;
        }

        public static VettingContext CreateAssetStoreContext(string packageName, string packageVersion, string packagePath, string previousPackagePath)
        {
            VettingContext context = new VettingContext();
            context.ProjectPackageInfo = new ManifestData() { path = packagePath, name = packageName, version = packageVersion };
            context.PublishPackageInfo = new ManifestData() { path = packagePath, name = packageName, version = packageVersion };
            context.PreviousPackageInfo = string.IsNullOrEmpty(previousPackagePath) ? null : new ManifestData() { path = previousPackagePath, name = packageName, version = "Previous" };
            context.ValidationType = ValidationType.AssetStore;
            return context;
        }

        public static ManifestData GetManifest(string packagePath)
        {
            // Start by parsing the package's manifest data.
            var manifestPath = Path.Combine(packagePath, Utilities.PackageJsonFilename);

            if (!File.Exists(manifestPath))
            {
                throw new FileNotFoundException(manifestPath);
            }

            // Read manifest json data, and convert it.
            var textManifestData = File.ReadAllText(manifestPath);
            var manifest = JsonUtility.FromJson<ManifestData>(textManifestData);
            manifest.path = packagePath;
            manifest.dependencies = ParseDictionary(textManifestData, "dependencies");
            manifest.relatedPackages = ParseDictionary(textManifestData, "relatedPackages");
            manifest.repository = ParseDictionary(textManifestData, "repository");
            manifest.lifecycle = ManifestData.EvaluateLifecycle(manifest.unity);

            return manifest;
        }

        private static Dictionary<string, string> ParseDictionary(string json, string key)
        {
            string minified = new Regex("[\"\\s]").Replace(json, "");
            var regex = new Regex(key + ":{(.*?)}");
            MatchCollection matches = regex.Matches(minified);
            if (matches.Count == 0)
                return new Dictionary<string, string>();

            string match = matches[0].Groups[1].Value;    // Group 0 is full match, group 1 is capture group
            if (match.Length == 0)                        // Found empty dictionary {}
                return new Dictionary<string, string>();

            string[] keyValuePairs = match.Split(',');
            return keyValuePairs.Select(kvp => kvp.Split(':')).ToDictionary(k => k[0], v => v[1]);
        }

        internal VersionChangeType VersionChangeType
        {
            get
            {
                if (PreviousPackageInfo == null || PreviousPackageInfo.version == null ||
                    PreviousPackageInfo == null || PreviousPackageInfo.version == null)
                {
                    return VersionChangeType.Unknown;
                }
                var prevVersion = SemVersion.Parse(PreviousPackageInfo.version);
                var curVersion = SemVersion.Parse(ProjectPackageInfo.version);

                if (curVersion.CompareByPrecedence(prevVersion) < 0)
                    throw new ArgumentException("Previous version number comes after current version number");

                if (curVersion.Major > prevVersion.Major)
                    return VersionChangeType.Major;
                if (curVersion.Minor > prevVersion.Minor)
                    return VersionChangeType.Minor;
                if (curVersion.Patch > prevVersion.Patch)
                    return VersionChangeType.Patch;

                throw new ArgumentException("Previous version number " + PreviousPackageInfo.version + " is the same major/minor/patch version as the current package " + ProjectPackageInfo.version);
            }
        }

        private static string PublishPackage(VettingContext context)
        {
            var packagePath = context.ProjectPackageInfo.path;
            if (context.ProjectPackageInfo.IsProjectTemplate)
            {
                return packagePath;
            }
            else
            {
                var tempPath = System.IO.Path.GetTempPath();
                string packageName = context.ProjectPackageInfo.Id.Replace("@", "-") + ".tgz";

                //Use upm-template-tools package-ci
                var packagesGenerated = PackageCIUtils.Pack(packagePath, tempPath);

                var publishPackagePath = Path.Combine(tempPath, "publish-" + context.ProjectPackageInfo.Id);
                var deleteOutput = true;
                foreach (var packageTgzName in packagesGenerated)
                {
                    Utilities.ExtractPackage(packageTgzName, tempPath, publishPackagePath, context.ProjectPackageInfo.name, deleteOutput);
                    deleteOutput = false;
                }

                return publishPackagePath;
            }
        }

        private static string GetPreviousPackage(ManifestData projectPackageInfo)
        {
            // List out available versions for a package.
            var foundPackages = Utilities.UpmSearch(projectPackageInfo.name);

            // If it exists, get the last one from that list.
            if (foundPackages != null && foundPackages.Length > 0)
            {
                var packageInfo = foundPackages[0];
                var version = SemVersion.Parse(projectPackageInfo.version);
                var previousVersions = packageInfo.versions.all.Where(v =>
                {
                    var prevVersion = SemVersion.Parse(v);
                // ignore pre-release and build tags when finding previous version
                return prevVersion < version && !(prevVersion.Major == version.Major && prevVersion.Minor == version.Minor && prevVersion.Patch == version.Patch);
                });

                // Find the last version on Production
                string previousVersion = null;
                previousVersions = previousVersions.Reverse();
                foreach (var prevVersion in previousVersions)
                {
                    if (Utilities.PackageExistsOnProduction(packageInfo.name + "@" + prevVersion))
                    {
                        previousVersion = prevVersion;
                        break;
                    }
                }

                if (previousVersion != null)
                {
                    try
                    {
                        var previousPackageId = ManifestData.GetPackageId(projectPackageInfo.name, previousVersion);
                        var tempPath = Path.GetTempPath();
                        var previousPackagePath = Path.Combine(tempPath, "previous-" + previousPackageId);
                        var packageFileName = Utilities.DownloadPackage(previousPackageId, tempPath);
                        Utilities.ExtractPackage(Path.Combine(tempPath, packageFileName), tempPath, previousPackagePath, projectPackageInfo.name);
                        return previousPackagePath;
                    }
                    catch (Exception exception)
                    {
                        // Failing to fetch when there is no prior version, which is an accepted case.
                        if ((string)exception.Data["reason"] == "fetchFailed")
                            EditorUtility.DisplayDialog("Data: " + exception.Message, "Failed", "ok");
                    }
                }
            }
            return string.Empty;
        }

        private void DownloadAssembliesForPreviousVersion()
        {
            if (Directory.Exists(PreviousVersionBinaryPath))
                Directory.Delete(PreviousVersionBinaryPath, true);

            Directory.CreateDirectory(PreviousVersionBinaryPath);

            var packageDataZipFilename = PackageBinaryZipping.PackageDataZipFilename(PreviousPackageInfo.name, PreviousPackageInfo.version);
            var zipPath = Path.Combine(PreviousVersionBinaryPath, packageDataZipFilename);
            var uri = Path.Combine("https://artifactory.eu-cph-1.unityops.net/pkg-api-validation", packageDataZipFilename);

            UnityWebRequest request = new UnityWebRequest(uri);
            request.timeout = 60; // 60 seconds time out
            request.downloadHandler = new DownloadHandlerFile(zipPath);
            var operation = request.SendWebRequest();
            while (!operation.isDone)
                Thread.Sleep(1);

            if (request.isHttpError || request.isNetworkError || !PackageBinaryZipping.Unzip(zipPath, PreviousVersionBinaryPath))
            {
                Debug.Log(String.Format("Could not download binary assemblies for previous package version from {0}. {1}", uri, request.responseCode));
                PreviousPackageBinaryDirectory = null;
            }
            else
                PreviousPackageBinaryDirectory = PreviousVersionBinaryPath;
        }

        private static ManifestData GetPackageValidationSuiteInfo(PackageInfo[] packageList)
        {
            var vSuitePackageInfo = packageList.SingleOrDefault(p => p.name == Utilities.VSuiteName);

            if (vSuitePackageInfo == null)
            {
                throw new ArgumentException($"The package {Utilities.VSuiteName} could not be found in this project.");
            }

            return new ManifestData()
            {
                version = vSuitePackageInfo.version,
                name = vSuitePackageInfo.name,
                displayName = vSuitePackageInfo.displayName
            };
        }
    }
}
