using ATTM_API.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using ATTM_API.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace ATTM_API.Services
{
    public class TestProjectService
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(Program));
        private static IMongoCollection<TestAUT> _testauts;
        private readonly IMongoCollection<Category> _categories;
        private readonly IMongoCollection<TestSuite> _testsuites;
        private readonly IMongoCollection<TestGroup> _testgroups;
        private readonly IMongoCollection<DevQueue> _devqueues;
        private readonly IMongoCollection<TestClient> _testclients;
        public TestProjectService(IATTMDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _testauts = database.GetCollection<TestAUT>(settings.TestAUTsCollectionName);
            _categories = database.GetCollection<Category>(settings.CategoriesCollectionName);
            _testsuites = database.GetCollection<TestSuite>(settings.TestSuitesCollectionName);
            _testgroups = database.GetCollection<TestGroup>(settings.TestGroupsCollectionName);
            _devqueues = database.GetCollection<DevQueue>(settings.DevQueuesCollectionName);
            _testclients = database.GetCollection<TestClient>(settings.TestClientsCollectionName);
        }

        public Task<JObject> GenerateCode(List<TestCase> testCases, string runType, bool isDebug = false)
        {
            return TestProjectHelper.GenerateCode(testCases, runType, _categories, _testsuites, _testgroups, _testauts);
        }
        public Task<JArray> CreateDevQueue(List<TestCase> testCases, TestClient testClient)
        {
            return TestProjectHelper.CreateDevQueue(testCases, testClient, _devqueues, _categories, _testsuites);
        }

        public Task<int> BuildProject()
        {
            return TestProjectHelper.BuildProject();
            
        }

    }
}