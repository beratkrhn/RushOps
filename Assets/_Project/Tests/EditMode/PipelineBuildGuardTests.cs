using System;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Build;

namespace RushOps.Tests.EditMode
{
    /// <summary>
    /// The Unity Pipeline package is an editor tool (ADR 0007). Its runtime server allows code execution
    /// over a local HTTP port, so it must never be compiled into a player build. These define symbols
    /// would switch it on.
    /// </summary>
    public class PipelineBuildGuardTests
    {
        private static readonly string[] ForbiddenSymbols = { "ENABLE_RUNTIME_PIPELINE", "ENABLE_RUNTIME_PIPELINE_DEV" };

        [TestCase(BuildTargetGroup.iOS)]
        [TestCase(BuildTargetGroup.Android)]
        [TestCase(BuildTargetGroup.Standalone)]
        public void ScriptingDefines_DoNotEnablePipelineInPlayer(BuildTargetGroup target)
        {
            string defines = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(target));
            string[] symbols = defines.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string forbidden in ForbiddenSymbols)
            {
                CollectionAssert.DoesNotContain(symbols, forbidden, $"{forbidden} is set for {target}");
            }
        }
    }
}
