using Harmony;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CHModloader
{
    public class ModLoader : MonoBehaviour
    {
        public static bool ModLoaderLoaded { get; private set; }
        public static List<Mod> LoadedMods { get; private set; }
        public static ModLoader Instance { get; private set; }
        public static Textures loadTextures;
        public static AssetBundles loadAssetBundles;
        public static Sounds loadSounds;
        public static HarmonyInstance Harmony { get; private set; }

        public static readonly string Version = "0.1";
        public static string ModsPath = GetGameRootPath() + "/Mods/";

        public static void Init()
        {
            if (!Directory.Exists(ModsPath))
            {
                Directory.CreateDirectory(ModsPath);
            }

            ModLogs.EnableLogs();

            SceneManager.sceneLoaded += OnSceneLoaded;

            if (ModLoaderLoaded || Instance)
            {
                ModLogs.Log("----- CHModloader is already loaded! -----\n");
                return; 
            }

            GameObject ModHandler = new GameObject();
            ModHandler.AddComponent<ModLoader>();
            ModHandler.AddComponent<Textures>();
            ModHandler.AddComponent<AssetBundles>();
            ModHandler.AddComponent<Sounds>();
            Instance = ModHandler.GetComponent<ModLoader>();
            loadTextures = ModHandler.GetComponent<Textures>();
            loadAssetBundles = ModHandler.GetComponent<AssetBundles>();
            loadSounds = ModHandler.GetComponent<Sounds>();
            GameObject.DontDestroyOnLoad(ModHandler);

            ModLogs.Log("----- Initializing CHModloader... -----\n");
            Console.WriteLine("CHModloader v{0}", Version);
            ModLoaderLoaded = false;
            LoadedMods = new List<Mod>();

            ModLogs.Log("Initializing harmony...");
            Harmony = HarmonyInstance.Create("com.github.harmony.ch.mod");

            ModLogs.Log("Loading internal mods...");
            LoadMod(new ModUI());
            LoadMod(new ModConsole());
            //LoadMod(new NoUpdate());

            ModLogs.Log("Loading mods...");
            LoadMods();

            ModLoaderLoaded = true;
            ModLogs.Log("Finished loading.");
        }

        public static void LoadMods()
        {
            foreach (string dir in Directory.GetDirectories(ModsPath))
            {
                foreach (string file in Directory.GetFiles(dir))
                {
                    if (file.EndsWith(".dll"))
                    {
                        LoadDLL(file);
                        continue;
                    }
                }
            }
        }
        
        private static void LoadDLL(string file)
        {
            Assembly assembly = Assembly.LoadFrom(file);

            foreach(Type type in assembly.GetTypes())
            {
                if (type.IsSubclassOf(typeof(Mod)))
                {
                    LoadMod((Mod)Activator.CreateInstance(type));
                }
            }

            // load any harmony patches within the Mod assembly
            Harmony.PatchAll(assembly);
        }

        private static void LoadMod(Mod mod)
        {
            if (!LoadedMods.Contains(mod))
            {
                try
                {
                    if (mod.HasTextures)
                    {
                        if (!Directory.Exists(ModsPath + mod.ID + "/Textures"))
                            Directory.CreateDirectory(ModsPath + mod.ID + "/Textures");
                    }

                    if (mod.HasAssetBundles)
                    {
                        if (!Directory.Exists(ModsPath + mod.ID + "/AssetBundles"))
                            Directory.CreateDirectory(ModsPath + mod.ID + "/AssetBundles");
                    }

                    if (mod.HasSounds)
                    {
                        if (!Directory.Exists(ModsPath + mod.ID + "/Sounds"))
                            Directory.CreateDirectory(ModsPath + mod.ID + "/Sounds");
                    }

                    mod.OnInit();
                    LoadedMods.Add(mod);
                    ModLogs.Log("Loaded mod " + mod.ID);
                } catch (Exception ex)
                {
                    ModLogs.Log("Failed to load mod '" + mod.ID + "'. Cause: " + ex.Message);
                }
            }
            else
            {
                ModLogs.Log("Mod " + mod.ID + " already loaded.");
            }
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ModLogs.Log("Scene " + scene.name + " (" + scene.buildIndex + ") loaded");
            if (scene.buildIndex == 2)
            {
                foreach (Mod mod in LoadedMods)
                {
                    mod.OnGame();
                }
            }
        }

        private void OnGUI()
        {
            foreach(Mod mod in LoadedMods)
            {
                mod.OnGUI();
            }
        }

        private void Update()
        {
            foreach (Mod mod in LoadedMods)
            {
                mod.Update();
            }
        }

        private void FixedUpdate()
        {
            foreach (Mod mod in LoadedMods)
            {
                mod.FixedUpdate();
            }
        }

        private static string GetGameRootPath()
        {
            var directoryInfo = Directory.GetParent(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).Parent;
            string path = directoryInfo?.ToString();

            return path;
        }
    }
}
