using System.IO;
using GbbExpender.Models;
using GbbExpender.Services.Generators;
using GbbExpender.Services.Registration;
using ConfigurationManager = System.Configuration.ConfigurationManager;

namespace GbbExpender.Services
{
    public class GbbGeneratorService
    {
        private readonly GbbPaths _paths;
        private readonly CSharpGenerator _csGen = new();
        private readonly CppHeaderGenerator _cppHGen = new();
        private readonly CppMonitorGenerator _cppMGen = new();
        private readonly FileService _file = new();
        private readonly EnumUpdateService _enum = new();
        private readonly CppRegistrationService _cppReg = new();
        private readonly CSharpRegistrationService _csReg = new();

        public GbbGeneratorService()
        {
            _paths = new GbbPaths {
                WorkspaceRoot = ConfigurationManager.AppSettings["WorkspaceRoot"] ?? "",
                CppDescriptorsPath = ConfigurationManager.AppSettings["CppDescriptorsPath"] ?? "",
                CppMessagesPath = ConfigurationManager.AppSettings["CppMessagesPath"] ?? "",
                CppIncPath = ConfigurationManager.AppSettings["CppIncPath"] ?? "",
                CppDllPath = ConfigurationManager.AppSettings["CppDllPath"] ?? "",
                CsDescriptorsPath = ConfigurationManager.AppSettings["CsDescriptorsPath"] ?? "",
                CsMessagesPath = ConfigurationManager.AppSettings["CsMessagesPath"] ?? "",
                CsEnumsPath = ConfigurationManager.AppSettings["CsEnumsPath"] ?? "",
                CsAgentPath = ConfigurationManager.AppSettings["CsAgentPath"] ?? ""
            };
        }

        public void Generate(GeneratorRequest req)
        {
            var isMsg = string.Equals(req.EntryType, "Message", System.StringComparison.OrdinalIgnoreCase);
            var cppH = _cppHGen.Generate(req);
            var cppM = _cppMGen.Generate(req);
            var csS = _csGen.Generate(req);

            var root = _paths.WorkspaceRoot;
            var cppSub = isMsg ? _paths.CppMessagesPath : _paths.CppDescriptorsPath;
            var csSub = isMsg ? _paths.CsMessagesPath : _paths.CsDescriptorsPath;

            _file.SaveFile(Path.Combine(root, cppSub, $"{req.ObjectName}.h"), cppH);
            _file.SaveFile(Path.Combine(root, csSub, $"{req.ObjectName}.cs"), csS);

            _cppReg.UpdateMonitor(Path.Combine(root, cppSub, isMsg ? "ExtendedMessagesMonitor.h" : "ExtendedMonitor.h"), req.ObjectName, cppM, isMsg);
            _enum.Update(Path.Combine(root, _paths.CppIncPath, "Extended_HT_GBB.h"), req.ObjectName, isMsg ? "ExtendedGBBmessageName" : "ExtendedGBBdescriptorName", isMsg ? "INTTCMaxMessage" : "INTTCMaxDescriptor");
            _cppReg.UpdateCppRegistration(Path.Combine(root, _paths.CppDllPath, "ExtendedSizeDLL.cpp"), req.ObjectName, isMsg);
            
            _enum.Update(Path.Combine(root, _paths.CsEnumsPath), req.ObjectName, isMsg ? "ExtendedGBBmessageName" : "ExtendedGBBdescriptorName", isMsg ? "INTTCMaxMessage" : "INTTCMaxDescriptor");
            _csReg.UpdateAgent(Path.Combine(root, _paths.CsAgentPath), req.ObjectName, isMsg);
        }
    }
}