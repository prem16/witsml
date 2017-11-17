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

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Energistics.Common;
using Energistics.DataAccess;
using Energistics.DataAccess.WITSML141;
using Energistics.DataAccess.WITSML141.ComponentSchemas;
using Energistics.DataAccess.WITSML141.ReferenceData;
using Energistics.Datatypes;
using Energistics.Protocol;
using Energistics.Protocol.Core;
using Energistics.Protocol.Discovery;
using Energistics.Protocol.Store;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PDS.WITSMLstudio.Store.Data.FluidsReports
{
    [TestClass]
    public partial class FluidsReport141EtpTests : FluidsReport141TestBase
    {
        partial void BeforeEachTest();

        partial void AfterEachTest();

        protected override void OnTestSetUp()
        {
            EtpSetUp(DevKit.Container);
            BeforeEachTest();
            _server.Start();
        }

        protected override void OnTestCleanUp()
        {
            _server?.Stop();
            EtpCleanUp();
            AfterEachTest();
        }

        [TestMethod]
        public void FluidsReport141_Ensure_Creates_FluidsReport_With_Default_Values()
        {
            DevKit.EnsureAndAssert<FluidsReportList, FluidsReport>(FluidsReport);
        }

        [TestMethod]
        public async Task FluidsReport141_GetResources_Can_Get_All_FluidsReport_Resources()
        {
            AddParents();
            DevKit.AddAndAssert<FluidsReportList, FluidsReport>(FluidsReport);
            await RequestSessionAndAssert();

            var uri = FluidsReport.GetUri();
            var parentUri = uri.Parent;

            await GetResourcesAndAssert(parentUri);

            var folderUri = parentUri.Append(uri.ObjectType);
            await GetResourcesAndAssert(folderUri);
        }

        [TestMethod]
        public async Task FluidsReport141_PutObject_Can_Add_FluidsReport()
        {
            AddParents();
            await RequestSessionAndAssert();

            var handler = _client.Handler<IStoreCustomer>();
            var uri = FluidsReport.GetUri();

            var dataObject = CreateDataObject<FluidsReportList, FluidsReport>(uri, FluidsReport);

            // Get Object Expecting it Not to Exist
            await GetAndAssert(handler, uri, Energistics.EtpErrorCodes.NotFound);

            // Put Object
            await PutAndAssert(handler, dataObject);

            // Get Object
            var args = await GetAndAssert(handler, uri);

            // Check Data Object XML
            Assert.IsNotNull(args?.Message.DataObject);
            var xml = args.Message.DataObject.GetString();

            var result = Parse<FluidsReportList, FluidsReport>(xml);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task FluidsReport141_PutObject_Can_Update_FluidsReport()
        {
            AddParents();
            await RequestSessionAndAssert();

            var handler = _client.Handler<IStoreCustomer>();
            var uri = FluidsReport.GetUri();

            // Add a Comment to Data Object            
            FluidsReport.CommonData = new CommonData()
            {
                Comments = "Test PutObject"
            };

            var dataObject = CreateDataObject<FluidsReportList, FluidsReport>(uri, FluidsReport);

            // Get Object Expecting it Not to Exist
            await GetAndAssert(handler, uri, Energistics.EtpErrorCodes.NotFound);

            // Put Object for Add
            await PutAndAssert(handler, dataObject);

            // Get Added Object
            var args =await GetAndAssert(handler, uri);

            // Check Added Data Object XML
            Assert.IsNotNull(args?.Message.DataObject);
            var xml = args.Message.DataObject.GetString();

            var result = Parse<FluidsReportList, FluidsReport>(xml);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.CommonData.Comments);

            // Remove Comment from Data Object
            result.CommonData.Comments = null;

            var updateDataObject = CreateDataObject<FluidsReportList, FluidsReport>(uri, result);

            // Put Object for Update
            await PutAndAssert(handler, updateDataObject);

            // Get Updated Object
            args = await GetAndAssert(handler, uri);

            // Check Added Data Object XML
            Assert.IsNotNull(args?.Message.DataObject);
            var updateXml = args.Message.DataObject.GetString();

            result = Parse<FluidsReportList, FluidsReport>(updateXml);
            Assert.IsNotNull(result);

            // Test Data Object overwrite
            Assert.IsNull(result.CommonData.Comments);
        }

        [TestMethod]
        public async Task FluidsReport141_DeleteObject_Can_Delete_FluidsReport()
        {
            AddParents();
            await RequestSessionAndAssert();

            var handler = _client.Handler<IStoreCustomer>();
            var uri = FluidsReport.GetUri();

            var dataObject = CreateDataObject<FluidsReportList, FluidsReport>(uri, FluidsReport);

            // Get Object Expecting it Not to Exist
            await GetAndAssert(handler, uri, Energistics.EtpErrorCodes.NotFound);

            // Put Object
            await PutAndAssert(handler, dataObject);

            // Get Object
            var args = await GetAndAssert(handler, uri);

            // Check Data Object XML
            Assert.IsNotNull(args?.Message.DataObject);
            var xml = args.Message.DataObject.GetString();

            var result = Parse<FluidsReportList, FluidsReport>(xml);
            Assert.IsNotNull(result);

            // Delete Object
            await DeleteAndAssert(handler, uri);

            // Get Object Expecting it Not to Exist
            await GetAndAssert(handler, uri, Energistics.EtpErrorCodes.NotFound);
        }
    }
}