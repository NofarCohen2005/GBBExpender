using System.IO;
using GbbExpender.Models;
using GbbExpender.Services.Generators;
using GbbExpender.Services.Registration;
using ConfigurationManager = System.Configuration.ConfigurationManager;

namespace GbbExpender.Services
{
    public class GbbGeneratorService
    {
        private readonly CSharpGenerator _csGen = new();
        private readonly CppHeaderGenerator _cppHGen = new();
        private readonly CppMonitorGenerator _cppMGen = new();
        private readonly FileService _file = new();
        private readonly EnumUpdateService _enum = new();
        private readonly CppRegistrationService _cppReg = new();
        private readonly CSharpRegistrationService _csReg = new();

        public GbbGeneratorService()
        {
        }

        private GbbPaths GetPaths()
        {
            var settings = Utils.ConfigHelper.GetAppSettings();
            return new GbbPaths
            {
                WorkspaceRoot = settings.GetValueOrDefault("WorkspaceRoot", ""),
                CppDescriptorsPath = settings.GetValueOrDefault("CppDescriptorsPath", ""),
                CppMessagesPath = settings.GetValueOrDefault("CppMessagesPath", ""),
                CppIncPath = settings.GetValueOrDefault("CppIncPath", ""),
                CppDllPath = settings.GetValueOrDefault("CppDllPath", ""),
                CsDescriptorsPath = settings.GetValueOrDefault("CsDescriptorsPath", ""),
                CsMessagesPath = settings.GetValueOrDefault("CsMessagesPath", ""),
                CsEnumsPath = settings.GetValueOrDefault("CsEnumsPath", ""),
                CsAgentPath = settings.GetValueOrDefault("CsAgentPath", "")
            };
        }

        public void Generate(GeneratorRequest req)
        {
            var paths = GetPaths();
            var isMsg = string.Equals(req.EntryType, "Message", System.StringComparison.OrdinalIgnoreCase);
            var cppH = _cppHGen.Generate(req);
            var cppM = _cppMGen.Generate(req);
            var csS = _csGen.Generate(req);

            var root = paths.WorkspaceRoot;
            var cppSub = isMsg ? paths.CppMessagesPath : paths.CppDescriptorsPath;
            var csSub = isMsg ? paths.CsMessagesPath : paths.CsDescriptorsPath;

            _file.SaveFile(Path.Combine(root, cppSub, $"{req.ObjectName}.h"), cppH);
            _file.SaveFile(Path.Combine(root, csSub, $"{req.ObjectName}.cs"), csS);

            _cppReg.UpdateMonitor(Path.Combine(root, cppSub, isMsg ? "ExtendedMessagesMonitor.h" : "ExtendedMonitor.h"), req.ObjectName, cppM, isMsg);
            _enum.Update(Path.Combine(root, paths.CppIncPath, "Extended_HT_GBB.h"), req.ObjectName, isMsg ? "ExtendedGBBmessageName" : "ExtendedGBBdescriptorName", isMsg ? "INTTCMaxMessage" : "INTTCMaxDescriptor");
            _cppReg.UpdateCppRegistration(Path.Combine(root, paths.CppDllPath, "ExtendedSizeDLL.cpp"), req.ObjectName, isMsg);
            
            _enum.Update(Path.Combine(root, paths.CsEnumsPath), req.ObjectName, isMsg ? "ExtendedGBBmessageName" : "ExtendedGBBdescriptorName", isMsg ? "INTTCMaxMessage" : "INTTCMaxDescriptor");
            _csReg.UpdateAgent(Path.Combine(root, paths.CsAgentPath), req.ObjectName, isMsg);
        }
    }
}