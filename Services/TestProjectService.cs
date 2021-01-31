using ATTM_API.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using ATTM_API.Helpers;

namespace ATTM_API.Services
{
    public class TestProjectService
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(Program));


        public async Task<JObject> generateCodeAsync(List<TestCase> testCases, string runType, bool isDebug = false)
        {
            CSharpTestProjectHelper.GenerateCode(testCases, runType);
            JObject result = new JObject();
            // Delete Compile node in TestProjectCharp.csproj
            
            return result;
        }

    }
}