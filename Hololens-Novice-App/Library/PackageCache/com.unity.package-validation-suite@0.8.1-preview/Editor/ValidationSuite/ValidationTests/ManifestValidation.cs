using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Semver;

namespace UnityEditor.PackageManager.ValidationSuite.ValidationTests
{
    internal class ManifestValidation : BaseValidation
    {
        private string[] PackageNamePrefixList = { "com.unity.", "com.autodesk.", "com.havok.", "com.ptc." };
        private const string UpmRegex = @"^[a-z0-9][a-z0-9-._]{0,213}$";
        private const string UpmDisplayRegex = @"^[a-zA-Z0-9 ]+$";
        private const int MinDescriptionSize = 50;
        private const int MaxDisplayNameLength = 50;

        public ManifestValidation()
        {
            TestName = "Manifest Validation";
            TestDescription = "Validate that the information found in the manifest is well formatted.";
            TestCategory = TestCategory.DataValidation;
            SupportedValidations = new[] { ValidationType.CI, ValidationType.LocalDevelopment, ValidationType.LocalDevelopmentInternal, ValidationType.Publishing, ValidationType.VerifiedSet };
        }

        protected override void Run()
        {
            // Start by declaring victory
            TestState = TestState.Succeeded;

            var manifestData = Context.ProjectPackageInfo;
            if (manifestData == null)
            {
                AddError("Manifest not available. Not validating manifest contents.");
                return;
            }

            ValidateManifestData();
            ValidateDependencies();
            ValidateDependencyChanges();
        }

        private void ValidateDependencies()
        {
            // if the package is a production quality package, it can't have preview dependencies.
            if (!Context.ProjectPackageInfo.IsPreview)
            {
                foreach (var dependency in Context.ProjectPackageInfo.dependencies)
                {
                    if (Utilities.IsPreviewVersion(dependency.Value))
                    {
                        AddError("This production quality package has a dependency on preview package \"{0}\".  Production quality packages can only depend on other production quality packages.", dependency.Value);
                    }
                }
            }

            // Make sure all dependencies are already published in production.
            foreach (var dependency in Context.ProjectPackageInfo.dependencies)
            {
                var packageId = Utilities.CreatePackageId(dependency.Key, dependency.Value);

                var dependencyInfo = Utilities.UpmListOffline(dependency.Key).FirstOrDefault();

                // Built in packages are shipped with the editor, and will therefore never by published to production.
                if (dependencyInfo != null && dependencyInfo.source == PackageSource.BuiltIn)
                {
                    continue;
                }

                // Check if this package's dependencies are in production. That is a requirement for publishing.
                if (Context.ValidationType != ValidationType.VerifiedSet && !Utilities.PackageExistsOnProduction(packageId))
                {
                    if (Context.ValidationType == ValidationType.Publishing || Context.ValidationType == ValidationType.AssetStore)
                        AddError("Package dependency {0} is not published in production.", packageId);
                    else
                        AddWarning("Package dependency {0} must be published to production before this package is published to production.  (Except for core packages)", packageId);
                }
                
                // only check this in CI or internal local development
                // Make sure the dependencies I ask for that exist in the project have the good version
                if (Context.ValidationType == ValidationType.CI || Context.ValidationType == ValidationType.LocalDevelopmentInternal) {
                    PackageInfo packageInfo = Utilities.UpmListOffline(dependency.Key).FirstOrDefault();
                    if (packageInfo != null && packageInfo.version != dependency.Value)
                    {
                        AddWarning("The package {2} depends on {0}, which is found locally but with another version. To remove this warning, in the package.json file of {2}, change the dependency of {0}@{1} to {0}@{3}.", dependency.Key, dependency.Value, Context.ProjectPackageInfo.name, packageInfo.version);
                    }
                }
            }

            // TODO: Validate the Package dependencies meet the minimum editor requirement (eg: 2018.3 minimum for package A is 2, make sure I don't use 1)
        }

        private void ValidateDependencyChanges()
        {
            var versionChangeType = Context.VersionChangeType;

            var previousRefs = Context.PreviousPackageInfo == null ? null : Context.PreviousPackageInfo.dependencies;
            var projectRefs = Context.ProjectPackageInfo.dependencies ?? new Dictionary<string, string>();

            foreach (var projectRef in projectRefs)
            {
                SemVersion projectRefSemver;
                if (!SemVersion.TryParse(projectRef.Value, out projectRefSemver))
                {
                    AddError(@"Invalid version number in dependency ""{0}"" : ""{1}""", projectRef.Key, projectRef.Value);
                    continue;
                }
            }

            if (previousRefs != null)
            {
                foreach (var previousRef in previousRefs)
                {
                    SemVersion previousSemver;
                    if (!SemVersion.TryParse(previousRef.Value, out previousSemver))
                        AddError(String.Format(@"Invalid version number in previous package dependency ""{0}"" : ""{1}""", previousRef.Key, previousRef.Value));
                }
            }
        }

        private void ValidateManifestData()
        {
            var manifestData = Context.ProjectPackageInfo;

            // Check the package Name, which needs to start with one of the approved company names.
            if (!PackageNamePrefixList.Any(namePrefix => (manifestData.name.StartsWith(namePrefix) && manifestData.name.Length > namePrefix.Length)))
            {
                AddError("In package.json, \"name\" needs to start with one of these approved company names: " + string.Join(", ", PackageNamePrefixList));
            }

            // There cannot be any capital letters in package names.
            if (manifestData.name.ToLower(CultureInfo.InvariantCulture) != manifestData.name)
            {
                AddError("In package.json, \"name\" cannot contain capital letter");
            }

            // Check name against our regex.
            Match match = Regex.Match(manifestData.name, UpmRegex);
            if (!match.Success)
            {
                AddError("In package.json, \"name\" is not a valid name.");
            }

            if (string.IsNullOrEmpty(manifestData.displayName))
            {
                AddError("In package.json, \"displayName\" must be set.");
            }
            else if (manifestData.displayName.Length > MaxDisplayNameLength)
            {
                AddError($"In package.json, \"displayName\" is too long. Max Length = {MaxDisplayNameLength}. Current Length = {manifestData.displayName.Length}");
            }
            else if (!Regex.Match(manifestData.displayName, UpmDisplayRegex).Success)
            {
                AddError("In package.json, \"displayName\" cannot have any special characters.");
            }

            // Check Description, make sure it's there, and not too short.
            if (manifestData.description.Length < MinDescriptionSize)
            {
                AddError("In package.json, \"description\" must be fleshed out and informative, as it is used in the user interface.");
            }

            if (Context.ValidationType == ValidationType.Publishing || Context.ValidationType == ValidationType.CI)
            {
                // Check if `repository.url` and `repository.revision` exist and the content is valid
                string value;
                if (!manifestData.repository.TryGetValue("url", out value) || string.IsNullOrEmpty(value))
                    AddError("In package.json for a published package, there must be a \"repository.url\" field.");
                if (!manifestData.repository.TryGetValue("revision", out value) || string.IsNullOrEmpty(value))
                    AddError("In package.json for a published package, there must be a \"repository.revision\" field.");
            }
            else
            {
                AddInformation("Skipping Git tags check as this is a package in development.");
            }

            ValidateVersion(manifestData);
        }

        private void ValidateVersion(ManifestData manifestData)
        {
            // Check package version, make sure it's a valid SemVer string.
            SemVersion packageVersionNumber;
            if (!SemVersion.TryParse(manifestData.version, out packageVersionNumber))
            {
                AddError("In package.json, \"version\" needs to be a valid \"Semver\".");
                return;
            }

            if (Context.IsCore)
            {
                if (!string.IsNullOrEmpty(packageVersionNumber.Prerelease) || packageVersionNumber.Major < 1)
                {
                    AddError("Core packages cannot be preview packages.");
                    return;
                }
            }

            if (packageVersionNumber.Major < 1)
            {
                if (string.IsNullOrEmpty(packageVersionNumber.Prerelease) || packageVersionNumber.Prerelease.Split('.')[0] != "preview")
                {
                    AddError("In package.json, \"version\" < 1, which makes it a preview version, please tag the package as " + packageVersionNumber.VersionOnly() + "-preview");
                    return;
                }
            }

            if (!string.IsNullOrEmpty(packageVersionNumber.Prerelease))
            {
                // We must strip the -build<commit> off the prerelease
                var buildInfoIndex = packageVersionNumber.Prerelease.IndexOf("build");
                if (buildInfoIndex > 0)
                {
                    var cleanPrerelease = packageVersionNumber.Prerelease.Substring(0, buildInfoIndex - 1);
                    packageVersionNumber = packageVersionNumber.Change(null, null, null, cleanPrerelease, null);
                }
                else
                {
                    packageVersionNumber = packageVersionNumber.Change(null, null, null, "", null);
                }

                // The only pre-release tag we support is -preview
                if (!string.IsNullOrEmpty(packageVersionNumber.Prerelease))
                {
                    var preleleaseParts = packageVersionNumber.Prerelease.Split('.');

                    if ((preleleaseParts.Length > 2) || (preleleaseParts[0] != ("preview")))
                    {
                        AddError("In package.json, \"version\": the only pre-release filter supported is \"-preview.[num < 999]\".");
                    }

                    if (preleleaseParts.Length > 1 && !string.IsNullOrEmpty(preleleaseParts[1]))
                    {
                        int previewVersion;
                        var results = int.TryParse(preleleaseParts[1], out previewVersion);
                        if (!results || previewVersion > 999)
                        {
                            AddError("In package.json, \"version\": the only pre-release filter supported is \"-preview.[num < 999]\".");
                        }
                    }
                }
            }
        }
    }
}
