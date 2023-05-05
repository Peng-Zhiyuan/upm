        
工具使用示例：
        var currentDir = System.Environment.CurrentDirectory;
        var protogenPath = $"{currentDir}/Packages/com.pzy.third.protobufnet/protogen~/protogen.exe";
        var protoRootDir = $"{currentDir}/proto";
        var genCsharpDir = $"{currentDir}/Assets/ProtoGen";
        ExecUtil.Run(protogenPath, $"--csharp_out={genCsharpDir} proto_message.proto", false, protoRootDir);
        ExecUtil.Run(protogenPath, $"--csharp_out={genCsharpDir} proto_const.proto", false, protoRootDir);

        CSharpCodeUtil.AddNamespaceToFile($"{currentDir}/Assets/ProtoGen/proto_message.cs", "ProtoBuf");
        CSharpCodeUtil.AddNamespaceToFile($"{currentDir}/Assets/ProtoGen/proto_const.cs", "ProtoBuf");