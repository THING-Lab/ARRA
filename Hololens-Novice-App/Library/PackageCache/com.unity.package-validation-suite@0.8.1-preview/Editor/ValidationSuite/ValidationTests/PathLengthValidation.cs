using System;
using System.IO;

namespace UnityEditor.PackageManager.ValidationSuite.ValidationTests
{
    internal class PathLengthValidation : BaseValidation
    {
        public int MaxPathLength { get; set; } = 140;

        public PathLengthValidation()
        {
            TestName = "Path Length Validation";
            TestDescription = "Validate that all package files are below a maximum path threshold, to ensure that excessively long paths are not produced on Windows machines within user projects.";
            TestCategory = TestCategory.ContentScan;
            SupportedValidations = new[] { ValidationType.CI, ValidationType.LocalDevelopment, ValidationType.LocalDevelopmentInternal, ValidationType.Publishing, ValidationType.VerifiedSet };
        }

        private static string CombineAllowingEmpty(string path1, string path2)
        {
            if (string.IsNullOrEmpty(path1))
                return path2;
            if (string.IsNullOrEmpty(path2))
                return path1;
            return Path.Combine(path1, path2);
        }

        private bool IsHiddenDirectory(string path)
        {
            if (path.Length > 0 
            && (path[path.Length-1] == Path.DirectorySeparatorChar
            ||  path[path.Length-1] == Path.AltDirectorySeparatorChar))
                path = path.Substring(0, path.Length-1);

            string dir = Path.GetFileName(path);

            if (dir.EndsWith("~") || dir.StartsWith("."))
                return true;
            return false;
        }

        void CheckPathLengthInFolderRecursively(string relativeFolder, string absoluteBasePath)
        {
            try
            {
                var fullFolder = CombineAllowingEmpty(absoluteBasePath, relativeFolder);
                if (IsHiddenDirectory(fullFolder))
                    return;

                foreach (string entry in Directory.GetFileSystemEntries(fullFolder))
                {
                    var fullPath = CombineAllowingEmpty(relativeFolder, Path.GetFileName(entry));
                    if (fullPath.Length > MaxPathLength)
                    {
                        AddError($"{fullPath} is {fullPath.Length} characters, which is longer than the limit of {MaxPathLength} characters. You must use shorter names.");
                    }
                }

                foreach (string dir in Directory.GetDirectories(fullFolder))
                {
                    CheckPathLengthInFolderRecursively(CombineAllowingEmpty(relativeFolder, Path.GetFileName(dir)), absoluteBasePath);
                }
            }
            catch (Exception e)
            {
                AddError("Exception " + e.Message);
            }
        }

        protected override void Run()
        {
            // Start by declaring victory
            TestState = TestState.Succeeded;

            var rootPath = Context.PublishPackageInfo.path;
            if (!Path.IsPathRooted(Context.PublishPackageInfo.path)) 
                rootPath = Path.GetFullPath(rootPath);

            //check if each file/folder has a sufficiently short path relative to the base
            CheckPathLengthInFolderRecursively(string.Empty, rootPath);
        }
    }
}
