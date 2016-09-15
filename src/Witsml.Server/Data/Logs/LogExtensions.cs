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
using log4net;
using PDS.Framework;
using PDS.Witsml.Data.Channels;
using Witsml131 = Energistics.DataAccess.WITSML131;
using Witsml141 = Energistics.DataAccess.WITSML141;

namespace PDS.Witsml.Server.Data.Logs
{
    /// <summary>
    /// Provides common helper methods for Log data objects.
    /// </summary>
    public static class LogExtensions
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(LogExtensions));

        /// <summary>
        /// Ensures the index curve is in the first position.
        /// </summary>
        /// <param name="list">The list of log curves.</param>
        /// <param name="mnemonic">The index curve mnemonic.</param>
        public static void MoveToFirst(this List<Witsml131.ComponentSchemas.LogCurveInfo> list, string mnemonic)
        {
            _log.DebugFormat("Moving index logCurveInfo to first position: {0}", mnemonic);

            if (list == null || !list.Any() || string.IsNullOrWhiteSpace(mnemonic)) return;
            if (list[0].Mnemonic.EqualsIgnoreCase(mnemonic)) return;

            var indexCurve = list.FirstOrDefault(x => mnemonic.EqualsIgnoreCase(x.Mnemonic));
            if (indexCurve == null) return;

            list.Remove(indexCurve);
            list.Insert(0, indexCurve);
        }

        /// <summary>
        /// Ensures the index curve is in the first position.
        /// </summary>
        /// <param name="list">The list of log curves.</param>
        /// <param name="mnemonic">The index curve mnemonic.</param>
        public static void MoveToFirst(this List<Witsml141.ComponentSchemas.LogCurveInfo> list, string mnemonic)
        {
            _log.DebugFormat("Moving index logCurveInfo to first position: {0}", mnemonic);

            if (list == null || !list.Any() || string.IsNullOrWhiteSpace(mnemonic)) return;
            if (list[0].Mnemonic.Value.EqualsIgnoreCase(mnemonic)) return;

            var indexCurve = list.FirstOrDefault(x => mnemonic.EqualsIgnoreCase(x.Mnemonic?.Value));
            if (indexCurve == null) return;

            list.Remove(indexCurve);
            list.Insert(0, indexCurve);
        }

        /// <summary>
        /// Determines whether Log data should be included in the query response.
        /// </summary>
        /// <param name="parser">The query parser.</param>
        /// <returns><c>true</c> if Log data should be included; otherwise, <c>false</c>.</returns>
        public static bool IncludeLogData(this WitsmlQueryParser parser)
        {
            var returnElements = parser.ReturnElements();

            _log.DebugFormat("Checking if log data should be included. Return Elements: {0};", returnElements);

            return OptionsIn.ReturnElements.All.Equals(returnElements) ||
                   OptionsIn.ReturnElements.DataOnly.Equals(returnElements) ||
                   (OptionsIn.ReturnElements.Requested.Equals(returnElements) && parser.Contains("logData"));
        }

        /// <summary>
        /// Gets the log curve information mnemonics.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <returns></returns>
        public static IEnumerable<string> GetLogCurveInfoMnemonics(this WitsmlQueryParser parser)
        {
            _log.Debug("Getting logCurveInfo mnemonics from parser.");

            var logCurveInfos = parser.Properties("logCurveInfo").ToArray();
            if (!logCurveInfos.Any()) return new string[0];

            var mnemonicList = parser.Properties(logCurveInfos, "mnemonic").ToArray();

            return mnemonicList.Any()
                ? mnemonicList.Select(x => x.Value)
                : Enumerable.Empty<string>();
        }

        /// <summary>
        /// Gets the log data mnemonics.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <returns></returns>
        public static IEnumerable<string> GetLogDataMnemonics(this WitsmlQueryParser parser)
        {
            _log.Debug("Getting logData mnemonics from parser.");

            var logData = parser.Property("logData");
            if (logData == null) return null;

            var mnemonicList = parser.Properties(logData, "mnemonicList").FirstOrDefault();
            if (mnemonicList == null) return null;

            return string.IsNullOrWhiteSpace(mnemonicList.Value)
                ? Enumerable.Empty<string>()
                : ChannelDataReader.Split(mnemonicList.Value);
        }

        /// <summary>
        /// Checks the log data for duplicate indexes.
        /// </summary>
        /// <param name="logData">The log data.</param>
        /// <param name="function">The context function</param>
        /// <param name="delimiter">The logData delimiter.</param>
        /// <param name="isTimeLog">Is the log a time log.</param>
        /// <returns><c>true</c> if Log data has duplicates; otherwise, <c>false</c>.</returns>
        public static bool HasDuplicateIndexes(this List<string> logData, Functions function, string delimiter, bool isTimeLog)
        {
            var indexValues = new HashSet<double>();
            foreach (var s in logData)
            {
                if (isTimeLog)
                {
                    var value = s.Substring(0, s.IndexOf(delimiter, StringComparison.InvariantCulture));

                    DateTimeOffset dto;
                    if (!DateTimeOffset.TryParse(value, out dto))
                        throw new WitsmlException(function.GetNonConformingErrorCode());

                    if (indexValues.Contains(dto.UtcTicks))
                        return true;

                    indexValues.Add(dto.UtcTicks);
                }
                else
                {
                    var value = s.Substring(0, s.IndexOf(delimiter, StringComparison.InvariantCulture));

                    double doubleValue;
                    if (!double.TryParse(value, out doubleValue))
                        throw new WitsmlException(function.GetNonConformingErrorCode());

                    if (indexValues.Contains(doubleValue))
                        return true;

                    indexValues.Add(doubleValue);
                }
            }
            return false;
        }
    }
}
