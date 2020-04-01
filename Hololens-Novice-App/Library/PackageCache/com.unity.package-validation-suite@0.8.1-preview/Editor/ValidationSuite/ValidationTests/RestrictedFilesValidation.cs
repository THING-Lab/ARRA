using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace UnityEditor.PackageManager.ValidationSuite.ValidationTests
{
    internal class RestrictedFilesValidation : BaseValidation
    {
        public RestrictedFilesValidation()
        {
            TestName = "Restricted File Type Validation";
            TestDescription = "Make sure no restricted file types are included with this package.";
            TestCategory = TestCategory.ContentScan;
        }

        protected override void Run()
        {
            // Start by declaring victory
            TestState = TestState.Succeeded;

            // from the published project dir, check if each file type is present.
            foreach (var fileType in restrictedFileList)
            {
                var isExtensionRestriction = fileType.StartsWith("*.");
                List < string> matchingFiles = new List<string>();
                DirectorySearch(Context.PublishPackageInfo.path, fileType, ref matchingFiles);

                if (matchingFiles.Any())
                {
                    foreach (var file in matchingFiles)
                    {
                        // For asset store packages, no exceptions.
                        // Internally, let's allow a specific set of exceptions.
                        if (Context.ValidationType == ValidationType.AssetStore ||
                            !internalExceptionFileList.Any(ex => ex.Equals(Path.GetFileName(file), StringComparison.OrdinalIgnoreCase)))
                        {
                            // Workaround for weird behavior in Directory.GetFiles call, which will return File.Commute when searching for *.com
                            if (!isExtensionRestriction || (Path.GetExtension(fileType.ToLower()) == Path.GetExtension(file.ToLower())))
                                AddError(file + " cannot be included in a package.");
                        }
                    }
                }
            }

            // from the published project dir, check if each file type is present.
            foreach (var fileType in unapprovedFileList)
            {
                var isExtensionRestriction = fileType.StartsWith("*.");
                List<string> matchingFiles = new List<string>();
                DirectorySearch(Context.PublishPackageInfo.path, fileType, ref matchingFiles);

                if (matchingFiles.Any())
                {
                    foreach (var file in matchingFiles)
                    {
                        if (!isExtensionRestriction || (Path.GetExtension(fileType.ToLower()) == Path.GetExtension(file.ToLower())))
                            AddWarning(file + " should not be included in packages unless absolutely necessary.  " + "Please confirm that it's inclusion is deliberate and intentional.");
                    }
                }
            }
        }

        private readonly string[] internalExceptionFileList =
        {
            "vswhere.exe",            // required for com.unity.ide.visualstudio
            "bcl.exe",                // required for com.unity.burst
            "lld.exe",                // required for com.unity.burst
            "burst-llvm.lib",         // required for com.unity.burst
            "burst-llvm.dll",         // required for com.unity.burst
            "Burst.Backend.dll",      // required for com.unity.burst
            "Burst.Compiler.IL.dll",  // required for com.unity.burst
            "Smash.dll",              // required for com.unity.burst
            "Unity.Cecil.dll",        // required for com.unity.burst
            "Unity.Cecil.Mdb.dll",    // required for com.unity.burst
            "Unity.Cecil.Pdb.dll",    // required for com.unity.burst
            "Unity.Cecil.Rocks.dll",  // required for com.unity.burst
            "uncrustify.exe",         // required for com.unity.coding
            "FindMissingDocs.exe",    // required for com.unity.package-validation-suite
            "coding-cli.exe",         // required for com.unity.coding
            "ApiScraper.exe",         // required for com.unity.coding
            "arcoreimg.exe",          // required for com.unity.xr.arcore
            "bee.exe",                // required for com.unity.tiny
            "HavokVisualDebugger.exe", // required for com.havok.physics
            "Unity.CollabProxy.Server.exe",  // required for com.unity.collab-proxy
            "COMIntegration.exe",       // required for com.unity.ide.visualstudio
            "Unity.ProcessServer.exe", // required for com.unity.process-server
            "ilspycmd.exe",             // required for com.unity.entities
            "bee.dll",                  // required by com.unity.platforms
            "pram.exe",                 // required by com.unity.platforms
            "tundra2.exe"               // required by com.unity.platforms
        };

        private readonly string[] restrictedFileList =
        {
            "*.bat",
            "*.bin",
            "*.com",
            "*.csh",
            "*.dom",
            "*.exe",
            "*.jse",
            "*.jpg",
            "*.jpeg",
            "*.msi",
            "*.msp",
            "*.mst",
            "*.ps1",
            "*.vb",
            "*.vbe",
            "*.vbs",
            "*.vbscript",
            "*.vs",
            "*.vsd",
            "*.vsh",
            "AssetStoreTools.dll",
            "AssetStoreToolsExtra.dll",
            "DroidSansMono.ttf"
        };

        private readonly string[] unapprovedFileList =
        {
            "Standard Assets.*",
            "*.unitypackage",
            "*.zip",
            "*.rar",
            "*.lib",
            "*.dll",
            "*.js",
        };
    }
}
