using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Unity.Platforms
{
    public abstract class BuildTarget
    {
        static readonly List<BuildTarget> m_AvailableBuildTargets;

        static BuildTarget()
        {
#if UNITY_2019_2_OR_NEWER
            var buildTargetTypes = UnityEditor.TypeCache.GetTypesDerivedFrom<BuildTarget>().ToList();
#else
            var buildTargetTypes = new List<Type>();
#endif

            if (buildTargetTypes.Count == 0)
            {
                // If UnityEditor.TypeCache wasn't ready, manually find all BuildTarget types
                var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(x => x.GetName().Name.Contains(typeof(BuildTarget).Assembly.GetName().Name));
                foreach (var assembly in assemblies)
                {
                    foreach (var type in assembly.GetLoadableTypes())
                    {
                        if (!typeof(BuildTarget).IsAssignableFrom(type))
                        {
                            continue;
                        }
                        buildTargetTypes.Add(type);
                    }
                }
            }

            m_AvailableBuildTargets = new List<BuildTarget>();
            foreach (var buildTargetType in buildTargetTypes)
            {
                try
                {
                    if (buildTargetType.IsAbstract)
                    {
                        continue;
                    }

                    var buildTarget = (BuildTarget)Activator.CreateInstance(buildTargetType);
                    if (!buildTarget.HideInBuildTargetPopup)
                    {
                        m_AvailableBuildTargets.Add(buildTarget);
                        if (buildTarget.IsDefaultBuildTarget)
                        {
                            if (DefaultBuildTarget != null)
                            {
                                UnityEngine.Debug.LogError($"Cannot set {nameof(DefaultBuildTarget)} to '{buildTarget.GetType().FullName}' because it is already set to '{DefaultBuildTarget.GetType().FullName}'.");
                                continue;
                            }
                            DefaultBuildTarget = buildTarget;
                        }
                    }
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError($"Error instantiating '{buildTargetType.FullName}': " + e.Message);
                }
            }
        }

        public static IReadOnlyList<BuildTarget> AvailableBuildTargets => m_AvailableBuildTargets;
        public static BuildTarget DefaultBuildTarget { get; }
        public virtual bool HideInBuildTargetPopup => false;
        protected virtual bool IsDefaultBuildTarget => false;

        public override string ToString()
        {
            return GetDisplayName();
        }

        public abstract string GetDisplayName();
        public abstract string GetUnityPlatformName();
        public abstract string GetExecutableExtension();
        public abstract string GetBeeTargetName();
        public abstract bool Run(FileInfo buildTarget);
    }

    public sealed class EditorBuildTarget : BuildTarget
    {
        public override bool HideInBuildTargetPopup => true;

        public override string GetDisplayName()
        {
            return "Editor";
        }

        public override string GetUnityPlatformName()
        {
            return UnityEditor.EditorUserBuildSettings.activeBuildTarget.ToString();
        }

        public override string GetExecutableExtension()
        {
            throw new NotSupportedException();
        }

        public override bool Run(FileInfo buildTarget)
        {
            throw new NotSupportedException();
        }

        public override string GetBeeTargetName()
        {
            throw new NotSupportedException();
        }
    }

    static class AssemblyExtensions
    {
        public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
        {
            if (assembly == null)
            {
                return Enumerable.Empty<Type>();
            }

            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException exception)
            {
                return exception.Types.Where(type => type != null);
            }
        }
    }
}
