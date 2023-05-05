using dnlib.DotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HybridCLR.Editor.Meta
{
    public class AssemblyReferenceDeepCollector : IDisposable
    {
        private readonly IAssemblyResolver _assemblyPathResolver;
        private readonly List<string> _rootAssemblies;

        private readonly ModuleContext _modCtx;
        private readonly AssemblyResolver _asmResolver;
        private bool disposedValue;

        public Dictionary<string, ModuleDefMD> LoadedModules { get; } = new Dictionary<string, ModuleDefMD>();

        public IReadOnlyList<string> GetRootAssemblyNames()
        {
            return _rootAssemblies;
        }

        public List<ModuleDefMD> GetLoadedModulesExcludeRootAssemblies()
        {
            return LoadedModules.Where(e => !_rootAssemblies.Contains(e.Key)).Select(e => e.Value).ToList();
        }

        public List<ModuleDefMD> GetLoadedModulesOfRootAssemblies()
        {
            return _rootAssemblies.Select(ass => LoadedModules[ass]).ToList();
        }

        public AssemblyReferenceDeepCollector(IAssemblyResolver assemblyResolver, List<string> rootAssemblies)
        {
            _assemblyPathResolver = assemblyResolver;
            _rootAssemblies = rootAssemblies;
            _modCtx = ModuleDef.CreateModuleContext();
            _asmResolver = (AssemblyResolver)_modCtx.AssemblyResolver;
            _asmResolver.EnableTypeDefCache = true;
            _asmResolver.UseGAC = false;
            LoadAllAssembiles();
        }

        private void LoadAllAssembiles()
        {
            foreach (var asm in _rootAssemblies)
            {
                LoadModule(asm);
            }
        }

        // pzy:
        // 修改
        static Dictionary<string, bool> nameToUnityLoadedDic;
        static bool IsUnityLoaded(string name)
        {
            if (nameToUnityLoadedDic == null)
            {
                nameToUnityLoadedDic = new Dictionary<string, bool>();
                var assList = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var ass in assList)
                {
                    var theName = ass.GetName().Name;
                    nameToUnityLoadedDic[theName] = true;
                }
            }

            if (nameToUnityLoadedDic.ContainsKey(name))
            {
                return true;
            }
            else
            {
                Debug.Log("!! skip : " + name);
                return false;
            }
        }

        private ModuleDefMD LoadModule(string moduleName)
        {

            // Debug.Log($"load module:{moduleName}");
            if (LoadedModules.TryGetValue(moduleName, out var mod))
            {
                return mod;
            }
            mod = DoLoadModule(_assemblyPathResolver.ResolveAssembly(moduleName, true));
            LoadedModules.Add(moduleName, mod);

            foreach (var refAsm in mod.GetAssemblyRefs())
            {
                //// pzy:
                //// 引用但是 unity 没加载的程序集跳过，因为会引发异常
                var b = IsUnityLoaded(refAsm.Name);
                if (!b)
                {
                    continue;
                }


                LoadModule(refAsm.Name);
            }
            return mod;
        }

        private ModuleDefMD DoLoadModule(string dllPath)
        {
            //Debug.Log($"do load module:{dllPath}");
            ModuleDefMD mod = ModuleDefMD.Load(dllPath, _modCtx);
            _asmResolver.AddToCache(mod);
            return mod;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    foreach(var mod in LoadedModules.Values)
                    {
                        mod.Dispose();
                    }
                }
                LoadedModules.Clear();
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
