using System.Text;

namespace ArmyAnt.IO {
    /// <summary>
    /// 用于处理文件、目录相关的一些处理包装类
    /// </summary>
    public partial class IOManager {
        public string Root { get; set; }

        public string FileDirRoot {
            get {
                return Root + System.IO.Path.AltDirectorySeparatorChar;
            }
        }

        public static bool ExistFile(string path) {
            return System.IO.File.Exists(path);
        }

        public bool ExistFile(params string[] path) {
            return ExistFile(ParsePath(path));
        }

        public static bool ExistDirectory(string path) {
            return System.IO.Directory.Exists(path);
        }

        public bool ExistDirectory(params string[] path) {
            return ExistDirectory(ParsePath(path));
        }

        public bool MkdirIfNotExist(params string[] path) {
            return MkdirIfNotExistWholePath(ParsePath(path));
        }

        public static bool MkdirIfNotExistWholePath(string path) {
            try {
                if (System.IO.Directory.Exists(path)) {
                    return true;
                }
                System.IO.Directory.CreateDirectory(path);
            } catch (System.IO.IOException) {
                return false;
            }
            return true;
        }

        public bool RemoveFolder(params string[] path) {
            try {
                System.IO.Directory.Delete(ParsePath(path), true);
            } catch (System.IO.IOException) {
                return false;
            }
            return true;
        }

        public string[] ListAllFiles(params string[] path) {
            return System.IO.Directory.GetFiles(ParsePath(path));
        }

        public string[] ListAllDirectories(params string[] path) {
            try {
                return System.IO.Directory.GetDirectories(ParsePath(path));
            } catch (System.IO.DirectoryNotFoundException) {
                return null;
            }
        }

        public string SaveToFile(byte[] content, params string[] path) {
            return SaveToFileWholePath(content, ParsePath(path));
        }

        public static string SaveToFileWholePath(byte[] content, string path) {
            System.IO.FileStream file = System.IO.File.Create(path, content.Length, System.IO.FileOptions.Asynchronous);
            file.Close();
            System.IO.File.WriteAllBytes(path, content);
            return path;
        }

        public byte[] LoadFromFile(params string[] path) {
            return LoadFromFileWholePath(ParsePath(path));
        }

        public string ReadTextFile(params string[] path) {
            return ReadTextFileWholePath(ParsePath(path));
        }

        public static byte[] LoadFromFileWholePath(string path) {
            try {
                var ret = System.IO.File.ReadAllBytes(path);
                if (ret == null || ret.Length <= 0) {
                    return null;
                }
                return ret;
            } catch (System.SystemException) {
                return null;
            }
        }

        public static string ReadTextFileWholePath(string path) {
            try {
                var ret = System.IO.File.ReadAllText(path);
                if (string.IsNullOrEmpty(ret)) {
                    return null;
                }
                return ret;
            } catch (System.SystemException) {
                return null;
            }
        }

        private string ParsePath(string[] path) {
            StringBuilder filename = new StringBuilder(Root);
            for (var i = 0; i < path.Length; ++i) {
                filename.Append(System.IO.Path.AltDirectorySeparatorChar).Append(path[i]);
            }
            return System.IO.Path.GetFullPath(filename.ToString());
        }
    }

}
