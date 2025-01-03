﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static EquinoxsModUtils.EMULogging;

namespace EquinoxsModUtils
{
    public static partial class EMU 
    {
        /// <summary>
        /// Contains several functions for loading images from file or from Resources
        /// </summary>
        public static class Images 
        {
            /// <summary>
            /// Gets a Texture2D of the Resource passed in the arguments for using in GUI.
            /// </summary>
            /// <param name="name">The name of the Resource.</param>
            /// <param name="shouldLog">Passed to GetResourceInfoByName()</param>
            /// <returns>A Texture2D of the Resource's Sprite for use in GUI.</returns>
            public static Texture2D GetImageForResource(string name, bool shouldLog = false) {
                ResourceInfo info = Resources.GetResourceInfoByName(name, shouldLog);
                if (info == null) return null;
                if (info.sprite == null) {
                    LogEMUWarning($"Resource '{info.displayName}' has no sprite. Returning null.");
                    return null;
                }

                Sprite sprite = info.sprite;
                if (sprite.rect.width != sprite.texture.width) {
                    Texture2D newText = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
                    Color[] newColors = sprite.texture.GetPixels((int)sprite.textureRect.x,
                                                                 (int)sprite.textureRect.y,
                                                                 (int)sprite.textureRect.width,
                                                                 (int)sprite.textureRect.height);
                    newText.SetPixels(newColors);
                    newText.Apply();
                    return newText;
                }
                else {
                    return sprite.texture;
                }
            }

            /// <summary>
            /// Creates a Texture2D from the Embedded Resource at the path provided in the argument.
            /// </summary>
            /// <param name="path">The path of the Embedded Resource image.</param>
            /// <param name="shouldLog">Whether an EMU Info message should be logged on successful load.</param>
            /// <param name="assembly">Ignore this, internal use.</param>
            /// <returns>Texture2D if file is found, null otherwise</returns>
            public static Texture2D LoadTexture2DFromFile(string path, bool shouldLog = false, Assembly assembly = null) {
                if (assembly == null) assembly = Assembly.GetCallingAssembly();

                string[] resourceNames = assembly.GetManifestResourceNames();
                string fullPath = Array.Find(resourceNames, name => name.EndsWith(path));

                if (fullPath == null) {
                    LogEMUError($"Could not find image resource '{path}' in mod assembly.");
                    LogEMUInfo($"Available resources are: {string.Join(", ", resourceNames)}");
                    return null;
                }

                using (Stream stream = assembly.GetManifestResourceStream(fullPath)) {
                    if (stream == null) {
                        LogEMUError($"Could not load image resource '{path}' from mod assembly stream.");
                        return null;
                    }

                    using (MemoryStream memoryStream = new MemoryStream()) {
                        stream.CopyTo(memoryStream);
                        byte[] fileData = memoryStream.ToArray();

                        Texture2D output = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                        output.LoadImage(fileData);

                        if (shouldLog) LogEMUInfo($"Created Texture2D from image resource '{path}'");

                        return output;
                    }
                }
            }

            /// <summary>
            /// Calls LoadTexture2DFromFile() and converts the result to a Sprite.
            /// </summary>
            /// <param name="path">The path of the Embedded Resource image.</param>
            /// <param name="shouldLog">Passed to LoadTexture2DFromFile()</param>
            /// <returns></returns>
            public static Sprite LoadSpriteFromFile(string path, bool shouldLog = false) {
                Texture2D texture = LoadTexture2DFromFile(path, shouldLog, Assembly.GetCallingAssembly());
                return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0), 512);
            }
        }
    }
}
