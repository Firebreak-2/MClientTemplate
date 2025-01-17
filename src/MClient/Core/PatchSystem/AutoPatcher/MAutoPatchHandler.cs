﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Harmony;

namespace MClient.Core.PatchSystem.AutoPatcher
{
    /// <summary>
    /// The handler that does the patching of AutoPatch attributes.
    /// </summary>
    public static class MAutoPatchHandler
    {
        
        /// <summary>
        /// The patching method that finds and executes Auto-Patches.
        /// </summary>
        public static void Patch()
        {

            var harmony = HarmonyLoader.Loader.harmonyInstance;
            
            MLogger.Log("AutoPatcher started", logSection: MLogger.MLogSection.Ptch);

            foreach (var origInfo in GetAllAutoPatches())
            {
                List<MAutoPatchAttribute> attributes = origInfo.GetCustomAttributes(typeof(MAutoPatchAttribute),false).Cast<MAutoPatchAttribute>().ToList();

                foreach (var attribute in attributes)
                {
                    var mPatch = AccessTools.DeclaredMethod(attribute.Type, attribute.Method, attribute.Params);

                    if (mPatch is null)
                    {
                        MLogger.Log("Failed to find specified method: " + attribute.Method + ". on type of: " + attribute.Type.Name, MLogger.MLogType.Warning,
                            MLogger.MLogSection.Ptch);
                    }
                    else
                    {
                        switch (attribute.PatchType)
                        {
                            case MPatchType.Prefix:
                                harmony.Patch(mPatch, new HarmonyMethod(origInfo));
                                break;
                            case MPatchType.Postfix:
                                harmony.Patch(mPatch, null, new HarmonyMethod(origInfo));
                                break;
                            case MPatchType.Transpiler:
                                harmony.Patch(mPatch, null, null, new HarmonyMethod(origInfo));
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                        MLogger.Log("Patched method " + origInfo.DeclaringType?.Name + "." + origInfo.Name + " Onto " + attribute.Type.Name + "." + attribute.Method, logSection:
                            MLogger.MLogSection.Ptch);
                    }
                }
            }

            MLogger.Log("AutoPatcher finished", logSection: MLogger.MLogSection.Ptch);
        }

        private static IEnumerable<MethodInfo> GetAllAutoPatches()
        {
            return Assembly.GetExecutingAssembly().GetTypes().SelectMany(x => x.GetMethods()).Where(x =>
                x.GetCustomAttributes(typeof(MAutoPatchAttribute), false).FirstOrDefault() != null);
        }
    }
}