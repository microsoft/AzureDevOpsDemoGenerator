﻿using System;
using System.Collections.Generic;

namespace AzureDevOpsDemoBuilder.Models
{
    public class CreateZips
    {
        public class FileInfo
        {
            public string Name { get; set; }
            public string Extension { get; set; }
            public Byte[] FileBytes { get; set; }
        }

        public class FolderItem
        {
            public string Name { get; set; }
            public string Extension { get; set; }
            public Byte[] FileBytes { get; set; }

        }
        public class FolderL2
        {
            public string FolderName { get; set; }
            public List<FolderItem> FolderItems { get; set; }

        }

        public class Folder
        {
            public string FolderName { get; set; }
            public List<FolderL2> FolderL2 { get; set; }
            public List<FolderItem> FolderItems { get; set; }
        }

        public class SourceDirectoriesFiles
        {
            public List<FileInfo> Files { get; set; }
            public List<Folder> Folder { get; set; }
        }
    }
}