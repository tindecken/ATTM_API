using ATTM_API.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ATTM_API.Services
{
    public class TestProjectExplorerService
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(Program));

        public TestProjectExplorerService()
        {
        }

        public async Task<JObject> GetAllTestsAsync()
        {
            JObject result = new JObject();
            JArray arrResult = new JArray();
            //Assembly assembly = Assembly.LoadFrom(@"c:\dev\ibs_main\QA\QAutomate\RegressionSuite7\bin\Debug\RegressionSuite7.dll");
            //Assembly assembly = Assembly.LoadFrom(@"c:\dev\ibs_main\QA\QAutomate\APISuite\bin\Debug\APISuite.dll");
            Assembly assembly = Assembly.LoadFrom(@"C:\dev\ATTM\TestProjectCSharp\bin\debug\TestProjectCSharp.dll");
            foreach (Type type in assembly.GetTypes())
            {
                JArray arrTC = new JArray();
                foreach (MethodInfo methodInfo in type.GetMethods())
                {
                    //Case 1
                    Logger.Debug($"----------------");
                    Logger.Debug($"Method Name: {methodInfo.Name}");
                    JObject testObject = new JObject();
                    testObject["TestName"] = methodInfo.Name;
                    testObject["Test"] = false;
                    var attributes = methodInfo.GetCustomAttributes(false);
                    if (attributes.Length <= 0) continue;
                    bool isTest = false;
                    foreach (var attInfo in attributes)
                    {
                        if (attInfo is TestAttribute)
                        {
                            isTest = true;
                            testObject["Test"] = true;
                            testObject["isTestComment"] = false;
                            break;
                        }
                    }

                    if (!isTest)
                    {
                        foreach (var attInfo in attributes)
                        {
                            if (attInfo is CategoryAttribute)
                            {
                                isTest = true;
                                testObject["Test"] = true;
                                testObject["isTestComment"] = true;
                                break;
                            }
                        }
                    }

                    if (isTest)
                    {
                        foreach (var attInfo in attributes)
                        {
                            if (attInfo is CategoryAttribute)
                            {
                                Logger.Debug($"Category: {((CategoryAttribute)attInfo).Name}");
                                testObject["Category"] = ((CategoryAttribute)attInfo).Name;
                            }
                            else if (attInfo.GetType().ToString().Contains(".OwnerAttribute")) //use comma . to prevent with RunOwnerAttribute
                            {
                                PropertyInfo pi = attInfo.GetType().GetProperty("Name");
                                String owner = (String)(pi.GetValue(attInfo, null));
                                Logger.Debug($"Owner: {owner}");
                                testObject["Owner"] = owner;
                            }
                            else if (attInfo.GetType().ToString().Contains("DesignerAttribute"))
                            {
                                PropertyInfo pi = attInfo.GetType().GetProperty("Name");
                                String designer = (String)(pi.GetValue(attInfo, null));
                                Logger.Debug($"Designer: {designer}");
                                testObject["Designer"] = designer;
                            }
                            else if (attInfo.GetType().ToString().Contains("PLevelAttribute"))
                            {
                                PropertyInfo pi = attInfo.GetType().GetProperty("Name");
                                String pLevel = (String)(pi.GetValue(attInfo, null));
                                Logger.Debug($"PLevel: {pLevel}");
                                testObject["PLevel"] = pLevel;
                            }
                            else if (attInfo.GetType().ToString().Contains("ClientAttribute"))
                            {
                                PropertyInfo pi = attInfo.GetType().GetProperty("Value");
                                String client = (String)(pi.GetValue(attInfo, null));
                                Logger.Debug($"Client: {client}");
                                testObject["Client"] = client;
                            }
                            else if (attInfo.GetType().ToString().Contains("DSNAttribute"))
                            {
                                PropertyInfo pi = attInfo.GetType().GetProperty("Value");
                                String dsn = (String)(pi.GetValue(attInfo, null));
                                Logger.Debug($"DSN: {dsn}");
                                testObject["DSN"] = dsn;
                            }
                            else if (attInfo.GetType().ToString().Contains(".ReadyForReviewAttribute")) //Use comma . for preventing with NotReadyForReviewAttribute
                            {
                                PropertyInfo pi = attInfo.GetType().GetProperty("Value");
                                String readyForReview = (String)(pi.GetValue(attInfo, null));
                                Logger.Debug($"ReadyForReview: {readyForReview}");
                                testObject["ReadyForReview"] = readyForReview;
                            }
                            else if (attInfo.GetType().ToString().Contains("CaseTypeAttribute"))
                            {
                                PropertyInfo pi = attInfo.GetType().GetProperty("Type");
                                String caseType = (String)(pi.GetValue(attInfo, null));
                                Logger.Debug($"CaseType: {caseType}");
                                testObject["CaseType"] = caseType;
                            }
                            else if (attInfo.GetType().ToString().Contains("ConvertByAttribute"))
                            {
                                PropertyInfo pi = attInfo.GetType().GetProperty("Name");
                                String convertBy = (String)(pi.GetValue(attInfo, null));
                                Logger.Debug($"ConvertBy: {convertBy}");
                                testObject["ConvertBy"] = convertBy;
                            }
                            else if (attInfo.GetType().ToString().Contains("QueueAttribute"))
                            {
                                PropertyInfo pi = attInfo.GetType().GetProperty("Name");
                                String queue = (String)(pi.GetValue(attInfo, null));
                                Logger.Debug($"Queue: {queue}");
                                testObject["Queue"] = queue;
                            }
                            else if (attInfo.GetType().ToString().Contains(".NotReadyForReviewAttribute")) //Use comma . for preventing with ReadyForReviewAttribute
                            {
                                PropertyInfo pi = attInfo.GetType().GetProperty("Value");
                                String notReadyForReview = (String)(pi.GetValue(attInfo, null));
                                Logger.Debug($"NotReadyForReview: {notReadyForReview}");
                                testObject["NotReadyForReview"] = notReadyForReview;
                            }
                            else if (attInfo.GetType().ToString().Contains("ReviewedAttribute"))
                            {
                                testObject["isReviewed"] = true;
                                PropertyInfo pi = attInfo.GetType().GetProperty("Type");
                                bool isReviewPassed = pi.GetValue(attInfo, null).ToString().ToLower().Contains("pass") ? true : false;
                                if (isReviewPassed)
                                {
                                    Logger.Debug($"Reviewed: Passed");
                                    testObject["ReviewedResult"] = "Passed";
                                }
                                else
                                {
                                    PropertyInfo piReason = attInfo.GetType().GetProperty("Reason");
                                    String reviewedFailedReason = (String)(piReason.GetValue(attInfo, null));
                                    Logger.Debug($"Reviewed: Failed");
                                    testObject["ReviewedResult"] = "Failed";
                                    testObject["ReviewFailedReason"] = reviewedFailedReason;
                                }
                            }
                            else if (attInfo is IgnoreAttribute)
                            {
                                Logger.Debug($"Ignore: true");
                                testObject["Ignore"] = true;
                            }
                            else if (attInfo is OrderAttribute)
                            {
                                Logger.Debug($"Order: {((OrderAttribute)attInfo).Order}");
                                testObject["Order"] = ((OrderAttribute)attInfo).Order;
                            }
                        }
                    }


                    PropertyAttribute[] propertyAttributes = (PropertyAttribute[])Attribute.GetCustomAttributes(methodInfo, typeof(PropertyAttribute));
                    if (propertyAttributes.Length <= 0) continue;
                    foreach (PropertyAttribute attribute in propertyAttributes)
                        foreach (string key in attribute.Properties.Keys)
                            foreach (var value in attribute.Properties[key])
                            {
                                testObject[key] = value.ToString();
                                Logger.Debug($"{key}: {value}");
                            }
                    if (isTest) arrTC.Add(testObject);
                    Logger.Debug($"----------------");
                }
                if (arrTC.HasValues) {
                    arrResult.Add(arrTC);
                } 
            }
            result["result"] = arrResult;
            return result;
        }

    }
}