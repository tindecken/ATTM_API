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
        private readonly IMongoCollection<TestCase> _testcases;
        private readonly IMongoCollection<Setting> _settings;
        private readonly IATTMAppSettings _appSettings;
        public TestProjectService(IATTMAppSettings appSettings, IATTMDatabaseSettings dbSettings)
        {
            _appSettings = appSettings;
            var client = new MongoClient(dbSettings.ConnectionString);
            var database = client.GetDatabase(dbSettings.DatabaseName);
            _testauts = database.GetCollection<TestAUT>(dbSettings.TestAUTsCollectionName);
            _categories = database.GetCollection<Category>(dbSettings.CategoriesCollectionName);
            _testsuites = database.GetCollection<TestSuite>(dbSettings.TestSuitesCollectionName);
            _testgroups = database.GetCollection<TestGroup>(dbSettings.TestGroupsCollectionName);
            _testclients = database.GetCollection<TestClient>(dbSettings.TestClientsCollectionName);
            _devqueues = database.GetCollection<DevQueue>(dbSettings.DevQueuesCollectionName);
            _testcases = database.GetCollection<TestCase>(dbSettings.TestCasesCollectionName);
            _settings = database.GetCollection<Setting>(dbSettings.SettingsCollectionName);
        }

        public Task<JObject> GenerateDevCode(List<TestCase> testCases)
        {
            return TestProjectHelper.GenerateDevCode(testCases, _categories, _testsuites, _testgroups, _testauts, _settings);
        }
        public Task<JObject> GenerateRegressionCode(List<RegressionTest> regressionTests)
        {
            return TestProjectHelper.GenerateRegressionCode(regressionTests, _categories, _testsuites, _testgroups, _testcases, _testauts);
        }
        public async Task<JObject> CreateDevQueue(List<TestCase> testCases, TestClient testClient)
        {
            return await TestProjectHelper.CreateDevQueue(testCases, testClient, _devqueues, _categories, _testsuites);
        }

        public Task<JObject> BuildProject()
        {
            return TestProjectHelper.BuildProject();
            
        }
        public Task<JObject> GetLatestCode()
        {
            return TestProjectHelper.GetLatestCode();

        }
        public Task<JObject> CopyCodeToClient(TestClient client, string type)
        {
            return TestProjectHelper.CopyCodeToClient(client, type, _appSettings);

        }
        public Task<JObject> UpdateReleaseForClient(TestClient client, string newValue)
        {
            return TestProjectHelper.UpdateReleaseForClient(client, newValue);

        }
        public Task<JObject> RunAutoRunner(TestClient client)
        {
            return TestProjectHelper.RunAutoRunner(client, _appSettings);

        }
        public Task<JObject> CheckRunner(TestClient client, string process)
        {
            return TestProjectHelper.CheckRunner(client, process, _appSettings);

        }
    }
}