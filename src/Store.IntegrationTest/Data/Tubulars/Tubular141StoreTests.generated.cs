//----------------------------------------------------------------------- 
// PDS WITSMLstudio Store, 2017.2
//
// Copyright 2017 PDS Americas LLC
// 
// Licensed under the PDS Open Source WITSML Product License Agreement (the
// "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//   
//     http://www.pds.group/WITSMLstudio/OpenSource/ProductLicenseAgreement
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-----------------------------------------------------------------------

// ----------------------------------------------------------------------
// <auto-generated>
//     Changes to this file may cause incorrect behavior and will be lost
//     if the code is regenerated.
// </auto-generated>
// ----------------------------------------------------------------------

using Energistics.DataAccess;
using Energistics.DataAccess.WITSML141;
using Energistics.DataAccess.WITSML141.ComponentSchemas;
using Energistics.DataAccess.WITSML141.ReferenceData;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace PDS.WITSMLstudio.Store.Data.Tubulars
{
    [TestClass]
    public partial class Tubular141StoreTests : Tubular141TestBase
    {
        partial void BeforeEachTest();

        partial void AfterEachTest();

        protected override void OnTestSetUp()
        {
            BeforeEachTest();
        }

        protected override void OnTestCleanUp()
        {
            AfterEachTest();
        }

        [TestMethod]
        public void Tubular141DataAdapter_GetFromStore_Can_Get_Tubular()
        {
            AddParents();
            DevKit.AddAndAssert<TubularList, Tubular>(Tubular);
            DevKit.GetAndAssert<TubularList, Tubular>(Tubular);
       }

        [TestMethod]
        public void Tubular141DataAdapter_AddToStore_Can_Add_Tubular()
        {
            AddParents();
            DevKit.AddAndAssert<TubularList, Tubular>(Tubular);
        }

        [TestMethod]
        public void Tubular141DataAdapter_UpdateInStore_Can_Update_Tubular()
        {
            AddParents();
            DevKit.AddAndAssert<TubularList, Tubular>(Tubular);
            DevKit.UpdateAndAssert<TubularList, Tubular>(Tubular);
            DevKit.GetAndAssert<TubularList, Tubular>(Tubular);
        }

        [TestMethod]
        public void Tubular141DataAdapter_DeleteFromStore_Can_Delete_Tubular()
        {
            AddParents();
            DevKit.AddAndAssert<TubularList, Tubular>(Tubular);
            DevKit.DeleteAndAssert<TubularList, Tubular>(Tubular);
            DevKit.GetAndAssert<TubularList, Tubular>(Tubular, isNotNull: false);
        }

        [TestMethod]
        public void Tubular141WitsmlStore_GetFromStore_Can_Transform_Tubular()
        {
            AddParents();
            DevKit.AddAndAssert<TubularList, Tubular>(Tubular);

            // Re-initialize all capServer providers
            DevKit.Store.CapServerProviders = null;
            DevKit.Container.BuildUp(DevKit.Store);

            string typeIn, queryIn;
            var query = DevKit.List(DevKit.CreateQuery(Tubular));
            DevKit.SetupParameters<TubularList, Tubular>(query, ObjectTypes.Tubular, out typeIn, out queryIn);

            var options = OptionsIn.Join(OptionsIn.ReturnElements.All, OptionsIn.DataVersion.Version131);
            var request = new WMLS_GetFromStoreRequest(typeIn, queryIn, options, null);
            var response = DevKit.Store.WMLS_GetFromStore(request);

            Assert.IsFalse(string.IsNullOrWhiteSpace(response.XMLout));
            Assert.AreEqual((short)ErrorCodes.Success, response.Result);

            var result = WitsmlParser.Parse(response.XMLout);
            var version = ObjectTypes.GetVersion(result.Root);
            Assert.AreEqual(OptionsIn.DataVersion.Version131.Value, version);
        }

        [TestMethod]
        public void Tubular141DataAdapter_AddToStore_Creates_ChangeLog()
        {
            AddParents();

            DevKit.AddAndAssert<TubularList, Tubular>(Tubular);

            var result = DevKit.GetAndAssert<TubularList, Tubular>(Tubular);
            var expectedHistoryCount = 1;
            var expectedChangeType = ChangeInfoType.add;
            DevKit.AssertChangeLog(result, expectedHistoryCount, expectedChangeType);
        }

        [TestMethod]
        public void Tubular141DataAdapter_UpdateInStore_Updates_ChangeLog()
        {
            AddParents();

            DevKit.AddAndAssert<TubularList, Tubular>(Tubular);

            // Update the Tubular141
            Tubular.Name = "Change";
            DevKit.UpdateAndAssert(Tubular);

            var result = DevKit.GetAndAssert<TubularList, Tubular>(Tubular);
            var expectedHistoryCount = 2;
            var expectedChangeType = ChangeInfoType.update;
            DevKit.AssertChangeLog(result, expectedHistoryCount, expectedChangeType);
        }

        [TestMethod]
        public void Tubular141DataAdapter_DeleteFromStore_Updates_ChangeLog()
        {
            AddParents();

            DevKit.AddAndAssert<TubularList, Tubular>(Tubular);

            // Delete the Tubular141
            DevKit.DeleteAndAssert(Tubular);

            var expectedHistoryCount = 2;
            var expectedChangeType = ChangeInfoType.delete;
            DevKit.AssertChangeLog(Tubular, expectedHistoryCount, expectedChangeType);
        }

        [TestMethod]
        public void Tubular141DataAdapter_ChangeLog_Tracks_ChangeHistory_For_Add_Update_Delete()
        {
            AddParents();

            // Add the Tubular141
            DevKit.AddAndAssert<TubularList, Tubular>(Tubular);

            // Verify ChangeLog for Add
            var result = DevKit.GetAndAssert<TubularList, Tubular>(Tubular);
            var expectedHistoryCount = 1;
            var expectedChangeType = ChangeInfoType.add;
            DevKit.AssertChangeLog(result, expectedHistoryCount, expectedChangeType);

            // Update the Tubular141
            Tubular.Name = "Change";
            DevKit.UpdateAndAssert(Tubular);

            result = DevKit.GetAndAssert<TubularList, Tubular>(Tubular);
            expectedHistoryCount = 2;
            expectedChangeType = ChangeInfoType.update;
            DevKit.AssertChangeLog(result, expectedHistoryCount, expectedChangeType);

            // Delete the Tubular141
            DevKit.DeleteAndAssert(Tubular);

            expectedHistoryCount = 3;
            expectedChangeType = ChangeInfoType.delete;
            DevKit.AssertChangeLog(Tubular, expectedHistoryCount, expectedChangeType);

            // Re-add the same Tubular141...
            DevKit.AddAndAssert<TubularList, Tubular>(Tubular);

            //... the same changeLog should be reused.
            result = DevKit.GetAndAssert<TubularList, Tubular>(Tubular);
            expectedHistoryCount = 4;
            expectedChangeType = ChangeInfoType.add;
            DevKit.AssertChangeLog(result, expectedHistoryCount, expectedChangeType);

            DevKit.AssertChangeHistoryTimesUnique(result);
        }

        [TestMethod]
        public void Tubular141DataAdapter_GetFromStore_Filter_ExtensionNameValue()
        {
            AddParents();

            var extensionName1 = DevKit.ExtensionNameValue("Ext-1", "1.0", "m");
            var extensionName2 = DevKit.ExtensionNameValue("Ext-2", "2.0", "cm", PrimitiveType.@float);
            extensionName2.MeasureClass = MeasureClass.Length;
            var extensionName3 = DevKit.ExtensionNameValue("Ext-3", "3.0", "cm", PrimitiveType.unknown);

            Tubular.CommonData = new CommonData()
            {
                ExtensionNameValue = new List<ExtensionNameValue>()
                {
                    extensionName1, extensionName2, extensionName3
                }
            };

            // Add the Tubular141
            DevKit.AddAndAssert(Tubular);

            // Query for first extension
            var commonDataXml = "<commonData>" + Environment.NewLine +
                                "<extensionNameValue uid=\"\">" + Environment.NewLine +
                                "<name />{0}" + Environment.NewLine +
                                "</extensionNameValue>" + Environment.NewLine +
                                "</commonData>";

            var extValueQuery = string.Format(commonDataXml, "<dataType>double</dataType>");
            var queryXml = string.Format(BasicXMLTemplate, Tubular.UidWell, Tubular.UidWellbore, Tubular.Uid, extValueQuery);
            var result = DevKit.Query<TubularList, Tubular>(ObjectTypes.Tubular, queryXml, null, OptionsIn.ReturnElements.Requested);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);

            var resultTubular = result[0];
            Assert.IsNotNull(resultTubular);

            var commonData = resultTubular.CommonData;
            Assert.IsNotNull(commonData);
            Assert.AreEqual(1, commonData.ExtensionNameValue.Count);

            var env = commonData.ExtensionNameValue[0];
            Assert.IsNotNull(env);
            Assert.AreEqual(extensionName1.Uid, env.Uid);
            Assert.AreEqual(extensionName1.Name, env.Name);

            // Query for second extension
            extValueQuery = string.Format(commonDataXml, "<measureClass>length</measureClass>");
            queryXml = string.Format(BasicXMLTemplate, Tubular.UidWell, Tubular.UidWellbore, Tubular.Uid, extValueQuery);
            result = DevKit.Query<TubularList, Tubular>(ObjectTypes.Tubular, queryXml, null, OptionsIn.ReturnElements.Requested);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);

            resultTubular = result[0];
            Assert.IsNotNull(resultTubular);

            commonData = resultTubular.CommonData;
            Assert.IsNotNull(commonData);
            Assert.AreEqual(1, commonData.ExtensionNameValue.Count);

            env = commonData.ExtensionNameValue[0];
            Assert.IsNotNull(env);
            Assert.AreEqual(extensionName2.Uid, env.Uid);
            Assert.AreEqual(extensionName2.Name, env.Name);

            // Query for third extension
            extValueQuery = string.Format(commonDataXml, "<dataType>unknown</dataType>");
            queryXml = string.Format(BasicXMLTemplate, Tubular.UidWell, Tubular.UidWellbore, Tubular.Uid, extValueQuery);
            result = DevKit.Query<TubularList, Tubular>(ObjectTypes.Tubular, queryXml, null, OptionsIn.ReturnElements.Requested);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);

            resultTubular = result[0];
            Assert.IsNotNull(resultTubular);

            commonData = resultTubular.CommonData;
            Assert.IsNotNull(commonData);
            Assert.AreEqual(1, commonData.ExtensionNameValue.Count);

            env = commonData.ExtensionNameValue[0];
            Assert.IsNotNull(env);
            Assert.AreEqual(extensionName3.Uid, env.Uid);
            Assert.AreEqual(extensionName3.Name, env.Name);
        }

        [TestMethod]
        public void Tubular141DataAdapter_ChangeLog_Syncs_Tubular_Name_Changes()
        {
            AddParents();

            // Add the Tubular141
            DevKit.AddAndAssert<TubularList, Tubular>(Tubular);

            // Assert that all Tubular names match corresponding changeLog names
            DevKit.AssertChangeLogNames(Tubular);

            // Update the Tubular141 names
            Tubular.Name = "Change";
            Tubular.NameWell = "Well Name Change";

            Tubular.NameWellbore = "Wellbore Name Change";

            DevKit.UpdateAndAssert(Tubular);

            // Assert that all Tubular names match corresponding changeLog names
            DevKit.AssertChangeLogNames(Tubular);
        }
    }
}