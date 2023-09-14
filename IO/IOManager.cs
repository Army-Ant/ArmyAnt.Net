﻿using System.Collections.Generic;
using System.Text;

namespace ArmyAnt.IO {
    public partial class IOManager {
        public string Root { get; set; }

        public string FileDirRoot {
            get {
                return Root + System.IO.Path.AltDirectorySeparatorChar;
            }
        }

        public bool ExistFile(params string[] path) {
            return System.IO.File.Exists(ParsePath(path));
        }

        public bool ExistDirectory(params string[] path) {
            return System.IO.Directory.Exists(ParsePath(path));
        }

        public bool MkdirIfNotExist(params string[] path) {
            var dir = ParsePath(path);
            try {
                if(System.IO.Directory.Exists(dir)) {
                    return true;
                }
                System.IO.Directory.CreateDirectory(dir);
            } catch(System.IO.IOException) {
                return false;
            }
            return true;
        }

        public bool RemoveFolder(params string[] path) {
            try {
                System.IO.Directory.Delete(ParsePath(path), true);
            } catch(System.IO.IOException) {
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
            } catch(System.IO.DirectoryNotFoundException) {
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

        public static byte[] LoadFromFileWholePath(string path) {
            try {
                var ret = System.IO.File.ReadAllBytes(path);
                if(ret == null || ret.Length <= 0) {
                    return null;
                }
                return ret;
            } catch(System.SystemException) {
                return null;
            }
        }

        private string ParsePath(string[] path) {
            StringBuilder filename = new StringBuilder(Root);
            for(var i = 0; i < path.Length; ++i) {
                filename.Append(System.IO.Path.AltDirectorySeparatorChar).Append(path[i]);
            }
            return System.IO.Path.GetFullPath(filename.ToString());
        }
    }

}
