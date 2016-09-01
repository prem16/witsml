﻿//----------------------------------------------------------------------- 
// PDS.Witsml, 2016.1
//
// Copyright 2016 Petrotechnical Data Systems
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//   
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Energistics.DataAccess.WITSML131;
using Energistics.DataAccess.WITSML131.ComponentSchemas;
using Energistics.DataAccess.WITSML131.ReferenceData;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PDS.Witsml.Server.Configuration;

namespace PDS.Witsml.Server.Data.Logs
{
    /// <summary>
    /// Log131DataAdapter Delete tests.
    /// </summary>
    [TestClass]
    public class Log131DataAdapterDeleteTests
    {
        private DevKit131Aspect _devKit;
        private Well _well;
        private Wellbore _wellbore;
        private Log _log;

        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void TestSetUp()
        {
            _devKit = new DevKit131Aspect(TestContext);

            _devKit.Store.CapServerProviders = _devKit.Store.CapServerProviders
                .Where(x => x.DataSchemaVersion == OptionsIn.DataVersion.Version131.Value)
                .ToArray();

            _well = new Well { Uid = _devKit.Uid(), Name = _devKit.Name("Well 01"), TimeZone = _devKit.TimeZone };

            _wellbore = new Wellbore
            {
                Uid = _devKit.Uid(),
                UidWell = _well.Uid,
                NameWell = _well.Name,
                Name = _devKit.Name("Wellbore 01")
            };

            _log = _devKit.CreateLog(_devKit.Uid(), _devKit.Name("Log 01"), _well.Uid, _well.Name, _wellbore.Uid, _wellbore.Name);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            WitsmlSettings.DepthRangeSize = DevKitAspect.DefaultDepthChunkRange;
            WitsmlSettings.TimeRangeSize = DevKitAspect.DefaultTimeChunkRange;
            WitsmlSettings.MaxDataPoints = DevKitAspect.DefaultMaxDataPoints;
            WitsmlSettings.MaxDataNodes = DevKitAspect.DefaultMaxDataNodes;
            WitsmlOperationContext.Current = null;
        }

        [TestMethod]
        public void Log131DataAdapter_DeleteFromStore_Can_Partial_Delete_Elements()
        {
            AddParents();

            _devKit.InitHeader(_log, LogIndexType.measureddepth);

            _log.ServiceCompany = "company 1";
            _log.StepIncrement = new RatioGenericMeasure { Uom = "m", Value = 1.0 };

            // Add log
            _devKit.AddAndAssert(_log);

            // Assert all testing elements are added
            var result = _devKit.GetOneAndAssert(_log);
            Assert.AreEqual(_log.ServiceCompany, result.ServiceCompany);
            Assert.AreEqual(_log.Direction, result.Direction);

            // Partial delete well
            const string delete = "<serviceCompany /><stepIncrement />";
            DeleteLog(_log, delete);

            // Assert the well elements has been deleted
            result = _devKit.GetOneAndAssert(_log);
            Assert.IsNull(result.ServiceCompany);
            Assert.IsNull(result.StepIncrement);
        }

        [TestMethod]
        public void Log131DataAdapter_DeleteFromStore_Can_Delete_All_Log_Channels_And_Data()
        {
            AddParents();

            _devKit.InitHeader(_log, LogIndexType.measureddepth);
            _log.LogData = new List<string> { "13,13.1,13.2", "14,14.1,14.2", "15,15.1,15.2" };

            _devKit.AddAndAssert(_log);

            var indexCurve = _log.LogCurveInfo.FirstOrDefault(l => l.Mnemonic == _log.IndexCurve.Value);
            Assert.IsNotNull(indexCurve);

            var result = _devKit.GetOneAndAssert(_log);
            Assert.AreEqual(_log.LogCurveInfo.Count, result.LogCurveInfo.Count);

            var delete = string.Empty;
            foreach (var curve in _log.LogCurveInfo)
            {
                delete += "<logCurveInfo uid=\"" + curve.Uid + "\" />";
            }
            DeleteLog(_log, delete);

            result = _devKit.GetOneAndAssert(_log);
            Assert.AreEqual(0, result.LogCurveInfo.Count);
            Assert.AreEqual(0, result.LogData.Count);
        }

        [TestMethod]
        public void Log131DataAdapter_DeleteFromStore_Can_Delete_All_Log_Data_By_Mnemonics_Only()
        {
            AddParents();

            _devKit.InitHeader(_log, LogIndexType.measureddepth);
            _log.LogData = new List<string> { "13,13.1,13.2", "14,14.1,14.2", "15,15.1,15.2" };

            _devKit.AddAndAssert(_log);

            var indexCurve = _log.LogCurveInfo.FirstOrDefault(l => l.Mnemonic == _log.IndexCurve.Value);
            Assert.IsNotNull(indexCurve);

            var result = _devKit.GetOneAndAssert(_log);
            Assert.AreEqual(_log.LogCurveInfo.Count, result.LogCurveInfo.Count);

            var delete = string.Empty;
            foreach (var curve in _log.LogCurveInfo)
            {
                delete += "<logCurveInfo><mnemonic>" + curve.Mnemonic + "</mnemonic></logCurveInfo>";
            }
            DeleteLog(_log, delete);

            result = _devKit.GetOneAndAssert(_log);
            Assert.AreEqual(0, result.LogData.Count);

            Assert.AreEqual(_log.LogCurveInfo.Count, result.LogCurveInfo.Count);
            foreach (var curve in result.LogCurveInfo)
            {
                Assert.IsNull(curve.MinIndex);
                Assert.IsNull(curve.MaxIndex);
            }
        }

        [TestMethod]
        public void Log131DataAdapter_DeleteFromStore_Can_Delete_Full_Increasing_Log_Data_By_Index()
        {
            AddParents();

            _devKit.InitHeader(_log, LogIndexType.measureddepth);
            _log.LogData = new List<string> { "13,13.1,13.2", "14,14.1,14.2", "15,15.1,15.2" };

            _devKit.AddAndAssert(_log);

            var indexCurve = _log.LogCurveInfo.FirstOrDefault(l => l.Mnemonic == _log.IndexCurve.Value);
            Assert.IsNotNull(indexCurve);

            var result = _devKit.GetOneAndAssert(_log);
            foreach (var curve in result.LogCurveInfo)
            {
                Assert.AreEqual(13, curve.MinIndex.Value);
                Assert.AreEqual(15, curve.MaxIndex.Value);
            }

            var delete = "<endIndex uom=\"" + indexCurve.Unit + "\">20</endIndex>";
            DeleteLog(_log, delete);

            result = _devKit.GetOneAndAssert(_log);
            Assert.AreEqual(0, result.LogData.Count);

            foreach (var curve in result.LogCurveInfo)
            {
                Assert.IsNull(curve.MinIndex);
                Assert.IsNull(curve.MaxIndex);
            }
        }

        [TestMethod]
        public void Log131DataAdapter_DeleteFromStore_Can_Delete_All_Log_Data_By_Index_Curve_With_Range()
        {
            AddParents();

            _devKit.InitHeader(_log, LogIndexType.measureddepth);
            _log.LogData = new List<string> {"13,13.1,13.2", "14,14.1,14.2", "15,15.1,15.2"};

            _devKit.AddAndAssert(_log);

            var indexCurve = _log.LogCurveInfo.FirstOrDefault(l => l.Mnemonic == _log.IndexCurve.Value);
            Assert.IsNotNull(indexCurve);

            var result = _devKit.GetOneAndAssert(_log);
            Assert.AreEqual(_log.LogCurveInfo.Count, result.LogCurveInfo.Count);

            var delete = "<logCurveInfo>" + Environment.NewLine +
                "<mnemonic>" + indexCurve.Mnemonic + "</mnemonic>" + Environment.NewLine +
                "<minIndex uom=\"" + indexCurve.Unit + "\">10</minIndex>" + Environment.NewLine +
                "</logCurveInfo>";

            DeleteLog(_log, delete);

            result = _devKit.GetOneAndAssert(_log);
            Assert.AreEqual(0, result.LogData.Count);

            Assert.AreEqual(_log.LogCurveInfo.Count, result.LogCurveInfo.Count);
            foreach (var curve in result.LogCurveInfo)
            {
                Assert.IsNull(curve.MinIndex);
                Assert.IsNull(curve.MaxIndex);
            }
        }

        [TestMethod]
        public void Log131DataAdapter_DeleteFromStore_Can_Delete_Full_Increasing_Channel_Data_By_Index()
        {
            AddParents();

            _devKit.InitHeader(_log, LogIndexType.measureddepth);
            _log.LogData = new List<string> { "13,13.1,13.2", "14,14.1,14.2", "15,15.1,15.2" };

            _devKit.AddAndAssert(_log);

            var indexCurve = _log.LogCurveInfo.FirstOrDefault(l => l.Mnemonic == _log.IndexCurve.Value);
            Assert.IsNotNull(indexCurve);
            var lastCurve = _log.LogCurveInfo.Last();
            Assert.IsNotNull(lastCurve);

            var result = _devKit.GetOneAndAssert(_log);
            foreach (var curve in result.LogCurveInfo)
            {
                Assert.AreEqual(13, curve.MinIndex.Value);
                Assert.AreEqual(15, curve.MaxIndex.Value);
            }

            var delete = "<logCurveInfo uid=\"" + lastCurve.Uid + "\">" + Environment.NewLine +
                    "<minIndex uom=\"" + indexCurve.Unit + "\">10</minIndex>" + Environment.NewLine +
                "</logCurveInfo>";
            DeleteLog(_log, delete);

            result = _devKit.GetOneAndAssert(_log);
            Assert.IsNotNull(result.LogData);

            foreach (var curve in result.LogCurveInfo)
            {
                Assert.AreEqual(13, curve.MinIndex.Value);
                Assert.AreEqual(15, curve.MaxIndex.Value);
            }
        }

        [TestMethod]
        public void Log131DataAdapter_DeleteFromStore_Can_Delete_Increasing_Channels_Data_With_Different_Index_Range()
        {
            AddParents();

            _devKit.InitHeader(_log, LogIndexType.measureddepth);           
            _log.LogData = new List<string>
            {
                "13,13.1,13.2",
                "14,14.1,14.2",
                "15,15.1,15.2",
                "16,16.1,16.2",
                "17,17.1,17.2",
                "18,18.1,18.2"
            }; 

            _devKit.AddAndAssert(_log);

            var indexCurve = _log.LogCurveInfo.FirstOrDefault(l => l.Mnemonic == _log.IndexCurve.Value);
            Assert.IsNotNull(indexCurve);
            var curve1 = _log.LogCurveInfo[1];
            Assert.IsNotNull(curve1);
            var curve2 = _log.LogCurveInfo[2];
            Assert.IsNotNull(curve2);

            var result = _devKit.GetOneAndAssert(_log);
            foreach (var curve in result.LogCurveInfo)
            {
                Assert.AreEqual(13, curve.MinIndex.Value);
                Assert.AreEqual(18, curve.MaxIndex.Value);
            }

            var delete = "<logCurveInfo uid=\"" + curve1.Uid + "\">" + Environment.NewLine +
                    "<minIndex uom=\"" + indexCurve.Unit + "\">15</minIndex>" + Environment.NewLine +
                "</logCurveInfo>" + Environment.NewLine +
                "<logCurveInfo uid=\"" + curve2.Uid + "\">" + Environment.NewLine +
                    "<maxIndex uom=\"" + indexCurve.Unit + "\">15</maxIndex>" + Environment.NewLine +
                "</logCurveInfo>";
            DeleteLog(_log, delete);

            result = _devKit.GetOneAndAssert(_log);

            // Assert log data
            var logData = result.LogData;
            Assert.IsNotNull(logData);
            Assert.AreEqual("13,13.1,", logData[0]);
            Assert.AreEqual("14,14.1,", logData[1]);
            Assert.AreEqual("16,,16.2", logData[2]);
            Assert.AreEqual("17,,17.2", logData[3]);
            Assert.AreEqual("18,,18.2", logData[4]);

            // Assert Index
            curve1 = result.LogCurveInfo[1];
            Assert.IsNotNull(curve1);
            Assert.AreEqual(14, curve1.MaxIndex.Value);
            curve2 = result.LogCurveInfo[2];
            Assert.IsNotNull(curve2);
            Assert.AreEqual(16, curve2.MinIndex.Value);
        }

        [TestMethod]
        public void Log131DataAdapter_DeleteFromStore_Can_Delete_Decreasing_Channels_Data_With_Different_Index_Range()
        {
            AddParents();

            _devKit.InitHeader(_log, LogIndexType.measureddepth, false);           
            _log.LogData = new List<string>
            {
                "18,18.1,18.2",
                "17,17.1,17.2",
                "16,16.1,16.2",
                "15,15.1,15.2",
                "14,14.1,14.2",
                "13,13.1,13.2"
            };

            _devKit.AddAndAssert(_log);

            var indexCurve = _log.LogCurveInfo.FirstOrDefault(l => l.Mnemonic == _log.IndexCurve.Value);
            Assert.IsNotNull(indexCurve);
            var curve1 = _log.LogCurveInfo[1];
            Assert.IsNotNull(curve1);
            var curve2 = _log.LogCurveInfo[2];
            Assert.IsNotNull(curve2);

            var result = _devKit.GetOneAndAssert(_log);
            foreach (var curve in result.LogCurveInfo)
            {
                Assert.AreEqual(13, curve.MinIndex.Value);
                Assert.AreEqual(18, curve.MaxIndex.Value);
            }
            Assert.AreEqual(18, result.StartIndex.Value);
            Assert.AreEqual(13, result.EndIndex.Value);

            var delete = "<logCurveInfo uid=\"" + curve1.Uid + "\">" + Environment.NewLine +
                    "<minIndex uom=\"" + indexCurve.Unit + "\">15</minIndex>" + Environment.NewLine +
                "</logCurveInfo>" + Environment.NewLine +
                "<logCurveInfo uid=\"" + curve2.Uid + "\">" + Environment.NewLine +
                    "<maxIndex uom=\"" + indexCurve.Unit + "\">15</maxIndex>" + Environment.NewLine +
                "</logCurveInfo>";
            DeleteLog(_log, delete);

            result = _devKit.GetOneAndAssert(_log);

            // Assert log data
            var logData = result.LogData;
            Assert.IsNotNull(logData);
            Assert.AreEqual("18,,18.2", logData[0]);
            Assert.AreEqual("17,,17.2", logData[1]);
            Assert.AreEqual("16,,16.2", logData[2]);
            Assert.AreEqual("14,14.1,", logData[3]);
            Assert.AreEqual("13,13.1,", logData[4]);

            // Assert Index
            curve1 = result.LogCurveInfo[1];
            Assert.IsNotNull(curve1);
            Assert.AreEqual(14, curve1.MaxIndex.Value);
            curve2 = result.LogCurveInfo[2];
            Assert.IsNotNull(curve2);
            Assert.AreEqual(16, curve2.MinIndex.Value);

            Assert.AreEqual(18, result.StartIndex.Value);
            Assert.AreEqual(13, result.EndIndex.Value);
        }

        [TestMethod]
        public void Log131DataAdapter_DeleteFromStore_Can_Delete_Increasing_Channels_Data_With_Default_And_Specific_Index_Range()
        {
            AddParents();

            _devKit.InitHeader(_log, LogIndexType.measureddepth);
            _log.LogData = new List<string>
            {
                "13,13.1,13.2",
                "14,14.1,14.2",
                "15,15.1,15.2",
                "16,16.1,16.2",
                "17,17.1,17.2",
                "18,18.1,18.2"
            };

            _devKit.AddAndAssert(_log);

            var indexCurve = _log.LogCurveInfo.FirstOrDefault(l => l.Mnemonic == _log.IndexCurve.Value);
            Assert.IsNotNull(indexCurve);
            var curve1 = _log.LogCurveInfo[1];
            Assert.IsNotNull(curve1);
            var curve2 = _log.LogCurveInfo[2];
            Assert.IsNotNull(curve2);

            var result = _devKit.GetOneAndAssert(_log);
            foreach (var curve in result.LogCurveInfo)
            {
                Assert.AreEqual(13, curve.MinIndex.Value);
                Assert.AreEqual(18, curve.MaxIndex.Value);
            }

            var delete = "<startIndex uom=\"" + indexCurve.Unit + "\">15</startIndex>" + Environment.NewLine +
                "<logCurveInfo><mnemonic>" + curve1.Mnemonic + "</mnemonic></logCurveInfo>" + Environment.NewLine +
                "<logCurveInfo uid=\"" + curve2.Uid + "\">" + Environment.NewLine +
                "<minIndex uom=\"" + indexCurve.Unit + "\">16</minIndex>" + Environment.NewLine +
                "</logCurveInfo>";
            DeleteLog(_log, delete);

            result = _devKit.GetOneAndAssert(_log);

            // Assert log data
            var logData = result.LogData;
            Assert.IsNotNull(logData);
            Assert.AreEqual("13,13.1,13.2", logData[0]);
            Assert.AreEqual("14,14.1,14.2", logData[1]);
            Assert.AreEqual("15,,15.2", logData[2]);

            // Assert Index
            curve1 = result.LogCurveInfo[1];
            Assert.IsNotNull(curve1);
            Assert.AreEqual(14, curve1.MaxIndex.Value);
            curve2 = result.LogCurveInfo[2];
            Assert.IsNotNull(curve2);
            Assert.AreEqual(15, curve2.MaxIndex.Value);
        }

        [TestMethod]
        public void Log131DataAdapter_DeleteFromStore_Can_Delete_Multiple_Curves_With_StartIndex_And_EndIndex()
        {
            AddParents();

            _devKit.InitHeader(_log, LogIndexType.measureddepth);

            // Add another curve
            var newCurve = _devKit.LogGenerator.CreateDoubleLogCurveInfo("RPM", "c/s");
            newCurve.ColumnIndex = 4;
            _log.LogCurveInfo.Add(newCurve);

            _log.LogData = new List<string>
            {
                "13,13.1,13.2,13.3",
                "14,14.1,14.2,14.3",
                "15,15.1,15.2,15.3",
                "16,16.1,16.2,16.3",
                "17,17.1,17.2,17.3",
                "18,18.1,18.2,18.3"
            };

            _devKit.AddAndAssert(_log);

            var indexCurve = _log.LogCurveInfo.FirstOrDefault(l => l.Mnemonic == _log.IndexCurve.Value);
            Assert.IsNotNull(indexCurve);
            var curve1 = _log.LogCurveInfo[1];
            Assert.IsNotNull(curve1);
            var curve2 = _log.LogCurveInfo[2];
            Assert.IsNotNull(curve2);
            var curve3 = _log.LogCurveInfo[3];
            Assert.IsNotNull(curve3);

            var result = _devKit.GetOneAndAssert(_log);
            foreach (var curve in result.LogCurveInfo)
            {
                Assert.AreEqual(13, curve.MinIndex.Value);
                Assert.AreEqual(18, curve.MaxIndex.Value);
            }

            var delete = "<startIndex uom=\"" + indexCurve.Unit + "\">0</startIndex>" + Environment.NewLine +
                         "<endIndex uom=\"" + indexCurve.Unit + "\">20</endIndex>" + Environment.NewLine +
                         "<logCurveInfo><mnemonic>" + curve1.Mnemonic + "</mnemonic></logCurveInfo>" + Environment.NewLine +
                         "<logCurveInfo><mnemonic>" + curve2.Mnemonic + "</mnemonic></logCurveInfo>";
            DeleteLog(_log, delete);

            result = _devKit.GetOneAndAssert(_log);

            // Assert log data
            var logData = result.LogData;
            Assert.IsNotNull(logData);
            Assert.AreEqual("13,13.3", logData[0]);
            Assert.AreEqual("14,14.3", logData[1]);
            Assert.AreEqual("15,15.3", logData[2]);
            Assert.AreEqual("16,16.3", logData[3]);
            Assert.AreEqual("17,17.3", logData[4]);
            Assert.AreEqual("18,18.3", logData[5]);

            // Assert Index
            curve1 = result.LogCurveInfo[0];
            Assert.IsNotNull(curve1);
            Assert.AreEqual(13, curve1.MinIndex.Value);
            Assert.AreEqual(18, curve1.MaxIndex.Value);
            curve2 = result.LogCurveInfo[1];
            Assert.IsNotNull(curve2);
            Assert.AreEqual(13, curve2.MinIndex.Value);
            Assert.AreEqual(18, curve2.MaxIndex.Value);
        }

        [TestMethod, Description("Tests you cannot do DeleteFromStore without plural container")]
        public void Log131DataAdapter_DeleteFromStore_Error_401_No_Plural_Root_Element()
        {
            var nonPluralLog = "<log xmlns=\"http://www.witsml.org/schemas/131\" version=\"1.3.1.1\">" + Environment.NewLine +
                           "<log uid=\"{0}\" uidWell=\"{1}\" uidWellbore=\"{2}\" />" + Environment.NewLine +
                           "</log>";

            var queryIn = string.Format(nonPluralLog, _log.Uid, _log.UidWell, _log.UidWellbore);
            var response = _devKit.DeleteFromStore(ObjectTypes.Log, queryIn, null, null);

            Assert.IsNotNull(response);
            Assert.AreEqual((short)ErrorCodes.MissingPluralRootElement, response.Result);
        }

        [TestMethod, Description("Tests you cannot do DeleteFromStore while missing the object type")]
        public void Log131DataAdapter_DeleteFromStore_Error_407_Missing_Witsml_Object_Type()
        {
            var response = _devKit.Delete<LogList, Log>(_log, string.Empty);
            Assert.IsNotNull(response);
            Assert.AreEqual((short)ErrorCodes.MissingWmlTypeIn, response.Result);
        }

        [TestMethod, Description("Tests you cannot do DeleteFromStore with empty queryIn")]
        public void Log131DataAdapter_DeleteFromStore_Error_408_Empty_QueryIn()
        {
            var response = _devKit.DeleteFromStore(ObjectTypes.Log, string.Empty, null, null);

            Assert.IsNotNull(response);
            Assert.AreEqual((short)ErrorCodes.MissingInputTemplate, response.Result);
        }

        [TestMethod, Description("Tests you cannot do DeleteFromStore with invalid xml")]
        public void Log131DataAdapter_DeleteFromStore_Error_409_QueryIn_Must_Conform_To_Schema()
        {
            AddParents();

            // Delete log with invalid element
            const string delete = "<serviceCompany /><serviceCompany />";
            DeleteLog(_log, delete, ErrorCodes.InputTemplateNonConforming);
        }

        [TestMethod, Description("Tests you cannot do DeleteFromStore without specifying the log uid")]
        public void Log131DataAdapter_DeleteFromStore_Error_415_Delete_Without_Specifing_UID()
        {
            AddParents();

            _log.Uid = string.Empty;
            DeleteLog(_log, string.Empty, ErrorCodes.DataObjectUidMissing);
        }

        [TestMethod, Description("Tests you cannot do DeleteFromStore with a missing uom")]
        public void Log131DataAdapter_DeleteFromStore_Error_417_Deleting_With_Empty_UOM_Attribute()
        {
            AddParents();

            _devKit.InitHeader(_log, LogIndexType.measureddepth);
            _log.StepIncrement = new RatioGenericMeasure { Uom = "m", Value = 1.0 };

            _devKit.AddAndAssert(_log);

            // Delete log
            const string delete = "<stepIncrement uom=\"\" />";
            DeleteLog(_log, delete, ErrorCodes.EmptyUomSpecified);
        }

        [TestMethod, Description("Tests you cannot do DeleteFromStore with empty logData element")]
        public void Log131DataAdapter_DeleteFromStore_Error_419_Deleting_With_Empty_LogData_Element()
        {
            AddParents();

            _devKit.InitHeader(_log, LogIndexType.measureddepth);
            _devKit.InitDataMany(_log, _devKit.Mnemonics(_log), _devKit.Units(_log), 10);
            _devKit.AddAndAssert(_log);

            // Delete log
            const string delete = "<logData />";
            DeleteLog(_log, delete, ErrorCodes.EmptyNonRecurringElementSpecified);
        }

        [TestMethod, Description("Tests you cannot do DeleteFromStore with an empty node for a non-recurring element or attribute that is mandatory in the write schema.")]
        public void Log131DataAdapter_DeleteFromStore_Error_420_Delete_Required_Element()
        {
            AddParents();

            _devKit.InitHeader(_log, LogIndexType.measureddepth);
            _devKit.AddAndAssert(_log);

            // Delete log
            const string delete = "<name />";
            DeleteLog(_log, delete, ErrorCodes.EmptyMandatoryNodeSpecified);
        }

        [TestMethod, Description("Tests you cannot do DeleteFromStore on a log that does not exist")]
        public void Log131DataAdapter_DeleteFromStore_Error_433_Log_Does_Not_Exist()
        {
            AddParents();

            // Delete log
            DeleteLog(_log, string.Empty, ErrorCodes.DataObjectNotExist);
        }

        [TestMethod, Description("Tests you cannot do DeleteFromStore more than one object")]
        public void Log131DataAdapter_DeleteFromStore_Error_444_Deleting_More_Than_One_Data_Object()
        {
            AddParents();

            _devKit.InitHeader(_log, LogIndexType.measureddepth);
            _devKit.AddAndAssert(_log);

            var log2 = _devKit.CreateLog(_devKit.Uid(), "log 2", _well.Uid, _well.Name, _wellbore.Uid, _wellbore.Name);
            _devKit.InitHeader(log2, LogIndexType.measureddepth);
            _devKit.AddAndAssert(log2);

            var delete = "<logs xmlns=\"http://www.witsml.org/schemas/131\" version=\"1.3.1.1\">" + Environment.NewLine +
                          "   <log uid=\"{0}\" uidWell=\"{1}\" uidWellbore=\"{2}\" />" + Environment.NewLine +
                          "   <log uid=\"{3}\" uidWell=\"{4}\" uidWellbore=\"{5}\" />" + Environment.NewLine +
                          "</logs>";
            var queryIn = string.Format(delete, _log.Uid, _log.UidWell, _log.UidWellbore, log2.Uid, log2.UidWell,
                log2.UidWellbore);

            var results = _devKit.DeleteFromStore(ObjectTypes.Log, queryIn, null, null);
            Assert.IsNotNull(results);
            Assert.AreEqual((short)ErrorCodes.InputTemplateMultipleDataObjects, results.Result);
        }

        [TestMethod, Description("Tests you cannot do DeleteFromStore of index curve unless all curve are being deleted")]
        public void Log131DataAdapter_DeleteFromStore_Error_1052_Deleting_Index_Curve()
        {
            AddParents();

            _devKit.InitHeader(_log, LogIndexType.measureddepth);
            _devKit.InitDataMany(_log, _devKit.Mnemonics(_log), _devKit.Units(_log), 10);
            _devKit.AddAndAssert(_log);

            // Delete log
            var delete = $"<logCurveInfo><mnemonic>{_log.IndexCurve.Value}</mnemonic></logCurveInfo>";
            DeleteLog(_log, delete, ErrorCodes.ErrorDeletingIndexCurve);
        }

        #region Helper Methods

        private void AddParents()
        {
            _devKit.AddAndAssert(_well);
            _devKit.AddAndAssert(_wellbore);
        }

        private void DeleteLog(Log log, string delete, ErrorCodes error = ErrorCodes.Success)
        {
            var queryIn = string.Format(DevKit131Aspect.BasicDeleteLogXmlTemplate, log.Uid, log.UidWell, log.UidWellbore, delete);
            var response = _devKit.DeleteFromStore(ObjectTypes.Log, queryIn, null, null);
            Assert.AreEqual((short)error, response.Result);
        }

        #endregion
    }
}
