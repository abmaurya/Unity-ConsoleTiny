using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.Win32;
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.PackageManager;

namespace ConsoleTiny
{
    public class ScriptAssetOpener
    {
        private static bool IsNotWindowsEditor()
        {
            return UnityEngine.Application.platform != UnityEngine.RuntimePlatform.WindowsEditor;
        }

        private static string QuotePathIfNeeded(string path)
        {
            if (!path.Contains(" "))
            {
                return path;
            }
            return "\"" + path + "\"";
        }

        public static IEnumerator OpenAsset(string file, int line)
        {
            if (string.IsNullOrEmpty(file) || file == "None")
            {
                yield break;
            }
            if (file.StartsWith("Assets/"))
            {
                var ext = Path.GetExtension(file).ToLower();
                if (ext == ".lua" && TryOpenLuaFile(file, line))
                {
                    yield break;
                }

                var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(file);
                if (obj)
                {
                    AssetDatabase.OpenAsset(obj, line);
                    yield break;
                }

                yield break;
            }

            char separatorChar = '\\';
            string fileFullPath;
            if (IsNotWindowsEditor())
            {
                separatorChar = '/';
                fileFullPath = Path.GetFullPath(file);
            }
            else
            {
                fileFullPath = Path.GetFullPath(file.Replace('/', separatorChar));
            }

            var packageListRequest = Client.List(true);
            while (!packageListRequest.IsCompleted)
            {
                yield return packageListRequest;
            }
            foreach (var packageInfo in packageListRequest.Result)
            {
                if (fileFullPath.StartsWith(packageInfo.resolvedPath, StringComparison.Ordinal))
                {
                    InternalEditorUtility.OpenFileAtLineExternal(fileFullPath, line);
                    yield break;
                }
            }

            // 别人编译的DLL，不存在文件路径，那么就以工程路径拼接组装来尝试获取本地路径
            if (!File.Exists(fileFullPath))
            {
                string directoryName = Directory.GetCurrentDirectory();
                while (true)
                {
                    if (string.IsNullOrEmpty(directoryName) || !Directory.Exists(directoryName))
                    {
                        yield break;
                    }

                    int pos = fileFullPath.IndexOf(separatorChar);
                    while (pos != -1)
                    {
                        string testFullPath = Path.Combine(directoryName, fileFullPath.Substring(pos + 1));
                        if (File.Exists(testFullPath) && TryOpenVisualStudioFile(testFullPath, line))
                        {
                            yield break;
                        }

                        pos = fileFullPath.IndexOf(separatorChar, pos + 1);
                    }

                    directoryName = Path.GetDirectoryName(directoryName);
                }
            }

            TryOpenVisualStudioFile(fileFullPath, line);
        }

        private static bool TryOpenVisualStudioFile(string file, int line)
        {
            string dirPath = file;
            do
            {
                dirPath = Path.GetDirectoryName(dirPath);
                dirPath = Directory.GetParent(dirPath).FullName;

                if (!string.IsNullOrEmpty(dirPath) && Directory.Exists(dirPath))
                {
                    var files = Directory.GetFiles(dirPath, "*.sln", SearchOption.TopDirectoryOnly);
                    if (files.Length > 0)
                    {
                        OpenVisualStudioFile(files[0], file, line);
                        return true;
                    }
                }
                else
                {
                    break;
                }
            } while (true);
            return false;
        }

        private static void OpenVisualStudioFile(string projectPath, string file, int line)
        {
            string vsPath = ScriptEditorUtility.GetExternalScriptEditor();

            if (IsNotWindowsEditor())
            {
                Process.Start("open", "-a " + QuotePathIfNeeded(vsPath) + " " + QuotePathIfNeeded(file));
                return;
            }

            if (string.IsNullOrEmpty(vsPath) || !File.Exists(vsPath))
            {
                return;
            }
            string exePath = Path.GetFullPath("Packages/com.wuhuan.consoletiny/Editor/VisualStudioFileOpenTool.exe");


            if (string.IsNullOrEmpty(exePath))
            {
                exePath = "Assets/Editor/VisualStudioFileOpenTool.exe";
            }

            if (!string.IsNullOrEmpty(exePath))
            {
                if (!File.Exists(exePath))
                {
                    return;
                }

                ThreadPool.QueueUserWorkItem(_ =>
                {
                    OpenVisualStudioFileInter(exePath, vsPath, projectPath, file, line);
                });
            }
        }

        private static void OpenVisualStudioFileInter(string exePath, string vsPath, string projectPath, string file, int line)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = String.Format("{0} {1} {2} {3}",
                    QuotePathIfNeeded(vsPath), QuotePathIfNeeded(projectPath), QuotePathIfNeeded(file), line),
                UseShellExecute = false,
                CreateNoWindow = true
            });
        }

        private static bool TryOpenLuaFile(string file, int line)
        {
            if (IsNotWindowsEditor())
            {
                return false;
            }
            string luaPath = LuaExecutablePath();
            if (string.IsNullOrEmpty(luaPath) || !File.Exists(luaPath))
            {
                return false;
            }

            ThreadPool.QueueUserWorkItem(_ =>
            {
                OpenLuaFileInter(luaPath, file, line);
            });
            return true;
        }

        private static void OpenLuaFileInter(string exePath, string file, int line)
        {
            string arg = string.Format("{0}:{1}", QuotePathIfNeeded(file), line);
            if (exePath.EndsWith("idea.exe", StringComparison.Ordinal) ||
                exePath.EndsWith("idea64.exe", StringComparison.Ordinal))
            {
                arg = String.Format("--line {1} {0}", QuotePathIfNeeded(file), line);
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = arg,
                UseShellExecute = false,
                CreateNoWindow = true
            });
        }

        private static string LuaExecutablePath()
        {
            using (RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\FileExts\\.lua\\UserChoice"))
            {
                if (registryKey != null)
                {
                    string val = registryKey.GetValue("Progid") as string;
                    if (!string.IsNullOrEmpty(val))
                    {
                        val = "Software\\Classes\\" + val + "\\shell\\open\\command";
                        using (RegistryKey registryKey2 = Registry.CurrentUser.OpenSubKey(val))
                        {
                            string val3 = LuaExecutablePathInter(registryKey2);
                            if (!string.IsNullOrEmpty(val3))
                            {
                                return val3;
                            }
                        }
                    }
                }
            }

            using (RegistryKey registryKey = Registry.ClassesRoot.OpenSubKey(".lua"))
            {
                if (registryKey != null)
                {
                    string val = registryKey.GetValue(null) as string;
                    if (val != null)
                    {
                        val += "\\shell\\open\\command";
                        using (RegistryKey registryKey2 = Registry.ClassesRoot.OpenSubKey(val))
                        {
                            string val3 = LuaExecutablePathInter(registryKey2);
                            if (!string.IsNullOrEmpty(val3))
                            {
                                return val3;
                            }
                        }
                    }
                }
            }
            return String.Empty;
        }

        private static string LuaExecutablePathInter(RegistryKey registryKey2)
        {
            if (registryKey2 != null)
            {
                string val2 = registryKey2.GetValue(null) as string;
                if (!string.IsNullOrEmpty(val2))
                {
                    string val3 = val2;
                    int pos = val2.IndexOf(" \"", StringComparison.Ordinal);
                    if (pos != -1)
                    {
                        val3 = val2.Substring(0, pos);
                    }

                    val3 = val3.Trim('"');
                    return val3;
                }
            }

            return String.Empty;
        }
    }
}