using System;
using System.Reflection;
using RushOps.Audio;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

namespace RushOps.Editor.Audio
{
    /// <summary>
    /// Creates the game's audio mixer (issue #108, F-AUD-04): Master with the groups Musik and Effekte, and the
    /// exposed volumes MasterVolume, MusicVolume and EffectsVolume (<see cref="AudioVolumeMath.ParameterName"/>).
    /// Unity has no public API to create a mixer, so this uses the editor's internal AudioMixerController by
    /// reflection. An existing mixer is kept as it is.
    /// </summary>
    public static class AudioMixerBuilder
    {
        /// <summary>Asset path of the mixer.</summary>
        public const string MixerPath = "Assets/_Project/Audio/RushOps.mixer";

        /// <summary>Name of the music group.</summary>
        public const string MusicGroup = "Musik";

        /// <summary>Name of the effects group.</summary>
        public const string EffectsGroup = "Effekte";

        private const BindingFlags Any = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        /// <summary>Loads the mixer, creating it first if needed.</summary>
        public static AudioMixer LoadOrCreate()
        {
            var mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(MixerPath);
            return mixer != null ? mixer : Create();
        }

        /// <summary>The group called <paramref name="name"/> of the mixer.</summary>
        public static AudioMixerGroup Group(AudioMixer mixer, string name)
        {
            AudioMixerGroup[] groups = mixer.FindMatchingGroups(name);
            if (groups.Length == 0)
            {
                throw new InvalidOperationException($"Mixer {mixer.name} has no group {name}");
            }

            return groups[0];
        }

        private static AudioMixer Create()
        {
            Assembly editor = typeof(UnityEditor.Editor).Assembly;
            Type controllerType = Require(editor.GetType("UnityEditor.Audio.AudioMixerController"), "AudioMixerController");
            Type groupType = Require(editor.GetType("UnityEditor.Audio.AudioMixerGroupController"), "AudioMixerGroupController");
            Type pathType = Require(editor.GetType("UnityEditor.Audio.AudioGroupParameterPath"), "AudioGroupParameterPath");

            object controller = Method(controllerType, "CreateMixerControllerAtPath").Invoke(null, new object[] { MixerPath });
            object master = controllerType.GetProperty("masterGroup", Any).GetValue(controller);
            object music = AddGroup(controllerType, controller, master, MusicGroup);
            object effects = AddGroup(controllerType, controller, master, EffectsGroup);

            MethodInfo expose = Method(controllerType, "AddExposedParameter");
            MethodInfo volumeGuid = Method(groupType, "GetGUIDForVolume");
            var names = new System.Collections.Generic.Dictionary<string, string>();
            foreach ((object group, AudioChannel channel) in new[]
                     { (master, AudioChannel.Master), (music, AudioChannel.Music), (effects, AudioChannel.Effects) })
            {
                object guid = volumeGuid.Invoke(group, null);
                expose.Invoke(controller, new[] { Activator.CreateInstance(pathType, group, guid) });
                names[guid.ToString()] = AudioVolumeMath.ParameterName(channel);
            }

            NameExposedParameters(controllerType, controller, names);
            EditorUtility.SetDirty((UnityEngine.Object)controller);
            AssetDatabase.SaveAssets();
            return AssetDatabase.LoadAssetAtPath<AudioMixer>(MixerPath);
        }

        private static object AddGroup(Type controllerType, object controller, object parent, string name)
        {
            object group = Method(controllerType, "CreateNewGroup").Invoke(controller, new object[] { name, false });
            Method(controllerType, "AddChildToParent").Invoke(controller, new[] { group, parent });
            return group;
        }

        // Exposed parameters start as "MyExposedParam..." and are kept sorted, so rename them by their GUID.
        private static void NameExposedParameters(
            Type controllerType, object controller, System.Collections.Generic.Dictionary<string, string> names)
        {
            PropertyInfo property = Require(controllerType.GetProperty("exposedParameters", Any), "exposedParameters");
            var parameters = (Array)property.GetValue(controller);
            if (parameters.Length != names.Count)
            {
                throw new InvalidOperationException($"Mixer has {parameters.Length} exposed parameters, expected {names.Count}");
            }

            for (int i = 0; i < parameters.Length; i++)
            {
                object parameter = parameters.GetValue(i);
                Type parameterType = parameter.GetType();
                string guid = parameterType.GetField("guid", Any).GetValue(parameter).ToString();
                parameterType.GetField("name", Any).SetValue(parameter, names[guid]);
                parameters.SetValue(parameter, i);
            }

            property.SetValue(controller, parameters);
        }

        private static MethodInfo Method(Type type, string name)
        {
            return Require(type.GetMethod(name, Any), $"{type.Name}.{name}");
        }

        private static T Require<T>(T value, string what) where T : class
        {
            return value ?? throw new InvalidOperationException($"Unity editor API not found: {what}");
        }
    }
}
