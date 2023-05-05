using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using NativeBuilder;
namespace NativeBuilder
{

	public static class PShellUtil
	{
	
		public enum FileExsitsOption{
			Override,
			NotCopy
		}
		
		public enum DirectoryExsitsOption{
			Override,
			Merge,
			NotCopy
		}
		
		// copy one dir to another dir
		public static void CopyInto(DirectoryInfo source, DirectoryInfo target, FileExsitsOption fileOption = PShellUtil.FileExsitsOption.Override, DirectoryExsitsOption directoryOption = PShellUtil.DirectoryExsitsOption.Merge, string[] exclude = null){
			var t = target.CreateSubdirectory(source.Name);
			CopyAll(source, t, fileOption, directoryOption, exclude);
		}
		
		public static void CopyTo(DirectoryInfo source, DirectoryInfo target, FileExsitsOption fileOption = PShellUtil.FileExsitsOption.Override, DirectoryExsitsOption directoryOption = PShellUtil.DirectoryExsitsOption.Merge, string[] exclude = null){
			
			CopyAll(source, target, fileOption, directoryOption, exclude);
		}
		
		public static void CopyInto(string source, string target, FileExsitsOption fileOption = PShellUtil.FileExsitsOption.Override, DirectoryExsitsOption directoryOption = PShellUtil.DirectoryExsitsOption.Merge, string[] exclude = null)
		{
			DirectoryInfo s = new DirectoryInfo(source);
			DirectoryInfo t = new DirectoryInfo(target);
			CopyInto(s, t, fileOption, directoryOption, exclude); 
		}
		
		public static void CopyTo(string source, string target, FileExsitsOption fileOption = PShellUtil.FileExsitsOption.Override, DirectoryExsitsOption directoryOption = PShellUtil.DirectoryExsitsOption.Merge, string[] exclude = null)
		{
			DirectoryInfo s = new DirectoryInfo(source);
			DirectoryInfo t = new DirectoryInfo(target);
			CopyTo(s, t, fileOption, directoryOption, exclude);
		}
		
		// copy whatever in a dir to anothr dir
		public static void CopyAll(DirectoryInfo source, DirectoryInfo target, FileExsitsOption fileOption = PShellUtil.FileExsitsOption.Override, DirectoryExsitsOption directoryOption = PShellUtil.DirectoryExsitsOption.Merge, string[] exclude = null)
		{
			if (source.FullName.ToLower() == target.FullName.ToLower())
			{
				return;
			}
			
			// Check if the source directory exists, if not, return
			if (Directory.Exists(source.FullName) == false)
			{
				return;
			}
			
			// Check if the target directory exists, if not, create it.
			if (Directory.Exists(target.FullName) == false)
			{
				Directory.CreateDirectory(target.FullName);
			}
			
			// Copy each file into it's new directory.
			foreach (FileInfo fi in source.GetFiles())
			{
				//if(exclude != null && Array.IndexOf(exclude, fi.Name) != -1) continue;
                if (exclude != null)
                {
                    bool find = false;
                    foreach (var e in exclude)
                    {
                        if(fi.Name.EndsWith(e))
                        {
                            find = true;
                            break;
                        }
                    }
                    if (find)
                    {
                        continue;
                    }
                }
				//Debug.Log(@"Copying " + target.FullName + "\\" + fi.Name);
				fi.CopyTo(Path.Combine(target.ToString(), fi.Name), fileOption == FileExsitsOption.Override ? true : false);
			}
			
			// Copy each subdirectory using recursion.
			foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
			{
				//if(exclude != null && Array.IndexOf(exclude, diSourceSubDir.Name) != -1) continue;
                if (exclude != null)
                {
                    bool find = false;
                    foreach (var e in exclude)
                    {
                        if (diSourceSubDir.Name.EndsWith(e))
                        {
                            find = true;
                            break;
                        }
                    }
                    if (find)
                    {
                        continue;
                    }
                }
				
				bool exsits = Directory.Exists(target.FullName + "/" + diSourceSubDir.Name);
				bool hasEndFix = diSourceSubDir.Name.Contains(".");
				if(!hasEndFix){
					// treat as folder
					if(directoryOption == DirectoryExsitsOption.Merge){
						DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
                        CopyAll(diSourceSubDir, nextTargetSubDir, fileOption, directoryOption, exclude);
					}
					else if(directoryOption == DirectoryExsitsOption.Override){
						DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
						nextTargetSubDir.Delete(true);
                        CopyAll(diSourceSubDir, nextTargetSubDir, fileOption, directoryOption, exclude);
						
					}else if(directoryOption == DirectoryExsitsOption.NotCopy){
						if(exsits) return;
						DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
                        CopyAll(diSourceSubDir, nextTargetSubDir, fileOption, directoryOption, exclude);
					}
				}else{
					// treat as file
					if(fileOption == FileExsitsOption.Override){
						DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
						nextTargetSubDir.Delete(true);
                        CopyAll(diSourceSubDir, nextTargetSubDir, fileOption, directoryOption, exclude);
					}else if(fileOption == FileExsitsOption.NotCopy){
						if(exsits) return;
						DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
                        CopyAll(diSourceSubDir, nextTargetSubDir, fileOption, directoryOption, exclude);
						
					}
				}
				
			}
		}
	}
}