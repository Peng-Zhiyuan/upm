﻿using dnlib.DotNet;
using HybridCLR.Editor.Meta;
using HybridCLR.Editor.Template;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace HybridCLR.Editor.MethodBridge
{
    public class Generator
    {
        public class Options
        {
            public PlatformABI CallConvention { get; set; }

            public string TemplateCode { get; set; }

            public string OutputFile { get; set; }

            public IReadOnlyList<MethodDef> NotGenericMethods { get; set; }

            public IReadOnlyCollection<GenericMethod> GenericMethods { get; set; }
        }

        private readonly IReadOnlyList<MethodDef> _notGenericMethods;

        private readonly IReadOnlyCollection<GenericMethod> _genericMethods;

        private readonly string _templateCode;

        private readonly string _outputFile;

        private readonly PlatformAdaptorBase _platformAdaptor;

        private readonly HashSet<MethodBridgeSig> _managed2nativeMethodSet = new HashSet<MethodBridgeSig>();

        private List<MethodBridgeSig> _managed2nativeMethodList;

        private readonly HashSet<MethodBridgeSig> _native2managedMethodSet = new HashSet<MethodBridgeSig>();

        private List<MethodBridgeSig> _native2managedMethodList;

        private readonly HashSet<MethodBridgeSig> _adjustThunkMethodSet = new HashSet<MethodBridgeSig>();

        private List<MethodBridgeSig> _adjustThunkMethodList;

        public Generator(Options options)
        {
            _notGenericMethods = options.NotGenericMethods;
            _genericMethods = options.GenericMethods;
            _templateCode = options.TemplateCode;
            _outputFile = options.OutputFile;
            _platformAdaptor = CreatePlatformAdaptor(options.CallConvention);
        }

        private static PlatformAdaptorBase CreatePlatformAdaptor(PlatformABI type)
        {
            switch (type)
            {
                case PlatformABI.Universal32: return new PlatformAdaptor_Universal32();
                case PlatformABI.Universal64: return new PlatformAdaptor_Universal64();
                case PlatformABI.Arm64: return new PlatformAdaptor_Arm64();
                default: throw new NotSupportedException();
            }
        }

        private MethodBridgeSig CreateMethodBridgeSig(MethodDef methodDef, bool forceRemoveThis, TypeSig returnType, List<TypeSig> parameters)
        {
            var paramInfos = new List<ParamInfo>();
            if (forceRemoveThis && !methodDef.IsStatic)
            {
                parameters.RemoveAt(0);
            }
            foreach (var paramInfo in parameters)
            {
                var a = _platformAdaptor.CreateTypeInfo(paramInfo);
                if(a == null)
                {
                    return null;
                }
                paramInfos.Add(new ParamInfo()
                {
                    Type = a
                }) ;
            }
            var mbs = new MethodBridgeSig()
            {
                MethodDef = methodDef,
                ReturnInfo = new ReturnInfo() { Type = returnType != null ? _platformAdaptor.CreateTypeInfo(returnType) : TypeInfo.s_void },
                ParamInfos = paramInfos,
            };
            try
            {
                _platformAdaptor.OptimizeMethod(mbs);
            }
            catch
            {
                return null;
            }
            return mbs;
        }

        private void AddManaged2NativeMethod(MethodBridgeSig method)
        {
            if (_managed2nativeMethodSet.Add(method))
            {
                method.Init();
            }
        }

        private void AddNative2ManagedMethod(MethodBridgeSig method)
        {
            if (_native2managedMethodSet.Add(method))
            {
                method.Init();
            }
        }

        private void AddAdjustThunkMethod(MethodBridgeSig method)
        {
            if (_adjustThunkMethodSet.Add(method))
            {
                method.Init();
            }
        }

        private void ProcessMethod(MethodDef method, List<TypeSig> klassInst, List<TypeSig> methodInst)
        {
            try
            {
                if (method.IsPrivate || (method.IsAssembly && !method.IsPublic && !method.IsFamily))
                {
                    return;
                }

                TypeSig returnType;
                List<TypeSig> parameters;
                if (klassInst == null && methodInst == null)
                {
                    returnType = method.ReturnType;
                    parameters = method.Parameters.Select(p => p.Type).ToList();
                }
                else
                {
                    var gc = new GenericArgumentContext(klassInst, methodInst);
                    returnType = MetaUtil.Inflate(method.ReturnType, gc);
                    parameters = method.Parameters.Select(p => MetaUtil.Inflate(p.Type, gc)).ToList();
                }


                var m2nMethod = CreateMethodBridgeSig(method, false, returnType, parameters);
                if (m2nMethod == null)
                {
                    return;
                }
                AddManaged2NativeMethod(m2nMethod);

                if (method.IsVirtual)
                {
                    if (method.DeclaringType.IsInterface)
                    {
                        AddAdjustThunkMethod(m2nMethod);
                    }
                    //var adjustThunkMethod = CreateMethodBridgeSig(method, true, returnType, parameters);
                    AddNative2ManagedMethod(m2nMethod);
                }
                if (method.Name == "Invoke" && method.DeclaringType.IsDelegate)
                {
                    var openMethod = CreateMethodBridgeSig(method, true, returnType, parameters);
                    if (openMethod != null)
                    {
                        AddNative2ManagedMethod(openMethod);
                    }

                }
            }
          catch
            {
                return;
            }
        }

        public void PrepareMethods()
        {
            foreach(var method in _notGenericMethods)
            {
                ProcessMethod(method, null, null);
            }

            foreach(var method in _genericMethods)
            {
                ProcessMethod(method.Method, method.KlassInst, method.MethodInst);
            }
            
            {
                var sortedMethods = new SortedDictionary<string, MethodBridgeSig>();
                foreach (var method in _managed2nativeMethodSet)
                {
                    sortedMethods.Add(method.CreateCallSigName(), method);
                }
                _managed2nativeMethodList = sortedMethods.Values.ToList();
            }
            {
                var sortedMethods = new SortedDictionary<string, MethodBridgeSig>();
                foreach (var method in _native2managedMethodSet)
                {
                    sortedMethods.Add(method.CreateCallSigName(), method);
                }
                _native2managedMethodList = sortedMethods.Values.ToList();
            }
            {
                var sortedMethods = new SortedDictionary<string, MethodBridgeSig>();
                foreach (var method in _adjustThunkMethodSet)
                {
                    sortedMethods.Add(method.CreateCallSigName(), method);
                }
                _adjustThunkMethodList = sortedMethods.Values.ToList();
            }
        }

        public void Generate()
        {
            var frr = new FileRegionReplace(_templateCode);

            List<string> lines = new List<string>(20_0000);

            Debug.LogFormat("== managed2native:{0} native2managed:{1} adjustThunk:{2}",
                _managed2nativeMethodList.Count, _native2managedMethodList.Count, _adjustThunkMethodList.Count);

            foreach(var method in _managed2nativeMethodList)
            {
                _platformAdaptor.GenerateManaged2NativeMethod(method, lines);
            }

            _platformAdaptor.GenerateManaged2NativeStub(_managed2nativeMethodList, lines);

            foreach (var method in _native2managedMethodList)
            {
                _platformAdaptor.GenerateNative2ManagedMethod(method, lines);
            }

            _platformAdaptor.GenerateNative2ManagedStub(_native2managedMethodList, lines);

            foreach (var method in _adjustThunkMethodList)
            {
                _platformAdaptor.GenerateAdjustThunkMethod(method, lines);
            }

            _platformAdaptor.GenerateAdjustThunkStub(_adjustThunkMethodList, lines);

            frr.Replace("INVOKE_STUB", string.Join("\n", lines));

            Directory.CreateDirectory(Path.GetDirectoryName(_outputFile));

            frr.Commit(_outputFile);
        }

    }
}
