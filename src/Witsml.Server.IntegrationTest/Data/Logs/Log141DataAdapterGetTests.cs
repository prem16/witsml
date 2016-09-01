﻿//----------------------------------------------------------------------- 
// PDS.Witsml.Server, 2016.1
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
using Energistics.DataAccess;
using Energistics.DataAccess.WITSML141;
using Energistics.DataAccess.WITSML141.ComponentSchemas;
using Energistics.DataAccess.WITSML141.ReferenceData;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PDS.Witsml.Data.Channels;
using PDS.Witsml.Server.Configuration;

namespace PDS.Witsml.Server.Data.Logs
{
    [TestClass]
    public class Log141DataAdapterGetTests
    {
        private DevKit141Aspect _devKit;
        private Well _well;
        private Wellbore _wellbore;
        private Log _log;

        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void TestSetUp()
        {
            _devKit = new DevKit141Aspect(TestContext);

            _devKit.Store.CapServerProviders = _devKit.Store.CapServerProviders
                .Where(x => x.DataSchemaVersion == OptionsIn.DataVersion.Version141.Value)
                .ToArray();

            _well = new Well
            {
                Uid = _devKit.Uid(),
                Name = _devKit.Name("Well 01"),
                TimeZone = _devKit.TimeZone
            };

            _wellbore = new Wellbore()
            {
                UidWell = _well.Uid,
                NameWell = _well.Name,
                Uid = _devKit.Uid(),
                Name = _devKit.Name("Wellbore 01")
            };

            _log = new Log()
            {
                UidWell = _well.Uid,
                NameWell = _well.Name,
                UidWellbore = _wellbore.Uid,
                NameWellbore = _wellbore.Name,
                Uid = _devKit.Uid(),
                Name = _devKit.Name("Log 01")
            };
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
        public void Log141DataAdapter_GetFromStore_Can_Be_Retrieved_With_All_Data()
        {
            // Initialize the _log
            var row = 10;
            int columnCountBeforeSave = 0;
            var response = AddLogWithAction( row, () =>
            {
                columnCountBeforeSave = _log.LogData.First().Data.First().Split(',').Length;
            });

            // Test that a Log was Added successfully
            Assert.AreEqual((short) ErrorCodes.Success, response.Result);

            // Create a query log
            var query = new Log
            {
                Uid = _log.Uid,
                UidWell = _log.UidWell,
                UidWellbore = _log.UidWellbore,
            };
            var result = _devKit.Query<LogList, Log>(query, ObjectTypes.Log, null, OptionsIn.ReturnElements.DataOnly);
            var queriedLog = result.FirstOrDefault();

            // Test that Log was returned
            Assert.IsNotNull(queriedLog);

            var logData = queriedLog.LogData.FirstOrDefault();
            Assert.IsNotNull(logData);

            var data = logData.Data;
            var firstRow = data.First().Split(',');
            var mnemonics = logData.MnemonicList.Split(',').ToList();

            // Test that all of the rows of data saved are returned.
            Assert.AreEqual(row, data.Count);

            // Test that the number of mnemonics matches the number of data values per row
            Assert.AreEqual(firstRow.Length, mnemonics.Count);

            // Update Test to verify that a column of LogData.Data with no values is NOT returned with the results.
            Assert.AreEqual(columnCountBeforeSave - 1, firstRow.Length);
        }

        [TestMethod]
        public void Log141DataAdapter_GetFromStore_Column_With_One_Value_Returned()
        {
            var row = 10;
            int columnCountBeforeSave = 0;
            AddLogWithAction(row, () =>
            {
                _log.LogData.First().Data[2] = _log.LogData.First().Data[2].Replace(",,", ",0,");
                columnCountBeforeSave = _log.LogData.First().Data.First().Split(',').Length;
            });

            // Create a query log
            var query = new Log
            {
                Uid = _log.Uid,
                UidWell = _log.UidWell,
                UidWellbore = _log.UidWellbore,
            };
            var result = _devKit.Query<LogList, Log>(query, ObjectTypes.Log, null, OptionsIn.ReturnElements.DataOnly);
            var queriedLog = result.FirstOrDefault();

            // Test that Log was returned
            Assert.IsNotNull(queriedLog);

            var logData = queriedLog.LogData.FirstOrDefault();
            Assert.IsNotNull(logData);

            var data = logData.Data;
            var firstRow = data.First().Split(',');

            // Update Test to verify that a column of LogData.Data with no values is NOT returned with the results.
            Assert.AreEqual(columnCountBeforeSave, firstRow.Length);
        }

        [TestMethod]
        public void Log141DataAdapter_GetFromStore_Can_Be_Retrieved_With_Increasing_Log_Data()
        {
            var row = 10;
            var response = AddLogWithAction(row);
            Assert.AreEqual((short) ErrorCodes.Success, response.Result);

            var logData = new LogData {MnemonicList = _devKit.Mnemonics(_log)};
            var query = new Log
            {
                Uid = _log.Uid,
                UidWell = _log.UidWell,
                UidWellbore = _log.UidWellbore,
                StartIndex = new GenericMeasure(2.0, "m"),
                EndIndex = new GenericMeasure(6.0, "m"),
                LogData = new List<LogData> {logData}
            };
            var result = _devKit.Query<LogList, Log>(query, ObjectTypes.Log, null, OptionsIn.ReturnElements.DataOnly);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Log141DataAdapter_GetFromStore_Can_Be_Retrieved_With_Decreasing_Log_Data()
        {
            var row = 10;
            var response = AddLogWithAction(row, null, 1, true, true, false);
            Assert.AreEqual((short) ErrorCodes.Success, response.Result);

            //var uidLog = response.SuppMsgOut;
            var logData = new LogData {MnemonicList = _devKit.Mnemonics(_log)};
            var query = new Log
            {
                Uid = _log.Uid,
                UidWell = _log.UidWell,
                UidWellbore = _log.UidWellbore,
                Direction = LogIndexDirection.decreasing,
                StartIndex = new GenericMeasure(-3.0, "m"),
                EndIndex = new GenericMeasure(-6.0, "m"),
                LogData = new List<LogData> {logData}
            };
            var result = _devKit.Query<LogList, Log>(query, ObjectTypes.Log, null, OptionsIn.ReturnElements.DataOnly);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Log141DataAdapter_GetFromStore_Empty_Elements_Are_Removed()
        {
            _devKit.Add<WellList, Well>(_well);
            _devKit.Add<WellboreList, Wellbore>(_wellbore);

            _devKit.InitHeader(_log, LogIndexType.measureddepth, false);
            var response = _devKit.Add<LogList, Log>(_log);
            Assert.AreEqual((short) ErrorCodes.Success, response.Result);

            // Query all of the log and Assert
            var query = new Log()
            {
                UidWell = _log.UidWell,
                UidWellbore = _log.UidWellbore,
                Uid = _log.Uid
            };
            var returnLog =
                _devKit.Query<LogList, Log>(query, ObjectTypes.Log, null, OptionsIn.ReturnElements.All).FirstOrDefault();
            Assert.IsNotNull(returnLog);
            Assert.IsNotNull(returnLog.IndexType);
            Assert.AreEqual(_log.IndexType, returnLog.IndexType);

            var queryIn = "<?xml version=\"1.0\"?>" + Environment.NewLine +
                          "<logs xmlns:xlink=\"http://www.w3.org/1999/xlink\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:dc=\"http://purl.org/dc/terms/\" " +
                          "xmlns:gml=\"http://www.opengis.net/gml/3.2\" version=\"1.4.1.1\" xmlns=\"http://www.witsml.org/schemas/1series\">" +
                          Environment.NewLine +
                          "<log uid=\"" + _log.Uid + "\" uidWell=\"" + _log.UidWell + "\" uidWellbore=\"" + _log.UidWellbore +
                          "\">" + Environment.NewLine +
                          "<nameWell />" + Environment.NewLine +
                          "</log>" + Environment.NewLine +
                          "</logs>";

            // Query log, requested by default.
            var result = _devKit.GetFromStore(ObjectTypes.Log, queryIn, null, null);
            Assert.IsNotNull(result);

            var document = WitsmlParser.Parse(result.XMLout);
            var parser = new WitsmlQueryParser(document.Root, ObjectTypes.Log, null);
            Assert.IsFalse(parser.HasElements("indexType"));
        }

        [TestMethod]
        public void Log141DataAdapter_GetFromStore_Header_Index_Value_Sorted_For_Decreasing_Log()
        {
            var row = 10;
            var response = AddLogWithAction(row, () =>
            {
                _log.StartIndex = new GenericMeasure { Uom = "m", Value = 100 };
                var logData = _log.LogData.First();
                logData.Data.Clear();
                logData.Data.Add("100,1,");
                logData.Data.Add("99,2,");
                logData.Data.Add("98,3,");
                logData.Data.Add("97,4,");
                logData.Data.Add("96,5,");
                logData.Data.Add("95,,6");
                logData.Data.Add("94,,7");
                logData.Data.Add("93,,8");
                logData.Data.Add("92,,9");
                logData.Data.Add("91,,10");
            }, 1, true, false, false);
            Assert.AreEqual((short) ErrorCodes.Success, response.Result);

            //var uidLog = response.SuppMsgOut;
            var query = new Log
            {
                Uid = _log.Uid,
                UidWell = _log.UidWell,
                UidWellbore = _log.UidWellbore,
                Direction = LogIndexDirection.decreasing,
                StartIndex = new GenericMeasure(98.0, "m"),
                EndIndex = new GenericMeasure(94.0, "m")
            };
            var result = _devKit.Query<LogList, Log>(query, ObjectTypes.Log, null, OptionsIn.ReturnElements.All);
            Assert.IsNotNull(result);

            var logAdded = result.First();
            var indexCurve = logAdded.LogCurveInfo.First();
            Assert.AreEqual(query.EndIndex.Value, indexCurve.MinIndex.Value);
            Assert.AreEqual(query.StartIndex.Value, indexCurve.MaxIndex.Value);
            Assert.AreEqual(query.StartIndex.Value, logAdded.StartIndex.Value);
            Assert.AreEqual(query.EndIndex.Value, logAdded.EndIndex.Value);

            var firstChannel = logAdded.LogCurveInfo[1];
            Assert.AreEqual(96, firstChannel.MinIndex.Value);
            Assert.AreEqual(query.StartIndex.Value, firstChannel.MaxIndex.Value);

            var secondChannel = logAdded.LogCurveInfo[2];
            Assert.AreEqual(query.EndIndex.Value, secondChannel.MinIndex.Value);
            Assert.AreEqual(95, secondChannel.MaxIndex.Value);
        }

        [TestMethod]
        public void Log141DataAdapter_GetFromStore_Decreasing_RequestLatestValue_OptionsIn()
        {
            var row = 10;
            var response = AddLogWithAction(row, () =>
            {
                _log.StartIndex = new GenericMeasure {Uom = "m", Value = 100};

                var logData = _log.LogData.First();
                logData.Data.Clear();
                logData.Data.Add("100,1,");
                logData.Data.Add("99,2,");
                logData.Data.Add("98,3,");
                logData.Data.Add("97,4,");

                // Our return data set should be all of these values.
                logData.Data.Add("96,5,"); // The latest 1 value for the 2nd channel
                logData.Data.Add("95,,6"); // should not be returned
                logData.Data.Add("94,,7"); // should not be returned
                logData.Data.Add("93,,8"); // should not be returned
                logData.Data.Add("92,,9"); // should not be returned
                logData.Data.Add("91,,10"); // The latest 1 value for the last channel
            }, 1, true, false, false);

            // Assert that the log was added successfully
            Assert.AreEqual((short) ErrorCodes.Success, response.Result);


            var uidLog = response.SuppMsgOut;
            var query = new Log
            {
                Uid = uidLog,
                UidWell = _log.UidWell,
                UidWellbore = _log.UidWellbore,
            };

            var result = _devKit.Query<LogList, Log>(
                query,
                ObjectTypes.Log, null, OptionsIn.ReturnElements.All + ';' +
                                       OptionsIn.RequestLatestValues.Eq(1));
                // Request the latest 1 value (for each channel)


            // Verify that a Log was returned with the results
            Assert.IsNotNull(result, "No results returned from Log query");
            Assert.IsNotNull(result.First(), "No Logs returned in results from Log query");

            // Verify that a LogData element was returned with the results.
            var queryLogData = result.First().LogData;
            Assert.IsTrue(queryLogData != null && queryLogData.First() != null,
                "No LogData returned in results from Log query");

            // Verify that data rows were returned with the LogData
            var queryData = queryLogData.First().Data;
            Assert.IsTrue(queryData != null && queryData.Count > 0, "No data returned in results from Log query");

            // Verify that only the rows of data (referenced above) for index values 96 and 91 were returned 
            //... in order to get the latest 1 value for each channel.
            Assert.AreEqual(2, queryData.Count, "Only rows for index values 96 and 91 should be returned.");
            Assert.AreEqual("96", queryData[0].Split(',')[0], "The first data row should be for index value 96");
            Assert.AreEqual("91", queryData[1].Split(',')[0], "The second data row should be for index value 91");
        }

        [TestMethod]
        public void Log141DataAdapter_GetFromStore_Increasing_RequestLatestValue_OptionsIn()
        {
            _devKit.Add<WellList, Well>(_well);
            _devKit.Add<WellboreList, Wellbore>(_wellbore);

            var row = 10;
            _devKit.InitHeader(_log, LogIndexType.measureddepth);

            // Add a 4th Log Curve
            _log.LogCurveInfo.Add(_devKit.CreateDoubleLogCurveInfo("GR2", "gAPI"));

            var startIndex = new GenericMeasure { Uom = "m", Value = 100 };
            _log.StartIndex = startIndex;
            _devKit.InitDataMany(_log, _devKit.Mnemonics(_log), _devKit.Units(_log), row, 1, true, false);

            // Reset for custom LogData
            var logData = _log.LogData.First();
            logData.Data.Clear();

            logData.Data.Add("1,1,,");
            logData.Data.Add("2,2,,");
            logData.Data.Add("3,3,,");
            logData.Data.Add("4,4,,"); // returned
            logData.Data.Add("5,5,,"); // returned

            logData.Data.Add("6,,1,");
            logData.Data.Add("7,,2,");
            logData.Data.Add("8,,3,");
            logData.Data.Add("9,,4,"); // returned
            logData.Data.Add("10,,5,"); // returned

            logData.Data.Add("11,,,1");
            logData.Data.Add("12,,,2");
            logData.Data.Add("13,,,3");
            logData.Data.Add("14,,,4"); // returned
            logData.Data.Add("15,,,5"); // returned

            // Add a decreasing log with several values
            var response = _devKit.Add<LogList, Log>(_log);

            // Assert that the log was added successfully
            Assert.AreEqual((short) ErrorCodes.Success, response.Result);


            var uidLog = response.SuppMsgOut;
            var query = new Log
            {
                Uid = uidLog,
                UidWell = _log.UidWell,
                UidWellbore = _log.UidWellbore,
            };

            var result = _devKit.Query<LogList, Log>(
                query,
                ObjectTypes.Log, null, OptionsIn.ReturnElements.All + ';' +
                                       OptionsIn.RequestLatestValues.Eq(2));
                // Request the latest 2 values (for each channel)


            // Verify that a Log was returned with the results
            Assert.IsNotNull(result, "No results returned from Log query");
            Assert.IsNotNull(result.First(), "No Logs returned in results from Log query");

            // Verify that a LogData element was returned with the results.
            var queryLogData = result.First().LogData;
            Assert.IsTrue(queryLogData != null && queryLogData.First() != null,
                "No LogData returned in results from Log query");

            // Verify that data rows were returned with the LogData
            var queryData = queryLogData.First().Data;
            Assert.IsTrue(queryData != null && queryData.Count > 0, "No data returned in results from Log query");

            // Verify that only the rows of data (referenced above) for index values 96 and 91 were returned 
            //... in order to get the latest 1 value for each channel.
            Assert.AreEqual(6, queryData.Count, "Only rows for index values 4,5,9,10,14 and 15 should be returned.");
            Assert.AreEqual("4", queryData[0].Split(',')[0], "The first data row should be for index value 4");
            Assert.AreEqual("15", queryData[5].Split(',')[0], "The last data row should be for index value 15");
        }

        [TestMethod]
        public void Log141DataAdapter_GetFromStore_Increasing_Time_RequestLatestValue_OptionsIn()
        {
            var row = 1;
            var response = AddLogWithAction(row, () =>
            {
                // Reset for custom LogData
                var logData = _log.LogData.First();
                logData.Data.Clear();

                logData.Data.Add("2012-07-26T15:17:20.0000000+00:00,1,0");
                logData.Data.Add("2012-07-26T15:17:30.0000000+00:00,2,1");
                logData.Data.Add("2012-07-26T15:17:40.0000000+00:00,,2");
                logData.Data.Add("2012-07-26T15:17:50.0000000+00:00,,3");
            }, 1, false, false);

            // Assert that the log was added successfully
            Assert.AreEqual((short) ErrorCodes.Success, response.Result);

            var uidLog = response.SuppMsgOut;

            var query = new Log
            {
                Uid = uidLog,
                UidWell = _log.UidWell,
                UidWellbore = _log.UidWellbore,
            };

            var result = _devKit.Query<LogList, Log>(
                query,
                ObjectTypes.Log, null, OptionsIn.ReturnElements.All + ';' +
                                       OptionsIn.RequestLatestValues.Eq(1));
                // Request the latest 1 values (for each channel)



            // Verify that a Log was returned with the results
            Assert.IsNotNull(result, "No results returned from Log query");
            Assert.IsNotNull(result.First(), "No Logs returned in results from Log query");

            var logInfos = result.First().LogCurveInfo;
            Assert.AreEqual(_log.LogCurveInfo.Count, logInfos.Count, "There should be 3 LogCurveInfos");

            // Verify that a LogData element was returned with the results.
            var queryLogData = result.First().LogData;
            Assert.IsTrue(queryLogData != null && queryLogData.First() != null,
                "No LogData returned in results from Log query");

            // Verify that data rows were returned with the LogData
            var queryData = queryLogData.First().Data;
            Assert.IsTrue(queryData != null && queryData.Count > 0, "No data returned in results from Log query");

            // Verify that only the rows of data (referenced above) for index values 96 and 91 were returned 
            //... in order to get the latest 1 value for each channel.
            Assert.AreEqual(2, queryData.Count, "Only rows for index values 4,5,9,10,14 and 15 should be returned.");
            Assert.AreEqual("2012-07-26T15:17:30.0000000+00:00", queryData[0].Split(',')[0],
                "The first data row should be for index value 2012-07-26T15:17:30.0000000+00:00");
            Assert.AreEqual("2012-07-26T15:17:50.0000000+00:00", queryData[1].Split(',')[0],
                "The last data row should be for index value 2012-07-26T15:17:50.0000000+00:00");

            Assert.IsNotNull(logInfos);

            // Validate the Min and Max of each LogCurveInfo #1
            var minDateTimeIndex0 = logInfos[0].MinDateTimeIndex;
            var maxDateTimeIndex0 = logInfos[0].MaxDateTimeIndex;
            var minDateTimeIndex1 = logInfos[1].MinDateTimeIndex;
            var maxDateTimeIndex1 = logInfos[1].MaxDateTimeIndex;
            var minDateTimeIndex2 = logInfos[2].MinDateTimeIndex;
            var maxDateTimeIndex2 = logInfos[2].MaxDateTimeIndex;

            Assert.IsNotNull(minDateTimeIndex0);
            Assert.IsNotNull(maxDateTimeIndex0);
            Assert.AreEqual(minDateTimeIndex0.Value,
                new Timestamp(DateTimeOffset.Parse(queryData[0].Split(',')[0])));
            Assert.AreEqual(maxDateTimeIndex0.Value,
                new Timestamp(DateTimeOffset.Parse(queryData[1].Split(',')[0])));

            // Validate the Min and Max of each LogCurveInfo #2
            Assert.IsNotNull(minDateTimeIndex1);
            Assert.IsNotNull(maxDateTimeIndex1);
            Assert.AreEqual(minDateTimeIndex1.Value,
                new Timestamp(DateTimeOffset.Parse(queryData[0].Split(',')[0])));
            Assert.AreEqual(maxDateTimeIndex1.Value,
                new Timestamp(DateTimeOffset.Parse(queryData[0].Split(',')[0])));

            // Validate the Min and Max of each LogCurveInfo #3
            Assert.IsNotNull(minDateTimeIndex2);
            Assert.IsNotNull(maxDateTimeIndex2);
            Assert.AreEqual(minDateTimeIndex2.Value,
                new Timestamp(DateTimeOffset.Parse(queryData[1].Split(',')[0])));
            Assert.AreEqual(maxDateTimeIndex2.Value,
                new Timestamp(DateTimeOffset.Parse(queryData[1].Split(',')[0])));
        }
        
        [TestMethod]
        public void Log141DataAdapter_GetFromStore_ReturnElements_DataOnly_Supports_Multiple_Queries()
        {
            var row = 3;
            var response = AddLogWithAction(row, null, hasEmptyChannel: false);
            Assert.AreEqual((short) ErrorCodes.Success, response.Result);

            var uidLog1 = response.SuppMsgOut;

            // Add second log
            var log2 = _devKit.CreateLog(null, _devKit.Name("Log 02"), _log.UidWell, _log.NameWell, _log.UidWellbore,
                _log.NameWellbore);
            _devKit.InitHeader(log2, LogIndexType.datetime);
            log2.LogCurveInfo.Clear();
            log2.LogCurveInfo.Add(_devKit.CreateDoubleLogCurveInfo("TIME", "s"));
            log2.LogCurveInfo.Add(_devKit.CreateDoubleLogCurveInfo("AAA", "m/h"));
            log2.LogCurveInfo.Add(_devKit.CreateDoubleLogCurveInfo("BBB", "gAPI"));
            log2.LogCurveInfo.Add(_devKit.CreateDoubleLogCurveInfo("CCC", "gAPI"));

            var logData = log2.LogData.First();
            logData.Data.Clear();
            logData.MnemonicList = "TIME,AAA,BBB,CCC";
            logData.UnitList = "s,m/h,gAPI,gAPI";
            logData.Data.Add("2012-07-26T15:17:20.0000000+00:00,1,0,1");
            logData.Data.Add("2012-07-26T15:17:30.0000000+00:00,2,1,2");
            logData.Data.Add("2012-07-26T15:17:40.0000000+00:00,3,2,3");
            logData.Data.Add("2012-07-26T15:17:50.0000000+00:00,4,3,4");

            response = _devKit.Add<LogList, Log>(log2);
            Assert.AreEqual((short) ErrorCodes.Success, response.Result);

            var uidLog2 = response.SuppMsgOut;

            // Query
            var query1 = _devKit.CreateLog(uidLog1, null, log2.UidWell, null, log2.UidWellbore, null);
            var query2 = _devKit.CreateLog(uidLog2, null, log2.UidWell, null, log2.UidWellbore, null);

            var result = _devKit.Get<LogList, Log>(_devKit.List(query1, query2), ObjectTypes.Log, null,
                OptionsIn.ReturnElements.DataOnly);
            Assert.AreEqual((short) ErrorCodes.Success, result.Result);

            var logList = EnergisticsConverter.XmlToObject<LogList>(result.XMLout);
            Assert.AreEqual(2, logList.Log.Count);

            // Query result of Depth log
            var mnemonicList1 = logList.Log[0].LogData[0].MnemonicList.Split(',');
            Assert.AreEqual(3, mnemonicList1.Length);
            Assert.IsTrue(!mnemonicList1.Except(new List<string>() {"MD", "ROP", "GR"}).Any());
            Assert.AreEqual(3, logList.Log[0].LogData[0].Data.Count);

            // Query result of Time log
            var mnemonicList2 = logList.Log[1].LogData[0].MnemonicList.Split(',');
            Assert.AreEqual(4, mnemonicList2.Length);
            Assert.IsFalse(mnemonicList2.Except(new List<string>() {"TIME", "AAA", "BBB", "CCC"}).Any());
            Assert.AreEqual(4, logList.Log[1].LogData[0].Data.Count);
        }

        [TestMethod]
        public void Log141DataAdapter_GetFromStore_ReturnElements_Requested_Supports_Multiple_Queries()
        {
            var row = 3;
            var response = AddLogWithAction(row, null, hasEmptyChannel: false);
            Assert.AreEqual((short) ErrorCodes.Success, response.Result);

            var uidLog1 = response.SuppMsgOut;

            // Add second log
            var log2 = _devKit.CreateLog(null, _devKit.Name("Log 02"), _log.UidWell, _log.NameWell, _log.UidWellbore,
                _log.NameWellbore);
            _devKit.InitHeader(log2, LogIndexType.datetime);
            log2.LogCurveInfo.Clear();
            log2.LogCurveInfo.Add(_devKit.CreateDoubleLogCurveInfo("TIME", "s"));
            log2.LogCurveInfo.Add(_devKit.CreateDoubleLogCurveInfo("AAA", "m/h"));
            log2.LogCurveInfo.Add(_devKit.CreateDoubleLogCurveInfo("BBB", "gAPI"));
            log2.LogCurveInfo.Add(_devKit.CreateDoubleLogCurveInfo("CCC", "gAPI"));

            var logData = log2.LogData.First();
            logData.Data.Clear();
            logData.MnemonicList = "TIME,AAA,BBB,CCC";
            logData.UnitList = "s,m/h,gAPI,gAPI";
            logData.Data.Add("2012-07-26T15:17:20.0000000+00:00,1,0,1");
            logData.Data.Add("2012-07-26T15:17:30.0000000+00:00,2,1,2");
            logData.Data.Add("2012-07-26T15:17:40.0000000+00:00,3,2,3");
            logData.Data.Add("2012-07-26T15:17:50.0000000+00:00,4,3,4");

            response = _devKit.Add<LogList, Log>(log2);
            Assert.AreEqual((short) ErrorCodes.Success, response.Result);

            var uidLog2 = response.SuppMsgOut;

            // Query
            var query1 = _devKit.CreateLog(uidLog1, null, log2.UidWell, null, log2.UidWellbore, null);
            query1.LogCurveInfo = new List<LogCurveInfo>();
            query1.LogCurveInfo.Add(new LogCurveInfo() {Uid = "MD", Mnemonic = new ShortNameStruct("MD")});
            query1.LogCurveInfo.Add(new LogCurveInfo() {Uid = "ROP", Mnemonic = new ShortNameStruct("ROP")});
            query1.LogCurveInfo.Add(new LogCurveInfo() {Uid = "GR", Mnemonic = new ShortNameStruct("GR")});
            query1.LogData = new List<LogData>() {new LogData()};
            query1.LogData.First().MnemonicList = "MD,ROP,GR";

            var query2 = _devKit.CreateLog(uidLog2, null, log2.UidWell, null, log2.UidWellbore, null);
            query2.LogCurveInfo = new List<LogCurveInfo>();
            query2.LogCurveInfo.Add(new LogCurveInfo() {Uid = "TIME", Mnemonic = new ShortNameStruct("TIME")});
            query2.LogCurveInfo.Add(new LogCurveInfo() {Uid = "AAA", Mnemonic = new ShortNameStruct("AAA")});
            query2.LogCurveInfo.Add(new LogCurveInfo() {Uid = "BBB", Mnemonic = new ShortNameStruct("BBB")});
            query2.LogCurveInfo.Add(new LogCurveInfo() {Uid = "CCC", Mnemonic = new ShortNameStruct("CCC")});
            query2.LogData = new List<LogData>() {new LogData()};
            query2.LogData.First().MnemonicList = "TIME,AAA,BBB,CCC";

            var list = _devKit.New<LogList>(x => x.Log = _devKit.List(query1, query2));
            var queryIn = WitsmlParser.ToXml(list);
            var result = _devKit.GetFromStore(ObjectTypes.Log, queryIn, null, "returnElements=requested");
            Assert.AreEqual((short) ErrorCodes.Success, result.Result);

            var logList = EnergisticsConverter.XmlToObject<LogList>(result.XMLout);
            Assert.AreEqual(2, logList.Log.Count);

            // Query result of Depth log
            var mnemonicList1 = logList.Log[0].LogData[0].MnemonicList.Split(',');
            Assert.AreEqual(3, mnemonicList1.Length);
            Assert.IsTrue(!mnemonicList1.Except(new List<string>() {"MD", "ROP", "GR"}).Any());
            Assert.AreEqual(3, logList.Log[0].LogData[0].Data.Count);

            // Query result of Time log
            var mnemonicList2 = logList.Log[1].LogData[0].MnemonicList.Split(',');
            Assert.AreEqual(4, mnemonicList2.Length);
            Assert.IsFalse(mnemonicList2.Except(new List<string>() {"TIME", "AAA", "BBB", "CCC"}).Any());
            Assert.AreEqual(4, logList.Log[1].LogData[0].Data.Count);
        }

        [TestMethod]
        public void Log141DataAdapter_GetFromStore_Supports_NaN_In_Numeric_Fields()
        {
            var row = 3;
            var response = AddLogWithAction(row, () =>
            {
                _log.BhaRunNumber = 123;
                _log.LogCurveInfo[0].ClassIndex = 1;
                _log.LogCurveInfo[1].ClassIndex = 2;
            });
            Assert.AreEqual((short) ErrorCodes.Success, response.Result);

            // Query log
            var queryIn = "<?xml version=\"1.0\"?>" + Environment.NewLine +
                          "<logs version=\"1.4.1.1\" xmlns=\"http://www.witsml.org/schemas/1series\">" +
                          Environment.NewLine +
                          "<log uid=\"" + _log.Uid + "\" uidWell=\"" + _log.UidWell + "\" uidWellbore=\"" +
                          _log.UidWellbore + "\">" + Environment.NewLine +
                          "<bhaRunNumber>NaN</bhaRunNumber>" + Environment.NewLine +
                          "<logCurveInfo uid=\"MD\">" + Environment.NewLine +
                          "  <mnemonic>MD</mnemonic>" + Environment.NewLine +
                          "  <classIndex>NaN</classIndex>" + Environment.NewLine +
                          "</logCurveInfo>" + Environment.NewLine +
                          "<logCurveInfo uid=\"ROP\">" + Environment.NewLine +
                          "  <mnemonic>ROP</mnemonic>" + Environment.NewLine +
                          "  <classIndex>NaN</classIndex>" + Environment.NewLine +
                          "</logCurveInfo>" + Environment.NewLine +
                          "</log>" + Environment.NewLine +
                          "</logs>";

            var results = _devKit.GetFromStore(ObjectTypes.Log, queryIn, null, "returnElements=requested");
            Assert.AreEqual((short) ErrorCodes.Success, results.Result);

            var logList = EnergisticsConverter.XmlToObject<LogList>(results.XMLout);
            Assert.AreEqual(1, logList.Log.Count);

            Assert.AreEqual((short) 123, logList.Log.First().BhaRunNumber);
            Assert.AreEqual(2, logList.Log.First().LogCurveInfo.Count);
            Assert.AreEqual((short) 1, logList.Log.First().LogCurveInfo[0].ClassIndex);
            Assert.AreEqual((short) 2, logList.Log.First().LogCurveInfo[1].ClassIndex);
        }


        [TestMethod]
        public void Log141DataAdapter_GetFromStore_With_Start_And_End_Index_On_Increasing_Depth_Log_Data_In_Different_Chunk()
        {
            // Set the depth range chunk size.
            WitsmlSettings.DepthRangeSize = 1000;

            _devKit.Add<WellList, Well>(_well);
            _devKit.Add<WellboreList, Wellbore>(_wellbore);

            _log.LogData = _devKit.List(new LogData() { Data = _devKit.List<string>() });
            var logData = _log.LogData.First();
            logData.Data.Add("1700.0,17.1,17.2");
            logData.Data.Add("1800.0,18.1,18.2");
            logData.Data.Add("1900.0,19.1,19.2");
            logData.Data.Add("2700.0,27.1,27.2");
            logData.Data.Add("2800.0,28.1,28.2");
            logData.Data.Add("2900.0,29.1,29.2");

            _devKit.InitHeader(_log, LogIndexType.measureddepth);

            var response = _devKit.Add<LogList, Log>(_log);
            Assert.AreEqual((short) ErrorCodes.Success, response.Result);

            var uidLog = response.SuppMsgOut;

            // Query
            var query = _devKit.CreateLog(uidLog, null, _log.UidWell, null, _log.UidWellbore, null);
            query.StartIndex = new GenericMeasure(1800, "ft");
            query.EndIndex = new GenericMeasure(2700, "ft");

            var result = _devKit.Get<LogList, Log>(_devKit.List(query), ObjectTypes.Log, null,
                OptionsIn.ReturnElements.DataOnly);
            Assert.AreEqual((short) ErrorCodes.Success, result.Result);

            var logList = EnergisticsConverter.XmlToObject<LogList>(result.XMLout);
            var resultLog = logList.Log;
            Assert.AreEqual(1, resultLog.Count);
            Assert.AreEqual(3, resultLog[0].LogData[0].Data.Count);
            Assert.AreEqual(1800, Convert.ToDouble(resultLog[0].LogData[0].Data[0].Split(',').First()));
            Assert.AreEqual(1900, Convert.ToDouble(resultLog[0].LogData[0].Data[1].Split(',').First()));
            Assert.AreEqual(2700, Convert.ToDouble(resultLog[0].LogData[0].Data[2].Split(',').First()));
        }

        [TestMethod]
        public void Log141DataAdapter_GetFromStore_With_Start_And_End_Index_On_decreasing_Depth_Log_Data_In_Different_Chunk()
        {
            // Set the depth range chunk size.
            WitsmlSettings.DepthRangeSize = 1000;

            _devKit.Add<WellList, Well>(_well);
            _devKit.Add<WellboreList, Wellbore>(_wellbore);

            _log.LogData = _devKit.List(new LogData() {Data = _devKit.List<string>()});
            var logData = _log.LogData.First();
            logData.Data.Add("2300.0,23.1,23.2");
            logData.Data.Add("2200.0,22.1,22.2");
            logData.Data.Add("2100.0,21.1,21.2");
            logData.Data.Add("2000.0,20.1,20.2");
            logData.Data.Add("1900.0,19.1,19.2");
            logData.Data.Add("1800.0,18.1,18.2");

            _devKit.InitHeader(_log, LogIndexType.measureddepth, false);

            var response = _devKit.Add<LogList, Log>(_log);
            Assert.AreEqual((short) ErrorCodes.Success, response.Result);

            var uidLog = response.SuppMsgOut;

            // Query
            var query = _devKit.CreateLog(uidLog, null, _log.UidWell, null, _log.UidWellbore, null);
            query.StartIndex = new GenericMeasure(2200, "ft");
            query.EndIndex = new GenericMeasure(1800, "ft");

            var result = _devKit.Get<LogList, Log>(_devKit.List(query), ObjectTypes.Log, null,
                OptionsIn.ReturnElements.DataOnly);
            Assert.AreEqual((short) ErrorCodes.Success, result.Result);

            var logList = EnergisticsConverter.XmlToObject<LogList>(result.XMLout);
            var resultLog = logList.Log;
            Assert.AreEqual(1, resultLog.Count);
            Assert.AreEqual(5, resultLog[0].LogData[0].Data.Count);
            Assert.AreEqual(2200, Convert.ToDouble(resultLog[0].LogData[0].Data[0].Split(',').First()));
            Assert.AreEqual(2100, Convert.ToDouble(resultLog[0].LogData[0].Data[1].Split(',').First()));
            Assert.AreEqual(2000, Convert.ToDouble(resultLog[0].LogData[0].Data[2].Split(',').First()));
            Assert.AreEqual(1900, Convert.ToDouble(resultLog[0].LogData[0].Data[3].Split(',').First()));
            Assert.AreEqual(1800, Convert.ToDouble(resultLog[0].LogData[0].Data[4].Split(',').First()));
        }

        [TestMethod]
        public void Log141DataAdapter_GetFromStore_With_Start_And_End_Index_On_Channel_With_Null_Values()
        {
            _devKit.Add<WellList, Well>(_well);
            _devKit.Add<WellboreList, Wellbore>(_wellbore);

            _devKit.InitHeader(_log, LogIndexType.measureddepth);

            _log.LogCurveInfo.Clear();
            _log.LogCurveInfo.Add(_devKit.CreateDoubleLogCurveInfo("MD", "ft"));
            _log.LogCurveInfo.Add(_devKit.CreateDoubleLogCurveInfo("AAA", "m/h"));
            _log.LogCurveInfo.Add(_devKit.CreateDoubleLogCurveInfo("BBB", "gAPI"));
            _log.LogCurveInfo.Add(_devKit.CreateDoubleLogCurveInfo("CCC", "gAPI"));
            _log.LogCurveInfo.Add(_devKit.CreateDoubleLogCurveInfo("DDD", "s"));

            _log.LogData = _devKit.List(new LogData() {Data = _devKit.List<string>()});
            var logData = _log.LogData.First();
            logData.Data.Clear();
            logData.MnemonicList = "MD,AAA,BBB,CCC,DDD";
            logData.UnitList = "ft,m/h,gAPI,gAPI,s";
            logData.Data.Add("1700.0, 17.1, 17.2, null, 17.4");
            logData.Data.Add("1800.0, 18.1, 18.2, null, 18.4");
            logData.Data.Add("1900.0, 19.1, 19.2, null, 19.4");
            logData.Data.Add("2000.0, 20.1, 20.2, null, 20.4");
            logData.Data.Add("2100.0, 21.1, 21.2, null, 21.4");
            logData.Data.Add("2200.0, 22.1, 22.2, null, 22.4");
            logData.Data.Add("2300.0, 23.1, 23.2, 23.3, 23.4");

            var response = _devKit.Add<LogList, Log>(_log);
            Assert.AreEqual((short) ErrorCodes.Success, response.Result);

            var uidLog = response.SuppMsgOut;

            // Query
            var query = _devKit.CreateLog(uidLog, null, _log.UidWell, null, _log.UidWellbore, null);
            query.StartIndex = new GenericMeasure(1800, "ft");
            query.EndIndex = new GenericMeasure(2200, "ft");

            var result = _devKit.Get<LogList, Log>(_devKit.List(query), ObjectTypes.Log, null,
                OptionsIn.ReturnElements.All);
            Assert.AreEqual((short) ErrorCodes.Success, result.Result);

            var logList = EnergisticsConverter.XmlToObject<LogList>(result.XMLout);
            var resultLog = logList.Log;
            Assert.AreEqual(1, resultLog.Count);

            // Result log header
            var logCurveInfoList = resultLog[0].LogCurveInfo;
            Assert.AreEqual(4, logCurveInfoList.Count());
            var lciMnemonics = logCurveInfoList.Select(x => x.Mnemonic.Value).ToArray();
            Assert.IsFalse(lciMnemonics.Except(new List<string>() {"MD", "AAA", "BBB", "DDD"}).Any());

            // Result log data
            var mnemonicList = resultLog[0].LogData[0].MnemonicList.Split(',');
            Assert.AreEqual(4, mnemonicList.Count());
            Assert.IsFalse(mnemonicList.Except(new List<string>() {"MD", "AAA", "BBB", "DDD"}).Any());
            var unitList = resultLog[0].LogData[0].UnitList.Split(',');
            Assert.AreEqual(4, unitList.Count());
            Assert.IsFalse(unitList.Except(new List<string>() {"ft", "m/h", "gAPI", "s"}).Any());

            var data = resultLog[0].LogData[0].Data;
            Assert.AreEqual(5, data.Count);

            double value = 18;
            foreach (string r in data)
            {
                var row = r.Split(',');
                Assert.AreEqual(4, row.Count());
                Assert.AreEqual(value*100, Convert.ToDouble(row[0]));
                Assert.AreEqual(value + 0.1, Convert.ToDouble(row[1]));
                Assert.AreEqual(value + 0.2, Convert.ToDouble(row[2]));
                Assert.AreEqual(value + 0.4, Convert.ToDouble(row[3]));
                value++;
            }
        }

        [TestMethod]
        public void Log141DataAdapter_GetFromStore_Can_Slice_On_Channel_On_Range_Of_Null_Indicator_Values_In_Different_Chunks()
        {
            // Set the depth range chunk size.
            WitsmlSettings.DepthRangeSize = 1000;

            _devKit.Add<WellList, Well>(_well);
            _devKit.Add<WellboreList, Wellbore>(_wellbore);

            _devKit.InitHeader(_log, LogIndexType.measureddepth);

            _log.NullValue = "-999.25";

            _log.LogCurveInfo.Clear();
            _log.LogCurveInfo.Add(_devKit.CreateDoubleLogCurveInfo("MD", "ft"));
            _log.LogCurveInfo.Add(_devKit.CreateDoubleLogCurveInfo("AAA", "m/h"));
            _log.LogCurveInfo.Add(_devKit.CreateDoubleLogCurveInfo("BBB", "gAPI"));
            _log.LogCurveInfo.Add(_devKit.CreateDoubleLogCurveInfo("CCC", "gAPI"));
            _log.LogCurveInfo.Add(_devKit.CreateDoubleLogCurveInfo("DDD", "s"));

            _log.LogData = _devKit.List(new LogData() {Data = _devKit.List<string>()});
            var logData = _log.LogData.First();
            logData.Data.Clear();
            logData.MnemonicList = "MD,AAA,BBB,CCC,DDD";
            logData.UnitList = "ft,m/h,gAPI,gAPI,s";
            logData.Data.Add("1700.0, 17.1, 17.2, -999.25, 17.4");
            logData.Data.Add("1800.0, 18.1, 18.2, -999.25, 18.4");
            logData.Data.Add("1900.0, 19.1, 19.2, -999.25, 19.4");
            logData.Data.Add("2000.0, 20.1, 20.2, -999.25, 20.4");
            logData.Data.Add("2100.0, 21.1, 21.2, -999.25, 21.4");
            logData.Data.Add("2200.0, 22.1, 22.2, -999.25, 22.4");
            logData.Data.Add("2300.0, 23.1, 23.2,    23.3, 23.4");

            var response = _devKit.Add<LogList, Log>(_log);
            Assert.AreEqual((short) ErrorCodes.Success, response.Result);

            var uidLog = response.SuppMsgOut;

            // Query
            var query = _devKit.CreateLog(uidLog, null, _log.UidWell, null, _log.UidWellbore, null);
            query.StartIndex = new GenericMeasure(1800, "ft");
            query.EndIndex = new GenericMeasure(2200, "ft");

            var result = _devKit.Get<LogList, Log>(_devKit.List(query), ObjectTypes.Log, null,
                OptionsIn.ReturnElements.All);
            Assert.AreEqual((short) ErrorCodes.Success, result.Result);

            var logList = EnergisticsConverter.XmlToObject<LogList>(result.XMLout);
            var resultLog = logList.Log;
            Assert.AreEqual(1, resultLog.Count);

            // Result log header
            var logCurveInfoList = resultLog[0].LogCurveInfo;
            Assert.AreEqual(4, logCurveInfoList.Count());
            var lciMnemonics = logCurveInfoList.Select(x => x.Mnemonic.Value).ToArray();
            Assert.IsFalse(lciMnemonics.Except(new List<string>() {"MD", "AAA", "BBB", "DDD"}).Any());

            // Result log data
            var mnemonicList = resultLog[0].LogData[0].MnemonicList.Split(',');
            Assert.AreEqual(4, mnemonicList.Count());
            Assert.IsFalse(mnemonicList.Except(new List<string>() {"MD", "AAA", "BBB", "DDD"}).Any());
            var unitList = resultLog[0].LogData[0].UnitList.Split(',');
            Assert.AreEqual(4, unitList.Count());
            Assert.IsFalse(unitList.Except(new List<string>() {"ft", "m/h", "gAPI", "s"}).Any());

            var data = resultLog[0].LogData[0].Data;
            Assert.AreEqual(5, data.Count);

            double value = 18;
            foreach (string s in data)
            {
                var row = s.Split(',');
                Assert.AreEqual(4, row.Count());
                Assert.AreEqual(value*100, Convert.ToDouble(row[0]));
                Assert.AreEqual(value + 0.1, Convert.ToDouble(row[1]));
                Assert.AreEqual(value + 0.2, Convert.ToDouble(row[2]));
                Assert.AreEqual(value + 0.4, Convert.ToDouble(row[3]));
                value++;
            }
        }

        [TestMethod]
        public void Log141DataAdapter_GetFromStore_Can_Calculate_Channels_Range_With_Different_Null_Indicators_In_Different_Chunks()
        {
            // Set the depth range chunk size.
            WitsmlSettings.DepthRangeSize = 1000;

            _devKit.Add<WellList, Well>(_well);
            _devKit.Add<WellboreList, Wellbore>(_wellbore);

            _devKit.InitHeader(_log, LogIndexType.measureddepth);

            _log.NullValue = "-999.25";

            _log.LogCurveInfo.Clear();
            _log.LogCurveInfo.Add(_devKit.CreateDoubleLogCurveInfo("MD", "ft"));
            _log.LogCurveInfo.Add(_devKit.CreateDoubleLogCurveInfo("AAA", "m/h"));
            _log.LogCurveInfo.Add(_devKit.CreateDoubleLogCurveInfo("BBB", "gAPI"));
            _log.LogCurveInfo.Add(_devKit.CreateDoubleLogCurveInfo("CCC", "gAPI"));
            _log.LogCurveInfo.Add(_devKit.CreateDoubleLogCurveInfo("DDD", "s"));

            // Set channels null value except channel "CCC"
            _log.LogCurveInfo[1].NullValue = "-1111.1";
            _log.LogCurveInfo[2].NullValue = "-2222.2";
            _log.LogCurveInfo[4].NullValue = "-4444.4";

            _log.LogData = _devKit.List(new LogData() {Data = _devKit.List<string>()});
            var logData = _log.LogData.First();
            logData.Data.Clear();
            logData.MnemonicList = "MD,AAA,BBB,CCC,DDD";
            logData.UnitList = "ft,m/h,gAPI,gAPI,s";
            logData.Data.Add("1700.0, -1111.1,    17.2, -999.25, -4444.4");
            logData.Data.Add("1800.0,    18.1,    18.2, -999.25, -4444.4");
            logData.Data.Add("1900.0,    19.1,    19.2, -999.25,    19.4");
            logData.Data.Add("2000.0,    20.1,    20.2, -999.25,    20.4");
            logData.Data.Add("2100.0,    21.1,    21.2, -999.25,    21.4");
            logData.Data.Add("2200.0,    22.1,    22.2, -999.25, -4444.4");
            logData.Data.Add("2300.0,    23.1, -2222.2,    23.3, -4444.4");

            var response = _devKit.Add<LogList, Log>(_log);
            Assert.AreEqual((short) ErrorCodes.Success, response.Result);

            var uidLog = response.SuppMsgOut;

            // Query
            var query = _devKit.CreateLog(uidLog, null, _log.UidWell, null, _log.UidWellbore, null);
            query.StartIndex = new GenericMeasure(1.0, "ft");
            query.EndIndex = new GenericMeasure(2.0, "ft");

            var result = _devKit.Get<LogList, Log>(_devKit.List(query), ObjectTypes.Log, null,
                OptionsIn.ReturnElements.All);
            Assert.AreEqual((short) ErrorCodes.Success, result.Result);

            var logList = EnergisticsConverter.XmlToObject<LogList>(result.XMLout);
            var resultLog = logList.Log;
            Assert.AreEqual(1, resultLog.Count);

            // Result log header
            var logCurveInfoList = resultLog[0].LogCurveInfo;
            Assert.AreEqual(5, logCurveInfoList.Count());
            var lciMnemonics = logCurveInfoList.Select(x => x.Mnemonic.Value).ToArray();
            Assert.IsFalse(lciMnemonics.Except(new List<string>() {"MD", "AAA", "BBB", "CCC", "DDD"}).Any());
            Assert.AreEqual(1800.0, logCurveInfoList[1].MinIndex.Value);
            Assert.AreEqual(2300.0, logCurveInfoList[1].MaxIndex.Value);
            Assert.AreEqual(1700.0, logCurveInfoList[2].MinIndex.Value);
            Assert.AreEqual(2200.0, logCurveInfoList[2].MaxIndex.Value);
            Assert.AreEqual(2300.0, logCurveInfoList[3].MinIndex.Value);
            Assert.AreEqual(2300.0, logCurveInfoList[3].MaxIndex.Value);
            Assert.AreEqual(1900.0, logCurveInfoList[4].MinIndex.Value);
            Assert.AreEqual(2100.0, logCurveInfoList[4].MaxIndex.Value);
        }

        [TestMethod]
        public void Log141DataAdapter_GetFromStore_With_Null_Indicator_Empty_Row_Should_Not_Be_Returned()
        {
            _devKit.Add<WellList, Well>(_well);
            _devKit.Add<WellboreList, Wellbore>(_wellbore);

            _devKit.InitHeader(_log, LogIndexType.measureddepth);
            _log.NullValue = "-999.25";
            _log.LogCurveInfo.Clear();
            _log.LogCurveInfo.Add(_devKit.CreateDoubleLogCurveInfo("MD", "ft"));
            _log.LogCurveInfo.Add(_devKit.CreateDoubleLogCurveInfo("AAA", "m/h"));
            _log.LogCurveInfo.Add(_devKit.CreateDoubleLogCurveInfo("BBB", "gAPI"));
            _log.LogCurveInfo.Add(_devKit.CreateDoubleLogCurveInfo("CCC", "gAPI"));

            _log.LogData = _devKit.List(new LogData() {Data = _devKit.List<string>()});
            var logData = _log.LogData.First();
            logData.Data.Clear();
            logData.MnemonicList = "MD,AAA,BBB,CCC";
            logData.UnitList = "ft,m/h,gAPI,gAPI";
            logData.Data.Add("1700.0, 17.1, 17.2, -999.25");
            logData.Data.Add("1800.0, 18.1, 18.2, -999.25");
            logData.Data.Add("1900.0, -999.25, -999.25, -999.25");
            logData.Data.Add("2000.0, 20.1, 20.2, -999.25");

            var response = _devKit.Add<LogList, Log>(_log);
            Assert.AreEqual((short) ErrorCodes.Success, response.Result);

            var uidLog = response.SuppMsgOut;

            // Query
            var query = _devKit.CreateLog(uidLog, null, _log.UidWell, null, _log.UidWellbore, null);

            var result = _devKit.Get<LogList, Log>(_devKit.List(query), ObjectTypes.Log, null,
                OptionsIn.ReturnElements.All);
            Assert.AreEqual((short) ErrorCodes.Success, result.Result);

            var logList = EnergisticsConverter.XmlToObject<LogList>(result.XMLout);
            var resultLog = logList.Log;
            Assert.AreEqual(1, resultLog.Count);

            // Result log header
            var logCurveInfoList = resultLog[0].LogCurveInfo;
            Assert.AreEqual(3, logCurveInfoList.Count());
            var lciMnemonics = logCurveInfoList.Select(x => x.Mnemonic.Value).ToArray();
            Assert.IsFalse(lciMnemonics.Except(new List<string>() {"MD", "AAA", "BBB", "CCC"}).Any());

            // Result log data
            var mnemonicList = resultLog[0].LogData[0].MnemonicList.Split(',');
            Assert.AreEqual(3, mnemonicList.Count());
            Assert.IsFalse(mnemonicList.Except(new List<string>() {"MD", "AAA", "BBB"}).Any());
            var unitList = resultLog[0].LogData[0].UnitList.Split(',');
            Assert.AreEqual(3, unitList.Count());
            Assert.IsFalse(unitList.Except(new List<string>() {"ft", "m/h", "gAPI"}).Any());

            var data = resultLog[0].LogData[0].Data;
            Assert.AreEqual(3, data.Count);

            Assert.IsTrue(data[0].Equals("1700,17.1,17.2"));
            Assert.IsTrue(data[1].Equals("1800,18.1,18.2"));
            Assert.IsTrue(data[2].Equals("2000,20.1,20.2"));
        }

        [TestMethod]
        public void Log141DataAdapter_GetFromStore_MaxReturnNodes_OptionsIn()
        {
            var numRows = 20;
            var maxReturnNodes = 5;

            // Add the Setup Well, Wellbore and Log to the store.
            WMLS_AddToStoreResponse logResponse =
                AddSetupWellWellboreLog(numRows, isDepthLog: true, hasEmptyChannel: false, increasing: true);

            // Assert that the log was added successfully
            Assert.AreEqual((short) ErrorCodes.Success, logResponse.Result);

            var query = new Log
            {
                Uid = _log.Uid,
                UidWell = _log.UidWell,
                UidWellbore = _log.UidWellbore,
            };

            short errorCode;
            var result = _devKit.QueryWithErrorCode<LogList, Log>(
                query, out errorCode,
                ObjectTypes.Log, null, OptionsIn.ReturnElements.All + ';' +
                                       OptionsIn.MaxReturnNodes.Eq(maxReturnNodes));

            Assert.AreEqual((short) ErrorCodes.ParialSuccess, errorCode, "Error code should indicate partial success");

            // Verify that a Log was returned with the results
            Assert.IsNotNull(result, "No results returned from Log query");

            var queriedLog = result.First();
            Assert.IsNotNull(queriedLog, "No Logs returned in results from Log query");

            // Verify that a LogData element was returned with the results.
            var queryLogData = queriedLog.LogData;
            Assert.IsTrue(queryLogData != null && queryLogData.First() != null,
                "No LogData returned in results from Log query");

            // Verify that data rows were returned with the LogData
            var queryData = queryLogData.First().Data;
            Assert.IsTrue(queryData != null && queryData.Count == maxReturnNodes,
                string.Format("Expected {0} rows returned because MaxReturnNodes = {0}", maxReturnNodes));
        }


        [TestMethod]
        public void Log141DataAdapter_GetFromStore_Slice_Empty_Channel_MaxReturnNodes_OptionsIn()
        {
            var numRows = 20;
            var maxReturnNodes = 5;

            // Add the Setup Well, Wellbore and Log to the store.
            WMLS_AddToStoreResponse logResponse =
                AddSetupWellWellboreLog(numRows, isDepthLog: true, hasEmptyChannel: true, increasing: true);

            // Assert that the log was added successfully
            Assert.AreEqual((short) ErrorCodes.Success, logResponse.Result);

            var query = new Log
            {
                Uid = _log.Uid,
                UidWell = _log.UidWell,
                UidWellbore = _log.UidWellbore,
            };

            short errorCode;
            var result = _devKit.QueryWithErrorCode<LogList, Log>(
                query, out errorCode,
                ObjectTypes.Log, null, OptionsIn.ReturnElements.All + ';' +
                                       OptionsIn.MaxReturnNodes.Eq(maxReturnNodes));

            Assert.AreEqual((short) ErrorCodes.ParialSuccess, errorCode, "Error code should indicate partial success");

            // Verify that a Log was returned with the results
            Assert.IsNotNull(result, "No results returned from Log query");

            var queriedLog = result.First();
            Assert.IsNotNull(queriedLog, "No Logs returned in results from Log query");

            // Test that the column count returned is reduced by one.
            Assert.AreEqual(_log.LogCurveInfo.Count - 1, queriedLog.LogCurveInfo.Count);

            // Verify that a LogData element was returned with the results.
            var queryLogData = queriedLog.LogData;
            Assert.IsTrue(queryLogData != null && queryLogData.First() != null,
                "No LogData returned in results from Log query");

            // Verify that data rows were returned with the LogData
            var queryData = queryLogData.First().Data;
            Assert.IsTrue(queryData != null && queryData.Count == maxReturnNodes,
                string.Format("Expected {0} rows returned because MaxReturnNodes = {0}", maxReturnNodes));
        }

        [TestMethod]
        public void Log141DataAdapter_GetFromStore_Multiple_MaxReturnNodes_OptionsIn()
        {
            var numRows = 20;
            var maxReturnNodes = 5;

            // Add the Setup Well, Wellbore and Log to the store.
            WMLS_AddToStoreResponse log1Response =
                AddSetupWellWellboreLog(numRows, isDepthLog: true, hasEmptyChannel: false, increasing: true);

            Assert.AreEqual((short) ErrorCodes.Success, log1Response.Result);

            // Add a second Log to the same wellbore as Setup log (_log)
            var log2 = _devKit.CreateLog(null, _devKit.Name("Log 02"), _log.UidWell, _well.Name, _log.UidWellbore,
                _wellbore.Name);
            _devKit.InitHeader(log2, LogIndexType.measureddepth);
            _devKit.InitDataMany(log2, _devKit.Mnemonics(log2), _devKit.Units(log2), numRows, hasEmptyChannel: false);

            // Add the 2nd log
            var log2Response = _devKit.Add<LogList, Log>(log2);

            var query1 = _devKit.CreateLog(log1Response.SuppMsgOut, null, _log.UidWell, null, _log.UidWellbore, null);
            var query2 = _devKit.CreateLog(log2Response.SuppMsgOut, null, log2.UidWell, null, log2.UidWellbore, null);

            // Perform a GetFromStore with multiple log queries
            var result = _devKit.Get<LogList, Log>(
                _devKit.List(query1, query2),
                ObjectTypes.Log,
                null,
                OptionsIn.ReturnElements.All + ';' + OptionsIn.MaxReturnNodes.Eq(maxReturnNodes));
            Assert.AreEqual((short) ErrorCodes.ParialSuccess, result.Result);

            var logList = EnergisticsConverter.XmlToObject<LogList>(result.XMLout);
            Assert.AreEqual(2, logList.Items.Count, "Two logs should be returned");

            // Test that each log has maxRetunNodes number of log data rows.
            foreach (var l in logList.Items)
            {
                var log = l as Log;
                Assert.IsNotNull(log);
                Assert.AreEqual(maxReturnNodes, log.LogData[0].Data.Count);
            }
        }

        [TestMethod]
        public void Log141DataAdapter_GetFromStore_Multiple_MaxDataNodes_MaxReturnNodes_OptionsIn()
        {
            var numRows = 20;
            var maxReturnNodes = 5;

            // Add the Setup Well, Wellbore and Log to the store.
            WMLS_AddToStoreResponse log1Response =
                AddSetupWellWellboreLog(numRows, isDepthLog: true, hasEmptyChannel: false, increasing: true);

            Assert.AreEqual((short) ErrorCodes.Success, log1Response.Result);

            // Add a second Log to the same wellbore as Setup log (_log)
            var log2 = _devKit.CreateLog(null, _devKit.Name("Log 02"), _log.UidWell, _well.Name, _log.UidWellbore,
                _wellbore.Name);
            _devKit.InitHeader(log2, LogIndexType.measureddepth);
            _devKit.InitDataMany(log2, _devKit.Mnemonics(log2), _devKit.Units(log2), numRows, hasEmptyChannel: false);

            // Add the 2nd log
            var log2Response = _devKit.Add<LogList, Log>(log2);

            var query1 = _devKit.CreateLog(log1Response.SuppMsgOut, null, _log.UidWell, null, _log.UidWellbore, null);
            var query2 = _devKit.CreateLog(log2Response.SuppMsgOut, null, log2.UidWell, null, log2.UidWellbore, null);

            // This will cap the total response nodes to 8 instead of 10 if this was not specified.
            var previousMaxDataNodes = WitsmlSettings.MaxDataNodes;
            WitsmlSettings.MaxDataNodes = 8;

            try
            {
                // Perform a GetFromStore with multiple log queries
                var result = _devKit.Get<LogList, Log>(
                    _devKit.List(query1, query2),
                    ObjectTypes.Log,
                    null,
                    OptionsIn.ReturnElements.All + ';' + OptionsIn.MaxReturnNodes.Eq(maxReturnNodes));
                Assert.AreEqual((short) ErrorCodes.ParialSuccess, result.Result);

                var logList = EnergisticsConverter.XmlToObject<LogList>(result.XMLout);
                Assert.IsNotNull(logList);
                Assert.IsNotNull(logList.Items);
                Assert.AreEqual(2, logList.Items.Count, "Two logs should be returned");

                // The first log should have maxReturnNodes log data rows
                var log0 = (logList.Items[0] as Log);
                Assert.IsNotNull(log0);
                Assert.AreEqual(maxReturnNodes, log0.LogData[0].Data.Count);

                // Since there is a total cap of 8 rows the last log should have only 3 rows.
                var log1 = (logList.Items[1] as Log);
                Assert.IsNotNull(log1);
                Assert.AreEqual(WitsmlSettings.MaxDataNodes - maxReturnNodes,
                    log1.LogData[0].Data.Count);

                WitsmlSettings.MaxDataNodes = previousMaxDataNodes;
            }
            catch
            {
                WitsmlSettings.MaxDataNodes = previousMaxDataNodes;
            }
        }

        [TestMethod]
        public void Log141DataAdapter_GetFromStore_Returns_Less_Than_MaxDataPoints()
        {
            int maxDataPoints = 10;
            int numRows = 10;

            // Add the Setup Well, Wellbore and Log to the store.
            var logResponse = AddSetupWellWellboreLog(numRows, isDepthLog: true, hasEmptyChannel: false,
                increasing: true);
            Assert.AreEqual((short) ErrorCodes.Success, logResponse.Result);

            var query = _devKit.CreateLog(_log.Uid, null, _log.UidWell, null, _log.UidWellbore, null);

            // Query the log and it returns the whole log data
            short errorCode;
            var result = _devKit.QueryWithErrorCode<LogList, Log>(query, out errorCode, ObjectTypes.Log, null,
                OptionsIn.ReturnElements.All);

            Assert.AreEqual((short) ErrorCodes.Success, errorCode);
            Assert.IsNotNull(result);

            var returnLog = result.First();
            Assert.IsNotNull(returnLog);
            Assert.AreEqual(1, returnLog.LogData.Count);

            var returnDataPoints = returnLog.LogData[0].Data[0].Split(',').Length*returnLog.LogData[0].Data.Count;
            Assert.IsTrue(maxDataPoints < returnDataPoints);

            // Change the MaxDataPoints in Settings to a small number and query the log again
            WitsmlSettings.MaxDataPoints = maxDataPoints;

            result = _devKit.QueryWithErrorCode<LogList, Log>(query, out errorCode, ObjectTypes.Log, null,
                OptionsIn.ReturnElements.All);

            Assert.AreEqual((short) ErrorCodes.ParialSuccess, errorCode, "Returning partial data.");
            Assert.IsNotNull(result);

            returnLog = result.First();
            Assert.IsNotNull(returnLog);
            Assert.AreEqual(1, returnLog.LogData.Count);

            returnDataPoints = returnLog.LogData[0].Data[0].Split(',').Length*returnLog.LogData[0].Data.Count;
            Assert.IsFalse(maxDataPoints < returnDataPoints);
        }

        [TestMethod]
        public void Log141DataAdapter_GetFromStore_Can_Get_Data_With_Empty_MneMonicList_And_ReturnElement_DataOnly()
        {
            // Add well
            var response = _devKit.Add<WellList, Well>(_well);
            Assert.AreEqual((short)ErrorCodes.Success, response.Result);

            // Add wellbore
            response = _devKit.Add<WellboreList, Wellbore>(_wellbore);
            Assert.AreEqual((short)ErrorCodes.Success, response.Result);

            // Add log
            _devKit.InitHeader(_log, LogIndexType.measureddepth);
            _devKit.InitDataMany(_log, _devKit.Mnemonics(_log), _devKit.Units(_log), 3);

            response = _devKit.Add<LogList, Log>(_log);
            Assert.AreEqual((short)ErrorCodes.Success, response.Result);

            // Query log
            var queryIn = "<?xml version=\"1.0\"?>" + Environment.NewLine +
                "<logs version=\"1.4.1.1\" xmlns=\"http://www.witsml.org/schemas/1series\">" + Environment.NewLine +
                    "<log uid=\"" + _log.Uid + "\" uidWell=\"" + _log.UidWell + "\" uidWellbore=\"" + _log.UidWellbore + "\">" + Environment.NewLine +
                        "<logData>" + Environment.NewLine +
                        "  <mnemonicList/>" + Environment.NewLine +
                        "</logData>" + Environment.NewLine +
                    "</log>" + Environment.NewLine +
               "</logs>";

            var results = _devKit.GetFromStore(ObjectTypes.Log, queryIn, null, "returnElements=data-only");
            Assert.AreEqual((short)ErrorCodes.Success, results.Result);

            var logList = EnergisticsConverter.XmlToObject<LogList>(results.XMLout);
            Assert.AreEqual(1, logList.Log.Count);
            Assert.AreEqual(1, logList.Log[0].LogData.Count);
            Assert.AreEqual(3, logList.Log[0].LogData[0].Data.Count);           
        }

        [TestMethod]
        public void Log141DataAdapter_GetFromStore_Can_Get_Data_With_Empty_MneMonicList_And_ReturnElement_Requested()
        {
            // Add well
            var response = _devKit.Add<WellList, Well>(_well);
            Assert.AreEqual((short)ErrorCodes.Success, response.Result);

            // Add wellbore
            response = _devKit.Add<WellboreList, Wellbore>(_wellbore);
            Assert.AreEqual((short)ErrorCodes.Success, response.Result);

            // Add log
            _devKit.InitHeader(_log, LogIndexType.measureddepth);
            _devKit.InitDataMany(_log, _devKit.Mnemonics(_log), _devKit.Units(_log), 3);

            response = _devKit.Add<LogList, Log>(_log);
            Assert.AreEqual((short)ErrorCodes.Success, response.Result);

            // Query log
            var queryIn = "<?xml version=\"1.0\"?>" + Environment.NewLine +
                "<logs version=\"1.4.1.1\" xmlns=\"http://www.witsml.org/schemas/1series\">" + Environment.NewLine +
                    "<log uid=\"" + _log.Uid + "\" uidWell=\"" + _log.UidWell + "\" uidWellbore=\"" + _log.UidWellbore + "\">" + Environment.NewLine +
                        "<logData>" + Environment.NewLine +
                        "  <mnemonicList/>" + Environment.NewLine +
                        "</logData>" + Environment.NewLine +
                    "</log>" + Environment.NewLine +
               "</logs>";

            var results = _devKit.GetFromStore(ObjectTypes.Log, queryIn, null, "returnElements=requested");
            Assert.AreEqual((short)ErrorCodes.Success, results.Result);

            var logList = EnergisticsConverter.XmlToObject<LogList>(results.XMLout);
            Assert.AreEqual(1, logList.Log.Count);
            Assert.AreEqual(1, logList.Log[0].LogData.Count);
            Assert.AreEqual(3, logList.Log[0].LogData[0].Data.Count);
        }

        [TestMethod]
        public void Log141DataAdapter_GetFromStore_Header_Only_No_Data()
        {
            int numRows = 10;

            // Add the Setup Well, Wellbore and Log to the store.
            var logResponse = AddSetupWellWellboreLog(numRows, isDepthLog: true, hasEmptyChannel: false,
                increasing: true);
            Assert.AreEqual((short)ErrorCodes.Success, logResponse.Result);

            var queryHeaderOnly = _devKit.CreateLog(logResponse.SuppMsgOut, null, _log.UidWell, null, _log.UidWellbore, null);

            // Perform a GetFromStore with multiple log queries
            var result = _devKit.Get<LogList, Log>(
                _devKit.List(queryHeaderOnly),
                ObjectTypes.Log,
                null,
                OptionsIn.ReturnElements.HeaderOnly);

            Assert.AreEqual((short)ErrorCodes.Success, result.Result);

            var logList = EnergisticsConverter.XmlToObject<LogList>(result.XMLout);
            Assert.IsNotNull(logList);
            Assert.AreEqual(1, logList.Log.Count);
            Assert.AreEqual(0, logList.Log[0].LogData.Count);
        }

        [TestMethod]
        public void Log141DataAdapter_GetFromStore_Header_Only_Query_As_Requested()
        {
            int numRows = 10;

            // Add the Setup Well, Wellbore and Log to the store.
            var logResponse = AddSetupWellWellboreLog(numRows, isDepthLog: true, hasEmptyChannel: false,
                increasing: true);
            Assert.AreEqual((short)ErrorCodes.Success, logResponse.Result);
            var queryIn = "<logs version=\"1.4.1.1\" xmlns=\"http://www.witsml.org/schemas/1series\">" + Environment.NewLine +
                            $"  <log uidWell=\"{_log.UidWell}\" uidWellbore=\"{_log.UidWellbore}\" uid=\"{logResponse.SuppMsgOut}\">" + Environment.NewLine +
                            "    <nameWell />" + Environment.NewLine +
                            "    <nameWellbore />" + Environment.NewLine +
                            "    <name />" + Environment.NewLine +
                            "    <objectGrowing />" + Environment.NewLine +
                            "    <serviceCompany />" + Environment.NewLine +
                            "    <runNumber />" + Environment.NewLine +
                            "    <creationDate />" + Environment.NewLine +
                            "    <indexType />" + Environment.NewLine +
                            "    <startIndex uom=\"\" />" + Environment.NewLine +
                            "    <endIndex uom=\"\" />" + Environment.NewLine +
                            "    <startDateTimeIndex />" + Environment.NewLine +
                            "    <endDateTimeIndex />" + Environment.NewLine +
                            "    <direction />" + Environment.NewLine +
                            "    <indexCurve />" + Environment.NewLine +
                            "    <logCurveInfo uid=\"\">" + Environment.NewLine +
                            "      <mnemonic />" + Environment.NewLine +
                            "      <unit />" + Environment.NewLine +
                            "      <minIndex uom=\"\" />" + Environment.NewLine +
                            "      <maxIndex uom=\"\" />" + Environment.NewLine +
                            "      <minDateTimeIndex />" + Environment.NewLine +
                            "      <maxDateTimeIndex />" + Environment.NewLine +
                            "      <curveDescription />" + Environment.NewLine +
                            "      <typeLogData />" + Environment.NewLine +
                            "    </logCurveInfo>" + Environment.NewLine +
                            "  </log>" + Environment.NewLine +
                            "</logs>";
            var result = _devKit.GetFromStore(ObjectTypes.Log, queryIn, null, "returnElements=requested");
            Assert.AreEqual((short)ErrorCodes.Success, result.Result);
            Assert.IsNotNull(result);
            var logs = EnergisticsConverter.XmlToObject<LogList>(result.XMLout);
            Assert.IsNotNull(logs);
            var log = logs.Log.FirstOrDefault();
            Assert.IsNotNull(log);
            Assert.AreEqual(log.LogCurveInfo.Count, 3);
            Assert.AreEqual(log.LogData.Count, 0);
        }

        [TestMethod]
        public void LogDataAdapter_GetFromStore_Return_Latest_N_Values()
        {
            _devKit.Add<WellList, Well>(_well);
            _devKit.Add<WellboreList, Wellbore>(_wellbore);

            _devKit.InitHeader(_log, LogIndexType.measureddepth);

            var channel3 = _log.LogCurveInfo[1];
            var channel4 = _log.LogCurveInfo[2];

            _log.LogCurveInfo.Add(new LogCurveInfo
            {
                Uid = "ROP1",
                Unit = channel3.Unit,
                TypeLogData = LogDataType.@double,
                Mnemonic = new ShortNameStruct
                {
                    Value = "ROP1"
                }
            });

            _log.LogCurveInfo.Add(new LogCurveInfo
            {
                Uid = "GR1",
                Unit = channel4.Unit,
                TypeLogData = LogDataType.@double,
                Mnemonic = new ShortNameStruct
                {
                    Value = "GR1"
                }
            });

            var logData = new LogData
            {
                MnemonicList = _devKit.Mnemonics(_log),
                UnitList = _devKit.Units(_log),
                Data = new List<string> {"0,,0.2,0.3,", "1,,1.2,,1.4", "2,,2.2,,2.4", "3,,3.2,,", "4,,4.2,,"}
            };
            _log.LogData = new List<LogData> {logData};

            var response = _devKit.Add<LogList, Log>(_log);
            Assert.AreEqual((short)ErrorCodes.Success, response.Result);

            var query = new Log
            {
                Uid = _log.Uid,
                UidWell = _log.UidWell,
                UidWellbore = _log.UidWellbore
            };

            var results = _devKit.Query<LogList, Log>(query, optionsIn: OptionsIn.ReturnElements.All + ';' + OptionsIn.RequestLatestValues.Eq(1));
            Assert.IsNotNull(results);

            var result = results.First();
            Assert.IsNotNull(result);

            logData = result.LogData.First();
            Assert.IsNotNull(logData);
            Assert.IsTrue(logData.Data.Count > 0);

            var data = new Dictionary<int, List<string>>();
            foreach (var row in logData.Data)
            {
                var points = row.Split(',');
                for (var i = 1; i < points.Length; i++)
                {
                    if (!data.ContainsKey(i))
                        data.Add(i, new List<string>());

                    if (!string.IsNullOrWhiteSpace(points[i]))
                        data[i].Add(points[i]);
                }
            }

            foreach (KeyValuePair<int, List<string>> pairs in data)
            {
                Assert.AreEqual(1, pairs.Value.Count);
            }
        }

        [TestMethod]
        public void Log141DataAdapter_GetFromStore_With_Custom_Data_Delimiter()
        {
            var delimiter = "|";
            _devKit.Add<WellList, Well>(_well);
            _devKit.Add<WellboreList, Wellbore>(_wellbore);

            // Set data delimiter to other charactrer than ","
            _log.DataDelimiter = delimiter;

            _devKit.InitHeader(_log, LogIndexType.measureddepth);
            _devKit.InitDataMany(_log, _devKit.Mnemonics(_log), _devKit.Units(_log), 10, hasEmptyChannel: false);

            var response = _devKit.Add<LogList, Log>(_log);
            Assert.AreEqual((short)ErrorCodes.Success, response.Result);

            var query = new Log
            {
                Uid = _log.Uid,
                UidWell = _log.UidWell,
                UidWellbore = _log.UidWellbore,
                DataDelimiter = delimiter
            };

            var results = _devKit.Query<LogList, Log>(query, optionsIn: OptionsIn.ReturnElements.All);
            var result = results.FirstOrDefault();
            Assert.IsNotNull(result);

            // Assert data delimiter
            Assert.AreEqual(delimiter, result.DataDelimiter);

            var data = result.LogData.FirstOrDefault()?.Data;
            Assert.IsNotNull(data);

            var channelCount = _log.LogCurveInfo.Count;

            // Assert data delimiter in log data
            foreach (var row in data)
            {
                var points = ChannelDataReader.Split(row, delimiter);
                Assert.AreEqual(channelCount, points.Length);
            }
        }

        #region Helper Methods

        private WMLS_AddToStoreResponse AddSetupWellWellboreLog(int numRows, bool isDepthLog, bool hasEmptyChannel, bool increasing)
        {
            _devKit.Add<WellList, Well>(_well);
            _devKit.Add<WellboreList, Wellbore>(_wellbore);

            _devKit.InitHeader(_log, LogIndexType.measureddepth, increasing);

            var startIndex = new GenericMeasure { Uom = "m", Value = 100 };
            _log.StartIndex = startIndex;
            _devKit.InitDataMany(_log, _devKit.Mnemonics(_log), _devKit.Units(_log), numRows, 1, isDepthLog, hasEmptyChannel, increasing);

            // Add a log
            return _devKit.Add<LogList, Log>(_log);
        }

        private WMLS_AddToStoreResponse AddLogWithAction(int row, Action executeLogChanges = null,
            double factor = 1D, bool isDepthLog = true, bool hasEmptyChannel = true, bool increasing = true)
        {
            _devKit.Add<WellList, Well>(_well);
            _devKit.Add<WellboreList, Wellbore>(_wellbore);

            // Initialize the _log
            //var row = 10;
            _devKit.InitHeader(_log, isDepthLog ? LogIndexType.measureddepth : LogIndexType.datetime, increasing);
            _devKit.InitDataMany(_log, _devKit.Mnemonics(_log), _devKit.Units(_log), row, factor, isDepthLog, hasEmptyChannel, increasing);

            executeLogChanges?.Invoke();

            return _devKit.Add<LogList, Log>(_log);
        }
        #endregion Helper Methods
    }
}
